using ClosedXML.Excel;
using CommonLib.Core.Utility;
using CommonLib.DataAccess;
using CommonLib.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.InvoiceManagement.InvoiceProcess;
using ModelCore.Locale;
using ModelCore.Models;
using ModelCore.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using Microsoft.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using WebHome.Helper;
using WebHome.Models;
using WebHome.Models.ViewModel;
using WebHome.Properties;
using WebHome.Infrastructure.BackgroundTasks;
using Microsoft.EntityFrameworkCore;
using ModelCore.DataEntityWrapper;

namespace WebHome.Controllers
{
    [Authorize]
    public class AllowanceProcessController : SampleController<InvoiceItem>
    {
        protected UserProfile _userProfile;

        public AllowanceProcessController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public ActionResult Index(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/AllowanceProcess/Index.cshtml");
        }

        public ActionResult Inquire([FromJsonOrForm] InquireInvoiceViewModel viewModel)
        {
            if (viewModel == null || !ModelState.IsValid)
            {
                return Json(new { result = false, message = ModelState.ErrorMessage() });
            }

            ViewBag.ViewModel = viewModel;

            viewModel.Status = viewModel.Status.GetEfficientString();
            switch(viewModel.Status)
            {
                case "Normal":
                    viewModel.Cancelled = false;
                    break;
                case "Voided":
                    viewModel.Cancelled = true;
                    break;
            }

            //DataLoadOptions ops = new DataLoadOptions();
            //ops.LoadWith<InvoiceAllowance>(i => i.InvoiceAllowanceBuyer);
            //ops.LoadWith<InvoiceAllowance>(i => i.InvoiceAllowanceSeller);
            //models.DataContext.LoadOptions = ops;

            var modelSource = new ModelSource<InvoiceAllowance>(models);

            var profile = HttpContext.GetUser();
            modelSource.Inquiry = viewModel.CreateAllowanceInquiry(profile);
            modelSource.BuildQuery();

            if(viewModel.Status == "ReadyToMIG")
            {
                var d0401Ready = models.GetTable<DataProcessQueue>()
                    //.Where(s => s.ProcessType == (int)Naming.InvoiceProcessType.G0401)
                    .Where(s => s.StepID == (int)Naming.InvoiceStepDefinition.待批次傳送);
                modelSource.Items = modelSource.Items.Where(a => d0401Ready.Any(d => d.DocID == a.AllowanceID));
            }

            if (viewModel.PageIndex.HasValue)
            {
                viewModel.PageIndex--;
                return View("~/Views/AllowanceProcess/Module/ItemList.cshtml", modelSource.Items);
            }
            else
            {
                viewModel.PageIndex = 0;
                return View("~/Views/AllowanceProcess/Module/QueryResult.cshtml", modelSource.Items);
            }
        }

        public ActionResult InquireReceivedB0101([FromJsonOrForm] InquireInvoiceViewModel viewModel)
        {
            if (viewModel == null || !ModelState.IsValid)
            {
                return Json(new { result = false, message = ModelState.ErrorMessage() });
            }

            bool hasPaging = viewModel.PageIndex.HasValue;
            ViewResult result = (ViewResult)Inquire(viewModel);
            IQueryable<InvoiceAllowance> items = (result.Model as IQueryable<InvoiceAllowance>)!;
            items = items.Where(i => i.CDS_Document.DataProcessQueue
                                .Where(q => q.ProcessType == (int)Naming.InvoiceProcessType.B0101)
                                .Where(s => s.StepID == (int)Naming.InvoiceStepDefinition.待接收)
                                .Any());
            viewModel.RecordCount = items?.Count();

            if (hasPaging)
            {
                return View("~/Views/AllowanceProcess/ReceivedB0101/AllowanceQueryList.cshtml", items);
            }
            else
            {
                return View("~/Views/AllowanceProcess/ReceivedB0101/AllowanceQueryResult.cshtml", items);
            }
        }

