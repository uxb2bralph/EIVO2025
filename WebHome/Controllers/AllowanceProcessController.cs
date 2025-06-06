﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using Microsoft.AspNetCore.Mvc;


using ClosedXML.Excel;
using WebHome.Helper;
using WebHome.Models;
using ModelCore.Models.ViewModel;
using WebHome.Models.ViewModel;
using WebHome.Properties;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Helper;

using ModelCore.Helper;
using CommonLib.Utility;
using ModelCore.InvoiceManagement;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using CommonLib.DataAccess;
using ModelCore.Models;

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

        public ActionResult Inquire(InquireInvoiceViewModel viewModel)
        {

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

            DataLoadOptions ops = new DataLoadOptions();
            ops.LoadWith<InvoiceAllowance>(i => i.InvoiceAllowanceBuyer);
            ops.LoadWith<InvoiceAllowance>(i => i.InvoiceAllowanceSeller);
            models.DataContext.LoadOptions = ops;

            var modelSource = new ModelSource<InvoiceAllowance>(models);

            var profile = HttpContext.GetUser();
            modelSource.Inquiry = viewModel.CreateAllowanceInquiry(profile);
            modelSource.BuildQuery();

            if(viewModel.Status == "ReadyToMIG")
            {
                var d0401Ready = models.GetTable<D0401DispatchQueue>().Where(s => s.StepID == (int)Naming.InvoiceStepDefinition.待批次傳送);
                var b0401Ready = models.GetTable<B0401DispatchQueue>().Where(s => s.StepID == (int)Naming.InvoiceStepDefinition.待批次傳送);
                modelSource.Items = modelSource.Items.Where(a => d0401Ready.Any(d => d.DocID == a.AllowanceID)
                    || b0401Ready.Any(d => d.DocID == a.AllowanceID));
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

        public ActionResult InquireToVoid(InquireInvoiceViewModel viewModel)
        {
            ViewResult result = (ViewResult)Index(viewModel);
            viewModel.ActionTitle = "作廢折讓";
            viewModel.CommitAction = "VoidAllowance";
            result.ViewName = "~/Views/AllowanceProcess/Index.cshtml";
            ViewBag.ResultAction = "VoidAllowance";
            return result;
        }

        public ActionResult InvokeCommitAction(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/AllowanceProcess/Module/InvokeCommitAction.cshtml");
        }

        public async Task<ActionResult> CreateXlsxAsync(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            DataLoadOptions ops = new DataLoadOptions();
            ops.LoadWith<InvoiceAllowance>(i => i.InvoiceAllowanceBuyer);
            ops.LoadWith<InvoiceAllowance>(i => i.InvoiceAllowanceSeller);
            models.DataContext.LoadOptions = ops;

            ModelSource<InvoiceAllowance> modelSource = new ModelSource<InvoiceAllowance>(models);

            var profile = HttpContext.GetUser();
            modelSource.Inquiry = viewModel.CreateAllowanceInquiry(profile);
            modelSource.BuildQuery();

            var prodItems = models.GetTable<InvoiceAllowanceDetail>()
                                .Join(models.GetTable<InvoiceAllowanceItem>(), d => d.ItemID, p => p.ItemID, (d, p) => new { d.AllowanceID, p.InvoiceNo, p.Amount, p.Tax, p.Remark })
                                .GroupBy(a => new { a.AllowanceID, a.InvoiceNo, a.Remark })
                                .Select(g => new { g.Key, TotalAmt = g.Sum(v => v.Amount), TotalTax = g.Sum(v => v.Tax) });

            var items = modelSource.Items
                .OrderBy(i => i.AllowanceID)
                .Join(prodItems, i => i.AllowanceID, g => g.Key.AllowanceID, (i, g) => new
                {
                    折讓單號碼 = i.AllowanceNumber,
                    原發票號碼 = g.Key.InvoiceNo,
                    折讓日期 = i.AllowanceDate,
                    客戶ID = i.InvoiceAllowanceBuyer.CustomerID,
                    發票開立人 = i.InvoiceAllowanceSeller.CustomerName,
                    開立人統編 = i.InvoiceAllowanceSeller.ReceiptNo,
                    未稅金額 = g.TotalAmt,
                    稅額 = g.TotalTax,
                    含稅金額 = i.TotalAmount + i.TaxAmount,
                    買受人名稱 = i.InvoiceAllowanceBuyer.CustomerName,
                    買受人統編 = i.InvoiceAllowanceBuyer.ReceiptNo,
                    連絡人名稱 = i.InvoiceAllowanceBuyer.ContactName,
                    連絡人地址 = i.InvoiceAllowanceBuyer.Address,
                    買受人EMail = i.InvoiceAllowanceBuyer.EMail,
                    備註 = g.Key.Remark
                });

            using (DataSet ds = new DataSet())
            {
                DataTable table = new DataTable("折讓資料明細");
                ds.Tables.Add(table);
                table.Columns.Add("折讓單號碼");
                table.Columns.Add("原發票號碼");
                table.Columns.Add("折讓日期");
                table.Columns.Add("客戶ID");
                table.Columns.Add("發票開立人");
                table.Columns.Add("開立人統編");
                table.Columns.Add("未稅金額");
                table.Columns.Add("稅額");
                table.Columns.Add("含稅金額");
                table.Columns.Add("買受人名稱");
                table.Columns.Add("買受人統編");
                table.Columns.Add("連絡人名稱");
                table.Columns.Add("連絡人地址");
                table.Columns.Add("買受人EMail");
                table.Columns.Add("備註");

                DataSource.GetDataSetResult(items, table);
                foreach (var r in table.Select("買受人統編 = '0000000000'"))
                {
                    r["買受人統編"] = "";
                }

                await ds.SaveAsExcelAsync(Response, String.Format("attachment;filename={0}", HttpUtility.UrlEncode("折讓資料明細.xlsx")));
            }

            return new EmptyResult();
        }

        public ActionResult CreateXlsx2021(InquireInvoiceViewModel viewModel)
        {
            ViewResult result = (ViewResult)Inquire(viewModel);
            IQueryable<InvoiceAllowance> allowanceItems = result.Model as IQueryable<InvoiceAllowance>;

            var prodItems = models.GetTable<InvoiceAllowanceDetail>()
                                .Join(models.GetTable<InvoiceAllowanceItem>(), d => d.ItemID, p => p.ItemID, (d, p) => new { d.AllowanceID, p.InvoiceNo, p.Amount, p.Tax, p.Remark })
                                .GroupBy(a => new { a.AllowanceID, a.InvoiceNo, a.Remark })
                                .Select(g => new { g.Key, TotalAmt = g.Sum(v => v.Amount), TotalTax = g.Sum(v => v.Tax) });

            var items = allowanceItems
                .OrderBy(i => i.AllowanceID)
                .Join(prodItems, i => i.AllowanceID, g => g.Key.AllowanceID, (i, g) => new
                {
                    折讓單號碼 = i.AllowanceNumber,
                    原發票號碼 = g.Key.InvoiceNo,
                    折讓日期 = i.AllowanceDate,
                    客戶ID = i.InvoiceAllowanceBuyer.CustomerID,
                    發票開立人 = i.InvoiceAllowanceSeller.CustomerName,
                    開立人統編 = i.InvoiceAllowanceSeller.ReceiptNo,
                    未稅金額 = g.TotalAmt,
                    稅額 = g.TotalTax,
                    含稅金額 = g.TotalAmt + g.TotalTax, //i.TotalAmount + i.TaxAmount,
                    幣別 = i.CurrencyID.HasValue ? i.CurrencyType.AbbrevName : null,
                    買受人名稱 = i.InvoiceAllowanceBuyer.CustomerName,
                    買受人統編 = i.InvoiceAllowanceBuyer.ReceiptNo,
                    連絡人名稱 = i.InvoiceAllowanceBuyer.ContactName,
                    連絡人地址 = i.InvoiceAllowanceBuyer.Address,
                    買受人EMail = i.InvoiceAllowanceBuyer.EMail,
                    備註 = g.Key.Remark
                });


            ProcessRequest processItem = new ProcessRequest
            {
                Sender = HttpContext.GetUser()?.UID,
                SubmitDate = DateTime.Now,
                ProcessStart = DateTime.Now,
                ResponsePath = System.IO.Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, Guid.NewGuid().ToString() + ".xlsx"),
            };
            models.GetTable<ProcessRequest>().InsertOnSubmit(processItem);
            models.SubmitChanges();

            SqlCommand sqlCmd = (SqlCommand)models.GetCommand(items);
            saveAsExcel(processItem.TaskID, processItem.ResponsePath, sqlCmd, viewModel.Attachment != 0);

            return View("~/Views/Shared/Module/PromptCheckDownload.cshtml",
                    new AttachmentViewModel
                    {
                        TaskID = processItem.TaskID,
                        FileName = processItem.ResponsePath,
                        FileDownloadName = "折讓資料明細.xlsx",
                    });

        }

        static void saveAsExcel(int taskID, String resultFile, SqlCommand sqlCmd, bool hasAttachment)
        {
            Task.Run(() =>
            {
                try
                {
                    using (DataSet ds = new DataSet())
                    {
                        DataTable table = new DataTable("折讓資料明細");
                        ds.Tables.Add(table);
                        table.Columns.Add("折讓單號碼");
                        table.Columns.Add("原發票號碼");
                        table.Columns.Add("折讓日期");
                        table.Columns.Add("客戶ID");
                        table.Columns.Add("發票開立人");
                        table.Columns.Add("開立人統編");
                        table.Columns.Add("未稅金額");
                        table.Columns.Add("稅額");
                        table.Columns.Add("含稅金額");
                        table.Columns.Add("幣別");
                        table.Columns.Add("買受人名稱");
                        table.Columns.Add("買受人統編");
                        table.Columns.Add("連絡人名稱");
                        table.Columns.Add("連絡人地址");
                        table.Columns.Add("買受人EMail");
                        table.Columns.Add("備註");

                        using (ModelSource<InvoiceItem> db = new ModelSource<InvoiceItem>())
                        {
                            Exception exception = null;

                            try
                            {
                                CommonLib.Core.Utility.FileLogger.Logger.Debug($"save excel sql cmd: {sqlCmd.CommandText}");
                                db.GetDataSetResult(sqlCmd, table);
                                foreach (var r in table.Select("買受人統編 = '0000000000'"))
                                {
                                    r["買受人統編"] = "";
                                }

                                using (var xls = ds.ConvertToExcel())
                                {
                                    xls.SaveAs(resultFile);
                                }
                                CommonLib.Core.Utility.FileLogger.Logger.Debug($"save excel file: {resultFile}");
                            }
                            catch (Exception ex)
                            {
                                CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                                exception = ex;
                            }

                            ProcessRequest taskItem = db.GetTable<ProcessRequest>()
                                            .Where(t => t.TaskID == taskID).FirstOrDefault();

                            if (taskItem != null)
                            {
                                if (exception != null)
                                {
                                    taskItem.ExceptionLog = new ExceptionLog
                                    {
                                        DataContent = exception.Message
                                    };
                                }
                                taskItem.ProcessComplete = DateTime.Now;
                                db.SubmitChanges();
                            }

                        }

                    }

                }
                catch (Exception ex)
                {
                    CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                }
            });

        }


        public ActionResult VoidAllowance(int?[] chkItem, Naming.InvoiceProcessType? proceType)
        {
            if (chkItem != null && chkItem.Count() > 0)
            {
                InvoiceManager mgr = new InvoiceManager(models);
                mgr.VoidAllowance(chkItem);
                if (mgr.EventItems_Allowance != null && mgr.EventItems_Allowance.Count > 0)
                {
                    ViewBag.Message = "下列折讓已作廢完成!!\r\n" + String.Join("\r\n", mgr.EventItems_Allowance.Select(i => i.AllowanceNumber));
                    EIVOTurnkeyFactory.Notify();
                }
                return View("~/Views/Shared/AlertMessage.cshtml");
            }
            else
            {
                ViewBag.Message = "請選擇作廢資料!!";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }

        }

        public ActionResult Print(int[] chkItem, RenderStyleViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            var profile = HttpContext.GetUser();
            if (profile.EnqueueDocumentPrint(models, chkItem))
            {
                return View("~/Views/AllowanceProcess/Module/PrintResult.cshtml");
            }
            else
                return Json(new { result = false, message = "資料已列印請重新選擇!!" });
        }

        public ActionResult IssueAllowanceNotice(int[] chkItem,bool? cancellation)
        {
            if (chkItem != null && chkItem.Count() > 0)
            {
                if (cancellation == true)
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

        public ActionResult TransferToMIG(int?[] chkItem)
        {
            if (chkItem != null && chkItem.Count() > 0)
            {
                foreach(var id in chkItem)
                {
                    if(models.ExecuteCommand(@"UPDATE [proc].D0401DispatchQueue
                        SET        StepID = {0}
                        WHERE   (DocID = {1}) AND (StepID = {2})",
                            (int)Naming.InvoiceStepDefinition.已開立,
                            id,
                            (int)Naming.InvoiceStepDefinition.待批次傳送) == 0)
                    {
                        models.ExecuteCommand(@"UPDATE [proc].B0401DispatchQueue
                        SET        StepID = {0}
                        WHERE   (DocID = {1}) AND (StepID = {2})",
                            (int)Naming.InvoiceStepDefinition.已開立,
                            id,
                            (int)Naming.InvoiceStepDefinition.待批次傳送);
                    }
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

        public ActionResult DeleteAllowance(int?[] chkItem)
        {
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

        //protected override void HandleUnknownAction(string actionName)
        //{
        //    if (!String.IsNullOrEmpty(actionName))
        //    {
        //        this.View(actionName).ExecuteResult(this.ControllerContext);
        //    }
        //}
    }
}
