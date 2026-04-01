using CommonLib.Core.DataWork;
using CommonLib.Utility;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using ModelCore.DataEntityWrapper;
using ModelCore.DTOs;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using ModelCore.Security;
using ModelCore.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;


namespace ModelCore.Helper
{
    public static class InvoiceBusinessExtensionMethods
    {
        public static void MarkPrintedLog(this GenericDbContext<ApplicationDbContext> models,InvoiceItem item,UserProfile profile)
        {
            models.MarkPrintedLog(item, profile.UID);
        }

        public static void MarkPrintedLog(this GenericDbContext<ApplicationDbContext> models, InvoiceAllowance item, UserProfile profile)
        {
            models.MarkPrintedLog(item, profile.UID);
        }


        public static IQueryable<InvoiceItem> FilterInvoiceByRole(this GenericDbContext<ApplicationDbContext> models, UserProfileWrapper profile, IQueryable<InvoiceItem> items, Naming.InvoiceCenterBusinessType? businessType = null)
        {
            String? receiptNo;
            IQueryable<InvoiceBuyer> buyerItems;
            switch ((Naming.CategoryID?)profile?.CurrentUserRole?.OrganizationCategory?.CategoryID)
            {
                case Naming.CategoryID.COMP_SYS:
                case Naming.CategoryID.COMP_WELFARE:
                    return items;

                case Naming.CategoryID.COMP_INVOICE_AGENT:
                    return models.GetInvoiceByAgent(items, profile.CurrentUserRole.OrganizationCategory.CompanyID, businessType: businessType);

                case Naming.CategoryID.COMP_E_INVOICE_GOOGLE_TW:
                case Naming.CategoryID.COMP_E_INVOICE_B2C_SELLER:
                    if (businessType == Naming.InvoiceCenterBusinessType.進項)
                    {
                        receiptNo = profile!.CurrentCompany(models)?.ReceiptNo;
                        buyerItems = models.GetTable<InvoiceBuyer>().Where(b => b.ReceiptNo == receiptNo);
                        return items.Where(i => buyerItems.Any(b => b.InvoiceID == i.InvoiceID));
                    }
                    else
                    {
                        return items.Where(i => i.SellerID == profile.CurrentUserRole.OrganizationCategory.CompanyID);
                    }

                case Naming.CategoryID.COMP_E_INVOICE_B2C_BUYER:
                    receiptNo = profile!.CurrentCompany(models)?.ReceiptNo;
                    buyerItems = models.GetTable<InvoiceBuyer>().Where(b => b.ReceiptNo == receiptNo);
                    return items.Where(i => buyerItems.Any(b => b.InvoiceID == i.InvoiceID));

                default:
                    if (businessType == Naming.InvoiceCenterBusinessType.進項)
                    {
                        receiptNo = profile!.CurrentCompany(models)?.ReceiptNo;
                        buyerItems = models.GetTable<InvoiceBuyer>().Where(b => b.ReceiptNo == receiptNo);
                        return items.Where(i => buyerItems.Any(b => b.InvoiceID == i.InvoiceID));
                    }
                    else
                    {
                        var companyID = profile!.CurrentUserRole?.OrganizationCategory.CompanyID;
                        return items.Where(i => i.SellerID == companyID);
                    }

            }

        }

        public static IQueryable<Organization> FilterOrganizationByRole(this GenericDbContext<ApplicationDbContext> models, UserProfileWrapper profile, IQueryable<Organization> items = null)
        {
            if (items == null)
            {
                items = models.GetTable<Organization>();
            }

            switch ((Naming.CategoryID)profile.CurrentUserRole.OrganizationCategory.CategoryID)
            {
                case Naming.CategoryID.COMP_SYS:
                    return items;

                case Naming.CategoryID.COMP_INVOICE_AGENT:
                    var issuers = models.GetTable<InvoiceIssuerAgent>().Where(a => a.AgentID == profile.CurrentUserRole.OrganizationCategory.CompanyID);
                    return items.Where(i => i.CompanyID == profile.CurrentUserRole.OrganizationCategory.CompanyID
                        || issuers.Any(a => a.IssuerID == i.CompanyID));

                case Naming.CategoryID.COMP_E_INVOICE_GOOGLE_TW:
                case Naming.CategoryID.COMP_E_INVOICE_B2C_BUYER:
                case Naming.CategoryID.COMP_WELFARE:
                default:
                    return items.Where(i => i.CompanyID == profile.CurrentUserRole.OrganizationCategory.CompanyID);
            }

        }