        public ActionResult InquireReceivedB0201([FromJsonOrForm] InquireInvoiceViewModel viewModel)
        {
            if (viewModel == null || !ModelState.IsValid)
            {
                return Json(new { result = false, message = ModelState.ErrorMessage() });
            }

            bool hasPaging = viewModel.PageIndex.HasValue;
            ViewResult result = (ViewResult)Inquire(viewModel);
            IQueryable<InvoiceAllowance> items = (result.Model as IQueryable<InvoiceAllowance>)!;
            var derivedItems = models!.GetTable<DerivedDocument>()
                                .Where(i => i.CDS_Document.DataProcessQueue
                                .Where(q => q.ProcessType == (int)Naming.InvoiceProcessType.B0201)
                                .Where(s => s.StepID == (int)Naming.InvoiceStepDefinition.待接收)
                                .Any());
            items = items.Join(derivedItems, i => i.AllowanceID, d => d.SourceID, (i, d) => i);
            viewModel.RecordCount = items?.Count();

            if (hasPaging)
            {
                return View("~/Views/AllowanceProcess/ReceivedB0101/AllowanceQueryList.cshtml", items);
            }
            else
            {
                return View("~/Views/AllowanceProcess/ReceivedB0201/AllowanceQueryResult.cshtml", items);
            }
        }

        public ActionResult InquireToVoid([FromJsonOrForm] InquireInvoiceViewModel viewModel)
        {
            if (viewModel == null || !ModelState.IsValid)
            {
                return Json(new { result = false, message = ModelState.ErrorMessage() });
            }

            ViewResult result = (ViewResult)Index(viewModel);
            viewModel.ActionTitle = "作廢折讓";
            viewModel.CommitAction = "VoidAllowance";
            result.ViewName = "~/Views/AllowanceProcess/Index.cshtml";
            ViewBag.ResultAction = "VoidAllowance";
            return result;
        }

        public ActionResult InvokeCommitAction([FromJsonOrForm] InquireInvoiceViewModel viewModel)
        {
            if (viewModel == null || !ModelState.IsValid)
            {
                return Json(new { result = false, message = ModelState.ErrorMessage() });
            }

            ViewBag.ViewModel = viewModel;
            return View("~/Views/AllowanceProcess/Module/InvokeCommitAction.cshtml");
        }

