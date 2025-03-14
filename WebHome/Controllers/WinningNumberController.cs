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
using Microsoft.AspNetCore.Authorization;
using ModelCore.Helper;
using CommonLib.Core.Utility;
using CommonLib.DataAccess;

namespace WebHome.Controllers
{
    [Authorize]
    public class WinningNumberController : SampleController<InvoiceItem>
    {
        public WinningNumberController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: WinningNumber
        public ActionResult Index()
        {
            ViewBag.InquiryView = "~/Views/WinningNumber/WinningNoQuery.cshtml";
            return View("~/Views/InvoiceProcess/Index.cshtml");
        }

        public ActionResult Inquire(InquireNoIntervalViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (!viewModel.Year.HasValue)
            {
                ModelState.AddModelError("Year", "請選擇年份!!");
            }

            if (!viewModel.PeriodNo.HasValue)
            {
                ModelState.AddModelError("PeriodNo", "請選擇期別!!");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ModelState = ModelState;
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            var items = models.GetTable<UniformInvoiceWinningNumber>().Where(w => w.Year == viewModel.Year && w.Period == viewModel.PeriodNo);

            return View("~/Views/WinningNumber/Module/QueryResult.ascx", items);
        }

        public ActionResult EditItem(int? id)
        {
            ViewResult result = (ViewResult)DataItem(id);
            UniformInvoiceWinningNumber model = result.Model as UniformInvoiceWinningNumber;
            if (model != null)
            {
                result.ViewName = "~/Views/WinningNumber/Module/EditItem.ascx";
            }
            return result;
        }

        public ActionResult DeleteItem(int? id)
        {
            var item = models.DeleteAny<UniformInvoiceWinningNumber>(d => d.WinningID == id);

            if (item == null)
            {
                return Json(new { result = false, message = "中獎號碼資料錯誤!!" });
            }

            if (item.Rank == (int)Naming.WinningPrizeType.頭獎)
            {
                models.DeleteAll<UniformInvoiceWinningNumber>(u => u.Period == item.Period && u.Year == item.Year && u.WinningNO == item.WinningNO.Substring(1));    //二獎
                models.DeleteAll<UniformInvoiceWinningNumber>(u => u.Period == item.Period && u.Year == item.Year && u.WinningNO == item.WinningNO.Substring(2));    //三獎
                models.DeleteAll<UniformInvoiceWinningNumber>(u => u.Period == item.Period && u.Year == item.Year && u.WinningNO == item.WinningNO.Substring(3));    //肆獎
                models.DeleteAll<UniformInvoiceWinningNumber>(u => u.Period == item.Period && u.Year == item.Year && u.WinningNO == item.WinningNO.Substring(4));    //伍獎
                models.DeleteAll<UniformInvoiceWinningNumber>(u => u.Period == item.Period && u.Year == item.Year && u.WinningNO == item.WinningNO.Substring(5));    //陸獎
            }

            return Json(new { result = true });

        }

        public ActionResult DataItem(int? id)
        {
            var item = models.GetTable<UniformInvoiceWinningNumber>()
                .Where(d => d.WinningID == id)
                .FirstOrDefault();

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "中獎號碼資料錯誤!!");
            }

            return View("~/Views/WinningNumber/Module/DataItem.ascx", item);

        }

