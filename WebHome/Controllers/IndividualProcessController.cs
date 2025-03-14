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

using ModelCore.InvoiceManagement.Validator;
using ModelCore.Helper;
using CommonLib.Utility;
using CommonLib.DataAccess;
using CommonLib.Core.Utility;
using Newtonsoft.Json.Linq;
using ModelCore.Security.MembershipManagement;
using System.Security.Cryptography;

namespace WebHome.Controllers
{
    public class IndividualProcessController : SampleController<InvoiceItem>
    {
        public IndividualProcessController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public ActionResult Index(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            return View("~/Views/IndividualProcess/Index.cshtml");

        }

        public ActionResult Inquire(InquireInvoiceViewModel viewModel,String ValidCode,String EncryptedCode)
        {
            ViewBag.ViewModel = viewModel;

            ValidCode = ValidCode.GetEfficientString();
            EncryptedCode = EncryptedCode.GetEfficientString();
            if (ValidCode == null)
            {
                ModelState.AddModelError("ValidCode", "請輸入驗證碼!!");
            }
            else if (EncryptedCode == null
                || String.Compare(ValidCode, Encoding.Default.GetString(AppResource.Instance.Decrypt(Convert.FromBase64String(EncryptedCode))), true) != 0)
            {
                ModelState.AddModelError("ValidCode", "驗證碼錯誤!!");
            }

            viewModel.InvoiceNo = viewModel.InvoiceNo.GetEfficientString();
            var match = viewModel.InvoiceNo.ParseInvoiceNo();

            viewModel.CarrierNo = viewModel.CarrierNo.GetEfficientString();

            if (viewModel.CarrierNo == null)
            {
                if (!match.Success)
                {
                    ModelState.AddModelError("InvoiceNo", "請輸入發票號碼!!");
                    ModelState.AddModelError("CarrierNo", "請輸入載具號碼!!");
                }

            }

            if (!viewModel.InvoiceDate.HasValue)
            {
                ModelState.AddModelError("InvoiceDate", "請輸入發票開立日期!!");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ModelState = ModelState;
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            IQueryable<InvoiceItem> items = models.GetTable<InvoiceItem>();

            if (match.Success)
            {
                items = items
                    .Where(i => i.TrackCode == match.Groups[1].Value && i.No == match.Groups[2].Value)
                    .Where(i => i.InvoiceDate >= viewModel.InvoiceDate && i.InvoiceDate < viewModel.InvoiceDate.Value.AddDays(1));
            }

            if (viewModel.CarrierNo != null)
            {
                items = items.Join(models.GetTable<InvoiceCarrier>()
                                    .Where(c => c.CarrierNo == viewModel.CarrierNo),
                                i => i.InvoiceID, c => c.InvoiceID, (i, c) => i);
            }

            viewModel.BuyerName = viewModel.BuyerName.GetEfficientString();
            if (viewModel.BuyerName != null)
            {
                items = items.Where(i => i.InvoiceBuyer.Name == viewModel.BuyerName);
            }

            viewModel.RandomNo = viewModel.RandomNo.GetEfficientString();
            if (viewModel.RandomNo != null)
            {
                items = items.Where(i => i.RandomNo == viewModel.RandomNo);
            }

            var item = items.FirstOrDefault();
            return View("~/Views/IndividualProcess/DataQuery/InvoiceQueryResult.cshtml", item);
        }

        public ActionResult GetInvoicePDF(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            viewModel.InvoiceNo = viewModel.InvoiceNo.GetEfficientString();
            var match = viewModel.InvoiceNo.ParseInvoiceNo();

            if (!match.Success || !viewModel.InvoiceDate.HasValue)
            {
                return NotFound();
            }

            IQueryable<InvoiceItem> items = models.GetTable<InvoiceItem>()
                .Where(i => i.TrackCode == match.Groups[1].Value && i.No == match.Groups[2].Value)
                .Where(i => i.InvoiceDate >= viewModel.InvoiceDate && i.InvoiceDate < viewModel.InvoiceDate.Value.AddDays(1));

            var item = items.FirstOrDefault();
            if (item == null)
            {
                return NotFound();
            }

            if (item.InvoiceBuyer.IsB2C())
            {
                if(item.RandomNo != viewModel.RandomNo.GetEfficientString())
                {
                    return NotFound();
                }

                if (item.InvoiceCarrier != null)
                {
                    if (item.InvoiceCarrier.CarrierNo != viewModel.CarrierNo.GetEfficientString())
                    {
                        return NotFound();
                    }
                }
            }
            else
            {
                if (item.InvoiceBuyer.ReceiptNo != viewModel.BuyerReceiptNo.GetEfficientString())
                {
                    return NotFound();
                }
            }

            return View("~/Views/IndividualProcess/GetInvoicePDF.cshtml", item);
        }

        public ActionResult GetPrint(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (viewModel.KeyID != null)
            {
                viewModel.InvoiceID = viewModel.DecryptKeyValue();
            }

            IQueryable<InvoiceItem> items = models.GetTable<InvoiceItem>()
                .Where(i => i.InvoiceID == viewModel.InvoiceID);

            var item = items.FirstOrDefault();
            viewModel.RandomNo = item?.RandomNo;

            return View("~/Views/IndividualProcess/Index.cshtml", item);

        }

        public ActionResult EditInvoiceBuyer(InvoiceBuyerViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            InvoiceBuyer item = null;

            if (viewModel.KeyID != null)
            {
                viewModel.InvoiceID = viewModel.DecryptKeyValue();
                item = models.GetTable<InvoiceBuyer>().Where(u => u.InvoiceID == viewModel.InvoiceID).FirstOrDefault();
            }

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "買受人資料錯誤!!");
            }
            else
            {
                viewModel.InvoiceID = item.InvoiceID;
                viewModel.ReceiptNo = item.ReceiptNo;
                viewModel.PostCode = item.PostCode;
                viewModel.Address = item.Address;
                viewModel.Name = item.Name;
                viewModel.BuyerID = item.BuyerID;
                viewModel.CustomerID = item.CustomerID;
                viewModel.ContactName = item.ContactName;
                viewModel.Phone = item.Phone;
                viewModel.EMail = item.EMail;
                viewModel.CustomerName = item.CustomerName;
                viewModel.Fax = item.Fax;
                viewModel.PersonInCharge = item.PersonInCharge;
                viewModel.RoleRemark = item.RoleRemark;
                viewModel.CustomerNumber = item.CustomerNumber;
                viewModel.BuyerMark = item.BuyerMark;
            }