        public async Task<ActionResult> CreateXlsxAsync(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            ModelSource<InvoiceAllowance> modelSource = new ModelSource<InvoiceAllowance>(models);

            var profile = HttpContext.GetUser();
            modelSource.Inquiry = viewModel.CreateAllowanceInquiry(profile);
            modelSource.BuildQuery();

            //原本以匿名型別投影 + SqlDataAdapter.Fill 依資料行名稱對應；
            //投影內含字串插值（狀態）會讓 EF Core 轉為用戶端計算，產生的 SQL 不會加上
            //中文資料行別名，Fill 對應不到預先建立的資料行，匯出就會整張表空白。
            //改為直接載入實體後逐列填值。
            var items = modelSource.Items
                .Include(a => a.InvoiceAllowanceDetails)
                .Include(a => a.InvoiceAllowanceBuyer)
                .Include(a => a.InvoiceAllowanceSeller)
                .Include(a => a.InvoiceAllowanceCancellation)
                .OrderBy(i => i.AllowanceID);

            using (DataSet ds = new DataSet())
            {
                DataTable table = new DataTable("折讓資料明細");
                ds.Tables.Add(table);
                table.Columns.Add("折讓單號碼", typeof(String));
                table.Columns.Add("原發票號碼", typeof(String));
                table.Columns.Add("折讓日期", typeof(String));
                table.Columns.Add("客戶ID", typeof(String));
                table.Columns.Add("發票開立人", typeof(String));
                table.Columns.Add("開立人統編", typeof(String));
                table.Columns.Add("未稅金額", typeof(decimal));
                table.Columns.Add("稅額", typeof(decimal));
                table.Columns.Add("含稅金額", typeof(decimal));
                table.Columns.Add("買受人名稱", typeof(String));
                table.Columns.Add("買受人統編", typeof(String));
                table.Columns.Add("連絡人名稱", typeof(String));
                table.Columns.Add("連絡人地址", typeof(String));
                table.Columns.Add("買受人EMail", typeof(String));
                table.Columns.Add("狀態", typeof(String));
                table.Columns.Add("備註", typeof(String));

                foreach (var item in await items.ToListAsync())
                {
                    //原以 SQL GROUP BY (AllowanceID, InvoiceNo, Remark) 彙總明細，維持每張原發票一列。
                    var details = item.InvoiceAllowanceDetails
                        .GroupBy(d => new { d.InvoiceNo, d.Remark })
                        .Select(g => new
                        {
                            g.Key.InvoiceNo,
                            g.Key.Remark,
                            TotalAmt = g.Sum(v => v.Amount ?? 0m),
                            TotalTax = g.Sum(v => v.Tax ?? 0m),
                        });

                    foreach (var d in details)
                    {
                        DataRow r = table.NewRow();
                        r["折讓單號碼"] = item.AllowanceNumber;
                        r["原發票號碼"] = d.InvoiceNo;
                        r["折讓日期"] = $"{item.AllowanceDate:yyyy/MM/dd}";
                        r["客戶ID"] = item.InvoiceAllowanceBuyer?.CustomerID;
                        r["發票開立人"] = item.InvoiceAllowanceSeller?.CustomerName;
                        r["開立人統編"] = item.InvoiceAllowanceSeller?.ReceiptNo;
                        r["未稅金額"] = d.TotalAmt;
                        r["稅額"] = d.TotalTax;
                        r["含稅金額"] = (item.TotalAmount ?? 0m) + (item.TaxAmount ?? 0m);
                        r["買受人名稱"] = item.InvoiceAllowanceBuyer?.CustomerName;
                        r["買受人統編"] = item.InvoiceAllowanceBuyer?.ReceiptNo == "0000000000"
                                            ? "" : item.InvoiceAllowanceBuyer?.ReceiptNo;
                        r["連絡人名稱"] = item.InvoiceAllowanceBuyer?.ContactName;
                        r["連絡人地址"] = item.InvoiceAllowanceBuyer?.Address;
                        r["買受人EMail"] = item.InvoiceAllowanceBuyer?.EMail;
                        r["狀態"] = item.InvoiceAllowanceCancellation != null
                                        ? $"已作廢({item.InvoiceAllowanceCancellation.CancelDate:yyyy/MM/dd})" : "";
                        r["備註"] = d.Remark;

                        table.Rows.Add(r);
                    }
                }

                await ds.SaveAsExcelAsync(Response, String.Format("attachment;filename={0}", HttpUtility.UrlEncode("折讓資料明細.xlsx")));
            }

            return new EmptyResult();
        }

        public ActionResult CreateXlsx2021([FromJsonOrForm] InquireInvoiceViewModel viewModel)
        {
            if (viewModel == null || !ModelState.IsValid)
            {
                return Json(new { result = false, message = ModelState.ErrorMessage() });
            }

            ViewBag.ViewModel = viewModel;

            var profile = HttpContext.GetUser();
            ProcessRequest processItem = new ProcessRequest
            {
                Sender = profile?.Entity.UID,
                SubmitDate = DateTime.Now,
                ProcessStart = DateTime.Now,
                ResponsePath = System.IO.Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, Guid.NewGuid().ToString() + ".xlsx"),
            };
            models.GetTable<ProcessRequest>().Add(processItem);
            models.SubmitChanges();

