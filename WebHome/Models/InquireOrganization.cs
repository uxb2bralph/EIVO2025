using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ModelCore.DataEntity;
using CommonLib.Utility;
using ModelCore.Models.ViewModel;
using ModelCore.Models;

namespace WebHome.Models
{
    public partial class InquireOrganizationReceiptNo : CommonInquiry<Organization, OrganizationQueryViewModel>
    {
        public override void BuildQueryExpression(ModelSource<Organization> models)
        {
            QueryViewModel.ReceiptNo = QueryViewModel.ReceiptNo.GetEfficientString();
            if (QueryViewModel.ReceiptNo!=null)
            {
                models.Items = models.Items.Where(i => i.ReceiptNo.StartsWith(QueryViewModel.ReceiptNo));
                this.HasSet = true;
            }

            base.BuildQueryExpression(models);
        }
    }

    public partial class InquireCompanyName : CommonInquiry<Organization, OrganizationQueryViewModel>
    {

        public override void BuildQueryExpression(ModelSource<Organization> models)
        {
            QueryViewModel.CompanyName = QueryViewModel.CompanyName.GetEfficientString();
            if (QueryViewModel.CompanyName!=null)
            {
                models.Items = models.Items.Where(i => i.CompanyName.Contains(QueryViewModel.CompanyName));
                this.HasSet = true;
            }

            base.BuildQueryExpression(models);
        }
    }

    public partial class InquireOrganizationStatus : CommonInquiry<Organization, OrganizationQueryViewModel>
    {
        public override void BuildQueryExpression(ModelSource<Organization> models)
        {
            if (QueryViewModel.OrganizationStatus.HasValue)
            {
                models.Items = models.Items.Where(i => i.OrganizationStatus.CurrentLevel == (int?)QueryViewModel.OrganizationStatus);
                this.HasSet = true;
            }

            base.BuildQueryExpression(models);
        }
    }

}