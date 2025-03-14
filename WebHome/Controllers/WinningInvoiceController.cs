using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Microsoft.AspNetCore.Mvc;



using ClosedXML.Excel;
using WebHome.Helper;
using WebHome.Models;
using WebHome.Models.ViewModel;
using ModelCore.Models.ViewModel;
using WebHome.Properties;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Schema.TurnKey.E0402;

using CommonLib.Utility;
using Microsoft.Net.Http.Headers;
using WebHome.Helper.Security.Authorization;

namespace WebHome.Controllers
{
    public class WinningInvoiceController : SampleController<InvoiceItem>
    {
        public WinningInvoiceController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected ModelSourceInquiry<InvoiceItem> createModelInquiry()
        {
            UserProfile userProfile = HttpContext.GetUser();

            return (ModelSourceInquiry<InvoiceItem>)(new InquireEffectiveInvoice { })
                .Append(new InquireWinningInvoice { })
                .Append(new InquireInvoiceByRole(userProfile) { })
                .Append(new InquireInvoiceSeller { ControllerName = "InquireInvoice", ActionName = "BySeller" })
                .Append(new InquireInvoicePeriod { ControllerName = "InquireInvoice", ActionName = "ByPeriod", QueryRequired = true, AlertMessage = "請選擇期別!!" });
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS })]
        public ActionResult ReportIndex()
        {
            //ViewBag.HasQuery = false;
            //ViewBag.RequiredError = false;
            DataSource.Inquiry = createModelInquiry();

            return View(DataSource.Inquiry);
        }

        public ActionResult InquireReport(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            //ViewBag.HasQuery = true;
            DataSource.Inquiry = createModelInquiry();
            DataSource.BuildQuery();
            DataSource.Items = DataSource.Items.Where(i => i.InvoiceWinningNumber != null);

            return View("ReportResult",DataSource.Inquiry);
        }

        //public ActionResult GridPage(int index,int size)
        //{
        //    //ViewBag.HasQuery = true;
        //    ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>();
        //    TempData.SetModelSource(models);
        //    DataSource.Inquiry = createModelInquiry();
        //    DataSource.BuildQuery();

        //    if (index > 0)
        //        index--;
        //    else
        //        index = 0;

        //    return View(DataSource.Items.OrderByDescending(d => d.InvoiceID)
        //        .Skip(index * size).Take(size)
        //        .ToArray());
        //}

        public ActionResult ReportGridPage(int index, int size, InquireInvoiceViewModel viewModel)
        {
            //ViewBag.HasQuery = true;
            ViewBag.ViewModel = viewModel;
            DataSource.Inquiry = createModelInquiry();
            DataSource.BuildQuery();
            DataSource.Items = DataSource.Items.Where(i => i.InvoiceWinningNumber != null);

            if (index > 0)
                index--;
            else
                index = 0;

            return View(DataSource.Items
                .GroupBy(i => i.SellerID)
                .OrderBy(g => g.Key)
                .Skip(index * size).Take(size)
                .Join(models.GetTable<Organization>(),
                    g => g.Key, o => o.CompanyID, (g, o) =>
                        new WinningInvoiceReportItem
                        {
                            Addr = o.Addr,
                            SellerName = o.CompanyName,
                            SellerReceiptNo = o.ReceiptNo,
                            WinningCount = g.Count(),
                            DonationCount = g.Where(i => i.InvoiceDonation != null).Count()
                        }
                ).ToArray());
        }


        public ActionResult DownloadCSV()
        {
            DataSource.Inquiry = createModelInquiry();
            DataSource.BuildQuery();
            DataSource.Items = DataSource.Items.Where(i => i.InvoiceWinningNumber != null);

            var mediaType = new MediaTypeHeaderValue("application/octet-stream")
            {
                Encoding = Encoding.GetEncoding(950)
            };
            Response.ContentType = mediaType.ToString();

            return View(DataSource.Items
                .GroupBy(i => i.SellerID)
                .OrderBy(g => g.Key)
                .Join(models.GetTable<Organization>(),
                    g => g.Key, o => o.CompanyID, (g, o) =>
                        new WinningInvoiceReportItem
                        {
                            Addr = o.Addr,
                            SellerName = o.CompanyName,
                            SellerReceiptNo = o.ReceiptNo,
                            WinningCount = g.Count(),
                            DonationCount = g.Where(i => i.InvoiceDonation != null).Count()
                        }
                ).ToArray());
        }

        public ActionResult PrintResult()
        {

            DataSource.Inquiry = createModelInquiry();
            DataSource.ResultModel = Naming.DataResultMode.Print;
            DataSource.BuildQuery();
            DataSource.Items = DataSource.Items.Where(i => i.InvoiceWinningNumber != null);

            return View(DataSource.Inquiry);
        }

    }
}
