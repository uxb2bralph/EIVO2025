using CommonLib.Core.Utility;
using CommonLib.DataAccess;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Presentation;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.InvoiceManagement.ErrorHandle;
using ModelCore.Models.ViewModel;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.TXN;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Business.Helper.ReportProcessor
{
    public static class InvoiceDataReportExtensions
    {
        public static void CreateReport(this MonthlyReportQueryViewModel viewModel)
        {
            if (!viewModel.DateFrom.HasValue)
            {
                viewModel.DateFrom = DateTime.Today;
            }

            if (!viewModel.DateTo.HasValue)
            {
                viewModel.DateTo = viewModel.DateFrom.Value.AddMonths(1);
            }
            else
            {
                viewModel.DateTo = viewModel.DateTo.Value.AddMonths(1);
            }

            viewModel.DateFrom = new DateTime(viewModel.DateFrom.Value.Year, viewModel.DateFrom.Value.Month, 1);
            viewModel.DateTo = new DateTime(viewModel.DateTo.Value.Year, viewModel.DateTo.Value.Month, 1);

            using (GenericManager<EIVOEntityDataContext> models = new GenericManager<EIVOEntityDataContext>())
            {
                ProcessRequest taskItem = models.GetTable<ProcessRequest>()
                                            .Where(p => p.TaskID == viewModel.TaskID)
                                            .FirstOrDefault();
                if (taskItem == null)
                    return;

                if (taskItem.ResponsePath == null)
                {
                    taskItem.ResponsePath = Path.Combine(Logger.LogDailyPath, Guid.NewGuid().ToString() + ".xlsx");
                }

                var issuers = models.GetTable<InvoiceIssuerAgent>().Where(a => a.AgentID == viewModel.AgentID);
                var sellers = models.GetTable<Organization>()
                    .Where(c => c.CompanyID == viewModel.SellerID
                        || issuers.Any(i => i.IssuerID == c.CompanyID));

                Exception exception = null;

                try
                {
                    using (DataSet ds = new DataSet())
                    {
                        int chargeAmountByAgent = 0;
                        DataTable table = new DataTable("發票資料明細");
                        //ds.Tables.Add(table);
                        table.Columns.Add("營業人名稱");
                        table.Columns.Add("統一編號");
                        table.Columns.Add("發票", typeof(int));
                        table.Columns.Add("作廢發票", typeof(int));
                        table.Columns.Add("折讓", typeof(int));
                        table.Columns.Add("作廢折讓", typeof(int));
                        table.Columns.Add("月份");
                        table.Columns.Add("上線日期", typeof(String));
                        table.Columns.Add("註記停用日期", typeof(String));
                        table.Columns.Add("總張數", typeof(int));
                        table.Columns.Add("計費", typeof(int));

                        foreach (var issuer in sellers)
                        {
                            for (DateTime idx = viewModel.DateFrom.Value; idx < viewModel.DateTo;)
                            {
                                var end = idx.AddMonths(1);
                                var r = table.NewRow();

                                var seller = issuer;
                                r[0] = seller.CompanyName;
                                r[1] = seller.ReceiptNo;
                                r[2] = models.GetInvoice(seller.CompanyID, idx, end).Count();
                                r[3] = models.GetInvoiceCancellation(seller.CompanyID, idx, end).Count();
                                r[4] = models.GetAllowance(seller.CompanyID, idx, end).Count();
                                r[5] = models.GetAllowanceCancellation(seller.CompanyID, idx, end).Count();
                                r[6] = $"{idx:yyyyMM}";
                                r[7] = $"{seller.OrganizationExtension?.GoLiveDate:yyyy/MM/dd}";
                                r[8] = $"{seller.OrganizationExtension?.ExpirationDate:yyyy/MM/dd}";

                                var sellerMonthlyBilling = GetSellerMonthlyBilling(
                                    dataModels: models
                                    , organization: seller
                                    , idx);

                                r[9] = (sellerMonthlyBilling == null) ? 0 : sellerMonthlyBilling.TotalIssueCount;
                                r[10] = (sellerMonthlyBilling == null) ? 0 : sellerMonthlyBilling.IssueChargeAmount;

                                //上線月份不計費
                                if ((seller.OrganizationExtension?.GoLiveDate?.ToString("yyyyMM")
                                            == $"{idx:yyyyMM}"))
                                {
                                    r[10] = 0;
                                }

                                chargeAmountByAgent += Convert.ToInt32(r[10]);

                                table.Rows.Add(r);
                                idx = end;

                            }
                        }

                        DataTable orderedDataTable
                            = table.AsEnumerable()
                            .OrderByDescending(row => !string.IsNullOrWhiteSpace(row.Field<string>("上線日期")))
                            .ThenBy(row => row.Field<string>("上線日期"))
                            .CopyToDataTable();

                        orderedDataTable.Rows.Add(new Object[] { "" });
                        orderedDataTable.Rows.Add(new Object[] { "月服務費", chargeAmountByAgent });
                        ds.Tables.Add(orderedDataTable);
                        using (var xls = ds.ConvertToExcel())
                        {
                            xls.SaveAs(taskItem.ResponsePath);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    exception = ex;
                    Logger.Error(ex);
                }

                if (exception != null)
                {
                    taskItem.ExceptionLog = new ExceptionLog
                    {
                        DataContent = exception.Message
                    };
                }

                taskItem.ProcessComplete = DateTime.Now;
                models.SubmitChanges();


            }
        }

        public static IQueryable<InvoiceItem> GetInvoice(this GenericManager<EIVOEntityDataContext> models, int sellerID, DateTime dateFrom, DateTime dateTo)
        {
            var invoice = models.GetTable<InvoiceItem>()
                                .Where(i => i.SellerID == sellerID)
                                .Where(i => i.InvoiceDate >= dateFrom)
                                .Where(i => i.InvoiceDate < dateTo);
            return invoice;
        }

        public static MonthlyBilling GetSellerMonthlyBilling(
            GenericManager<EIVOEntityDataContext> dataModels
            , Organization organization
            , DateTime queryFromDate)
        {

            return dataModels.GetTable<Settlement>()
                    .Where(i => i.Year == Convert.ToInt32($"{queryFromDate:yyyy}"))
                    .Where(i => i.Month == Convert.ToInt32($"{queryFromDate:MM}"))
                    .Join(dataModels.GetTable<MonthlyBilling>().Where(i => i.Organization == organization),
                                    c => c.SettlementID
                                    , i => i.SettlementID
                                    , (c, i) => i).FirstOrDefault();
        }

        public static IQueryable<InvoiceCancellation> GetInvoiceCancellation(this GenericManager<EIVOEntityDataContext> models, int sellerID, DateTime dateFrom, DateTime dateTo)
        {
            var cancelInvoice = models.GetTable<InvoiceCancellation>()
                                
                                .Join(models.GetTable<InvoiceItem>()
                                    .Where(i => i.InvoiceDate >= dateFrom)
                                    .Where(i => i.InvoiceDate < dateTo)
                                    .Where(i => i.SellerID == sellerID),
                                    c => c.InvoiceID, i => i.InvoiceID, (c, i) => c);

            return cancelInvoice;
        }


        public static IQueryable<InvoiceAllowance> GetAllowance(this GenericManager<EIVOEntityDataContext> models, int sellerID, DateTime dateFrom, DateTime dateTo)
        {
            var allowance = models.GetTable<InvoiceAllowance>()
                                .Where(i => i.AllowanceDate >= dateFrom)
                                .Where(i => i.AllowanceDate < dateTo)
                                .Join(models.GetTable<InvoiceAllowanceSeller>()
                                    .Where(i => i.SellerID == sellerID),
                        a => a.AllowanceID, s => s.AllowanceID, (a, s) => a);
            return allowance;
        }

        public static IQueryable<InvoiceAllowanceCancellation> GetAllowanceCancellation(this GenericManager<EIVOEntityDataContext> models, int sellerID, DateTime dateFrom, DateTime dateTo)
        {
            var cancelAllowance = models.GetTable<InvoiceAllowanceCancellation>()
                                .Where(i => i.CancelDate >= dateFrom)
                                .Where(i => i.CancelDate < dateTo)
                                .Join(models.GetTable<InvoiceAllowance>()
                                    .Join(models.GetTable<InvoiceAllowanceSeller>()
                                            .Where(i => i.SellerID == sellerID),
                                        a => a.AllowanceID, s => s.AllowanceID, (a, s) => a),
                                c => c.AllowanceID, a => a.AllowanceID, (c, a) => c);

            return cancelAllowance;
        }
    }
}