        public static IQueryable<InvoiceAllowance> FilterAllowanceByRole(this GenericDbContext<ApplicationDbContext> models, UserProfileWrapper profile, IQueryable<InvoiceAllowance> items)
        {
            switch ((Naming.CategoryID)profile.CurrentUserRole.OrganizationCategory.CategoryID)
            {
                case Naming.CategoryID.COMP_SYS:
                case Naming.CategoryID.COMP_WELFARE:
                    return items;

                case Naming.CategoryID.COMP_INVOICE_AGENT:
                case Naming.CategoryID.COMP_E_INVOICE_GOOGLE_TW:
                    return models.GetAllowanceByAgent(items, profile.CurrentUserRole.OrganizationCategory.CompanyID);

                case Naming.CategoryID.COMP_E_INVOICE_B2C_BUYER:
                    return items.Where(i => i.InvoiceAllowanceBuyer.BuyerID == profile.CurrentUserRole.OrganizationCategory.CompanyID);

                default:
                    return items.Where(i => i.InvoiceAllowanceSeller.SellerID == profile.CurrentUserRole.OrganizationCategory.CompanyID
                        || i.InvoiceAllowanceBuyer.BuyerID == profile.CurrentUserRole.OrganizationCategory.CompanyID);

            }

        }

        public static IQueryable<ProcessRequest> FilterProcessRequestByRole(this GenericDbContext<ApplicationDbContext> models, UserProfileWrapper profile, IQueryable<ProcessRequest> items)
        {
            switch ((Naming.CategoryID?)profile.CurrentUserRole?.OrganizationCategory.CategoryID)
            {
                case Naming.CategoryID.COMP_SYS:
                    return items;

                default:
                    var agentID = profile.CurrentUserRole?.OrganizationCategory.CompanyID;
                    var issuers = models.GetTable<InvoiceIssuerAgent>().Where(a => a.AgentID == agentID)
                        .Select(a => a.IssuerID);
                    return items.Where(i => i.Sender == profile.Entity.UID
                                    || i.AgentID == agentID
                                    || issuers.Any(a => i.AgentID == a));

            }

        }

        public static IQueryable<ProductCatalog> FilterProductCatalogByRole(this GenericDbContext<ApplicationDbContext> models, UserProfileWrapper profile, IQueryable<ProductCatalog> items)
        {
            switch ((Naming.CategoryID?)profile.CurrentUserRole?.OrganizationCategory.CategoryID)
            {
                case Naming.CategoryID.COMP_SYS:
                    return items;

                default:
                    var agentID = profile.CurrentUserRole?.OrganizationCategory.CompanyID;
                    return items.Where(i => i.Supplier.Any(s => s.CompanyID == agentID));

            }

        }

        public static List<InvoiceNoAllocation> AllocateInvoiceNo(this GenericDbContext<ApplicationDbContext> models, POSDeviceViewModel viewModel)
        {
            List<InvoiceNoAllocation> items = new List<InvoiceNoAllocation>();

            var receiptNo = viewModel.company_id.GetEfficientString();
            var orgItems = models.GetTable<Organization>().Where(c => c.ReceiptNo == receiptNo);
            var seller = orgItems.FirstOrDefault();
            bool auth = true;
            if (seller != null)
            {
                if (viewModel.Seed != null && viewModel.Authorization != null)
                {
                    auth = models.CheckAuthToken(seller, viewModel) != null;
                }

                if (auth)
                {
                    try
                    {
                        using (TrackNoManager mgr = new TrackNoManager(new GenericEntityRepository<ApplicationDbContext,InvoiceItem>(models), seller.CompanyID,viewModel.DeviceNo))
                        {
                            if(viewModel.InvoiceDate.HasValue)
                            {
                                mgr.ApplyInvoiceDate(viewModel.InvoiceDate.Value);
                            }

                            for (int i = 0; i < viewModel.quantity; i++)
                            {
                                var item = mgr.AllocateInvoiceNo();
                                if (item == null)
                                    break;

                                item.RandomNo = String.Format("{0:0000}", ValidityAgent.RANDOM.Next(10000));
                                item.EncryptedContent = String.Concat(item.Interval.InvoiceTrackCodeAssignment.Track.TrackCode,
                                        String.Format("{0:00000000}", item.InvoiceNo),
                                        item.RandomNo).EncryptContent();
                                models.SubmitChanges();

                                items.Add(item);
                            }
                            mgr.Close();
                        }
                    }
                    catch(Exception ex)
                    {
                        CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                    }
                }
            }

            return items;
        }

        public static bool CheckAvailableInterval(this GenericDbContext<ApplicationDbContext> models, POSDeviceViewModel viewModel, out String reason)
        {
            reason = null;
            var seller = models.GetTable<Organization>().Where(c => c.ReceiptNo == viewModel.company_id).FirstOrDefault();
            bool auth = true;
            if (seller != null)
            {
                if (viewModel.Seed != null && viewModel.Authorization != null)
                {
                    auth = models.CheckAuthToken(seller, viewModel) != null;
                }

                if (auth)
                {
                    try
                    {
                        using (TrackNoManager mgr = new TrackNoManager(models, seller.CompanyID))
                        {
                            if (!mgr.PeekInvoiceNo().HasValue)
                            {
                                auth = false;
                                reason = "inovice no not available!";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                    }
                }
                else
                {
                    reason = "auth failed!";
                }
            }

            return auth;
        }

    }
}