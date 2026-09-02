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
using WebHome.Helper.Security.Authorization;
using ModelCore.Models;
using ModelCore.DataEntityWrapper;
using Microsoft.EntityFrameworkCore;

namespace WebHome.Controllers
{
    public class DonatedInvoiceController : SampleController<InvoiceItem>
    {
        public DonatedInvoiceController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        //每個 InquireXXX 都會直接取用 QueryViewModel，未指派就會在 BuildQueryExpression 時 NullReference。
        protected static ModelSourceInquiry<InvoiceItem> createModelInquiry(InquireInvoiceViewModel viewModel, UserProfileWrapper? userProfile)
        {
            return (ModelSourceInquiry<InvoiceItem>)(new InquireDonatedInvoice { ControllerName = "InquireInvoice", ActionName = "ByDonation", QueryViewModel = viewModel })
                .Append(new InquireInvoiceByRole(userProfile) { QueryViewModel = viewModel })
                .Append(new InquireInvoiceSeller { ControllerName = "InquireInvoice", ActionName = "BySeller", QueryViewModel = viewModel })
                .Append(new InquireInvoiceDate { ControllerName = "InquireInvoice", ActionName = "ByInvoiceDate", QueryViewModel = viewModel })
                .Append(new InquireDonatory { ControllerName = "InquireInvoice", ActionName = "ByDonatory", QueryViewModel = viewModel });
        }

        protected ModelSourceInquiry<InvoiceItem> createModelInquiry(InquireInvoiceViewModel viewModel)
        {
            return createModelInquiry(viewModel, HttpContext.GetUser());
        }

        /// <summary>
        /// 捐贈統計查詢，抽成不依賴 HttpContext 的靜態方法，讓背景作業（CreateXlsx.cshtml）
        /// 能以自己的 DbContext 重建同一份查詢。
        /// </summary>
        public static IQueryable<InvoiceItem> BuildDonatedInvoiceReport(
            CommonLib.Core.DataWork.GenericDbContext<ApplicationDbContext> models,
            InquireInvoiceViewModel viewModel,
            UserProfileWrapper? profile)
        {
            ModelSource<InvoiceItem> dataSource = new ModelSource<InvoiceItem>(models);
            dataSource.Inquiry = createModelInquiry(viewModel, profile);
            dataSource.BuildQuery();

            //每一列會存取 InvoiceDonation／InvoiceSeller／InvoiceWinningNumber，先一次載入避免 N+1；
            //Skip/Take 需要穩定排序。
            return dataSource.Items
                .Include(i => i.InvoiceDonation)
                .Include(i => i.InvoiceSeller)
                .Include(i => i.InvoiceWinningNumber)
                .OrderByDescending(i => i.InvoiceID);
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS })]
        public ActionResult ReportIndex(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            DataSource.Inquiry = createModelInquiry(viewModel);

            return View(DataSource.Inquiry);
        }

        public ActionResult InquireReport(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            IQueryable<InvoiceItem> items = BuildDonatedInvoiceReport(models!, viewModel, HttpContext.GetUser());

            viewModel.ResultView = "~/Views/DonatedInvoice/DataQuery/DonatedInvoiceReportList.cshtml";
            viewModel.ResultAction = "~/Views/DonatedInvoice/DataAction/QueryResultAction.cshtml";
            return PageResult(viewModel, items);
        }

        public ActionResult CreateXlsx(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            IQueryable<InvoiceItem> items = BuildDonatedInvoiceReport(models!, viewModel, HttpContext.GetUser());
            viewModel.RecordCount = items.Count();

            return View("~/Views/DonatedInvoice/Module/CreateXlsx.cshtml", items);
        }

    }
}
