using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using Microsoft.AspNetCore.Mvc;

using System.Xml;

using ClosedXML.Excel;
using WebHome.Helper;
using WebHome.Models;
using WebHome.Models.ViewModel;
using ModelCore.Models.ViewModel;
using WebHome.Properties;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;

using CommonLib.Utility;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebHome.Controllers
{
    public class InquireInvoiceController : SampleController<InvoiceItem>
    {
        public InquireInvoiceController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public ActionResult BySeller(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            var userProfile = HttpContext.GetUser();
            var orgItems = userProfile.InitializeOrganizationQuery(models);
            return View(orgItems);
        }

        public ActionResult ByBuyer(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View();
        }
        public ActionResult ByBuyerName(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View();
        }
        public ActionResult ByCustomerID(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View();
        }

        public ActionResult ByInvoiceDate(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (!viewModel.InvoiceDateFrom.HasValue)
            {
                ModelState.AddModelError("InvoiceDateFrom", "請輸入查詢起日");
            }

            if (!viewModel.InvoiceDateTo.HasValue)
            {
                ModelState.AddModelError("InvoiceDateTo", "請輸入查詢迄日");
            }

            if (viewModel.InvoiceDateFrom.HasValue && viewModel.InvoiceDateTo.HasValue)
            {
                if (viewModel.InvoiceDateFrom > viewModel.InvoiceDateTo)
                {
                    ModelState.AddModelError("InvoiceDateFrom", "查詢起日不可晚於查詢迄日");
                }
                else if (viewModel.InvoiceDateTo > viewModel.InvoiceDateFrom.Value.AddYears(2))
                {
                    ModelState.AddModelError("InvoiceDateFrom", "查詢區間最長不可超過2年");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            return View();
        }

        public ActionResult ByConsumption(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View();
        }

        public ActionResult ByPeriod(String dateFrom, String dateTo)
        {
            DateTime endDate;
            if (dateTo == null || !DateTime.TryParse(dateTo, out endDate))
            {
                endDate = DateTime.Today;
            }
            DateTime startDate;
            if (dateFrom == null || !DateTime.TryParse(dateFrom, out startDate))
            {
                //startDate = endDate.AddYears(-2);
                using (ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>())
                {
                    var item = models.EntityList.OrderBy(i => i.InvoiceID).FirstOrDefault();
                    startDate = item == null ? endDate.AddYears(-2) : item.InvoiceDate.Value;
                }
            }

            startDate = new DateTime(startDate.Year, (startDate.Month + 1) / 2 * 2 - 1, 1);
            endDate = new DateTime(endDate.Year, (endDate.Month + 1) / 2 * 2 - 1, 1);

            List<SelectListItem> items = null;

            if (endDate >= startDate)
            {
                items = new List<SelectListItem>();
                for (DateTime d = endDate; d >= startDate; d = d.AddMonths(-2))
                {
                    items.Add(new SelectListItem
                    {
                        Text = String.Format("{0:000}年 {1:00}月-{2:00}月", d.Year - 1911, d.Month, d.Month + 1),
                        Value = String.Format("{0},{1}", d.Year, (d.Month + 1) / 2)
                    });
                }
            }
            else
            {
                items = new List<SelectListItem>();
                for (DateTime d = startDate; d >= endDate; d = d.AddMonths(-2))
                {
                    items.Add(new SelectListItem
                    {
                        Text = String.Format("{0:000}年 {1:00}月-{2:00}月", d.Year - 1911, d.Month, d.Month + 1),
                        Value = String.Format("{0},{1}", d.Year, (d.Month + 1) / 2)
                    });
                }
            }

            return View(items);
        }

        public ActionResult ByDonation(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View();
        }

        public ActionResult ByDonatory(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View();
        }

        public ActionResult ByAttachment(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View();
        }

        public ActionResult ByAgent(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            var userProfile = HttpContext.GetUser();
            IQueryable<Organization> items = models.GetTable<Organization>()
                    .Where(o => models.GetTable<InvoiceIssuerAgent>().Any(a => a.AgentID == o.CompanyID));

            switch ((Naming.CategoryID)userProfile.CurrentUserRole.OrganizationCategory.CategoryID)
            {
                case Naming.CategoryID.COMP_SYS:
                    break;
                case Naming.CategoryID.COMP_INVOICE_AGENT:
                    items = items
                        .Where(a => a.CompanyID == userProfile.CurrentUserRole.OrganizationCategory.CompanyID);
                    break;

                default:
                    items = items.Where(f => false);
                    break;
            }
            return View(items);
        }



    }
}
