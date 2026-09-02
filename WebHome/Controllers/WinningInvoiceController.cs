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
using ModelCore.Models;
using ModelCore.Helper;
using ModelCore.DataEntityWrapper;

namespace WebHome.Controllers
{
    public class WinningInvoiceController : SampleController<InvoiceItem>
    {
        public WinningInvoiceController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected ModelSourceInquiry<InvoiceItem> createModelInquiry()
        {
            UserProfileWrapper userProfile = HttpContext.GetUser();

            return (ModelSourceInquiry<InvoiceItem>)(new InquireEffectiveInvoice { })
                .Append(new InquireWinningInvoice { })
                .Append(new InquireInvoiceByRole(userProfile) { })
                .Append(new InquireInvoiceSeller { ControllerName = "InquireInvoice", ActionName = "BySeller" })
                .Append(new InquireInvoicePeriod { ControllerName = "InquireInvoice", ActionName = "ByPeriod", QueryRequired = true, AlertMessage = "請選擇期別!!" });
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS })]
        public ActionResult ReportIndex(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            //ViewBag.HasQuery = false;
            //ViewBag.RequiredError = false;
            var profile = HttpContext.GetUser();
            DataSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);

            return View(DataSource.Inquiry);
        }

        public ActionResult InquireReport(InquireInvoiceViewModel viewModel)
        {
            IQueryable<WinningInvoiceReportItem> items = InquireWinningInvoice(viewModel);

            viewModel.ResultView = "~/Views/WinningInvoice/DataQuery/WinningInvoiceReportList.cshtml";
            viewModel.ResultAction = "~/Views/BusinessRelationship/DataAction/QueryResultAction.cshtml";
            return PageResult(viewModel, items);


            //return View("~/Views/WinningInvoice/ReportResult.cshtml", items);
        }

        /// <summary>
        /// 中獎統計查詢，抽成不依賴 HttpContext 的靜態方法，讓背景作業（CreateXlsx.cshtml）
        /// 能以自己的 DbContext 重建同一份查詢。
        /// </summary>
        public static IQueryable<WinningInvoiceReportItem> BuildWinningInvoiceReport(
            CommonLib.Core.DataWork.GenericDbContext<ApplicationDbContext> models,
            InquireInvoiceViewModel viewModel,
            UserProfileWrapper? profile)
        {
            ModelSource<InvoiceItem> dataSource = new ModelSource<InvoiceItem>(models);
            dataSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);
            dataSource.BuildQuery();
            dataSource.Items = dataSource.Items.Where(i => i.InvoiceWinningNumber != null);

            //先把分組的彙總值投影出來再 Join；直接把 IGrouping 帶進 Join 的 result selector
            //會讓 EF Core 無法翻譯（GroupBy 之後群組元素已不存在於 SQL 結果中）。
            var summary = dataSource.Items
                .GroupBy(i => i.SellerID)
                .Select(g => new
                {
                    SellerID = g.Key,
                    WinningCount = g.Count(),
                    DonationCount = g.Count(i => i.InvoiceDonation != null),
                });

            return summary
                .Join(models.GetTable<Organization>(),
                    s => s.SellerID, o => (int?)o.CompanyID, (s, o) =>
                        new
                        {
                            o.CompanyID,
                            Item = new WinningInvoiceReportItem
                            {
                                Addr = o.Addr,
                                SellerName = o.CompanyName,
                                SellerReceiptNo = o.ReceiptNo,
                                WinningCount = s.WinningCount,
                                DonationCount = s.DonationCount,
                            }
                        })
                .OrderBy(r => r.CompanyID)
                .Select(r => r.Item);
        }

        private IQueryable<WinningInvoiceReportItem> InquireWinningInvoice(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            //ViewBag.HasQuery = true;
            var profile = HttpContext.GetUser();
            var items = BuildWinningInvoiceReport(models!, viewModel, profile);
            return items;
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
            var profile = HttpContext.GetUser();
            DataSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);
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
                .Join(models!.GetTable<Organization>(),
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


        public ActionResult CreateXlsx(InquireInvoiceViewModel viewModel)
        {
            //原本以 _dbInstance = false 阻止請求結束時釋放 DbContext，
            //供 CreateXlsx.cshtml 的 Task.Run 續用；該報表已改為背景作業自建 DbContext，
            //這裡恢復正常釋放。
            IQueryable<WinningInvoiceReportItem> items = InquireWinningInvoice(viewModel);
            viewModel.RecordCount = items.Count();

            return View("~/Views/WinningInvoice/Module/CreateXlsx.cshtml", items);
        }

        public ActionResult PrintResult(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            //ViewBag.HasQuery = true;
            var profile = HttpContext.GetUser();
            DataSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);
            DataSource.ResultModel = Naming.DataResultMode.Print;
            DataSource.BuildQuery();
            DataSource.Items = DataSource.Items.Where(i => i.InvoiceWinningNumber != null);

            return View(DataSource.Inquiry);
        }

    }
}
