using System.Linq;
using ModelCore.DataEntity;
using WebHome.Helper;
using ModelCore.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using CommonLib.Utility;

namespace WebHome.Components
{
    public class CounterpartBusinessSelectorViewComponent : ViewComponent
    {
        protected ModelSource<InvoiceItem> models;
        protected ModelStateDictionary _modelState;

        public IViewComponentResult Invoke(CounterpartBusinessSelectorViewModel viewModel = null)
        {
            models = (ModelSource<InvoiceItem>)HttpContext.Items["Models"];
            _modelState = ViewContext.ModelState;

            if (viewModel == null)
            {
                viewModel = new CounterpartBusinessSelectorViewModel
                {
                    FieldName = ViewBag.FieldName ?? "BusinessID", // NEW: 允許用 ViewBag 傳入
                    FieldID = ViewBag.FieldID ?? "BusinessID",     // NEW: 允許用 ViewBag 傳入
                    SelectAll = false
                };
            }
            return CounterpartSelector(viewModel);
        }

        public IViewComponentResult CounterpartSelector(CounterpartBusinessSelectorViewModel viewModel)
        {
            var userProfile = HttpContext.GetUser();
            IQueryable<Organization> orgItems = userProfile.InitializeOrganizationQuery(models);

            ViewBag.ViewModel = viewModel;
            ViewBag.ModelState = _modelState;

            return View("~/Views/DataFlow/CounterpartBusinessSelector.cshtml", orgItems);
        }
    }
}