        public ActionResult CommitItem(WinningNumberViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            viewModel.WinningNo = viewModel.WinningNo.GetEfficientString();
            if (viewModel.Rank == (int)Naming.EditableWinningPrizeType.特別獎
                || viewModel.Rank == (int)Naming.EditableWinningPrizeType.特獎
                || viewModel.Rank == (int)Naming.EditableWinningPrizeType.頭獎)
            {
                if (viewModel.WinningNo == null || !Regex.IsMatch(viewModel.WinningNo, "^[0-9]{8}$"))
                {
                    ModelState.AddModelError("WinningNo", "中獎號碼為8碼數字!!");
                }
            }
            else if (viewModel.Rank == (int)Naming.EditableWinningPrizeType.增開六獎)
            {
                if (viewModel.WinningNo == null || !Regex.IsMatch(viewModel.WinningNo, "^[0-9]{3}$"))
                {
                    ModelState.AddModelError("WinningNo", "中獎號碼為3碼數字!!");
                }
            }
            else
            {
                ModelState.AddModelError("Rank", "獎別錯誤!!");
            }

            var table = models.GetTable<UniformInvoiceWinningNumber>();
            var model = table.Where(t => t.WinningID == viewModel.WinningID).FirstOrDefault();
            if (model == null)
            {
                if (!viewModel.Period.HasValue || viewModel.Period > 6 || viewModel.Period < 1)
                {
                    ViewBag.Message = "請選擇期別!!";
                    return View("~/Views/Shared/AlertMessage.cshtml");
                }
                else if (!viewModel.Year.HasValue)
                {
                    ViewBag.Message = "請選擇年份!!";
                    return View("~/Views/Shared/AlertMessage.cshtml");
                }
                else
                {
                    if (table.Any(t => t.Year == viewModel.Year && t.WinningNO == viewModel.WinningNo && t.Period == viewModel.Period))
                    {
                        ModelState.AddModelError("WinningNo", "中獎號碼重複!!");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                if (model != null)
                    ViewBag.DataRole = "edit";
                else
                    ViewBag.DataRole = "add";

                ViewBag.ModelState = ModelState;
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            if (model == null)
            {
                model = new UniformInvoiceWinningNumber
                {
                    Year = viewModel.Year.Value,
                    Period = viewModel.Period.Value
                };
                table.InsertOnSubmit(model);
            }
            else
            {
                if (model.Rank == (int)Naming.WinningPrizeType.頭獎)
                {
                    models.DeleteAllOnSubmit<UniformInvoiceWinningNumber>(u => u.Period == model.Period && u.Year == model.Year && u.WinningNO == model.WinningNO.Substring(1));    //二獎
                    models.DeleteAllOnSubmit<UniformInvoiceWinningNumber>(u => u.Period == model.Period && u.Year == model.Year && u.WinningNO == model.WinningNO.Substring(2));    //三獎
                    models.DeleteAllOnSubmit<UniformInvoiceWinningNumber>(u => u.Period == model.Period && u.Year == model.Year && u.WinningNO == model.WinningNO.Substring(3));    //肆獎
                    models.DeleteAllOnSubmit<UniformInvoiceWinningNumber>(u => u.Period == model.Period && u.Year == model.Year && u.WinningNO == model.WinningNO.Substring(4));    //伍獎
                    models.DeleteAllOnSubmit<UniformInvoiceWinningNumber>(u => u.Period == model.Period && u.Year == model.Year && u.WinningNO == model.WinningNO.Substring(5));    //陸獎
                }
            }

            model.WinningNO = viewModel.WinningNo;
            model.Year = viewModel.Year.Value;
            model.Period = viewModel.Period.Value;
            model.Rank = viewModel.Rank.Value;
            model.PrizeType = ((Naming.WinningPrizeType)viewModel.Rank.Value).ToString();
            model.Bonus = Naming.WinningBonus[viewModel.Rank.Value];

            if (model.Rank == (int)Naming.WinningPrizeType.頭獎)
            {
                createWinningNo(table, model.Year, model.Period, viewModel.WinningNo.Substring(1), Naming.WinningPrizeType.二獎);
                createWinningNo(table, model.Year, model.Period, viewModel.WinningNo.Substring(2), Naming.WinningPrizeType.三獎);
                createWinningNo(table, model.Year, model.Period, viewModel.WinningNo.Substring(3), Naming.WinningPrizeType.四獎);
                createWinningNo(table, model.Year, model.Period, viewModel.WinningNo.Substring(4), Naming.WinningPrizeType.五獎);
                createWinningNo(table, model.Year, model.Period, viewModel.WinningNo.Substring(5), Naming.WinningPrizeType.六獎);
                models.SubmitChanges();
                return View("~/Views/WinningNumber/Module/QueryRequired.ascx", model);
            }
            else
            {
                models.SubmitChanges();
                return View("~/Views/WinningNumber/Module/DataItem.ascx", model);
            }

        }

        private void createWinningNo(Table<UniformInvoiceWinningNumber> table, int year, int period, string winningNo, Naming.WinningPrizeType prizeType)
        {
            table.InsertOnSubmit(new UniformInvoiceWinningNumber
            {
                Year = year,
                Period = period,
                WinningNO = winningNo,
                PrizeType = prizeType.ToString(),
                Rank = (int)prizeType,
                Bonus = Naming.WinningBonus[(int)prizeType]
            });
        }

        public ActionResult MatchWinningInvoiceNo(InquireNoIntervalViewModel viewModel)
        {
            ViewResult result = (ViewResult)Inquire(viewModel);
            IQueryable<UniformInvoiceWinningNumber> items = result.Model as IQueryable<UniformInvoiceWinningNumber>;
            if (items != null && items.Count() > 0)
            {
                models.DataContext.MatchWinningInvoiceNo(viewModel.Year, viewModel.PeriodNo);

                var invoiceItems = models.PromptWinningInvoiceForNotification(viewModel.Year ?? -1, viewModel.PeriodNo?? -1);
                invoiceItems.Select(i => i.InvoiceID).NotifyWinningInvoice(false);

                //SharedFunction.doSendSMSMessage(new SharedFunction._MailQueryState { setYear = viewModel.Year.Value, setPeriod = viewModel.PeriodNo.Value });

                ViewBag.Message = "對獎作業完成!!";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }
            else
            {
                return result;
            }
        }

        public ActionResult ClearWinningInvoiceNo(InquireNoIntervalViewModel viewModel)
        {
            ViewResult result = (ViewResult)Inquire(viewModel);
            IQueryable<UniformInvoiceWinningNumber> items = result.Model as IQueryable<UniformInvoiceWinningNumber>;
            if (items != null && items.Count() > 0)
            {
                models.ExecuteCommand(@"	
                    DELETE FROM InvoiceWinningNumber
	                FROM     UniformInvoiceWinningNumber INNER JOIN
					                InvoiceWinningNumber ON UniformInvoiceWinningNumber.WinningID = InvoiceWinningNumber.WinningID
	                WHERE   (UniformInvoiceWinningNumber.Period = {0}) AND (UniformInvoiceWinningNumber.Year = {1})", viewModel.PeriodNo, viewModel.Year);
                ViewBag.Message = "中獎發票已清除完成!!";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }
            else
            {
                return result;
            }
        }

        public ActionResult UploadWinningNo(InquireNoIntervalViewModel viewModel, IEnumerable<IFormFile> excelFile)
        {
            ViewBag.ViewModel = viewModel;

            if (excelFile == null || excelFile.Count() < 1)
            {
                return Json(new { result = false, message = "未選取檔案或檔案上傳失敗!!" });
            }

            if (excelFile.Count() != 1)
            {
                return Json(new { result = false, message = "請上傳單一檔案!!" });
            }

            var file = excelFile.First();
            String fileName = Path.Combine(Logger.LogDailyPath, $"{DateTime.Now.Ticks}_{Path.GetFileName(file.FileName)}");
            file.SaveAs(fileName);


            ProcessRequest processItem = new ProcessRequest
            {
                Sender = HttpContext.GetUser()?.UID,
                SubmitDate = DateTime.Now,
                ProcessStart = DateTime.Now,
                RequestPath = fileName,
                ResponsePath = System.IO.Path.Combine(Logger.LogDailyPath, Guid.NewGuid().ToString() + ".xlsx"),
            };
            models.GetTable<ProcessRequest>().InsertOnSubmit(processItem);
            models.SubmitChanges();

            ProcessWinningNoExcel(processItem.TaskID, processItem.ResponsePath, fileName);

            return View("~/Views/Shared/Module/PromptCheckDownload.cshtml",
                    new AttachmentViewModel
                    {
                        TaskID = processItem.TaskID,
                        FileName = processItem.ResponsePath,
                        FileDownloadName = "中獎發票回應.xlsx",
                    });

        }

        static void ProcessWinningNoExcel(int taskID, String resultFile, String excelPath)
        {
            Task.Run(() =>
            {
                try
                {

                    using (ModelSource<InvoiceItem> db = new ModelSource<InvoiceItem>())
                    {
                        using (DataSet ds = excelPath.ImportExcelXLS())
                        {
                            Exception exception = null;
                            if (ds.Tables.Count > 0)
                            {
                                List<int> winningID = new List<int>();
                                DataTable table = ds.Tables[0];
                                table.Columns.Add(new DataColumn("處理狀態", typeof(String)));
                                var statusIdx = table.Columns.Count - 1;

                                IEnumerable<DataRow> rows = table.Rows.Cast<DataRow>();
                                IEnumerable<DataRow> assumedRows = rows
                                        .Where(r => !r.IsNull(0))
                                        .Where(r => !r.IsNull(1))
                                        .Where(r => !r.IsNull(2))
                                        .Where(r => !r.IsNull(3))
                                        .Where(r => !r.IsNull(4));

                                foreach (var row in assumedRows)
                                {
                                    try
                                    {
                                        String periodNo = row.GetString(0).GetEfficientString();
                                        var trackCode = row.GetString(1).GetEfficientString();
                                        var no = row.GetString(2).GetEfficientString();
                                        var items = db.GetTable<InvoiceItem>()
                                                .Where(i => i.TrackCode == trackCode)
                                                .Where(i => i.No == no)
                                                .ToList();

                                        var invoice = items.Where(i => $"{i.InvoiceDate.Value.Year - 1911:000}{(i.InvoiceDate.Value.Month + 1) / 2 * 2:00}" == periodNo)
                                                .FirstOrDefault();

                                        if (invoice == null)
                                        {
                                            row[statusIdx] = "發票號碼不存在";
                                            continue;
                                        }

                                        var winningInvoice = invoice.InvoiceWinningNumber;
                                        if (winningInvoice == null)
                                        {
                                            winningInvoice = new InvoiceWinningNumber
                                            {
                                                InvoiceID = invoice.InvoiceID,
                                            };

                                            db.GetTable<InvoiceWinningNumber>().InsertOnSubmit(winningInvoice);
                                        }

                                        winningInvoice.PrizeType = row.GetString(3).GetEfficientString();
                                        winningInvoice.Bonus = row.GetData<int>(4);

                                        db.SubmitChanges();
                                        winningID.Add(invoice.InvoiceID);

                                    }
                                    catch (Exception ex)
                                    {
                                        exception = exception ?? ex;
                                        Logger.Error(ex);
                                        row[statusIdx] = ex.Message;
                                    }
                                }

                                if (winningID.Count > 0)
                                {
                                    winningID.NotifyWinningInvoice(false);
                                }
                            }


                            using (var xls = ds.ConvertToExcel())
                            {
                                xls.SaveAs(resultFile);
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
                    Logger.Error(ex);
                }
            });

        }



    }
}