            return View("~/Views/IndividualProcess/EditInvoiceBuyer.cshtml", item);

        }

        public ActionResult CommitInvoiceBuyer(InvoiceBuyerViewModel viewModel)
        {

            InvoiceBuyer item = null;

            if (viewModel.KeyID != null)
            {
                viewModel.InvoiceID = viewModel.DecryptKeyValue();
                item = models.GetTable<InvoiceBuyer>().Where(u => u.InvoiceID == viewModel.InvoiceID).FirstOrDefault();
            }

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "買受人資料錯誤!!");
            }

            item.PostCode = viewModel.PostCode;
            item.Address = viewModel.Address;
            item.ContactName = viewModel.ContactName;
            item.Phone = viewModel.Phone;

            models.SubmitChanges();

            return Json(new { result = true });
        }

        public async Task<ActionResult> ValidateCarrierAsync()
        {
            await Request.SaveAsAsync(System.IO.Path.Combine(Logger.LogDailyPath, $"{DateTime.Now.Ticks}.txt"), true);

            var form = Request.Form;
            if (form == null || String.IsNullOrEmpty(form["token"]) || form["ban"] != AppSettings.Default.InvoiceCarrierProviderID)
            {
                return new EmptyResult { };
            }

            var tokenReq = new
            {
                token = Request.Form["token"],
                nonce = 16.GenerateHexCode(),
            };

            dynamic result;
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                result = JObject.Parse(client.UploadString(AppSettings.Default.GovCarrierVerrificationUrl, tokenReq.JsonStringify()));
                Logger.Debug($"GOV result: {result}");
            }

            if (result.token_flag == "Y" && result.nonce == tokenReq.nonce)
            {
                result.token = tokenReq.token;
                return View("~/Views/IndividualProcess/ArrangeCarrierNo.cshtml", result);
            }
            else
            {
                return View("~/Views/IndividualProcess/InvalidGovToken.cshtml", result);
            }

        }

        public ActionResult ValidateUserCarrier(LoginViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            if (!ModelState.IsValid)
            {
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            var item = UserProfileFactory.CreateInstance(viewModel.PID, viewModel.Password);
            if (item == null)
            {
                ModelState.AddModelError("PID", "登入失敗!!");
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            item = item.LoadInstance(models);
            var carrier = item.InvoiceUserCarrier.FirstOrDefault();
            if (carrier == null)
            {
                ModelState.AddModelError("PID", "會員未申請載具!!");
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            viewModel.CardNo1 = Convert.ToBase64String(Encoding.UTF8.GetBytes(carrier.CarrierNo));
            viewModel.CardNo2 = Convert.ToBase64String(Encoding.UTF8.GetBytes(carrier.CarrierNo2));
            viewModel.CardType = Convert.ToBase64String(Encoding.UTF8.GetBytes(ModelExtension.Properties.AppSettings.Default.DefaultUserCarrierType));

            if (viewModel.Token != null)
            {
                StringBuilder postData = new StringBuilder();
                postData.Append("card_ban=").Append(AppSettings.Default.InvoiceCarrierProviderID)
                    .Append("&card_no1=").Append(carrier.CarrierNo)
                    .Append("&card_no2=").Append(carrier.CarrierNo2)
                    .Append("&card_type=").Append(ModelExtension.Properties.AppSettings.Default.DefaultUserCarrierType)
                    .Append("&token=").Append(viewModel.Token);

                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(AppSettings.Default.GovApiKeyBase64)))
                {
                    var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(postData.ToString()));
                    var signature = Convert.ToBase64String(computeHash);
                    postData.Append("&signature=").Append(signature);
                    viewModel.Signature = signature;
                    Logger.Debug($"{postData}:{signature}");
                }

                return View("~/Views/IndividualProcess/ApplyUserCarrier.cshtml", carrier);

            }
            else
            {
                viewModel.Token = item.UID.EncryptKey();
                return View("~/Views/IndividualProcess/ApplyUserCarrierFromLocal.cshtml", carrier);
            }
        }

        public async Task<ActionResult> ValidateUserTokenAsync(LoginViewModel viewModel)
        {
            await Request.SaveAsAsync(System.IO.Path.Combine(Logger.LogDailyPath, $"ValidateUserToken-{DateTime.Now.Ticks}.txt"), true);

            ViewBag.ViewModel = viewModel;
            viewModel.Token = viewModel.Token.GetEfficientString();
            if (viewModel.Token != null)
            {
                int uid = viewModel.Token.DecryptKeyValue();
                if (models.GetTable<UserProfile>().Any(u => u.UID == uid))
                {
                    return Content("Y");
                }
            }

            return Content("N");
        }

        //protected override void HandleUnknownAction(string actionName)
        //{
        //    if (!String.IsNullOrEmpty(actionName))
        //    {
        //        this.View(actionName).ExecuteResult(this.ControllerContext);
        //    }
        //}

    }
}
