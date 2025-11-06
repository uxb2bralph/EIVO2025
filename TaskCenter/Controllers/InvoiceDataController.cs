using ApplicationResource;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ModelCore.Helper;
using TaskCenter.Helper.RequestAction;
using TaskCenter.Properties;
using ModelCore.InvoiceManagement;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Threading.Tasks;

namespace TaskCenter.Controllers
{
    public class InvoiceDataController : SampleController
    {
        public ActionResult DeleteProcessRequest(InvoiceRequestViewModel viewModel)
        {
            if (viewModel.KeyID != null)
            {
                viewModel.TaskID = DecryptKeyValue(viewModel, out bool expired);
                if (expired)
                {
                    ModelState.AddModelError("E1001", ErrorMessage.E1001);
                }
            }

            if (!ModelState.IsValid)
            {
                return Json(new { result = false, errorCode = ModelState.AllErrorKey() });
            }

            var count = models!.ExecuteCommand("delete [proc].ProcessRequest where TaskID = {0}", viewModel.TaskID);
            return Json(new { result = true });

        }

        public ActionResult UploadProcessRequest(InvoiceRequestViewModel viewModel)
        {
            ViewBag.DataItem = viewModel.SaveProcessRequest(this);

            if (!ModelState.IsValid)
            {
                return Json(new { result = false, errorCode = ModelState.AllErrorKey() });
            }

            return Json(new { result = true });
        }

        public async Task<ActionResult> UploadAttachmentAsync()
        {
            InvoiceRequestViewModel? viewModel = await PrepareViewModelAsync<InvoiceRequestViewModel>();

            if (viewModel == null)
            {
                return BadRequest("Invalid request data.");
            }

            viewModel.SaveAttachment(this);
            if (!ModelState.IsValid)
            {
                return Json(new { result = false, errorCode = ModelState.AllErrorKey() });
            }

            AttachmentManager.MatchAttachment();

            return Json(new { result = true });
        }

        public async Task<ActionResult> UploadInvoiceRequestXlsxAsync(InvoiceRequestViewModel viewModel)
        {
            //await Request.SaveAsAsync(Path.Combine(CommonLib.Core.Utility.Logger.LogDailyPath, $"{DateTime.Now.Ticks}.txt"), true);
            await Request.SaveAsAsync(null, true);

            if (!viewModel.ProcessType.HasValue)
            {
                viewModel.ProcessType = Naming.InvoiceProcessType.F0401_Xlsx;
            }

            return UploadProcessRequest(viewModel);

        }

        public ActionResult UploadVoidInvoiceRequestXlsx(InvoiceRequestViewModel viewModel)
        {
            viewModel.ProcessType = Naming.InvoiceProcessType.F0501_Xlsx;
            return UploadProcessRequest(viewModel);
        }

        public ActionResult UploadAllowanceRequestXlsx(InvoiceRequestViewModel viewModel)
        {
            viewModel.ProcessType = Naming.InvoiceProcessType.G0401_Xlsx;
            return UploadProcessRequest(viewModel);
        }

        public ActionResult UploadFullAllowanceRequestXlsx(InvoiceRequestViewModel viewModel)
        {
            viewModel.ProcessType = Naming.InvoiceProcessType.G0401_Full_Xlsx;
            return UploadProcessRequest(viewModel);
        }

        public ActionResult UploadVoidAllowanceRequestXlsx(InvoiceRequestViewModel viewModel)
        {
            viewModel.ProcessType = Naming.InvoiceProcessType.G0501_Xlsx;
            return UploadProcessRequest(viewModel);
        }

        public ActionResult GetResource(String path)
        {
            if (path != null)
            {
                String filePath = path.DecryptData().StoreTargetPath();
                if (System.IO.File.Exists(filePath))
                {
                    return PhysicalFile(filePath, "application/octet-stream", Path.GetFileName(filePath));
                }
            }
            return new EmptyResult { };
        }

        //[HttpGet]
        //[HttpPost]
        public async Task<ActionResult> NotifyRequestCompletion()
        {
            InvoiceRequestViewModel? viewModel = await PrepareViewModelAsync<InvoiceRequestViewModel>();

            if (viewModel == null)
            {
                return BadRequest("Invalid request data.");
            }

            var result = CheckAuth(viewModel);
            if (result != null)
            {
                return result;
            }

            var items = models!.GetTable<ProcessRequest>().Where(q => q.AgentID == viewModel.AgentID)
                                .Where(q => q.ProcessCompletionNotification != null);

            if(viewModel.Sender.HasValue)
            {
                items = items.Where(p => p.Sender == viewModel.Sender);
            }

            return Json(new
            {
                result = items.Count() > 0,
                data = items.Select(q =>
                        new
                        {
                            q.TaskID,
                            q.ProcessRequestType.ChannelName,
                            q.ProcessRequestType.ChannelResponse,
                            ResponseName = Path.GetFileName(q.ResponsePath),
                            TxnPath = q.ViewModel != null ? JsonConvert.DeserializeObject<InvoiceRequestViewModel>(q.ViewModel).StoragePath : null
                        }).ToArray()
            });

        }