            //原本以 models.GetCommand(items) 取出 SqlCommand 丟給背景執行緒填 DataTable；
            //EF Core 產生的 SQL 不會以匿名型別成員名稱（折讓單號碼…）當作資料行別名，
            //SqlDataAdapter.Fill 依名稱對應不到預先建立的資料行，結果就是整張表沒有資料。
            //改為與 InvoiceProcess/CreateXlsx2024 相同做法：背景作業自建 DbContext 重建查詢，逐列填值。
            int taskID = processItem.TaskID;
            String resultFile = processItem.ResponsePath;
            var (jobUID, jobRoleIndex) = profile.ForBackgroundWork();
            HttpContext.EnqueueBackgroundWork("AllowanceProcess/CreateXlsx2021",
                _ => saveAsExcel(taskID, resultFile, viewModel, jobUID, jobRoleIndex));

            return View("~/Views/Shared/Module/PromptCheckDownload.cshtml",
                    new AttachmentViewModel
                    {
                        TaskID = processItem.TaskID,
                        FileName = processItem.ResponsePath,
                        FileDownloadName = "折讓資料明細.xlsx",
                    });

        }

        //背景作業：不可沿用請求的 models／profile，改以 viewModel + uid/roleIndex 重建查詢。
        static void saveAsExcel(int taskID, String resultFile, InquireInvoiceViewModel viewModel, int? uid, int? roleIndex)
        {
            try
            {
                using (ModelSource<InvoiceAllowance> db = new ModelSource<InvoiceAllowance>())
                using (DataSet ds = new DataSet())
                {
                    UserProfileWrapper? profile = db.ReloadProfile(uid, roleIndex);

                    //與 Inquire 相同的查詢條件還原（含 Status 對應 Cancelled）。
                    viewModel.Status = viewModel.Status.GetEfficientString();
                    switch (viewModel.Status)
                    {
                        case "Normal":
                            viewModel.Cancelled = false;
                            break;
                        case "Voided":
                            viewModel.Cancelled = true;
                            break;
                    }

                    db.Inquiry = viewModel.CreateAllowanceInquiry(profile);
                    db.BuildQuery();

                    if (viewModel.Status == "ReadyToMIG")
                    {
                        var d0401Ready = db.GetTable<DataProcessQueue>()
                            .Where(s => s.StepID == (int)Naming.InvoiceStepDefinition.待批次傳送);
                        db.Items = db.Items.Where(a => d0401Ready.Any(d => d.DocID == a.AllowanceID));
                    }

                    //輸出每一列會用到的導覽屬性一次載入，避免逐筆 lazy loading。
                    IQueryable<InvoiceAllowance> items = db.Items
                        .Include(a => a.InvoiceAllowanceDetails)
                        .Include(a => a.InvoiceAllowanceBuyer)
                        .Include(a => a.InvoiceAllowanceSeller)
                        .Include(a => a.Currency)
                        .Include(a => a.InvoiceAllowanceCancellation);

                    Exception? exception = null;

                    try
                    {
                        DataTable table = new DataTable("折讓資料明細");
                        ds.Tables.Add(table);
                        table.Columns.Add("折讓單號碼", typeof(String));
                        table.Columns.Add("原發票號碼", typeof(String));
                        table.Columns.Add("折讓日期", typeof(String));
                        table.Columns.Add("客戶ID", typeof(String));
                        table.Columns.Add("發票開立人", typeof(String));
                        table.Columns.Add("開立人統編", typeof(String));
                        table.Columns.Add("未稅金額", typeof(decimal));
                        table.Columns.Add("稅額", typeof(decimal));
                        table.Columns.Add("含稅金額", typeof(decimal));
                        table.Columns.Add("幣別", typeof(String));
                        table.Columns.Add("買受人名稱", typeof(String));
                        table.Columns.Add("買受人統編", typeof(String));
                        table.Columns.Add("連絡人名稱", typeof(String));
                        table.Columns.Add("連絡人地址", typeof(String));
                        table.Columns.Add("買受人EMail", typeof(String));
                        table.Columns.Add("狀態", typeof(String));
                        table.Columns.Add("備註", typeof(String));

                        foreach (var item in items.OrderBy(a => a.AllowanceID).ToList())
                        {
                            //原本以 SQL GROUP BY (AllowanceID, InvoiceNo, Remark) 彙總明細，
                            //這裡於記憶體內以相同鍵值分組，維持每張原發票一列。
                            var details = item.InvoiceAllowanceDetails
                                .GroupBy(d => new { d.InvoiceNo, d.Remark })
                                .Select(g => new
                                {
                                    g.Key.InvoiceNo,
                                    g.Key.Remark,
                                    TotalAmt = g.Sum(v => v.Amount ?? 0m),
                                    TotalTax = g.Sum(v => v.Tax ?? 0m),
                                });

                            foreach (var d in details)
                            {
                                DataRow r = table.NewRow();
                                r["折讓單號碼"] = item.AllowanceNumber;
                                r["原發票號碼"] = d.InvoiceNo;
                                r["折讓日期"] = $"{item.AllowanceDate:yyyy/MM/dd}";
                                r["客戶ID"] = item.InvoiceAllowanceBuyer?.CustomerID;
                                r["發票開立人"] = item.InvoiceAllowanceSeller?.CustomerName;
                                r["開立人統編"] = item.InvoiceAllowanceSeller?.ReceiptNo;
                                r["未稅金額"] = d.TotalAmt;
                                r["稅額"] = d.TotalTax;
                                r["含稅金額"] = d.TotalAmt + d.TotalTax;
                                r["幣別"] = item.CurrencyID.HasValue ? item.Currency?.AbbrevName : DBNull.Value;
                                r["買受人名稱"] = item.InvoiceAllowanceBuyer?.CustomerName;
                                r["買受人統編"] = item.InvoiceAllowanceBuyer?.ReceiptNo == "0000000000"
                                                    ? "" : item.InvoiceAllowanceBuyer?.ReceiptNo;
                                r["連絡人名稱"] = item.InvoiceAllowanceBuyer?.ContactName;
                                r["連絡人地址"] = item.InvoiceAllowanceBuyer?.Address;
                                r["買受人EMail"] = item.InvoiceAllowanceBuyer?.EMail;
                                r["狀態"] = item.InvoiceAllowanceCancellation != null
                                                ? $"已作廢({item.InvoiceAllowanceCancellation.CancelDate:yyyy/MM/dd})" : "";
                                r["備註"] = d.Remark;

                                table.Rows.Add(r);
                            }
                        }

                        using (var xls = ds.ConvertToExcel())
                        {
                            xls.SaveAs(resultFile);
                        }
                        CommonLib.Core.Utility.FileLogger.Logger.Debug($"save excel file: {resultFile}");
                    }
                    catch (Exception ex)
                    {
                        CommonLib.Core.Utility.FileLogger.Logger.Error($"{ex}\r\n{items}");
                        exception = ex;
                    }

                    ProcessRequest? taskItem = db.GetTable<ProcessRequest>()
                                    .Where(t => t.TaskID == taskID).FirstOrDefault();

                    if (taskItem != null)
                    {
                        if (exception != null)
                        {
                            var logItem = new ExceptionLog
                            {
                                DataContent = exception.Message,
                                LogTime = DateTime.Now,
                            };
                            db.GetTable<ExceptionLog>().Add(logItem);
                            db.SubmitChanges();

                            taskItem.LogID = logItem.LogID;
                        }

                        taskItem.ProcessComplete = DateTime.Now;
                        db.SubmitChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
            }
        }


        public ActionResult VoidAllowance([FromJsonOrForm] InquireInvoiceViewModel viewModel)
        {
            if (viewModel == null || !ModelState.IsValid)
            {
                return Json(new { result = false, message = ModelState.ErrorMessage() });
            }

            ViewBag.ViewModel = viewModel;
            var chkItem = viewModel.ChkItem;
            if (chkItem != null && chkItem.Count() > 0)
            {
                InvoiceManager mgr = new InvoiceManager(models);
                mgr.VoidAllowance(chkItem);
                if (mgr.EventItems_Allowance != null && mgr.EventItems_Allowance.Count > 0)
                {
                    ViewBag.Message = "下列折讓已作廢完成!!\r\n" + String.Join("\r\n", mgr.EventItems_Allowance.Select(i => i.AllowanceNumber));
                    //EIVONotificationFactory.Notify();
                }
                return View("~/Views/Shared/AlertMessage.cshtml");
            }
            else
            {
                ViewBag.Message = "請選擇作廢資料!!";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }

        }

        public async Task<ActionResult> PrintAsync([FromJsonOrForm] InquireInvoiceViewModel dataModel, [FromJsonOrForm] RenderStyleViewModel viewModel)
        {
            if(dataModel == null)
            {
                dataModel = await PrepareViewModelAsync<InquireInvoiceViewModel>();
            }

            if(viewModel == null)
            {
                viewModel = await PrepareViewModelAsync<RenderStyleViewModel>();
            }

            viewModel ??= new RenderStyleViewModel();
            ViewBag.ViewModel = viewModel;

            var profile = HttpContext.GetUser();
            if (dataModel.ChkItem?.Count > 0 && profile.EnqueueDocumentPrint(models, dataModel.ChkItem))
            {
                return View("~/Views/AllowanceProcess/Module/PrintResult.cshtml");
            }
            else
                return Json(new { result = false, message = "資料已列印請重新選擇!!" });
        }

        public async Task<ActionResult> IssueAllowanceNoticeAsync([FromJsonOrForm] InquireInvoiceViewModel viewModel)
        {
            if(viewModel == null)
            {
                viewModel = await PrepareViewModelAsync<InquireInvoiceViewModel>();
                ModelState.Clear();
            }

            var chkItem = viewModel.ChkItem;
            if (chkItem != null && chkItem.Count() > 0)
            {
                if (viewModel.Cancelled == true)
                {
                    chkItem.NotifyIssuedAllowanceCancellation();
                }
                else
                {
                    chkItem.NotifyIssuedAllowance();
                }
                ViewBag.Message = "Email通知已重送!!";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }
            else
            {
                ViewBag.Message = "請選擇重送資料!!";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }

        }

        public async Task<ActionResult> TransferToMIGAsync()
        {
            QueryViewModel viewModel = await PrepareViewModelAsync<QueryViewModel>();
            ViewBag.ViewModel = viewModel;

            var chkItem = viewModel.ChkItem;
            if (chkItem != null && chkItem.Count() > 0)
            {
                foreach(var id in chkItem)
                {
                    models!.ExecuteCommand(@"UPDATE [proc].DataProcessQueue
                        SET        StepID = {0}
                        WHERE   (DocID = {1}) AND (StepID = {2})",
                                                (int)Naming.InvoiceStepDefinition.已開立,
                                                id,
                                                (int)Naming.InvoiceStepDefinition.待批次傳送);
                }

                ViewBag.Message = "折讓資料已排定送出至財政部雲端。";
                return View("~/Views/AllowanceProcess/Module/RedoQuery.cshtml");
            }
            else
            {
                ViewBag.Message = "請選擇折讓資料!!";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }

        }

        public async Task<ActionResult> DeleteAllowanceAsync([FromJsonOrForm] InquireInvoiceViewModel viewModel)
        {
            if (viewModel == null)
            {
                viewModel = await PrepareViewModelAsync<InquireInvoiceViewModel>();
                ModelState.Clear();
            }

            var chkItem = viewModel.ChkItem;
            if (chkItem != null && chkItem.Count() > 0)
            {
                foreach (var id in chkItem)
                {
                    models.ExecuteCommand(@"DELETE FROM CDS_Document
                        FROM     CDS_Document INNER JOIN
                                        InvoiceAllowance ON CDS_Document.DocID = InvoiceAllowance.AllowanceID
                        WHERE   (CDS_Document.DocID = {0})", id);
                }

                ViewBag.Message = "折讓資料已刪除。";
                return View("~/Views/AllowanceProcess/Module/RedoQuery.cshtml");
            }
            else
            {
                ViewBag.Message = "請選擇折讓資料!!";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }

        }
        public async Task<ActionResult> CommitReceivedB0101Async([FromJsonOrForm] QueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            if (viewModel == null)
            {
                viewModel = await PrepareViewModelAsync<QueryViewModel>();
                ModelState.Clear();
            }

            bool? hasError = null;
            if (viewModel.KeyItems?.Any() == true)
            {
                InvoiceHandler handler = new InvoiceHandler(models!);
                var chkItem = viewModel.KeyItems.Select(k => k.DecryptKeyValue());

                foreach (var id in chkItem)
                {
                    InvoiceAllowance? item = models!.GetTable<InvoiceAllowance>().Where(i => i.AllowanceID == id)
                        .FirstOrDefault();

                    if (item == null)
                    {
                        hasError = true;
                        continue;
                    }

                    var queueItem = InvoiceHandler.GetReadyItem(models!, item.CDS_Document, Naming.InvoiceStepDefinition.待接收, Naming.InvoiceProcessType.B0101);
                    if (queueItem == null)
                    {
                        hasError = true;
                        continue;
                    }

                    handler.PrepareStep(queueItem, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.B0102);

                    if (!hasError.HasValue)
                    {
                        hasError = false;
                    }
                }
            }

            if (!hasError.HasValue)
            {
                return Json(new { result = false, message = "請選擇接收資料!!" });
            }
            else if (hasError == true)
            {
                return Json(new { result = true, message = "部份資料未完成!!" });
            }
            else
            {
                return Json(new
                {
                    result = true,
                    message = "發票已確認接收!!"
                });
            }
        }

        public async Task<ActionResult> CommitReceivedB0201Async([FromJsonOrForm] QueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            if (viewModel == null)
            {
                viewModel = await PrepareViewModelAsync<QueryViewModel>();
                ModelState.Clear();
            }

            bool? hasError = null;
            if (viewModel.KeyItems?.Any() == true)
            {
                InvoiceHandler handler = new InvoiceHandler(models!);
                var chkItem = viewModel.KeyItems.Select(k => k.DecryptKeyValue());

                foreach (var id in chkItem)
                {
                    InvoiceAllowance? item = models!.GetTable<InvoiceAllowance>().Where(i => i.AllowanceID == id)
                        .FirstOrDefault();

                    if (item == null)
                    {
                        hasError = true;
                        continue;
                    }

                    CDS_Document? doc;
                    if (item == null || (doc = item.CDS_Document.ChildDocument.FirstOrDefault()?.CDS_Document) == null)
                    {
                        hasError = true;
                        continue;
                    }

                    var queueItem = InvoiceHandler.GetReadyItem(models!, doc, Naming.InvoiceStepDefinition.待接收, Naming.InvoiceProcessType.B0201);
                    if (queueItem == null)
                    {
                        hasError = true;
                        continue;
                    }

                    handler.PrepareStep(queueItem, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.B0202);

                    if (!hasError.HasValue)
                    {
                        hasError = false;
                    }
                }
            }

            if (!hasError.HasValue)
            {
                return Json(new { result = false, message = "請選擇接收資料!!" });
            }
            else if (hasError == true)
            {
                return Json(new { result = true, message = "部份資料未完成!!" });
            }
            else
            {
                return Json(new
                {
                    result = true,
                    message = "發票已確認接收!!"
                });
            }
        }

    }
}
