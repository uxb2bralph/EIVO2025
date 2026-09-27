using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CommonLib.Utility;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using Newtonsoft.Json;
using WebHome.Helper;
using ModelCore.Helper;

namespace WebHome.Controllers
{
    public class DataEntityController : SampleController<InvoiceItem>
    {
        public DataEntityController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: DataFlow
        public ActionResult Organization([FromJsonOrForm] DocumentQueryViewModel viewModel)
        {
            if (viewModel == null || !ModelState.IsValid)
            {
                return Json(new { result = false, message = ModelState.ErrorMessage() });
            }

            BusinessRelationship? relation = null;
            if (viewModel?.SellerID.HasValue == true)
            {
                relation = models!.GetTable<BusinessRelationship>()
                    .Where(r => r.MasterID == viewModel.SellerID!)
                    .Where(r => r.RelativeID == viewModel.id!).FirstOrDefault();
            }

            if (relation != null)
            {
                var orgItem = relation.Relative;
                return Content((new
                {
                    orgItem.ReceiptNo,
                    relation.CompanyName,
                    relation.ContactEmail,
                    relation.Addr,
                    relation.Phone,
                }).JsonStringify(), "application/json");
            }

            var item = models!.GetTable<Organization>().Where(o => o.CompanyID == viewModel!.id).FirstOrDefault();
            return Content(item.JsonStringifyShallow(), "application/json");
        }

        public ActionResult OrganizationExtension([FromJsonOrForm] int id)
        {
            var item = models!.GetTable<OrganizationExtension>().Where(o => o.CompanyID == id).FirstOrDefault();
            return Content(item.JsonStringifyShallow(), "application/json");
        }

    }
}