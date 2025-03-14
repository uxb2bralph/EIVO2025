using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using WebHome.Helper;
using WebHome.Models;


using System.Text;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using Microsoft.Net.Http.Headers;
using WebHome.Helper.Security.Authorization;

namespace WebHome.Controllers
{
    public class DonatedInvoiceController : SampleController<InvoiceItem>
    {
        public DonatedInvoiceController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected ModelSourceInquiry<InvoiceItem> createModelInquiry()
        {
            UserProfile userProfile = HttpContext.GetUser();


            return (ModelSourceInquiry<InvoiceItem>)(new InquireDonatedInvoice { ControllerName = "InquireInvoice", ActionName = "ByDonation" })
                .Append(new InquireInvoiceByRole(userProfile) { })
                .Append(new InquireInvoiceSeller { ControllerName = "InquireInvoice", ActionName = "BySeller" })
                .Append(new InquireInvoiceDate { ControllerName = "InquireInvoice", ActionName = "ByInvoiceDate" })
                .Append(new InquireDonatory { ControllerName = "InquireInvoice", ActionName = "ByDonatory" });
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS })]
        public ActionResult ReportIndex(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            DataSource.Inquiry = createModelInquiry();

            return View(DataSource.Inquiry);
        }

        public ActionResult InquireReport(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            DataSource.Inquiry = createModelInquiry();
            DataSource.BuildQuery();

            return View("ReportResult", DataSource.Inquiry);
        }

        public ActionResult ReportGridPage(InquireInvoiceViewModel viewModel,int index,int size)
        {
            //ViewBag.HasQuery = true;
            ViewBag.ViewModel = viewModel;
            DataSource.Inquiry = createModelInquiry();
            DataSource.BuildQuery();

            if (index > 0)
                index--;
            else
                index = 0;
            
            var items = DataSource.Items.OrderByDescending(d => d.InvoiceID)
                .Skip(index * size).Take(size);

            return View(items);
        }

        public ActionResult DownloadCSV(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            DataSource.Inquiry = createModelInquiry();
            DataSource.BuildQuery();

            var mediaType = new MediaTypeHeaderValue("application/octet-stream")
            {
                Encoding = Encoding.GetEncoding(950)
            };
            Response.ContentType = mediaType.ToString();

            return View(DataSource.Items);
        }

        public ActionResult PrintResult(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            DataSource.Inquiry = createModelInquiry();
            DataSource.BuildQuery();
            ((ModelSource<InvoiceItem>)models).ResultModel = Naming.DataResultMode.Print;

            return View(DataSource.Inquiry);
        }

    }
}
