using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using ModelCore.Locale;
using WebHome.Helper;
using ModelCore.Models.ViewModel;

using CommonLib.Utility;

namespace WebHome.Controllers
{
    public class DataFlowController : SampleController<InvoiceItem>
    {
        public DataFlowController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: DataFlow
        public ActionResult SellerSelector(SellerSelectorViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            if ((viewModel.FieldName = viewModel.FieldName.GetEfficientString()) == null)
            {
                viewModel.FieldName = "SellerID";
            }

            if (viewModel.SelectAll==true && (viewModel.SelectorIndication = viewModel.SelectorIndication.GetEfficientString()) == null)
            {
                viewModel.SelectorIndication = "全部";
            }

            var userProfile = HttpContext.GetUser();
            IQueryable<Organization> orgItems = userProfile.InitializeOrganizationQuery(models);

            return View("~/Views/DataFlow/SellerSelector.cshtml", orgItems);

        }
    }
}