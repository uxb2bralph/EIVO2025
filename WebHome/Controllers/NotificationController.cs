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
using ModelCore.Helper;
using ModelCore.Security;
using CommonLib.Core.Utility;

namespace WebHome.Controllers
{
    public class NotificationController : SampleController<InvoiceItem>
    {
        public NotificationController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: Notification
        public ActionResult IssueAllowance([FromBody] DocumentQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (viewModel.KeyID != null)
            {
                viewModel.id = viewModel.DecryptKeyValue();
            }

            var item = models!.GetTable<InvoiceAllowance>().Where(a => a.AllowanceID == viewModel.id).FirstOrDefault();
            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "資料錯誤!!");
            }


            return View("~/Views/Notification/IssueAllowance.cshtml", item);
        }

        public ActionResult DataUploadExceptionList([FromBody] ExceptionLogQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            var items = models.GetTable<ExceptionLog>().Where(g => g.ExceptionReplication != null);

            if (viewModel.CompanyID.HasValue)
                items = items.Where(g => g.CompanyID == viewModel.CompanyID);
            if (viewModel.MaxLogID.HasValue)
                items = items.Where(g => g.LogID <= viewModel.MaxLogID);

            return View("~/Views/Notification/DataUploadExceptionList.cshtml", items);
        }

        public ActionResult IssueA0401([FromBody] DocumentQueryViewModel viewModel)
        {
            //Request.SaveAsAsync(Path.Combine(Logger.LogDailyPath,$"{DateTime.Now.Ticks}.txt"), includeHeader: true).Wait();
            ViewBag.ViewModel = viewModel;

            if (viewModel.KeyID != null)
            {
                viewModel.id = viewModel.DecryptKeyValue();
            }

            var item = models.GetTable<InvoiceItem>().Where(a => a.InvoiceID == viewModel.id).FirstOrDefault();
            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "資料錯誤!!");
            }

            return View("~/Views/Notification/IssueA0401.cshtml", item);
        }

        public ActionResult CommissionedToReceiveA0401([FromBody] DocumentQueryViewModel viewModel)
        {
            ViewResult result = (ViewResult)IssueA0401(viewModel);
            InvoiceItem item = result.Model as InvoiceItem;

            if (item == null)
                return result;

            return View("~/Views/Notification/CommissionedToReceiveA0401.cshtml", item);

        }

        public ActionResult NotifyToReceiveA0401([FromBody] DocumentQueryViewModel viewModel)
        {
            ViewResult result = (ViewResult)IssueA0401(viewModel);
            InvoiceItem item = result.Model as InvoiceItem;

            if (item == null)
                return result;

            return View("~/Views/Notification/NotifyToReceiveA0401.cshtml", item);

        }

        public ActionResult ActivateUser(UserProfileViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            if (viewModel.KeyID != null)
            {
                viewModel.UID = viewModel.DecryptKeyValue();
            }

            var item = models.GetTable<UserProfile>().Where(u => u.UID == viewModel.UID).FirstOrDefault();
            if (item == null)
            {
                item = models.GetTable<UserProfile>().Where(u => u.PID == viewModel.PID).FirstOrDefault();
            }

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "資料錯誤!!");
            }

            if (item.ResetUserPassword == null)
            {
                item.ResetUserPassword = new ResetUserPassword { };
            }
            item.ResetUserPassword.ResetID = Guid.NewGuid();

            if (viewModel.ResetPass == true)
            {
                item.UserProfileStatus.CurrentLevel = (int)Naming.MemberStatusDefinition.Wait_For_Check;
            }

            models.SubmitChanges();

            viewModel.ResetID = item.ResetUserPassword.ResetID;

            return View("~/Views/Notification/ActivateUser.cshtml", item);

        }

        public ActionResult IssueC0401([FromBody] DocumentQueryViewModel viewModel)
        {
            ViewResult result = (ViewResult)IssueA0401(viewModel);
            InvoiceItem item = result.Model as InvoiceItem;

            if (item == null)
                return result;

            return View("~/Views/Notification/IssueC0401.cshtml", item);
        }

        public ActionResult IssueC0701([FromBody] DocumentQueryViewModel viewModel)
        {
            ViewResult result = (ViewResult)IssueA0401(viewModel);
            InvoiceItem item = result.Model as InvoiceItem;

            if (item == null)
                return result;

            return View("~/Views/Notification/IssueC0701.cshtml", item);
        }


        public ActionResult IssueWinningInvoice([FromBody] DocumentQueryViewModel viewModel)
        {
            ViewResult result = (ViewResult)IssueA0401(viewModel);
            InvoiceItem item = result.Model as InvoiceItem;

            if (item == null)
                return result;

            return View("~/Views/Notification/IssueWinningInvoice.cshtml", item);
        }

        public ActionResult IssueC0501([FromBody] DocumentQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (viewModel.KeyID != null)
            {
                viewModel.id = viewModel.DecryptKeyValue();
            }

            var item = models.GetTable<DerivedDocument>().Where(d => d.DocID == viewModel.id || d.SourceID == viewModel.id)
                .Select(d => d.ParentDocument.InvoiceItem).FirstOrDefault();

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "資料錯誤!!");
            }

            return View("~/Views/Notification/IssueC0501.cshtml", item);
        }

        public ActionResult IssueAllowanceCancellation([FromBody] DocumentQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (viewModel.KeyID != null)
            {
                viewModel.id = viewModel.DecryptKeyValue();
            }

            var item = models!.GetTable<DerivedDocument>().Where(d => d.DocID == viewModel.id || d.SourceID == viewModel.id)
                .Select(d => d.ParentDocument.InvoiceAllowance).FirstOrDefault();

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "資料錯誤!!");
            }

            return View("~/Views/Notification/IssueAllowanceCancellation.cshtml", item);
        }

        public ActionResult IssueCustomMessage([FromBody] DocumentQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("IssueCustomMessage");
        }

        public ActionResult NotifyProcessException(ProcessRequestQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            var profile = HttpContext.GetUser();

            if (viewModel.KeyID != null)
            {
                viewModel.TaskID = viewModel.DecryptKeyValue();
            }

            ProcessRequest item = models.GetTable<ProcessRequest>().Where(r => r.TaskID == viewModel.TaskID).FirstOrDefault();
            if (item != null)
            {
                return View("~/Views/Notification/NotifyProcessException.cshtml", item);
            }

            return Content("Data not found!!");
        }

        public ActionResult SendDailyReport([FromBody] InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            Organization item = models.GetTable<Organization>().Where(c => c.CompanyID == viewModel.SellerID)
                .FirstOrDefault();

            if (item == null)
            {
                viewModel.ReceiptNo = viewModel.ReceiptNo.GetEfficientString();
                item = models.GetTable<Organization>().Where(c => c.ReceiptNo == viewModel.ReceiptNo)
                        .FirstOrDefault();
            }

            if (item == null)
            {
                ModelState.AddModelError("SellerID", "未指定營業人!!");
            }

            if (!ModelState.IsValid)
            {
                return Json(new { result = false, message = ModelState.ErrorMessage() });
            }

            if(!viewModel.DateFrom.HasValue)
            {
                viewModel.DateFrom = DateTime.Today.AddDays(-1);
            }

            if (!viewModel.DateTo.HasValue)
            {
                viewModel.DateTo = viewModel.DateFrom;
            }

            return View("~/Views/Notification/SendDailyReport.cshtml", item);

        }

        public ActionResult SendMessage([FromBody] MailMessageViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/Notification/SendMessage.cshtml");
        }

        public ActionResult CommitMessage(MailMessageViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (viewModel.KeyID != null)
            {
                viewModel.CompanyID = viewModel.DecryptKeyValue();
            }

            Organization item = models.GetTable<Organization>().Where(c => c.CompanyID == viewModel.CompanyID)
                .FirstOrDefault();

            if (item == null)
            {
                ModelState.AddModelError("CompanyID", "未指定營業人!!");
            }

            viewModel.Subject = viewModel.Subject.GetEfficientString();
            if (viewModel.Subject == null)
            {
                ModelState.AddModelError("Subject", "請輸入信件主旨!!");
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/Shared/ReportInputError.cshtml");
                //return Json(new { result = false, message = ModelState.ErrorMessage() });
            }

            return View("~/Views/Notification/IssueGeneralMessage.cshtml", item);

        }

        public ActionResult NotifyTwoFactorSettings([FromBody] UserProfileViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            if (viewModel.KeyID != null)
            {
                viewModel.UID = viewModel.DecryptKeyValue();
            }

            var item = models.GetTable<UserProfile>().Where(u => u.UID == viewModel.UID).FirstOrDefault();
            if (item == null)
            {
                item = models.GetTable<UserProfile>().Where(u => u.PID == viewModel.PID).FirstOrDefault();
            }

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "資料錯誤!!");
            }

            item = item.LoadInstance(models).PrepareTwoFactorKey(models);

            return View("~/Views/Notification/NotifyTwoFactorSettings.cshtml", item);

        }

        public ActionResult NotifySystemAnnouncement(String[] mailTo)
        {
            return View("~/Views/Notification/NotifySystemAnnouncement.cshtml", mailTo);
        }

        public ActionResult NotifyLowerInvoiceNoStock([FromBody] OrganizationViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (viewModel.KeyID != null)
            {
                viewModel.CompanyID = viewModel.DecryptKeyValue();
            }

            Organization item = models.GetTable<Organization>()
                    .Where(o => o.CompanyID == viewModel.CompanyID).FirstOrDefault();

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "資料錯誤!!");
            }

            return View("~/Views/Notification/NotifyLowerInvoiceNoStock.cshtml", item);
        }

    }

}