using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Html;
using WebHome.Helper;
using WebHome.Models.ViewModel;
using ModelCore.Models.ViewModel;

using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Security.MembershipManagement;
using CommonLib.Utility;
using CommonLib.DataAccess;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebHome.Models
{
    public partial class CommonInquiry<TEntity,TViewModel> : ModelSourceInquiry<TEntity>
        where TEntity : class, new()
        where TViewModel : class, new()
    {
        public CommonInquiry() : base()
        {

        }

        public TViewModel? QueryViewModel
        {
            get => _viewModel as TViewModel;
            set => _viewModel = value;
        }

    }


    public partial class InquireCustomerID : CommonInquiry<InvoiceItem, InquireInvoiceViewModel>
    {
        
        public override void BuildQueryExpression(ModelSource<InvoiceItem> models)
        {
            models.Items = models.Items.QueryByCustomerID(QueryViewModel, models, ref effective);
            base.BuildQueryExpression(models);
        }
    }

    public partial class InquireAllowanceCustomerID : CommonInquiry<InvoiceAllowance, InquireInvoiceViewModel>
    {

        public override void BuildQueryExpression(ModelSource<InvoiceAllowance> models)
        {
            QueryViewModel.CustomerID = QueryViewModel.CustomerID.GetEfficientString();
            if (QueryViewModel.CustomerID!=null)
            {
                effective = true;
                models.Items = models.Items.Where(i => i.InvoiceAllowanceBuyer.CustomerID == QueryViewModel.CustomerID);
            }

            base.BuildQueryExpression(models);
        }
    }


    public partial class InquireInvoiceSeller : CommonInquiry<InvoiceItem, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceItem> models)
        {
            if (QueryViewModel.SellerID.HasValue)
            {
                effective = true;
                models.Items = models.Items.Where(d => d.InvoiceSeller.SellerID == QueryViewModel.SellerID);
            }

            base.BuildQueryExpression(models);
        }
    }

    public partial class InquireAllowanceSeller : CommonInquiry<InvoiceAllowance, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceAllowance> models)
        {
            models.Items = models.Items.QueryBySeller(QueryViewModel, models, ref effective);
            base.BuildQueryExpression(models);
        }
    }

    public partial class InquireInvoiceBuyer : CommonInquiry<InvoiceItem, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceItem> models)
        {
            models.Items = models.Items.QueryByBuyerReceiptNo(QueryViewModel, models, ref effective);
            base.BuildQueryExpression(models);
        }
    }

    public partial class InquireAllowanceBuyer : CommonInquiry<InvoiceAllowance, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceAllowance> models)
        {
            models.Items = models.Items.QueryByBuyerReceiptNo(QueryViewModel, models, ref effective);
            base.BuildQueryExpression(models);
        }
    }

    public partial class InquireInvoiceBuyerByName : CommonInquiry<InvoiceItem, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceItem> models)
        {
            models.Items = models.Items.QueryByBuyerName(QueryViewModel, models, ref effective);
            base.BuildQueryExpression(models);
        }
    }

    public partial class InquireAllowanceBuyerByName : CommonInquiry<InvoiceAllowance, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceAllowance> models)
        {
            models.Items = models.Items.QueryByBuyerName(QueryViewModel, models, ref effective);
            base.BuildQueryExpression(models);
        }
    }

    public partial class InquireInvoiceDate : CommonInquiry<InvoiceItem, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceItem> models)
        {
            models.Items = models.Items.QueryByInvoiceDate(QueryViewModel, models, ref effective);
            base.BuildQueryExpression(models);
        }

    }

    public partial class InquireAllowanceDate : CommonInquiry<InvoiceAllowance, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceAllowance> models)
        {
            models.Items = models.Items.QueryByAllowanceDate(QueryViewModel, models, ref effective);
            base.BuildQueryExpression(models);
        }

    }

    public partial class InquireInvoiceConsumption : CommonInquiry<InvoiceItem, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceItem> models)
        {
            models.Items = models.Items.QueryByProcessType(QueryViewModel, models, ref effective);
            base.BuildQueryExpression(models);
        }

        //public bool QueryForB2C
        //{
        //    get
        //    {
        //        return CurrentController.Request["cc1"] == "B2C";
        //    }
        //}
    }

    public partial class InquireAllowanceConsumption : CommonInquiry<InvoiceAllowance, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceAllowance> models)
        {
            models.Items = models.Items.QueryByProcessType(QueryViewModel, models, ref effective);
            base.BuildQueryExpression(models);
        }

        //public bool QueryForB2C
        //{
        //    get
        //    {
        //        return CurrentController.Request["cc1"] == "B2C";
        //    }
        //}
    }

    public partial class InquireInvoiceConsumptionExtensionToPrint : CommonInquiry<InvoiceItem, InquireInvoiceViewModel>
    {
        //private InquireInvoiceConsumption _inquiry;

        //public InquireInvoiceConsumptionExtensionToPrint(InquireInvoiceConsumption inquiry)
        //{
        //    _inquiry = inquiry;
        //}

        public override void BuildQueryExpression(ModelSource< InvoiceItem> models)
        {
            models.Items = models.Items.Where(i => i.PrintMark == "Y" || (i.PrintMark == "N" && i.InvoiceWinningNumber != null)
                /*&& (!i.CDS_Document.DocumentPrintLog.Any(l => l.TypeID == (int)Naming.DocumentTypeDefinition.E_Invoice) || i.CDS_Document.DocumentAuthorization != null)*/);
        }
    }

    public partial class InquireInvoiceByRole : CommonInquiry<InvoiceItem, InquireInvoiceViewModel>
    {
        protected UserProfile _userProfile;

        public InquireInvoiceByRole(UserProfile profile)
        {
            _userProfile = profile;
        }

        public override void BuildQueryExpression(ModelSource< InvoiceItem> models)
        {
            models.Items = models.FilterInvoiceByRole(_userProfile, models.Items);
            base.BuildQueryExpression(models);
        }
    }

    public partial class InquireAllowanceByRole : CommonInquiry<InvoiceAllowance, InquireInvoiceViewModel>
    {
        protected UserProfile _userProfile;

        public InquireAllowanceByRole(UserProfile profile)
        {
            _userProfile = profile;
        }

        public override void BuildQueryExpression(ModelSource< InvoiceAllowance> models)
        {
            switch ((Naming.RoleID)_userProfile.CurrentUserRole.RoleID)
            {
                case Naming.RoleID.ROLE_GUEST:
                case Naming.RoleID.ROLE_BUYER:
                case Naming.RoleID.ROLE_SYS:
                    break;

                default:
                    models.Items = models.GetAllowanceByAgent(models.Items, _userProfile.CurrentUserRole.OrganizationCategory.CompanyID);
                    break;
            }


            base.BuildQueryExpression(models);
        }
    }

    public partial class InquireEffectiveInvoice : CommonInquiry<InvoiceItem, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceItem> models)
        {
            models.Items = models.Items.QueryEffective(QueryViewModel, models, ref effective);
            base.BuildQueryExpression(models);
        }
    }

    public partial class InquireEffectiveAllowance : CommonInquiry<InvoiceAllowance, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceAllowance> models)
        {
            models.Items = models.Items.QueryEffective(QueryViewModel, models, ref effective);
            base.BuildQueryExpression(models);
        }

    }

    public partial class InquireWinningInvoice : CommonInquiry<InvoiceItem, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceItem> models)
        {
            models.Items = models.Items.QueryByWinning(QueryViewModel, models, ref effective);
            base.BuildQueryExpression(models);
        }
    }

    public partial class InquireInvoicePeriod : CommonInquiry<InvoiceItem, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource<InvoiceItem> models)
        {
            QueryViewModel.Period = QueryViewModel.Period.GetEfficientString();
            if (QueryViewModel.Period != null)
            {
                String[] period = QueryViewModel.Period.Split(',');
                if (period != null && period.Length == 2)
                {
                    int year, p;
                    if (int.TryParse(period[0], out year) && int.TryParse(period[1], out p) && p >= 1 && p <= 6)
                    {
                        DateTime dateFrom = new DateTime(year, p * 2 - 1, 1);
                        models.Items = models.Items.Where(i => i.InvoiceDate >= dateFrom && i.InvoiceDate < dateFrom.AddMonths(2));
                        effective = true;
                        QueryMessage = (dateFrom.Year - 1911) + "年 " + dateFrom.Month + "~" + (dateFrom.Month + 1) + "月";
                    }
                }
            }

            base.BuildQueryExpression(models);
        }

    }

    public partial class InquireDonatedInvoice : CommonInquiry<InvoiceItem, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceItem> models)
        {
            models.Items = models.Items.Where(i => i.InvoiceCancellation == null
                && i.InvoiceDonation != null);

            QueryViewModel.Donation = QueryViewModel.Donation.GetEfficientString();
            if (QueryViewModel.Donation == "winning")
            {
                models.Items = models.Items.Where(i => i.InvoiceWinningNumber != null);
                effective = true;
            }

            base.BuildQueryExpression(models);
        }
    }

    public partial class InquireDonatory : CommonInquiry<InvoiceItem, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceItem> models)
        {
            models.Items = models.Items.QueryByWelfare(QueryViewModel, models, ref effective);
            base.BuildQueryExpression(models);
        }
    }

    public partial class InquireInvoiceAttachment : CommonInquiry<InvoiceItem, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceItem> models)
        {
            models.Items = models.Items.QueryByAttachment(QueryViewModel, models, ref effective);
            base.BuildQueryExpression(models);
        }
    }


    public partial class InquireInvoiceNo : CommonInquiry<InvoiceItem, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceItem> models)
        {
            models.Items = models.Items.QueryByInvoiceNo(QueryViewModel, models, ref effective);
            base.BuildQueryExpression(models);
        }

    }

    public partial class InquireAllowanceNo : CommonInquiry<InvoiceAllowance, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceAllowance> models)
        {
            models.Items = models.Items.QueryByDataNo(QueryViewModel, models, ref effective);
            base.BuildQueryExpression(models);
        }

    }

    public partial class InquireInvoiceAgent : CommonInquiry<InvoiceItem, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceItem> models)
        {
            if (QueryViewModel.AgentID.HasValue)
            {
                effective = true;
                models.Items = models.GetInvoiceByAgent(models.Items, QueryViewModel.AgentID.Value);
            }

            base.BuildQueryExpression(models);
        }
    }

    public partial class InquireAllowanceAgent : CommonInquiry<InvoiceAllowance, InquireInvoiceViewModel>
    {
        public override void BuildQueryExpression(ModelSource< InvoiceAllowance> models)
        {
            if (QueryViewModel!=null && QueryViewModel.AgentID.HasValue)
            {
                effective = true;
                models.Items = models.GetAllowanceByAgent(models.Items, QueryViewModel.AgentID.Value);
            }

            base.BuildQueryExpression(models);
        }
    }



}