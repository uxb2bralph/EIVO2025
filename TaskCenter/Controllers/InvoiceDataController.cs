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

        public ActionResult UploadAttachment(InvoiceRequestViewModel viewModel)
        {
            viewModel.SaveAttachment(this);
            if (!ModelState.IsValid)
            {
                return Json(new { result = false, errorCode = ModelState.AllErrorKey() });
            }

            AttachmentManager.MatchAttachment();

            return Json(new { result = true });
        }

        public ActionResult UploadInvoiceRequestXlsx(InvoiceRequestViewModel viewModel)
        {
            if (!viewModel.ProcessType.HasValue)
            {
                viewModel.ProcessType = Naming.InvoiceProcessType.C0401_Xlsx;
            }

            return UploadProcessRequest(viewModel);

        }

        public ActionResult UploadVoidInvoiceRequestXlsx(InvoiceRequestViewModel viewModel)
        {
            viewModel.ProcessType = Naming.InvoiceProcessType.C0501_Xlsx;
            return UploadProcessRequest(viewModel);
        }

        public ActionResult UploadAllowanceRequestXlsx(InvoiceRequestViewModel viewModel)
        {
            viewModel.ProcessType = Naming.InvoiceProcessType.D0401_Xlsx;
            return UploadProcessRequest(viewModel);
        }

        public ActionResult UploadFullAllowanceRequestXlsx(InvoiceRequestViewModel viewModel)
        {
            viewModel.ProcessType = Naming.InvoiceProcessType.D0401_Full_Xlsx;
            return UploadProcessRequest(viewModel);
        }

        public ActionResult UploadVoidAllowanceRequestXlsx(InvoiceRequestViewModel viewModel)
        {
            viewModel.ProcessType = Naming.InvoiceProcessType.D0501_Xlsx;
            return UploadProcessRequest(viewModel);
        }

        public ActionResult GetResource(String path)
        {
            if (path != null)
            {
                String filePath = path.DecryptData().StoreTargetPath();
                if (System.IO.File.Exists(filePath))
                {
                    return File(filePath, "application/octet-stream", Path.GetFileName(filePath));
                }
            }
            return new EmptyResult { };
        }

        public ActionResult NotifyRequestCompletion(InvoiceRequestViewModel viewModel)
        {
            var result = CheckAuth(viewModel);
            if (result != null)
            {
                return result;
            }

            var items = models.GetTable<ProcessRequest>().Where(q => q.AgentID == viewModel.AgentID)
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

        public ActionResult CommitProcessResponse(InvoiceRequestViewModel viewModel)
        {
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
                    return File(item.ResponsePath, "application/octet-stream", fileName);
                }
            }
            return new EmptyResult { };
        }

        private readonly static String[] _DeleteMIGQ = {
            @"DELETE FROM [proc].C0401DispatchQueue
            WHERE   (DocID = {0}) AND (StepID = {1})",
            @"DELETE FROM [proc].C0501DispatchQueue
            WHERE   (DocID = {0}) AND (StepID = {1})",
            @"DELETE FROM [proc].D0401DispatchQueue
            WHERE   (DocID = {0}) AND (StepID = {1})",
            @"DELETE FROM [proc].D0501DispatchQueue
            WHERE   (DocID = {0}) AND (StepID = {1})",
            @"DELETE FROM [proc].A0401DispatchQueue
            WHERE   (DocID = {0}) AND (StepID = {1})",
            @"DELETE FROM [proc].A0501DispatchQueue
            WHERE   (DocID = {0}) AND (StepID = {1})",
            @"DELETE FROM [proc].B0401DispatchQueue
            WHERE   (DocID = {0}) AND (StepID = {1})",
            @"DELETE FROM [proc].B0501DispatchQueue
            WHERE   (DocID = {0}) AND (StepID = {1})"
             };

        private const int MAX_ITEMS = 100;

        public InvoiceDataController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
        }

        public async Task<ActionResult> RetrieveMIGResponseAsync(MIGResponseViewModel viewModel)
        {
            await Request.SaveAsAsync(Path.Combine(CommonLib.Core.Utility.Logger.LogDailyPath, $"{DateTime.Now.Ticks}.txt"), true);
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
                    invoiceItems = models!.GetInvoiceByAgent(models!.GetTable<InvoiceItem>(), viewModel.AgentID ?? -1);
                    var migC0401 = models.GetTable<C0401DispatchQueue>().Where(d => d.StepID == (int)Naming.InvoiceStepDefinition.回傳MIG)
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
                            MIG = d.CDS_Document.InvoiceItem?.CreateInvoiceMIG().GetXml()
                        }).ToArray();
                    break;

                case Naming.InvoiceProcessType.C0501:
                    invoiceItems = models!.GetInvoiceByAgent(models!.GetTable<InvoiceItem>(), viewModel.AgentID ?? -1);
                    var migC0501 = models!.GetTable<C0501DispatchQueue>().Where(d => d.StepID == (int)Naming.InvoiceStepDefinition.回傳MIG)
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
                            MIG = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.CreateInvoiceCancellationMIG().GetXml()
                        }).ToArray();
                    break;

                case Naming.InvoiceProcessType.A0401:
                    invoiceItems = models!.GetInvoiceByAgent(models!.GetTable<InvoiceItem>(), viewModel.AgentID ?? -1);
                    var migA0401 = models.GetTable<A0401DispatchQueue>().Where(d => d.StepID == (int)Naming.InvoiceStepDefinition.回傳MIG)
                                        .Where(d => invoiceItems.Any(i => i.InvoiceID == d.DocID))
                                        .Take(MAX_ITEMS)
                                        .ToList();
                    viewModel.LastReceivedKey = migA0401.Select(d => d.DocID).ToArray();
                    viewModel.Items = migA0401
                        .Select(d =>
                        new MIGContent
                        {
                            DocID = d.DocID,
                            DocDate = d.CDS_Document.DocDate,
                            No = d.CDS_Document.InvoiceItem?.InvoiceNo(),
                            ReceiptNo = d.CDS_Document.InvoiceItem?.Organization?.ReceiptNo,
                            MIG = d.CDS_Document.InvoiceItem?.CreateB2BInvoiceMIG().GetXml()
                        }).ToArray();
                    break;

                case Naming.InvoiceProcessType.A0501:
                    invoiceItems = models!.GetInvoiceByAgent(models!.GetTable<InvoiceItem>(), viewModel.AgentID ?? -1);
                    var migA0501 = models.GetTable<A0501DispatchQueue>().Where(d => d.StepID == (int)Naming.InvoiceStepDefinition.回傳MIG)
                                        .Where(d => invoiceItems.Any(i => i.InvoiceID == d.CDS_Document.DerivedDocument.SourceID))
                                        .Take(MAX_ITEMS)
                                        .ToList();
                    viewModel.LastReceivedKey = migA0501.Select(d => d.DocID).ToArray();
                    viewModel.Items = migA0501
                        .Select(d =>
                        new MIGContent
                        {
                            DocID = d.DocID,
                            DocDate = d.CDS_Document.DocDate,
                            No = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.InvoiceNo(),
                            ReceiptNo = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.Organization?.ReceiptNo,
                            MIG = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.CreateB2BInvoiceCancellationMIG().GetXml()
                        }).ToArray();
                    break;

                case Naming.InvoiceProcessType.D0401:
                    allowanceItems = models!.GetAllowanceByAgent(viewModel.AgentID ?? -1);
                    var migD0401 = models!.GetTable<D0401DispatchQueue>().Where(d => d.StepID == (int)Naming.InvoiceStepDefinition.回傳MIG)
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
                            MIG = d.CDS_Document.InvoiceAllowance?.CreateAllowanceMIG().GetXml()
                        }).ToArray();
                    break;

                case Naming.InvoiceProcessType.D0501:
                    allowanceItems = models!.GetAllowanceByAgent(viewModel.AgentID ?? -1);
                    var migD0501 = models.GetTable<D0501DispatchQueue>().Where(d => d.StepID == (int)Naming.InvoiceStepDefinition.回傳MIG)
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
                            MIG = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceAllowance ?? d.CDS_Document.InvoiceAllowance)?.CreateAllowanceCancellationMIG().GetXml()
                        }).ToArray();
                    break;

                case Naming.InvoiceProcessType.B0401:
                    allowanceItems = models!.GetAllowanceByAgent(viewModel.AgentID ?? -1);
                    var migB0401 = models.GetTable<B0401DispatchQueue>().Where(d => d.StepID == (int)Naming.InvoiceStepDefinition.回傳MIG)
                                        .Where(d => allowanceItems.Any(i => i.AllowanceID == d.DocID))
                                        .Take(MAX_ITEMS)
                                        .ToList();
                    viewModel.LastReceivedKey = migB0401.Select(d => d.DocID).ToArray();
                    viewModel.Items = migB0401
                        .Select(d =>
                        new MIGContent
                        {
                            DocID = d.DocID,
                            DocDate = d.CDS_Document.DocDate,
                            No = d.CDS_Document.InvoiceAllowance?.AllowanceNumber,
                            ReceiptNo = d.CDS_Document.InvoiceAllowance?.InvoiceAllowanceSeller?.ReceiptNo,
                            MIG = d.CDS_Document.InvoiceAllowance?.CreateB2BAllowanceMIG().GetXml()
                        }).ToArray();
                    break;

                case Naming.InvoiceProcessType.B0501:
                    allowanceItems = models!.GetAllowanceByAgent(viewModel.AgentID ?? -1);
                    var migB0501 = models!.GetTable<B0501DispatchQueue>().Where(d => d.StepID == (int)Naming.InvoiceStepDefinition.回傳MIG)
                                        .Where(d => allowanceItems.Any(i => i.AllowanceID == d.CDS_Document.DerivedDocument.SourceID))
                                        .Take(MAX_ITEMS)
                                        .ToList();
                    viewModel.LastReceivedKey = migB0501.Select(d => d.DocID).ToArray();
                    viewModel.Items = migB0501
                        .Select(d =>
                        new MIGContent
                        {
                            DocID = d.DocID,
                            DocDate = d.CDS_Document.DocDate,
                            No = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceAllowance ?? d.CDS_Document.InvoiceAllowance)?.AllowanceNumber,
                            ReceiptNo = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceAllowance ?? d.CDS_Document.InvoiceAllowance)?.InvoiceAllowanceSeller?.ReceiptNo,
                            MIG = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceAllowance ?? d.CDS_Document.InvoiceAllowance)?.CreateB2BAllowanceCancellationMIG().GetXml()
                        }).ToArray();
                    break;

            }

            return Content(viewModel.JsonStringify(), "application/json");

        }

        private ActionResult CheckAuth(AuthQueryViewModel viewModel)
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

        public ActionResult UploadMIG(InvoiceRequestViewModel viewModel)
        {
            viewModel.StoreMIG(this);

            if (!ModelState.IsValid)
            {
                return Json(new { result = false, errorCode = ModelState.AllErrorKey() });
            }

            return Json(new { result = true });
        }

        public ActionResult CheckMIG(InvoiceRequestViewModel viewModel)
        {
            viewModel.StoreMIG(this);

            if (!ModelState.IsValid)
            {
                return Json(new { result = false, errorCode = ModelState.AllErrorKey() });
            }

            return Json(viewModel);
        }

    }
}