        public async Task<ActionResult> CommitProcessResponseAsync()
        {
            InvoiceRequestViewModel? viewModel = await PrepareViewModelAsync<InvoiceRequestViewModel>();

            if (viewModel == null)
            {
                return BadRequest("Invalid request data.");
            }

            var authResult = CheckAuth(viewModel);
            if (authResult != null)
            {
                return authResult;
            }

            var result = models.ExecuteCommand(@"
                        DELETE FROM [proc].ProcessCompletionNotification
                        FROM              [proc].ProcessCompletionNotification INNER JOIN
                                                    [proc].ProcessRequest ON [proc].ProcessCompletionNotification.TaskID = [proc].ProcessRequest.TaskID
                        WHERE          ([proc].ProcessCompletionNotification.TaskID = {0}) AND ([proc].ProcessRequest.AgentID = {1})", viewModel.TaskID, viewModel.AgentID);

            if (result > 0)
            {
                var item = models.GetTable<ProcessRequest>().Where(q => q.TaskID == viewModel.TaskID).FirstOrDefault();
                if (item != null && System.IO.File.Exists(item.ResponsePath))
                {
                    String fileName = Path.GetFileName(item.ResponsePath);
                    fileName = fileName.Substring(fileName.IndexOf('_') + 1);
                    return PhysicalFile(item.ResponsePath, "application/octet-stream", fileName);
                }
            }
            return new EmptyResult { };
        }

        private readonly static String[] _DeleteMIGQ = {
            @"DELETE FROM [proc].DataProcessQueue
                WHERE   (DocID = {0}) AND (StepID = {1})",
             };

        private const int MAX_ITEMS = 100;

        public InvoiceDataController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
        }

