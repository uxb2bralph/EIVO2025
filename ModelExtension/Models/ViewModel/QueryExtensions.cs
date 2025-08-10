using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using ModelCore.Models.ViewModel;
using ModelCore.DataEntity;
using ModelCore.Helper;

using ModelCore.Locale;
using ModelCore.Security.MembershipManagement;
using CommonLib.Utility;
using CommonLib.DataAccess;

namespace ModelCore.Models.ViewModel
{
    public static class QueryExtensions
    {
        public static IQueryable<InvoiceItem> InquireInvoice(this IQueryable<InvoiceItem> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models,ref bool effective, bool queryVoid = true)
        {
            items = items.InquireInvoiceIgnoreDate(viewModel, models, ref effective, queryVoid)
                        .QueryByInvoiceDate(viewModel, models, ref effective);

            return items;
        }

        private static IQueryable<InvoiceItem> InquireInvoiceIgnoreDate(this IQueryable<InvoiceItem> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective, bool queryVoid = true)
        {
            viewModel ??= new InquireInvoiceViewModel { };
            items = items.QueryByProcessType(viewModel, models, ref effective)
                        .QueryByInvoiceNo(viewModel, models, ref effective)
                        .QueryByDataNo(viewModel, models, ref effective)
                        .QueryByBuyerReceiptNo(viewModel, models, ref effective)
                        .QueryByBuyerName(viewModel, models, ref effective)
                        .QueryByCustomerID(viewModel, models, ref effective)
                        .QueryByWinning(viewModel, models, ref effective);

            if (queryVoid)
            {
                items = items.QueryEffective(viewModel, models, ref effective);
            }

            if (viewModel.SellerID.HasValue)
            {
                items = items.Where(i => i.SellerID == viewModel.SellerID);
            }

            if (viewModel.AgentID.HasValue)
            {
                items = models.GetInvoiceByAgent(items, viewModel.AgentID.Value);
            }

            IQueryable<InvoiceCarrier>? carrierItems = null;
            if (!String.IsNullOrEmpty(viewModel.CarrierType))
            {
                carrierItems = models.GetTable<InvoiceCarrier>().Where(c => c.CarrierType == viewModel.CarrierType);
            }

            viewModel.CarrierNo = viewModel.CarrierNo.GetEfficientString();
            if (viewModel.CarrierNo != null)
            {
                if (carrierItems == null)
                    carrierItems = models.GetTable<InvoiceCarrier>();
                carrierItems = carrierItems.Where(c => c.CarrierNo == viewModel.CarrierNo || c.CarrierNo2 == viewModel.CarrierNo);
            }

            if (carrierItems != null)
            {
                items = items.Join(carrierItems, i => i.InvoiceID, c => c.InvoiceID, (i, c) => i);
            }

            viewModel.PrintMark = viewModel.PrintMark.GetEfficientString();
            if (viewModel.PrintMark != null)
            {
                items = items.Where(i => i.PrintMark == viewModel.PrintMark);
            }

            items = items.QueryByAttachment(viewModel, models, ref effective);

            return items;
        }

        public static IQueryable<InvoiceItem> InquireVoidInvoice(this IQueryable<InvoiceItem> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };
            items = items.InquireInvoiceIgnoreDate(viewModel, models, ref effective, false);

            IQueryable<InvoiceCancellation> voidItems = models.GetTable<InvoiceCancellation>();
            if (viewModel.DateFrom.HasValue)
            {
                voidItems = voidItems.Where(i => i.CancelDate >= viewModel.DateFrom);
            }

            if (viewModel.DateTo.HasValue)
            {
                voidItems = voidItems.Where(i => i.CancelDate < viewModel.DateTo.Value.AddDays(1));
            }

            return items.Join(voidItems, i => i.InvoiceID, c => c.InvoiceID, (i, c) => i);
        }

