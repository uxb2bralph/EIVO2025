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
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebHome.Components
{
    public class InquireInvoiceViewComponent : ViewComponent
    {
        protected ModelSource<InvoiceItem> models;
        protected ModelStateDictionary _modelState;

        public virtual IViewComponentResult Invoke(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            models = (ModelSource<InvoiceItem>)HttpContext.Items["Models"];
            _modelState = ViewContext.ModelState;

            return View(viewModel);
        }

        public IViewComponentResult BySeller(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            var userProfile = HttpContext.GetUser();
            var orgItems = userProfile.InitializeOrganizationQuery(models);
            return View("~/Views/InquireInvoice/BySeller.cshtml", orgItems);
        }

        public IViewComponentResult ByBuyer(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/InquireInvoice/ByBuyer.cshtml");
        }
        public IViewComponentResult ByBuyerName(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/InquireInvoice/ByBuyerName.cshtml");
        }
        public IViewComponentResult ByCustomerID(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/InquireInvoice/ByCustomerID.cshtml");
        }

        public IViewComponentResult ByInvoiceDate(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/InquireInvoice/ByInvoiceDate.cshtml");
        }

        public IViewComponentResult ByConsumption(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/InquireInvoice/ByConsumption.cshtml");
        }

        public IViewComponentResult ByPeriod(String dateFrom, String dateTo)
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

            List<SelectListItem>? items = null;

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

            return View("~/Views/InquireInvoice/ByPeriod.cshtml", items);
        }

        public IViewComponentResult ByDonation(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/InquireInvoice/ByDonation.cshtml");
        }

        public IViewComponentResult ByDonatory(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/InquireInvoice/ByDonatory.cshtml");
        }

        public IViewComponentResult ByAttachment(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/InquireInvoice/ByAttachment.cshtml");
        }

        public IViewComponentResult ByAgent(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            var userProfile = HttpContext.GetUser();
            IQueryable<Organization> items = models.GetTable<Organization>()
                    .Where(o => models.GetTable<InvoiceIssuerAgent>().Any(a => a.AgentID == o.CompanyID));

            switch ((Naming.CategoryID?)userProfile.CurrentUserRole?.OrganizationCategory.CategoryID)
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

            return View("~/Views/InquireInvoice/ByAgent.cshtml", items);
        }

    }

    public class InquireInvoiceConsumptionViewComponent : InquireInvoiceViewComponent
    {
        public override IViewComponentResult Invoke(InquireInvoiceViewModel viewModel)
        {
            base.Invoke(viewModel);
            return ByConsumption(viewModel);
        }
    }

    public class InquireInvoiceSellerViewComponent : InquireInvoiceViewComponent
    {
        public override IViewComponentResult Invoke(InquireInvoiceViewModel viewModel)
        {
            base.Invoke(viewModel);
            return BySeller(viewModel);
        }
    }

    public class InquireInvoiceBuyerViewComponent : InquireInvoiceViewComponent
    {
        public override IViewComponentResult Invoke(InquireInvoiceViewModel viewModel)
        {
            base.Invoke(viewModel);
            return ByBuyer(viewModel);
        }
    }

    public class InquireInvoiceBuyerByNameViewComponent : InquireInvoiceViewComponent
    {
        public override IViewComponentResult Invoke(InquireInvoiceViewModel viewModel)
        {
            base.Invoke(viewModel);
            return ByBuyerName(viewModel);
        }
    }

    public class InquireCustomerIDViewComponent : InquireInvoiceViewComponent
    {
        public override IViewComponentResult Invoke(InquireInvoiceViewModel viewModel)
        {
            base.Invoke(viewModel);
            return ByCustomerID(viewModel);
        }
    }

    public class InquireInvoiceDateViewComponent : InquireInvoiceViewComponent
    {
        public override IViewComponentResult Invoke(InquireInvoiceViewModel viewModel)
        {
            base.Invoke(viewModel);
            return ByInvoiceDate(viewModel);
        }
    }

    public class InquireInvoiceAttachmentViewComponent : InquireInvoiceViewComponent
    {
        public override IViewComponentResult Invoke(InquireInvoiceViewModel viewModel)
        {
            base.Invoke(viewModel);
            return ByAttachment(viewModel);
        }
    }

    public class InquireInvoiceAgentViewComponent : InquireInvoiceViewComponent
    {
        public override IViewComponentResult Invoke(InquireInvoiceViewModel viewModel)
        {
            base.Invoke(viewModel);
            return ByAgent(viewModel);
        }
    }
}
