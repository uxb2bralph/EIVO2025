using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ModelCore.DataEntity;
using ModelCore.Locale;
using WebHome.Helper;
using ModelCore.Models.ViewModel;
using CommonLib.Utility;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace WebHome.Components
{
    public class SellerSelectorViewComponent : ViewComponent
    {
        protected ModelSource<InvoiceItem> models;
        protected ModelStateDictionary _modelState;

        public IViewComponentResult Invoke(SellerSelectorViewModel viewModel)
        {
            if (viewModel == null)
            {
                viewModel = new SellerSelectorViewModel();
            }
            ViewBag.ViewModel = viewModel;

            viewModel.FieldName ??= "SellerID";

            if (viewModel.SelectAll == true && String.IsNullOrEmpty(viewModel.SelectorIndication))
            {
                viewModel.SelectorIndication = "全部";
            }

            ViewBag.ModelState = ViewContext.ModelState;

            var models = (ModelSource<InvoiceItem>)HttpContext.Items["Models"];
            var userProfile = HttpContext.GetUser();
            IQueryable<Organization> orgItems = userProfile.InitializeOrganizationQuery(models);

            return View("~/Views/DataFlow/SellerSelector.cshtml", orgItems);
        }

       

    }
}