        public static IQueryable<InvoiceAllowance> InquireVoidAllowance(this IQueryable<InvoiceAllowance> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models,ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };
            items = items.InquireAllowanceIgnoreDate(viewModel, models, ref effective, false);

            IQueryable<InvoiceAllowanceCancellation> voidItems = models.GetTable<InvoiceAllowanceCancellation>();
            if (viewModel.DateFrom.HasValue)
            {
                voidItems = voidItems.Where(i => i.CancelDate >= viewModel.DateFrom);
            }

            if (viewModel.DateTo.HasValue)
            {
                voidItems = voidItems.Where(i => i.CancelDate < viewModel.DateTo.Value.AddDays(1));
            }

            return items.Join(voidItems, i => i.AllowanceID, c => c.AllowanceID, (i, c) => i);
        }


        public static IQueryable<InvoiceItem> QueryByInvoiceNo(this IQueryable<InvoiceItem> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };

            viewModel.InvoiceNo = viewModel.InvoiceNo.GetEfficientString();
            viewModel.EndNo = viewModel.EndNo.GetEfficientString();
            String invoiceNo = viewModel.InvoiceNo ?? viewModel.EndNo;
            if (viewModel.InvoiceNo != null)
            {
                if (viewModel.EndNo != null && invoiceNo != viewModel.EndNo)
                {
                    if (String.Compare(invoiceNo, viewModel.EndNo) > 0)
                    {
                        String tmp = invoiceNo;
                        invoiceNo = viewModel.EndNo;
                        viewModel.EndNo = tmp;
                    }

                    if (invoiceNo.Length == 10)
                    {
                        String trackCode = invoiceNo.Substring(0, 2);
                        String no = invoiceNo.Substring(2);
                        items = items.Where(i => String.Compare(i.No, no) >= 0 && String.Compare(i.TrackCode, trackCode) >= 0);

                    }
                    else
                    {
                        items = items.Where(i => String.Compare(i.No, invoiceNo) >= 0);
                    }

                    invoiceNo = viewModel.EndNo;
                    if (invoiceNo.Length == 10)
                    {
                        String trackCode = invoiceNo.Substring(0, 2);
                        String no = invoiceNo.Substring(2);
                        items = items.Where(i => String.Compare(i.No, no) <= 0 && String.Compare(i.TrackCode, trackCode) <= 0);

                    }
                    else
                    {
                        items = items.Where(i => String.Compare(i.No, invoiceNo) <= 0);
                    }
                }
                else
                {
                    if (invoiceNo.Length == 10)
                    {
                        String trackCode = invoiceNo.Substring(0, 2);
                        String no = invoiceNo.Substring(2);
                        items = items.Where(i => i.No == no && i.TrackCode == trackCode);

                    }
                    else
                    {
                        items = items.Where(i => i.No == invoiceNo);
                    }
                }

                effective = true;
            }
            return items;
        }

        public static IQueryable<InvoiceItem> QueryByDataNo(this IQueryable<InvoiceItem> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };

            viewModel.DataNo = viewModel.DataNo.GetEfficientString();
            if (viewModel.DataNo != null)
            {
                var poItems = models.GetTable<InvoicePurchaseOrder>().Where(p => p.OrderNo.StartsWith(viewModel.DataNo));
                items = items.Join(poItems, i => i.InvoiceID, p => p.InvoiceID, (i, p) => i);
                effective = true;
            }
            return items;
        }


        public static IQueryable<InvoiceItem> QueryByProcessType(this IQueryable<InvoiceItem> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };

            //if (viewModel.ProcessType.HasValue)
            //{
            //    if (viewModel.ProcessType == Naming.InvoiceProcessType.C0401)
            //    {
            //        items = items
            //            .Join(models.GetTable<CDS_Document>()
            //                .Where(d => !d.ProcessType.HasValue
            //                    || (d.ProcessType != (int)Naming.InvoiceProcessType.A0401
            //                        && d.ProcessType != (int)Naming.InvoiceProcessType.A0401_Xlsx_Allocation_ByIssuer)),
            //                i => i.InvoiceID, d => d.DocID, (i, d) => i);
            //        effective = true;
            //    }
            //    else if (viewModel.ProcessType == Naming.InvoiceProcessType.A0401)
            //    {
            //        items = items
            //            .Join(models.GetTable<InvoiceBuyer>()
            //                .Where(d => d.ReceiptNo != "0000000000"),
            //                i => i.InvoiceID, d => d.InvoiceID, (i, d) => i);
            //        effective = true;
            //    }
            //}
            return items;
        }

        public static IQueryable<InvoiceItem> QueryByAttachment(this IQueryable<InvoiceItem> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };

            if (viewModel.Attachment.HasValue)
            {
                var attachment = models.GetTable<Attachment>();
                if (viewModel.Attachment == 1)
                {
                    items = items.Where(i => attachment.Any(a => a.DocID == i.InvoiceID));
                    effective = true;
                }
                else if (viewModel.Attachment == 0)
                {
                    items = items.Where(i => !attachment.Any(a => a.DocID == i.InvoiceID));
                    effective = true;
                }
            }
            return items;
        }

        public static IQueryable<InvoiceItem> QueryByWelfare(this IQueryable<InvoiceItem> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };

            viewModel.AgencyCode = viewModel.AgencyCode.GetEfficientString();
            if (viewModel.AgencyCode != null)
            {
                items = items.Where(i => i.InvoiceDonation.AgencyCode == viewModel.AgencyCode);
                effective = true;
            }

            return items;
        }

        public static IQueryable<InvoiceItem> QueryByBuyerReceiptNo(this IQueryable<InvoiceItem> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };

            viewModel.BuyerReceiptNo = viewModel.BuyerReceiptNo.GetEfficientString();
            if (viewModel.BuyerReceiptNo != null)
            {
                if (viewModel.BuyerReceiptNo.StartsWith("!"))
                {
                    var receiptNo = viewModel.BuyerReceiptNo.Substring(1);
                    if (!String.IsNullOrEmpty(receiptNo))
                    {
                        effective = true;
                        items = items.Where(d => d.InvoiceBuyer.ReceiptNo != receiptNo);
                    }
                }
                else
                {
                    effective = true;
                    items = items.Where(d => d.InvoiceBuyer.ReceiptNo == viewModel.BuyerReceiptNo);
                }
            }

            return items;
        }

        public static IQueryable<InvoiceItem> QueryByBuyerName(this IQueryable<InvoiceItem> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };

            viewModel.BuyerName = viewModel.BuyerName.GetEfficientString();
            if (viewModel.BuyerName != null)
            {
                effective = true;
                items = items.Join(models.GetTable<InvoiceBuyer>().Where(d => d.CustomerName.Contains(viewModel.BuyerName)),
                            i => i.InvoiceID, b => b.InvoiceID, (i, b) => i);
            }
            return items;
        }

        public static IQueryable<InvoiceItem> QueryByCustomerID(this IQueryable<InvoiceItem> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };

            viewModel.CustomerID = viewModel.CustomerID.GetEfficientString();
            if (viewModel.CustomerID != null)
            {
                effective = true;
                items = items.Join(models.GetTable<InvoiceBuyer>().Where(d => d.CustomerID == viewModel.CustomerID),
                            i => i.InvoiceID, b => b.InvoiceID, (i, b) => i);
            }
            return items;
        }

        public static IQueryable<InvoiceItem> QueryByInvoiceDate(this IQueryable<InvoiceItem> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };
            if (viewModel.InvoiceDateFrom.HasValue)
            {
                items = items.Where(i => i.InvoiceDate >= viewModel.InvoiceDateFrom);
                effective = true;
            }

            if (viewModel.InvoiceDateTo.HasValue)
            {
                items = items.Where(i => i.InvoiceDate < viewModel.InvoiceDateTo.Value.AddDays(1));
                effective = true;
            }
            return items;
        }

        public static IQueryable<InvoiceItem> QueryByWinning(this IQueryable<InvoiceItem> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };
            if (viewModel.Winning == 1)
            {
                items = items.Where(i => i.InvoiceWinningNumber != null && i.InvoiceDonation == null);
                effective = true;
            }
            return items;
        }

        public static IQueryable<InvoiceItem> QueryEffective(this IQueryable<InvoiceItem> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };
            if (viewModel.Cancelled.HasValue)
            {
                if (viewModel.Cancelled == true)
                {
                    items = items.Where(i => i.InvoiceCancellation != null);
                }
                else
                {
                    items = items.Where(i => i.InvoiceCancellation == null);
                }
                effective = true;
            }
            return items;
        }

        public static IQueryable<InvoiceAllowance> QueryEffective(this IQueryable<InvoiceAllowance> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };
            if (viewModel != null && viewModel.Cancelled.HasValue)
            {
                if (viewModel.Cancelled == true)
                {
                    items = items.Where(i => i.InvoiceAllowanceCancellation != null);
                }
                else
                {
                    items = items.Where(i => i.InvoiceAllowanceCancellation == null);
                }
                effective = true;
            }
            return items;
        }

        public static IQueryable<InvoiceAllowance> InquireAllowance(this IQueryable<InvoiceAllowance> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models,ref bool effective, bool queryVoid = false)
        {
            viewModel ??= new InquireInvoiceViewModel { };
            items = items.InquireAllowanceIgnoreDate(viewModel, models, ref effective, queryVoid)
                        .QueryByAllowanceDate(viewModel, models, ref effective);

            return items;
        }

        private static IQueryable<InvoiceAllowance> InquireAllowanceIgnoreDate(this IQueryable<InvoiceAllowance> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective, bool queryVoid = false)
        {
            viewModel = viewModel ?? new InquireInvoiceViewModel { };
            items = items.QueryByProcessType(viewModel, models, ref effective)
                        .QueryByDataNo(viewModel, models, ref effective)
                        .QueryByBuyerReceiptNo(viewModel, models, ref effective)
                        .QueryByBuyerName(viewModel, models, ref effective)
                        .QueryBySeller(viewModel, models, ref effective)
                        .QueryByCustomerID(viewModel, models, ref effective);

            if (queryVoid == false)
            {
                items = items.QueryEffective(viewModel, models, ref effective);
            }

            if (viewModel.AgentID.HasValue)
            {
                items = models.GetAllowanceByAgent(items, viewModel.AgentID.Value);
            }

            return items;
        }

        public static IQueryable<InvoiceAllowance> QueryByBuyerName(this IQueryable<InvoiceAllowance> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };
            viewModel.BuyerName = viewModel.BuyerName.GetEfficientString();
            if (viewModel.BuyerName != null)
            {
                effective = true;
                items = items.Where(d => d.InvoiceAllowanceBuyer.CustomerName.Contains(viewModel.BuyerName));
            }
            return items;
        }

        public static IQueryable<InvoiceAllowance> QueryBySeller(this IQueryable<InvoiceAllowance> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };
            if (viewModel != null && viewModel.CompanyID.HasValue)
            {
                effective = true;
                var sellerItems = models.GetTable<InvoiceAllowanceSeller>().Where(a => a.SellerID == viewModel.CompanyID);
                items = items.Where(d => sellerItems.Any(s => s.AllowanceID == d.AllowanceID));
            }

            return items;
        }

        public static IQueryable<InvoiceAllowance> QueryByCustomerID(this IQueryable<InvoiceAllowance> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };
            viewModel.CustomerID = viewModel.CustomerID.GetEfficientString();
            if (viewModel.CustomerID != null)
            {
                effective = true;
                items = items.Where(i => i.InvoiceAllowanceBuyer.CustomerID == viewModel.CustomerID);
            }

            return items;
        }
        public static IQueryable<InvoiceAllowance> QueryByBuyerReceiptNo(this IQueryable<InvoiceAllowance> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };
            viewModel.BuyerReceiptNo = viewModel.BuyerReceiptNo.GetEfficientString();
            if (viewModel.BuyerReceiptNo != null)
            {
                effective = true;
                items = items.Where(d => d.InvoiceAllowanceBuyer.ReceiptNo == viewModel.BuyerReceiptNo.GetEfficientString());
            }

            return items;
        }

        public static IQueryable<InvoiceAllowance> QueryByAllowanceDate(this IQueryable<InvoiceAllowance> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            if (viewModel != null)
            {
                if (viewModel.DateFrom.HasValue)
                {
                    items = items.Where(i => i.AllowanceDate >= viewModel.DateFrom);
                    effective = true;
                }

                if (viewModel.DateTo.HasValue)
                {
                    items = items.Where(i => i.AllowanceDate < viewModel.DateTo.Value.AddDays(1));
                    effective = true;
                }
            }
            return items;
        }


        public static IQueryable<InvoiceAllowance> QueryByDataNo(this IQueryable<InvoiceAllowance> items, InquireInvoiceViewModel viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel.DataNo = viewModel.DataNo.GetEfficientString();
            viewModel.EndNo = viewModel.EndNo.GetEfficientString();

            if (viewModel.DataNo != null && viewModel.EndNo != null)
            {
                if (String.Compare(viewModel.DataNo, viewModel.EndNo) > 0)
                {
                    String tmp = viewModel.DataNo;
                    viewModel.DataNo = viewModel.EndNo;
                    viewModel.EndNo = tmp;
                }

                items = items.Where(i => (i.AllowanceNumber.CompareTo(viewModel.DataNo) >= 0 && i.AllowanceNumber.CompareTo(viewModel.EndNo) <= 0)
                    || i.InvoiceAllowanceDetails.Any(d => d.InvoiceAllowanceItem.InvoiceNo.CompareTo(viewModel.DataNo) >= 0 && d.InvoiceAllowanceItem.InvoiceNo.CompareTo(viewModel.EndNo) <= 0));
                effective = true;
            }
            else
            {
                var dataNo = viewModel.DataNo ?? viewModel.EndNo;
                if (dataNo != null)
                {
                    items = items.Where(i => i.AllowanceNumber == dataNo
                                || i.InvoiceAllowanceDetails.Any(d => d.InvoiceAllowanceItem.InvoiceNo == dataNo));
                    effective = true;
                }
            }

            return items;
        }


        public static IQueryable<InvoiceAllowance> QueryByProcessType(this IQueryable<InvoiceAllowance> items, InquireInvoiceViewModel? viewModel, GenericManager<EIVOEntityDataContext> models, ref bool effective)
        {
            viewModel ??= new InquireInvoiceViewModel { };
            //if (viewModel.ProcessType.HasValue)
            //{
            //    if (viewModel.ProcessType == Naming.InvoiceProcessType.D0401)
            //    {
            //        items = items
            //            .Join(models.GetTable<CDS_Document>()
            //                .Where(d => !d.ProcessType.HasValue || d.ProcessType == (int)viewModel.ProcessType),
            //                i => i.AllowanceID, d => d.DocID, (i, d) => i);
            //        effective = true;
            //    }
            //    else
            //    {
            //        items = items
            //            .Join(models.GetTable<CDS_Document>()
            //                .Where(d => d.ProcessType == (int)viewModel.ProcessType),
            //                i => i.AllowanceID, d => d.DocID, (i, d) => i);
            //        effective = true;
            //    }
            //}
            return items;
        }


        public static IQueryable<InvoiceNoInterval> InquireInvoiceNoInterval(this InquireNoIntervalViewModel viewModel, GenericManager<EIVOEntityDataContext> models, UserProfile profile = null)
        {
            IQueryable<InvoiceNoInterval> items = models.GetTable<InvoiceNoInterval>();
            if (profile == null || profile.IsSystemAdmin())
            {
                if (viewModel.SellerID.HasValue)
                {
                    if (viewModel.BranchRelation == true)
                    {
                        items = items.Join(models.GetQueryByAgent(viewModel.SellerID.Value), 
                                    n => n.SellerID, i => i.CompanyID, (n, i) => n);
                    }
                    else
                    {
                        items = items.Where(t => t.SellerID == viewModel.SellerID);
                    }
                }
            }
            else
            {
                var issuerItems = profile.InitializeOrganizationQuery(models);
                if(viewModel.BranchRelation == true)
                {
                }
                else
                {
                    issuerItems = issuerItems.Where(o => o.CompanyID == viewModel.SellerID);
                }

                items = items.Join(issuerItems,
                    n => n.SellerID, o => o.CompanyID, (n, o) => n);
            }

            if (viewModel.Year.HasValue)
            {
                items = items.Where(i => i.InvoiceTrackCodeAssignment.InvoiceTrackCode.Year == viewModel.Year);
            }

            if (viewModel.PeriodNo.HasValue)
                items = items.Where(i => i.InvoiceTrackCodeAssignment.InvoiceTrackCode.PeriodNo == viewModel.PeriodNo);

            return items;

        }

        public static IQueryable<Organization> InitializeOrganizationQuery(this UserProfile userProfile, GenericManager<EIVOEntityDataContext> mgr)
        {
            switch ((Naming.CategoryID?)userProfile.CurrentUserRole?.OrganizationCategory.CategoryID)
            {
                case Naming.CategoryID.COMP_SYS:
                    return mgr.GetTable<Organization>().Where(
                        o => o.OrganizationCategory.Any(
                            c => c.CategoryID == (int)Naming.CategoryID.COMP_E_INVOICE_B2C_SELLER
                                || c.CategoryID == (int)Naming.CategoryID.COMP_VIRTUAL_CHANNEL
                                || c.CategoryID == (int)Naming.CategoryID.COMP_E_INVOICE_GOOGLE_TW
                                || c.CategoryID == (int)Naming.CategoryID.COMP_INVOICE_AGENT));

                case Naming.CategoryID.COMP_INVOICE_AGENT:
                case Naming.CategoryID.COMP_E_INVOICE_GOOGLE_TW:
                    return mgr.GetQueryByAgent(userProfile.CurrentUserRole.OrganizationCategory.CompanyID);

                case Naming.CategoryID.COMP_E_INVOICE_B2C_SELLER:
                case Naming.CategoryID.COMP_VIRTUAL_CHANNEL:
                case Naming.CategoryID.COMP_CROSS_BORDER_MURCHANT:
                    return mgr.GetTable<Organization>().Where(
                        o => o.CompanyID == userProfile.CurrentUserRole.OrganizationCategory.CompanyID);
                default:
                    break;
            }

            return mgr.GetTable<Organization>().Where(o => false);
        }

        public static IQueryable<InvoiceNoMainAssignment> InquireHeaquarterNoAssignment(this InquireNoIntervalViewModel viewModel, GenericManager<EIVOEntityDataContext> models)
        {
            IQueryable<InvoiceNoMainAssignment> items = models.GetTable<InvoiceNoMainAssignment>()
                .Where(m => m.MasterID == viewModel.SellerID);

            var trackCodeItems = models.GetTable<InvoiceTrackCode>()
                .Where(t => t.Year == viewModel.Year)
                .Where(t => t.PeriodNo == viewModel.PeriodNo);

            items = items.Join(trackCodeItems,
                m => m.TrackID, t => t.TrackID, (m, t) => m);

            return items;

        }

    }
}