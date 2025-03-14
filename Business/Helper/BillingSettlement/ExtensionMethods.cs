using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Business.Helper.ReportProcessor;
using CommonLib.DataAccess;

namespace Business.Helper.BillingSettlement
{
    public static class ExtensionMethods
    {
        public static Settlement AssertMonthlyBillingSettlement(this DateTime settlementDate, GenericManager<EIVOEntityDataContext> models)
        {
            lock (typeof(ExtensionMethods))
            {
                Settlement? settlement = models.GetTable<Settlement>()
                            .Where(s => s.Year == settlementDate.Year)
                            .Where(s => s.Month == settlementDate.Month)
                            .FirstOrDefault();
                if (settlement == null)
                {
                    settlement = new Settlement
                    {
                        Year = settlementDate.Year,
                        Month = settlementDate.Month,
                        SettlementDate = settlementDate,
                        StartDate = new DateTime(settlementDate.Year, settlementDate.Month, 1),
                    };
                    settlement.EndExclusiveDate = settlement.StartDate.AddMonths(1);
                    models.GetTable<Settlement>().InsertOnSubmit(settlement);
                    models.SubmitChanges();
                }

                return settlement;
            }
        }

        public static void DoMonthlyBillingSettlement(this Settlement settlement, GenericManager<EIVOEntityDataContext> models)
        {
            lock (typeof(ExtensionMethods))
            {
                var category = models.GetTable<OrganizationCategory>()
                        .Where(c => c.CategoryID == (int)CategoryDefinition.CategoryEnum.發票開立營業人 
                            || c.CategoryID == (int)CategoryDefinition.CategoryEnum.經銷商);
                var orgItems = models.GetTable<Organization>()
                                    .Where(o => category.Any(c => c.CompanyID == o.CompanyID));

                foreach(var item in orgItems) 
                {
                    if (item.OrganizationExtension?.ExpirationDate.HasValue == true
                        && item.OrganizationExtension.ExpirationDate <= settlement.StartDate)
                    {
                        continue;
                    }

                    if(item.OrganizationExtension?.GoLiveDate.HasValue == false)
                    {
                        continue;
                    }

                    MonthlyBilling? billing = models.GetTable<MonthlyBilling>()
                        .Where(b => b.CompanyID == item.CompanyID)
                        .Where(b => b.SettlementID == settlement.SettlementID)
                        .FirstOrDefault();

                    if (billing == null)
                    {
                        billing = new MonthlyBilling
                        {
                            CompanyID = item.CompanyID,
                            SettlementID = settlement.SettlementID,
                        };
                        models.GetTable<MonthlyBilling>().InsertOnSubmit(billing);
                    }

                    billing.TotalIssueCount = models.GetInvoice(item.CompanyID, settlement.StartDate, settlement.EndExclusiveDate).Count();
                    if (item.BillingCalculation.Any(c => c.TypeID == (int)Naming.DocumentTypeDefinition.E_InvoiceCancellation))
                    {
                        billing.TotalIssueCount += models.GetInvoiceCancellation(item.CompanyID, settlement.StartDate, settlement.EndExclusiveDate).Count();
                    }
                    if (item.BillingCalculation.Any(c => c.TypeID == (int)Naming.DocumentTypeDefinition.E_Allowance))
                    {
                        billing.TotalIssueCount += models.GetAllowance(item.CompanyID, settlement.StartDate, settlement.EndExclusiveDate).Count();
                    }
                    if (item.BillingCalculation.Any(c => c.TypeID == (int)Naming.DocumentTypeDefinition.E_AllowanceCancellation))
                    {
                        billing.TotalIssueCount += models.GetAllowanceCancellation(item.CompanyID, settlement.StartDate, settlement.EndExclusiveDate).Count();
                    }

                    foreach (var grade in item.BillingGrade
                        .OrderByDescending(b => b.GradeCount))
                    {
                        if (billing.TotalIssueCount >= grade.GradeCount)
                        {
                            billing.IssueChargeAmount = grade.BasicFee;
                            break;
                        }
                    }

                    foreach (var increment in item.BillingIncrement
                        .OrderByDescending(c => c.UpperBound))
                    {
                        if (billing.TotalIssueCount > increment.UpperBound)
                        {
                            billing.IssueChargeAmount += (int)Math.Round((billing.TotalIssueCount - increment.UpperBound) * (increment.UnitFee ?? 0M));
                            break;
                        }
                    }

                    foreach (var extra in item.ExtraBillingItem.ToList())
                    {
                        if (!extra.BillingDate.HasValue || extra.BillingDate < settlement.StartDate)
                        {
                            billing.MonthlyExtraBilling.Add(new MonthlyExtraBilling
                            {
                                ItemID = extra.ItemID,
                                ItemName = extra.ItemName,
                                Fee = extra.Fee,
                            });

                            if (!extra.BillingType.HasValue || extra.BillingType == (int)ExtraBillingItem.BillingTypeEnum.PayOnce)
                            {
                                models.ExecuteCommand(@"DELETE FROM billing.ExtraBillingItem
                                        WHERE   (ItemID = {0})", extra.ItemID);
                            }
                        }
                    }

                    models.SubmitChanges();
                }
            }
        }

        public static void DoSubmitBill(this GenericManager<EIVOEntityDataContext> models)
        {
            lock (typeof(ExtensionMethods))
            {

                var billingItems = models.GetTable<MonthlyBilling>()
                        .Where(b => !b.BillID.HasValue)
                        .ToList();

                if(billingItems.Count > 0) 
                {
                    BillSubmission submission = new BillSubmission 
                    {
                        BillDate = DateTime.Now,
                    };

                    models.GetTable<BillSubmission>().InsertOnSubmit(submission);

                    foreach (var billing in billingItems.GroupBy(b => b.CompanyID))
                    {
                        var cust = billing.First().Organization;
                        if (cust.BillingExtension == null || cust.BillingExtension.BillingCycleInMonth <= billing.Count())
                        {
                            foreach(var item in billing) 
                            {
                                item.BillSubmission = submission;
                            }
                        }
                    }

                    models.SubmitChanges();

                }
            }
        }

    }
}