        public async Task<ActionResult> RetrieveMIGResponseAsync()
        {
            //await Request.SaveAsAsync(Path.Combine(CommonLib.Core.Utility.Logger.LogDailyPath, $"{DateTime.Now.Ticks}.txt"), true);
            await Request.SaveAsAsync(null, true);

            MIGResponseViewModel? viewModel = await PrepareViewModelAsync<MIGResponseViewModel>();

            if (viewModel == null)
            {
                return BadRequest("Invalid request data.");
            }

            var authResult = CheckAuth(viewModel);
            if (authResult != null)
            {
                return authResult;
            }

            void deleteMIG(int docID)
            {
                foreach (var sqlCmd in _DeleteMIGQ)
                {
                    if (models!.ExecuteCommand(sqlCmd, docID, (int)Naming.InvoiceStepDefinition.回傳MIG) > 0)
                    {
                        return;
                    }
                }
            }

            if (viewModel.LastReceivedKey != null)
            {
                foreach (var docID in viewModel.LastReceivedKey)
                {
                    deleteMIG(docID);
                }
            }

            IQueryable<InvoiceItem> invoiceItems;
            IQueryable<InvoiceAllowance> allowanceItems;
            viewModel = new MIGResponseViewModel
            {
                AgentID = viewModel.AgentID,
                ProcessType = viewModel.ProcessType,
            };

            switch (viewModel.ProcessType)
            {
                case Naming.InvoiceProcessType.C0401:
                case Naming.InvoiceProcessType.F0401:
                case Naming.InvoiceProcessType.A0401:
                    invoiceItems = models!.GetInvoiceByAgent(models!.GetTable<InvoiceItem>(), viewModel.AgentID ?? -1);
                    var migC0401 = models.GetTable<DataProcessQueue>()
                                        .Where(d=> d.ProcessType == (int)Naming.InvoiceProcessType.F0401)
                                        .Where(d => d.StepID == (int)Naming.InvoiceStepDefinition.回傳MIG)
                                        .Where(d => invoiceItems.Any(i => i.InvoiceID == d.DocID))
                                        .Take(MAX_ITEMS)
                                        .ToList();
                    viewModel.LastReceivedKey = migC0401.Select(d => d.DocID).ToArray();
                    viewModel.Items = migC0401
                        .Select(d => 
                        new MIGContent 
                        {
                            DocID = d.DocID,
                            DocDate = d.CDS_Document.DocDate,
                            No = d.CDS_Document.InvoiceItem?.InvoiceNo(),
                            ReceiptNo = d.CDS_Document.InvoiceItem?.Organization?.ReceiptNo,
                            MIG = d.CDS_Document.InvoiceItem?.CreateF0401().GetXml()
                        }).ToArray();
                    break;

                case Naming.InvoiceProcessType.C0501:
                case Naming.InvoiceProcessType.F0501:
                case Naming.InvoiceProcessType.A0501:
                    invoiceItems = models!.GetInvoiceByAgent(models!.GetTable<InvoiceItem>(), viewModel.AgentID ?? -1);
                    var migC0501 = models!.GetTable<DataProcessQueue>()
                                        .Where(d => d.ProcessType == (int)Naming.InvoiceProcessType.F0501)
                                        .Where(d => d.StepID == (int)Naming.InvoiceStepDefinition.回傳MIG)
                                        .Where(d => invoiceItems.Any(i => i.InvoiceID == d.CDS_Document.DerivedDocument.SourceID))
                                        .Take(MAX_ITEMS)
                                        .ToList();
                    viewModel.LastReceivedKey = migC0501.Select(d => d.DocID).ToArray();
                    viewModel.Items = migC0501
                        .Select(d =>
                        new MIGContent
                        {
                            DocID = d.DocID,
                            DocDate = d.CDS_Document.DocDate,
                            No = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.InvoiceNo(),
                            ReceiptNo = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.Organization?.ReceiptNo,
                            MIG = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.CreateF0501()?.OuterXml
                        }).ToArray();
                    break;

                case Naming.InvoiceProcessType.D0401:
                case Naming.InvoiceProcessType.G0401:
                case Naming.InvoiceProcessType.B0401:
                    allowanceItems = models!.GetAllowanceByAgent(viewModel.AgentID ?? -1);
                    var migD0401 = models!.GetTable<DataProcessQueue>()
                                        .Where(d => d.ProcessType == (int)Naming.InvoiceProcessType.G0401)
                                        .Where(d => d.StepID == (int)Naming.InvoiceStepDefinition.回傳MIG)    
                                        .Where(d => allowanceItems.Any(i => i.AllowanceID == d.DocID))
                                        .Take(MAX_ITEMS)
                                        .ToList();
                    viewModel.LastReceivedKey = migD0401.Select(d => d.DocID).ToArray();
                    viewModel.Items = migD0401
                        .Select(d =>
                        new MIGContent
                        {
                            DocID = d.DocID,
                            DocDate = d.CDS_Document.DocDate,
                            No = d.CDS_Document.InvoiceAllowance?.AllowanceNumber,
                            ReceiptNo = d.CDS_Document.InvoiceAllowance?.InvoiceAllowanceSeller?.ReceiptNo,
                            MIG = d.CDS_Document.InvoiceAllowance?.CreateG0401().OuterXml
                        }).ToArray();
                    break;

                case Naming.InvoiceProcessType.D0501:
                case Naming.InvoiceProcessType.G0501:
                case Naming.InvoiceProcessType.B0501:
                    allowanceItems = models!.GetAllowanceByAgent(viewModel.AgentID ?? -1);
                    var migD0501 = models!.GetTable<DataProcessQueue>()
                                        .Where(d => d.ProcessType == (int)Naming.InvoiceProcessType.G0501)
                                        .Where(d => d.StepID == (int)Naming.InvoiceStepDefinition.回傳MIG)
                                        .Where(d => allowanceItems.Any(i => i.AllowanceID == d.CDS_Document.DerivedDocument.SourceID))
                                        .Take(MAX_ITEMS)
                                        .ToList();
                    viewModel.LastReceivedKey = migD0501.Select(d => d.DocID).ToArray();
                    viewModel.Items = migD0501
                        .Select(d =>
                        new MIGContent
                        {
                            DocID = d.DocID,
                            DocDate = d.CDS_Document.DocDate,
                            No = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceAllowance ?? d.CDS_Document.InvoiceAllowance)?.AllowanceNumber,
                            ReceiptNo = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceAllowance ?? d.CDS_Document.InvoiceAllowance)?.InvoiceAllowanceSeller?.ReceiptNo,
                            MIG = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceAllowance ?? d.CDS_Document.InvoiceAllowance)?.CreateG0501()?.OuterXml
                        }).ToArray();
                    break;

            }

            return Content(viewModel.JsonStringify(), "application/json");

        }

        private ActionResult? CheckAuth(AuthQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            if (viewModel.KeyID != null)
            {
                viewModel.AgentID = DecryptKeyValue(viewModel, out bool expired);
                if (expired)
                {
                    return Json(new { result = false, message = ErrorMessage.E1001 });
                }
            }
            return null;
        }

        public async Task<ActionResult> UploadMIGAsync()
        {
            InvoiceRequestViewModel? viewModel = await PrepareViewModelAsync<InvoiceRequestViewModel>();

            if (viewModel == null)
            {
                return BadRequest("Invalid request data.");
            }

            viewModel.StoreMIG(this);

            if (!ModelState.IsValid)
            {
                return Json(new { result = false, errorCode = ModelState.AllErrorKey() });
            }

            return Json(new { result = true });
        }

        public async Task<ActionResult> CheckMIGAsync()
        {
            InvoiceRequestViewModel? viewModel = await PrepareViewModelAsync<InvoiceRequestViewModel>();

            if (viewModel == null)
            {
                return BadRequest("Invalid request data.");
            }

            viewModel.StoreMIG(this);

            if (!ModelState.IsValid)
            {
                return Json(new { result = false, errorCode = ModelState.AllErrorKey() });
            }

            return Json(viewModel);
        }

    }
}