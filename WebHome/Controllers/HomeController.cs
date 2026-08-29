using CommonLib.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement.Validator;
using ModelCore.Models.ViewModel;
using Newtonsoft.Json;
using System.Diagnostics;
using WebHome.Helper;
using WebHome.Models;

namespace WebHome.Controllers
{
    public class HomeController : SampleController<InvoiceItem>
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(IServiceProvider serviceProvider, ILogger<HomeController> logger) : base(serviceProvider)
        {
            _logger = logger;
        }

        // GET: Home
        public ActionResult MainPage()
        {
            return View();
        }

        public ActionResult SearchCompany(String term, bool? encrypt)
        {
            IQueryable<Organization> items = models.GetTable<Organization>();

            if (!String.IsNullOrEmpty(term))
            {
                items = items
                    .Where(f => f.ReceiptNo.StartsWith(term) || f.CompanyName.Contains(term));
            }
            else
            {
                items = items.Where(f => false);
            }

            return Content(items.OrderBy(o => o.ReceiptNo).ToArray()
                .Select(o => new
                {
                    label = $"{o.ReceiptNo} {o.CompanyName}",
                    value = encrypt == true ? o.CompanyID.EncryptKey() : o.CompanyID.ToString()
                }).JsonStringify(), "application/json");
        }

        public ActionResult SearchHeadquarter(String term, bool? encrypt)
        {
            IQueryable<Organization> items = models!
                .GetTable<Organization>()
                .Where(o => o.MasterOrganization != null);

            if (!String.IsNullOrEmpty(term))
            {
                items = items
                    .Where(f => f.ReceiptNo.StartsWith(term) || f.CompanyName.Contains(term));
            }
            else
            {
                items = items.Where(f => false);
            }

            return Content(items.OrderBy(o => o.ReceiptNo).ToArray()
                .Select(o => new
                {
                    label = $"{o.ReceiptNo} {o.CompanyName}",
                    value = encrypt == true ? o.CompanyID.EncryptKey() : o.CompanyID.ToString()
                }).JsonStringify(), "application/json");
        }

        [Authorize]
        public ActionResult GetCompany(String term)
        {
            IQueryable<Organization> items = models.GetTable<Organization>();

            if (!String.IsNullOrEmpty(term))
            {
                items = items
                    .Where(f => f.ReceiptNo.StartsWith(term) || f.CompanyName.Contains(term));
            }
            else
            {
                items = items.Where(f => false);
            }

            var item = items.FirstOrDefault();

            if (item != null)
            {
                return Content(JsonConvert.SerializeObject(item), "application/json");
            }
            else
            {
                return new EmptyResult();
            }
        }

        [Authorize]
        public ActionResult SearchCounterpart(EncQueryViewModel viewModel)
        {
            GetCounterpart(viewModel?.Term!);

            IQueryable<Organization> items = (IQueryable<Organization>)ViewBag.DataItems;

            if (viewModel?.SellerID.HasValue == true)
            {
                var dataItems = items
                    .OrderBy(o => o.ReceiptNo)
                    .Select(o => new { C = o, R = o.RelativeRelation.Where(b => b.MasterID == viewModel.SellerID).FirstOrDefault() })
                    .ToArray();
                return Content(dataItems
                    .Select(o => new
                    {
                        label = $"{o.C.ReceiptNo} {o.R?.CompanyName ?? o.C.CompanyName}",
                        value = o.C.CompanyID
                    }).JsonStringify(), "application/json");
            }
            else
            {
                return Content(items.OrderBy(o => o.ReceiptNo).ToArray()
                    .Select(o => new
                    {
                        label = $"{o.ReceiptNo} {o.CompanyName}",
                        value = o.CompanyID
                    }).JsonStringify(), "application/json");

            }
        }

        [Authorize]
        public ActionResult GetCounterpart(String term)
        {
            var profile = HttpContext.GetUser();

            IQueryable<Organization> items = models.GetTable<Organization>();

            if (!String.IsNullOrEmpty(term))
            {
                items = items
                    .Where(f => f.ReceiptNo.StartsWith(term) || f.CompanyName.Contains(term));
            }
            else
            {
                items = items.Where(f => false);
            }

            ViewBag.DataItems = items;

            if (profile.IsSystemAdmin())
            {
                var item = items.FirstOrDefault();

                if (item != null)
                {
                    return Content(item.JsonStringify(), "application/json");
                }
                else
                {
                    return new EmptyResult();
                }
            }
            else
            {
                var item = models.GetTable<BusinessRelationship>().Where(b => b.MasterID == profile.CurrentUserRole.OrganizationCategory.CompanyID)
                    .Join(items, b => b.RelativeID, o => o.CompanyID, (b, o) => b).FirstOrDefault();

                if (item != null)
                {
                    return Content((new { item.Counterpart.ReceiptNo, item.CompanyName, item.Addr, item.Phone, item.ContactEmail, item.CustomerNo }).JsonStringify(), "application/json");
                }
                else
                {
                    return new EmptyResult();
                }
            }

        }

        public ActionResult ReportError(ActionResultViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/Home/Module/ReportError.cshtml");
        }

        [Authorize]
        public ActionResult Download(QueryViewModel viewModel)
        {
            if (viewModel.KeyID != null)
            {
                String fileName = viewModel.KeyID.DecryptData();
                if (System.IO.File.Exists(fileName))
                {
                    return PhysicalFile(fileName, "application/octet-stream");
                }
            }

            ViewBag.CloseWindow = true;
            return View("~/Views/Shared/AlertMessage.cshtml", model: "檔案錯誤!!");
        }

        public ActionResult SystemInfo()
        {
            return Content((new
            {
                Version = "2023-02-13",
            }).JsonStringify(), "application/json");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature =
                 HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            //return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            return View(exceptionHandlerPathFeature?.Error);
        }

        //public ActionResult HandleUnknownAction(string actionName, IFormCollection formValues)
        //{
        //    ViewBag.FormValues = formValues;
        //    return View(actionName, formValues);
        //    //this.View(actionName).ExecuteResult(this.ControllerContext);
        //}
    }
}