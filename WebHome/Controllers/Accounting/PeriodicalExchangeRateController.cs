using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using System.Xml;

using WebHome.Helper;
using WebHome.Models.ViewModel;
using ModelCore.Models.ViewModel;

using WebHome.Properties;
using ModelCore.DataEntity;
using ModelCore.DocumentManagement;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TurnKey;
using ModelCore.Schema.TXN;

using CommonLib.Utility;
using CommonLib.Security.UseCrypto;
using Newtonsoft.Json;
using System.Data;
using ModelCore.Helper;
using CommonLib.DataAccess;
using CommonLib.Core.Utility;

namespace WebHome.Controllers.Accounting
{
    public class PeriodicalExchangeRateController : SampleController<InvoiceItem>
    {
        public PeriodicalExchangeRateController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: TrackCode
        public ActionResult Index(ExchangeRateQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/PeriodicalExchangeRate/Index.cshtml");
        }

        public ActionResult Inquire(ExchangeRateQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            ExchangeRateQueryViewModel tmp = viewModel;
            if (viewModel.KeyID != null)
            {
                tmp = JsonConvert.DeserializeObject<ExchangeRateQueryViewModel>(viewModel.KeyID.DecryptData());

            }
            if (!tmp.PeriodID.HasValue)
            {
                tmp.PeriodID = tmp.Year * 100 + tmp.PeriodNo;
            }

            IQueryable<InvoicePeriodExchangeRate> items = models.GetTable<InvoicePeriodExchangeRate>();
            if(tmp.PeriodID.HasValue)
            {
                items = items
                    .Where(t => t.PeriodID == tmp.PeriodID);
            }

            if(tmp.CurrencyID.HasValue)
            {
                items = items.Where(t => t.CurrencyID == tmp.CurrencyID);
            }

            tmp.Currency = tmp.Currency.GetEfficientString();
            if (tmp.Currency != null)
            {
                if (tmp.Currency == "NTD")
                {
                    tmp.Currency = "TWD";
                }
                items = items.Where(t => t.CurrencyType.AbbrevName.StartsWith(tmp.Currency));
            }

            if (!(tmp.PageSize > 0))
            {
                tmp.PageSize = ModelCore.Properties.AppSettings.Default.PageSize;
            }

            viewModel.ResultView = "~/Views/PeriodicalExchangeRate/DataQuery/ExchangeRateList.cshtml";

            return PageResult(viewModel, items);

        }

