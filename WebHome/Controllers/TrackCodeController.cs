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
using ClosedXML.Excel;
using ModelCore.DataExchange;

namespace WebHome.Controllers
{
    public class TrackCodeController : SampleController<InvoiceItem>
    {
        public TrackCodeController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: TrackCode
        public ActionResult Index(TrackCodeQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View();
        }

        public ActionResult Inquire(TrackCodeQueryViewModel viewModel)
        {

            ViewBag.ViewModel = viewModel;
            IQueryable<InvoiceTrackCode> items = models.GetTable<InvoiceTrackCode>()
                .Where(t => t.Year == viewModel.Year);

            if (viewModel.PeriodNo.HasValue)
            {
                items = items.Where(t => t.PeriodNo == viewModel.PeriodNo);
            }

            viewModel.ResultView = "~/Views/TrackCode/Module/ItemList.cshtml";
            viewModel.QueryResult = "~/Views/TrackCode/Module/QueryResult.cshtml";
            return PageResult(viewModel, items);

        }

        public ActionResult EditItem(int? id)
        {
            var item = models.GetTable<InvoiceTrackCode>()
                .Where(d => d.TrackID == id)
                .FirstOrDefault();

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "發票字軌資料錯誤!!");
            }

            return View("~/Views/TrackCode/Module/EditItem.cshtml", item);

        }

        public ActionResult DeleteItem(int? id)
        {
            var item = models.DeleteAny<InvoiceTrackCode>(d => d.TrackID == id);

            if (item == null)
            {
                return Json(new { result = false, message = "發票字軌資料錯誤!!" });
            }

            return Json(new { result = true });

        }

        public ActionResult DataItem(int? id)
        {
            var item = models.GetTable<InvoiceTrackCode>()
                .Where(d => d.TrackID == id)
                .FirstOrDefault();

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "發票字軌資料錯誤!!");
            }

            return View("~/Views/TrackCode/Module/DataItem.cshtml", item);

        }

        public ActionResult CommitItem(TrackCodeViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            viewModel.TrackCode = viewModel.TrackCode.GetEfficientString();
            if (viewModel.TrackCode == null || !Regex.IsMatch(viewModel.TrackCode, "^[A-Za-z]{2}$"))
            {
                ModelState.AddModelError("TrackCode", "字軌應為二位英文字母!!");
            }

            var model = models.GetTable<InvoiceTrackCode>().Where(t => t.TrackID == viewModel.TrackID).FirstOrDefault();
            if (model == null)
            {
                if (!viewModel.PeriodNo.HasValue || viewModel.PeriodNo > 6 || viewModel.PeriodNo < 1)
                {
                    ModelState.AddModelError("PeriodNo", "請選擇期別!!");
                }
                else if (!viewModel.Year.HasValue)
                {
                    ModelState.AddModelError("Year", "請選擇年份!!");
                }
                else
                {
                    var item = models.GetTable<InvoiceTrackCode>().Where(t => t.Year == viewModel.Year && t.TrackCode == viewModel.TrackCode
                    && t.PeriodNo == viewModel.PeriodNo).FirstOrDefault();

                    if (item != null && item.TrackID != viewModel.TrackID)
                    {
                        ModelState.AddModelError("TrackCode", "字軌重複!!");
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
                model = new InvoiceTrackCode
                {
                    Year = viewModel.Year.Value,
                    PeriodNo = viewModel.PeriodNo.Value,
                };
                models.GetTable<InvoiceTrackCode>().InsertOnSubmit(model);
            }

            model.TrackCode = viewModel.TrackCode;
            model.InvoiceType = viewModel.InvoiceType.HasValue
                ? (byte)viewModel.InvoiceType.Value
                : (byte)Naming.InvoiceTypeDefinition.一般稅額計算之電子發票;

            models.SubmitChanges();

            return View("~/Views/TrackCode/Module/DataItem.cshtml", model);

        }

        public ActionResult UpdateTrackCode()
        {
            var profile = HttpContext.GetUser();

            try
            {
                var xlFile = Request.Form.Files["TrackCode"];
                if (xlFile != null)
                {
                    using var input = xlFile.OpenReadStream();
                    using (XLWorkbook xlwb = new XLWorkbook(input))
                    {
                        TrackCodeExchange exchange = new TrackCodeExchange();
                        switch ((Naming.RoleID)profile.CurrentUserRole.RoleID)
                        {
                            case Naming.RoleID.ROLE_SYS:
                                exchange.ExchangeData(xlwb);
                                break;
                            default:
                                break;
                        }

                        String result = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, Guid.NewGuid().ToString() + ".xslx");
                        xlwb.SaveAs(result);

                        return View("~/Views/Shared/Module/PromptFileDownload.cshtml",
                            PhysicalFile(result, "application/octet-stream", "相對營業人(回應).xlsx"));
                    }
                }
                ViewBag.AlertMessage = "檔案錯誤!!";
            }
            catch (Exception ex)
            {
                CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                ViewBag.AlertMessage = ex.ToString();
            }
            return View("Index");
        }

        public async Task<ActionResult> GetSampleAsync()
        {
            TrackCodeExchange exchange = new TrackCodeExchange();
            using (var xls = exchange.GetSample())
            {
                await xls.SaveAsExcelAsync(Response, $"attachment;filename={HttpUtility.UrlEncode("發票字軌資料")}.xlsx");
            }

            return new EmptyResult { };
        }



    }
}