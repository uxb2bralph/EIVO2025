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
            ViewBag.ViewModel = viewModel;
            models = (ModelSource<InvoiceItem>)HttpContext.Items["Models"];
            _modelState = ViewContext.ModelState;

            return SellerSelector(viewModel);
        }

        public IViewComponentResult SellerSelector(SellerSelectorViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            if ((viewModel.FieldName = viewModel.FieldName.GetEfficientString()) == null)
            {
                viewModel.FieldName = "SellerID";
            }

            if (viewModel.SelectAll == true && (viewModel.SelectorIndication = viewModel.SelectorIndication.GetEfficientString()) == null)
            {
                viewModel.SelectorIndication = "全部";
            }

            var userProfile = HttpContext.GetUser();
            IQueryable<Organization> orgItems = userProfile.InitializeOrganizationQuery(models);

            return View("~/Views/DataFlow/SellerSelector.cshtml", orgItems);

        }

    }
}