        public ActionResult CommitItem(ExchangeRateQueryViewModel viewModel, GenericManager<EIVOEntityDataContext>? db = null)
        {
            UserProfile profile = HttpContext.GetUser();
            ViewBag.ViewModel = viewModel;

            if (db == null)
            {
                db = DataSource;
            }

            ExchangeRateQueryViewModel tmp = viewModel;
            if (viewModel.KeyID != null)
            {
                tmp = JsonConvert.DeserializeObject<ExchangeRateQueryViewModel>(viewModel.KeyID.DecryptData());
            }

            CurrencyType currency = null;
            viewModel.Currency = viewModel.Currency.GetEfficientString();
            if (viewModel.Currency != null)
            {
                if (viewModel.Currency == "NTD")
                {
                    viewModel.Currency = "TWD";
                }
                currency = db.GetTable<CurrencyType>().Where(c => c.AbbrevName == viewModel.Currency)
                   .FirstOrDefault();
            }

            if (!viewModel.ExchangeRate.HasValue || viewModel.ExchangeRate <= 0)
            {
                ModelState.AddModelError("ExchangeRate", "請輸入匯率");
            }

            if (!viewModel.PeriodID.HasValue)
            {
                viewModel.PeriodID = viewModel.Year * 100 + viewModel.PeriodNo;
            }

            if (!viewModel.PeriodID.HasValue)
            {
                ModelState.AddModelError("PeriodID", "請輸入期別");
            }

            if (currency == null)
            {
                ModelState.AddModelError("Currency", "幣別錯誤");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ModelState = ModelState;
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            int count = 0;
            bool toUpdate = false;
            if(tmp.PeriodID.HasValue && tmp.CurrencyID.HasValue)
            {
                if (tmp.PeriodID != viewModel.PeriodID || tmp.CurrencyID != currency.CurrencyID)
                {
                    toUpdate = true;
                    count = models.ExecuteCommand(@"
                            UPDATE          InvoicePeriodExchangeRate
                            SET                   PeriodID = {0}, CurrencyID = {1}, ExchangeRate = {2}
                            WHERE          (PeriodID = {3}) AND (CurrencyID = {4}) 
                                AND (EXISTS
                                    (SELECT          NULL
                                      FROM               InvoicePeriod
                                      WHERE           (PeriodID = {0}))) 
                                AND EXISTS
                                    (SELECT          NULL
                                      FROM               CurrencyType
                                      WHERE           (CurrencyID = {1}))",
                            viewModel.PeriodID, currency.CurrencyID, viewModel.ExchangeRate, tmp.PeriodID, tmp.CurrencyID);
                }
            }

            if (count > 0)
            {
                return Json(new { result = true, keyID = (new { viewModel.PeriodID, currency.CurrencyID }).JsonStringify().EncryptData() });
            }

            InvoicePeriodExchangeRate item = models.GetTable<InvoicePeriodExchangeRate>()
                    .Where(p => p.PeriodID == viewModel.PeriodID)
                    .Where(p => p.CurrencyID == currency.CurrencyID)
                    .FirstOrDefault();

            bool isNew = false;
            if (item == null)
            {
                isNew = true;
                var period = db.GetTable<InvoicePeriod>().Where(p => p.PeriodID == viewModel.PeriodID)
                    .FirstOrDefault();

                if (period == null)
                {
                    period = new InvoicePeriod
                    {
                        PeriodID = viewModel.PeriodID.Value,
                    };
                    db.GetTable<InvoicePeriod>().InsertOnSubmit(period);
                }

                item = new InvoicePeriodExchangeRate
                {
                    CurrencyID = currency.CurrencyID,
                    InvoicePeriod = period,
                };
                db.GetTable<InvoicePeriodExchangeRate>().InsertOnSubmit(item);
            }

            item.ExchangeRate = viewModel.ExchangeRate.Value;

            try
            {
                db.SubmitChanges();

                db.ExecuteCommand("update InvoiceTrackCode set PeriodID = {0} where Year = {1} and PeriodNo = {2}", item.PeriodID, item.PeriodID / 100, item.PeriodID % 100);

                return Json(new { result = true, keyID = (new { item.PeriodID, item.CurrencyID }).JsonStringify().EncryptData(), isNew = isNew && !toUpdate });
            }
            catch (Exception ex)
            {
                CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                ModelState.AddModelError("Message", ex.Message);
                return Json(new { result = false, message = ex.Message });
            }

        }


        public ActionResult DeleteItem(ExchangeRateQueryViewModel viewModel)
        {
            UserProfile profile = HttpContext.GetUser();
            ViewBag.ViewModel = viewModel;

            if (viewModel.KeyID != null)
            {
                viewModel = JsonConvert.DeserializeObject<ExchangeRateQueryViewModel>(viewModel.KeyID.DecryptData());
            }

            int count = models.ExecuteCommand("delete InvoicePeriodExchangeRate where PeriodID = {0} and CurrencyID = {1}",
                    viewModel.PeriodID, viewModel.CurrencyID);

            if (count == 0)
            {
                return Json(new { result = false, message = "資料錯誤" });
            }

            return Json(new { result = true });

        }

        public ActionResult ProcessDataItem(ExchangeRateQueryViewModel viewModel)
        {
            ViewResult result = (ViewResult)Inquire(viewModel);
            result.ViewName = "~/Views/PeriodicalExchangeRate/DataQuery/ExchangeRateList.cshtml";
            if (viewModel.DisplayType == Naming.FieldDisplayType.DataItem)
            {
                IQueryable<InvoicePeriodExchangeRate> items = (IQueryable<InvoicePeriodExchangeRate>)result.Model;
                if (items.Count() == 0)
                {
                    viewModel.DisplayType = Naming.FieldDisplayType.Create;
                }
            }

            ViewBag.DisplayType = viewModel.DisplayType;

            return result;
        }

        public async Task<ActionResult> GetExchangeRateSampleAsync()
        {

            var items = models.GetTable<InvoicePeriodExchangeRate>().Where(i => false).Take(100);
            var dataItems = items.ToArray().Select(i => new
            {
                年度 = i.PeriodID / 100,
                期別 = i.PeriodID % 100,
                幣別代碼 = i.CurrencyType.AbbrevName,
                匯率 = i.ExchangeRate,
            });

            var detailItems = models.GetTable<CurrencyType>().Select(d => new
            {
                幣別代碼 = d.AbbrevName,
                幣名 = d.CurrencyName,
            });

            using DataSet ds = new DataSet();

            DataTable table = dataItems.ToDataTable();
            table.TableName = "匯率";
            var sample = table.NewRow();
            sample[0] = 2020;
            sample[1] = 1;
            sample[2] = "USD";
            sample[3] = 27.2M;
            table.Rows.Add(sample);
            ds.Tables.Add(table);

            table = detailItems.ToDataTable();
            table.TableName = "幣別";
            ds.Tables.Add(table);

            await ds.SaveAsExcelAsync(Response, String.Format("attachment;filename={0}", HttpUtility.UrlEncode("ExchangeRateSample.xlsx")));

            return new EmptyResult();
        }

        public ActionResult UploadExchangeRate(IEnumerable<IFormFile> excelFile)
        {
            if (excelFile == null || excelFile.Count() < 1)
            {
                return Json(new { result = false, message = "未選取檔案或檔案上傳失敗" });
            }

            if (excelFile.Count() != 1)
            {
                return Json(new { result = false, message = "請上傳單一檔案" });
            }

            try
            {
                var file = excelFile.First();
                String fileName = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, $"{DateTime.Now.Ticks}_{Path.GetFileName(file.FileName)}");
                file.SaveAs(fileName);

                using (var ds = fileName.ImportExcelXLS())
                {
                    DataTable table;
                    if (ds.Tables.Count == 0
                        || (table = ds.Tables.Cast<DataTable>()
                            .Where(t => t.TableName.Contains("匯率")).FirstOrDefault()) == null)
                    {
                        return Json(new { result = false, message = "Excel檔未包含【匯率】資料表" });
                    }
                     
                    table.Columns.Add(new DataColumn("處理狀態", typeof(String)));
                    int colStatus = table.Columns.Count - 1;
                    ExchangeRateQueryViewModel item = new ExchangeRateQueryViewModel { };

                    foreach (DataRow r in table.Rows)
                    {
                        try
                        {
                            item.PeriodID = null;
                            item.CurrencyID = null;
                            item.Year = r.GetData<int>(0);
                            item.PeriodNo = r.GetData<int>(1);
                            item.Currency = r.GetString(2);
                            item.ExchangeRate = r.GetData<decimal>(3);

                            ModelState.Clear();
                            using (GenericManager<EIVOEntityDataContext> db = new GenericManager<EIVOEntityDataContext>())
                            {
                                CommitItem(item, db);
                            }
                            if (!ModelState.IsValid)
                            {
                                r[colStatus] = ModelState.ErrorMessage();
                            }
                        }
                        catch (Exception ex)
                        {
                            r[colStatus] = ex.Message;
                        }
                    }

                    using (var xls = ds.ConvertToExcel())
                    {
                        xls.SaveAs(fileName);
                    }

                }

                return View("~/Views/Shared/Module/PromptFileDownload.cshtml", 
                    File(fileName, "application/octet-stream", "匯率資料(回應).xlsx"));

            }
            catch (Exception ex)
            {
                CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                return Json(new { result = false, message = ex.Message });
            }
        }



    }
}