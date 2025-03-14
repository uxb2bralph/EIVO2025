using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Wordprocessing;
using WebHome.Helper;
using WebHome.Helper.Security.Authorization;
using WebHome.Models;
using WebHome.Models.ViewModel;

using WebHome.Properties;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Services.Description;
using System.Xml;
using CommonLib.Core.Utility;
using static ModelCore.Locale.Naming;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebHome.Controllers
{
    public class InvoiceNumberApplyController : SampleController<InvoiceItem>
    {
        InvoiceNumberApplyResponse response = new InvoiceNumberApplyResponse();
        InvoiceNumberApplyService service;

        public InvoiceNumberApplyController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: InvoiceApply
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Message(InvoiceNumberApplyResponse response)
        {
            return View(response);
        }

        [AuthorizedSysAdmin()]
        public ActionResult QueryIndex(InvoiceNumberApplyQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View();
        }

        //[AuthorizedSysAdmin()]
        //public ActionResult Query(InvoiceNumberApplyQueryViewModel viewModel)
        //{
        //    service = new InvoiceNumberApplyService(viewModel.BusinessId);

        //    IEnumerable<FileInfo> fileInfos 
        //        = service.GetApplyJsonFilesInfo();

        //    IEnumerable<InvoiceNumberApplyQueryViewModel> applyFilterResult
        //        = fileInfos
        //        .Select(x => new InvoiceNumberApplyQueryViewModel
        //            {
        //                BusinessId = x.Name.Substring(6, 8),
        //                ApplyUpdateTime = x.LastWriteTime,
        //            }).ToList();

        //    ViewBag.ViewModel = viewModel;
        //    ViewBag.Model = applyFilterResult;
        //    return View("../InvoiceNumberApply/QueryResult", ViewBag);
        //}

        [AuthorizedSysAdmin()]
        public ActionResult Query(InvoiceNumberApplyQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            service = new InvoiceNumberApplyService(viewModel.BusinessId);

            IEnumerable<String> items
                = service.GetApplyJsonFiles();

            return View("~/Views/InvoiceNumberApply/QueryResult2024.cshtml", items);
        }



        public ActionResult Apply(string templateID = "AppointApply")
        {
            InvoiceNumberApply apply = null;
            if (templateID.Equals("AppointApply"))
            {
                apply = new InvoiceNumberApply().getTemplateNumberApplyData();
            }
            else if (templateID.Equals("TestData"))
            {
                apply = new InvoiceNumberApply().getTestData();
            }
            else if (templateID.Equals("TestDataWithWrongFormat"))
            {
                apply = new InvoiceNumberApply().getTestDataWithWrongFormat();
            }

            if (apply == null)
            {
                response.Message = "傳入參數有誤";
                return RedirectToAction("Message",
                    "InvoiceNumberApply",
                    response);
            }

            InvoiceNumberApplyViewModel applyViewModel = new InvoiceNumberApplyViewModel(apply);
            return View(applyViewModel);
        }

        [HttpGet]
        public JsonResult SysSupplier(string sysSupplierID)
        {
            return Json(
                 AppSettings.Default.InvoiceNumberApplySetting.SysSupplier
                    .Where(x => x.ID == sysSupplierID)
                    .FirstOrDefault().JsonStringify()
                );
        }

        [HttpGet]
        public JsonResult PaperTestSet(string paperTestSetID)
        {
            return Json(
                 AppSettings.Default.InvoiceNumberApplySetting.PaperTestSet
                    .Where(x => x.ID == paperTestSetID)
                    .FirstOrDefault().JsonStringify()
                );
        }
        [AuthorizedSysAdmin()]
        public ActionResult Update(string businessID)
        {
            service = new InvoiceNumberApplyService(businessID);

            InvoiceNumberApply apply
                = service.GetApplyViewModelFromJson();
            InvoiceNumberApplyViewModel applyViewModel
                = new InvoiceNumberApplyViewModel(apply);
            return View("../InvoiceNumberApply/Apply", applyViewModel);
        }

        //[AuthorizedSysAdmin()]
        //public ActionResult MoveFile(string businessID)
        //{
        //    InvoiceNumberApplyService.MoveJsonFile(businessID);
        //    return Json(new { result = true });
        //}

        [AuthorizedSysAdmin()]
        public ActionResult MoveFile(QueryViewModel viewModel)
        {
            if (viewModel.KeyID != null)
            {
                String filePath = viewModel.KeyID.DecryptData();
                InvoiceNumberApplyService.MoveJsonFile(filePath);
                return Json(new { result = true });
            }
            return Json(new { result = false, message = "file not found!!" });
        }

        //[AuthorizedSysAdmin()]
        //public ActionResult TransferOrganization(string businessID)
        //{
        //    #region check
        //    response.BusinessID = businessID;

        //    if (string.IsNullOrEmpty(businessID))
        //    {
        //        response.Message = "統編不存在";
        //        response.IsInvalid = true;
        //    }

        //    service = new InvoiceNumberApplyService(businessID);

        //    InvoiceNumberApply apply
        //        = service.GetApplyViewModelFromJson();

        //    if (apply == null)
        //    {
        //        response.Message = "Json檔不存在";
        //        response.IsInvalid = true;
        //    }
        //    #endregion
        //    if (response.IsInvalid)
        //    {
        //        ModelState.AddModelError("Message", response.DisplayMessage);
        //        Logger.Info(response.DisplayMessage);
        //        return View("~/Views/Shared/ReportInputError.cshtml");
        //    }

        //    try
        //    {
        //        OrganizationViewModel organizationViewModel
        //            = service.ApplyConvertedOrganization(apply);

        //        Organization organization
        //            = organizationViewModel.CommitOrganizationViewModel(models, ModelState);

        //        if (organization == null || !ModelState.IsValid)
        //        {
        //            var msg = ModelStateErrLog();
        //            ModelState.AddModelError("Message", msg);
        //            return View("~/Views/Shared/ReportInputError.cshtml");
        //        }

        //        InvoiceNumberApplyService.MoveJsonFile(businessID);
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Message = "營業人轉換失敗.";
        //        response.IsInvalid = true;
        //        Logger.Warn(response.DisplayMessage + "   " + ex.ToString());
        //        ModelState.AddModelError("Message", response.DisplayMessage);
        //        return View("~/Views/Shared/ReportInputError.cshtml");
        //    }

        //    return Json(new { result = true });
        //}

        [AuthorizedSysAdmin()]
        public ActionResult TransferOrganization(QueryViewModel viewModel)
        {
            String filePath = null;
            if (viewModel.KeyID != null)
            {
                filePath = viewModel.KeyID.DecryptData();
            }

            #region check

            if (filePath == null || !System.IO.File.Exists(filePath))
            {
                return Json(new { result = false, message = "file not found!!" });
            }

            InvoiceNumberApply apply
                = filePath.DeserializeObjectFromFile<InvoiceNumberApply>();

            if (apply == null)
            {
                return Json(new { result = false, message = "file not found!!" });
            }

            #endregion

            try
            {
                service = new InvoiceNumberApplyService();

                OrganizationViewModel organizationViewModel
                    = service.ApplyConvertedOrganization(apply);

                Organization organization
                    = organizationViewModel.CommitOrganizationViewModel(models, ModelState);

                if (organization == null || !ModelState.IsValid)
                {
                    var msg = ModelStateErrLog();
                    ModelState.AddModelError("Message", msg);
                    return View("~/Views/Shared/ReportInputError.cshtml");
                }

                InvoiceNumberApplyService.MoveJsonFile(filePath);

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Json(new { result = false, message = "營業人轉換失敗!!" });
            }

            return Json(new { result = true });
        }

        public ActionResult SetAll(string businessID)
        {
            new InvoiceNumberApplyService(businessID);

            if (businessID == "" || businessID == null)
            {
                response.IsInvalid = true;
                response
                    = new InvoiceNumberApplyResponse(
                        message: "businessID is null or empty.");
            }
            response.BusinessID = businessID;

            InvoiceNumberApply viewBase =
                 new InvoiceNumberApplyService(businessID).GetApplyViewModelFromJson();
            if (!response.IsInvalid && viewBase == null)
            {
                response.IsInvalid = true;
                response.Message = "找不到相關JSON檔,或JSON格式有誤.";
            }

            if (response.IsInvalid)
            {
                Logger.Info(response.DisplayMessage);
                return RedirectToAction("Message",
                    "InvoiceNumberApply",
                    response);
            }

            IEnumerable<InvoiceNumberApplyWordSetting> wordSets
                 = AppSettings.Default.InvoiceNumberApplyWordSetting;
            UTF8Encoding encoding = new UTF8Encoding();
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (InvoiceNumberApplyWordSetting wordset in wordSets)
                        {
                            var file = archive.CreateEntry(wordset.OutputName);
                            InvoiceNumberApplyService sb
                                = new InvoiceNumberApplyService(viewBase, wordset);

                            byte[] contentAsBytes = encoding.GetBytes(sb.GetReplacedWordXmlString());
                            using (var stream = file.Open())
                            {
                                stream.Write(contentAsBytes, 0, contentAsBytes.Length);
                            }
                        }
                    }

                    return File(memoryStream.ToArray(), "application/zip", AppSettings.Default.InvoiceNumberApplySetting.GetZipFileName(businessID));
                };
            }
            catch (Exception ex)
            {
                Logger.Info(ex.ToString());
                response.IsInvalid = true;
                response.Message = "JSON轉Word失敗.";
                return RedirectToAction("Message",
                    "InvoiceNumberApply",
                    response);
            }

        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Create(InvoiceNumberApplyViewModel viewModel)
        {
            if (viewModel == null)
            {
                return View("../InvoiceNumberApply/Apply", viewModel);
            }

            if (viewModel.Apply == null || !this.ModelState.IsValid)
            {
                ModelStateErrLog();
                viewModel.ValidCode = string.Empty;
                viewModel.EncryptedCode = string.Empty;
                return View("../InvoiceNumberApply/Apply", viewModel);
            }

            response.BusinessID = viewModel.Apply.BusinessID;

            try
            {
                string jsonFilePath = AppSettings.Default.InvoiceNumberApplySetting.GetApplyFilePath(viewModel.Apply.BusinessID);
                viewModel.Apply.SerializeObjectToJsonFile(jsonFilePath);
            }
            catch (Exception ex)
            {
                response.IsInvalid = true;
                response.Message = "資料儲存失敗";
                Logger.Warn(ex.ToString());
                return RedirectToAction("Message",
                    "InvoiceNumberApply",
                    response);
            }

            try
            {
                if (AppSettings.Default.InvoiceNumberApplySetting.NotifyEnable)
                {
                    SharedFunction.SendMailMessage(viewModel.Apply.ToString()
                        , ModelExtension.Properties.AppSettings.Default.WebMaster
                        , string.Format("電子發票號碼申請-{0}", viewModel.Apply.BusinessID));
                }
            }
            catch (Exception)
            {
                response.IsInvalid = true;
                response.Message = "申請email寄送失敗.";
                Logger.Error(response.DisplayMessage);
                return RedirectToAction("Message",
                    "InvoiceNumberApply",
                    response);
            }

            return View(response);
        }

        private string ModelStateErrLog()
        {
            IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
            string logModelStateMsg = string.Empty;
            var ModelStateErrors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    k => k.Key,
                    k => k.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            if (ModelStateErrors.Count > 0)
            {
                foreach (KeyValuePair<string, string[]> kvp in ModelStateErrors)
                {
                    logModelStateMsg += (string.Format("Key = {0}, Value = {1}", kvp.Key, kvp.Value[0].ToString()));
                }
                Logger.Info(logModelStateMsg);
            }

            return logModelStateMsg;
        }
    }

}