using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ModelCore.DataEntity;
using WebHome.Helper;
using WebHome.Models;


using System.Text;
using ModelCore.Locale;

using System.IO;
using System.IO.Compression;
using CommonLib.Utility;
using System.Threading;
using System.Data.SqlClient;
using System.Data;
using ClosedXML.Excel;
using System.Data.Linq;
using WebHome.Models.ViewModel;
using ModelCore.Models.ViewModel;
using ModelCore.Helper;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using WebHome.Helper.Security.Authorization;
using CommonLib.Core.Utility;
using CommonLib.Core.Helper;
using CommonLib.DataAccess;
using ModelCore.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using ModelCore.Models;

namespace WebHome.Controllers
{
    [Authorize]
    public class InvoiceQueryController : SampleController<InvoiceItem>
    {
        protected UserProfile _userProfile;

        public InvoiceQueryController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        //protected ModelSourceInquiry<InvoiceItem> createModelInquiry()
        //{
        //    _userProfile = HttpContext.GetUser();

        //    var inquireConsumption = new InquireInvoiceConsumption { ControllerName = "InquireInvoice", ActionName = "ByConsumption" };
        //    //inquireConsumption.Append(new InquireInvoiceConsumptionExtensionToPrint { });

        //    return (ModelSourceInquiry<InvoiceItem>)(new InquireEffectiveInvoice { })
        //        .Append(new InquireInvoiceByRole(_userProfile) { })
        //        .Append(inquireConsumption)
        //        .Append(new InquireInvoiceSeller { ControllerName = "InquireInvoice", ActionName = "BySeller", /*QueryRequired = true, AlertMessage = "請選擇公司名稱!!",*/  })
        //        .Append(new InquireInvoiceBuyer { ControllerName = "InquireInvoice", ActionName = "ByBuyer" })
        //        .Append(new InquireInvoiceBuyerByName { ControllerName = "InquireInvoice", ActionName = "ByBuyerName" })
        //        .Append(new InquireInvoiceDate { ControllerName = "InquireInvoice", ActionName = "ByInvoiceDate" })
        //        .Append(new InquireInvoiceAttachment { ControllerName = "InquireInvoice", ActionName = "ByAttachment" })
        //        .Append(new InquireInvoiceNo { })
        //        .Append(new InquireInvoiceAgent { ControllerName = "InquireInvoice", ActionName = "ByAgent" })
        //        .Append(new InquireWinningInvoice { });
        //}

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS })]
        public ActionResult InvoiceReport(InquireInvoiceViewModel viewModel)
        {
            //ViewBag.HasQuery = false;
            ViewBag.QueryAction = "Inquire";
            ViewBag.ViewModel = viewModel;

            var profile = HttpContext.GetUser();
            DataSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);
            return View(DataSource.Inquiry);
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS })]
        public ActionResult InquireMonthlyReport(MonthlyReportQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (!viewModel.DateFrom.HasValue)
            {
                ModelState.AddModelError("InvoiceDateFrom", "請輸入查詢起日");
            }

            if (!viewModel.DateTo.HasValue)
            {
                ModelState.AddModelError("InvoiceDateFrom", "請輸入查詢迄日");
            }

            if (!viewModel.AgentID.HasValue && !viewModel.SellerID.HasValue)
            {
                ModelState.AddModelError("AgentID", "請選擇代理人");
                ModelState.AddModelError("SellerID", "請輸入開立人統編");
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            viewModel.Message = "發票月報表";

            ProcessRequest processItem = new ProcessRequest
            {
                Sender = HttpContext.GetUser()?.UID,
                SubmitDate = DateTime.Now,
                ProcessStart = DateTime.Now,
                ResponsePath = System.IO.Path.Combine(Logger.LogDailyPath, Guid.NewGuid().ToString() + ".xlsx"),
                ViewModel = viewModel.JsonStringify(),
            };
            models.GetTable<ProcessRequest>().InsertOnSubmit(processItem);
            models.SubmitChanges();

            viewModel.TaskID = processItem.TaskID;
            viewModel.Push($"{DateTime.Now.Ticks}.json");

            return View("~/Views/Shared/Module/PromptCheckDownload.cshtml",
                    new AttachmentViewModel
                    {
                        TaskID = processItem.TaskID,
                        FileName = processItem.ResponsePath,
                        FileDownloadName = "月報表資料明細.xlsx",
                    });

        }


        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS, Naming.RoleID.ROLE_SELLER })]
        public ActionResult InvoiceMediaReport(TaxMediaQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            if (!viewModel.BusinessBorder.HasValue)
            {
                viewModel.BusinessBorder = CategoryDefinition.CategoryEnum.發票開立營業人;
            }
            ViewBag.QueryAction = "InquireInvoiceMedia";

            return View("~/Views/InvoiceQuery/InvoiceMediaReport.cshtml");
        }

        public ActionResult InquireInvoiceMedia(TaxMediaQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (viewModel.BusinessBorder == CategoryDefinition.CategoryEnum.發票開立營業人)
            {
                viewModel.TaxNo = viewModel.TaxNo.GetEfficientString();
                if (viewModel.TaxNo == null || viewModel.TaxNo.Length != 9)
                {
                    ModelState.AddModelError("TaxNo", "請輸入9位數稅籍編號!!");
                }
            }

            if (!viewModel.Year.HasValue)
            {
                ModelState.AddModelError("Year", "請選擇年度!!");
            }

            if (!viewModel.PeriodNo.HasValue)
            {
                ModelState.AddModelError("PeriodNo", "請選擇期別!!");
            }

            if (!viewModel.SellerID.HasValue)
            {
                ModelState.AddModelError("SellerID", "請選擇發票開立人!!");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ModelState = ModelState;
                return View("~/Views/InvoiceQuery/InvoiceMediaReport.cshtml");
            }

            if (viewModel.BusinessBorder == CategoryDefinition.CategoryEnum.境外電商)
            {
                ProcessRequest processItem = new ProcessRequest
                {
                    Sender = HttpContext.GetUser()?.UID,
                    SubmitDate = DateTime.Now,
                    ProcessStart = DateTime.Now,
                    ResponsePath = System.IO.Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, Guid.NewGuid().ToString() + ".csv"),
                };
                models.GetTable<ProcessRequest>().InsertOnSubmit(processItem);
                models.SubmitChanges();

                saveAsCsv(processItem.TaskID, processItem.ResponsePath, viewModel);

                return View("~/Views/InvoiceQuery/InvoiceMediaReport.cshtml",
                        new AttachmentViewModel
                        {
                            TaskID = processItem.TaskID,
                            FileName = processItem.ResponsePath,
                            FileDownloadName = $"{DateTime.Today:yyyy-MM-dd}.csv",
                        });

                //return View("~/Views/InvoiceQuery/InvoiceMediaReportCBM.cshtml");
            }

            DateTime startDate = new DateTime(viewModel.Year.Value, viewModel.PeriodNo.Value * 2 - 1, 1);
            DataLoadOptions ops = new DataLoadOptions();
            ops.LoadWith<InvoiceItem>(i => i.InvoiceBuyer);
            ops.LoadWith<InvoiceItem>(i => i.InvoiceCancellation);
            ops.LoadWith<InvoiceItem>(i => i.InvoiceAmountType);
            ops.LoadWith<InvoiceItem>(i => i.InvoiceSeller);
            models.DataContext.LoadOptions = ops;

            var items = models.GetTable<InvoiceItem>().Where(i => i.SellerID == viewModel.SellerID
                    && i.InvoiceDate >= startDate
                    && i.InvoiceDate < startDate.AddMonths(2))
                .OrderBy(i => i.InvoiceID);

            var allowance = models.GetTable<InvoiceAllowance>()
                    .Where(a => a.InvoiceAllowanceCancellation == null)
                    .Where(a => a.InvoiceAllowanceSeller.SellerID == viewModel.SellerID)
                    .Where(a => a.AllowanceDate >= startDate && a.AllowanceDate < startDate.AddMonths(2));
            ViewBag.AllowanceItems = allowance;


            if (items.Count() > 0 || allowance.Count() > 0)
            {
                ViewBag.FileName = String.Format("{0:d4}{1:d2}({2}).txt", viewModel.Year, viewModel.PeriodNo, items.First().Organization.ReceiptNo);
                var orgItem = models.GetTable<Organization>().Where(o => o.CompanyID == viewModel.SellerID).First();
                if (orgItem.OrganizationExtension == null)
                    orgItem.OrganizationExtension = new OrganizationExtension { };
                if (orgItem.OrganizationExtension.TaxNo != viewModel.TaxNo)
                {
                    orgItem.OrganizationExtension.TaxNo = viewModel.TaxNo;
                    models.SubmitChanges();
                }
            }
            else
            {
                ViewBag.Message = "資料不存在!!";
                return View("InvoiceMediaReport");
            }

            return View("~/Views/InvoiceQuery/InquireInvoiceMedia.cshtml", items);
        }

        static void saveAsCsv(int taskID, String resultFile, TaxMediaQueryViewModel viewModel)
        {
            Task.Run(() =>
            {
                try
                {
                    String tmp = $"{resultFile}.tmp";
                    Exception exception = null;
                    using (StreamWriter writer = new StreamWriter(tmp, false, Encoding.UTF8))
                    {
                        using (ModelSource<InvoiceItem> db = new ModelSource<InvoiceItem>())
                        {

                            try
                            {
                                DateTime dateFrom = new DateTime(viewModel.Year.Value, viewModel.PeriodNo.Value * 2 - 1, 1);
                                writer.WriteLine("格式代號,資料所屬期別(年月),幣別,銷售總計金額,匯率,換算後銷售總計金額,銷售額,銷項稅額,交易筆數,統一發票字軌,發票起號,發票迄號,發票開立年月");

                                for (int idx = 0; idx < 2; idx++)
                                {
                                    var invoiceItems = db.DataContext.GetInvoiceReport(viewModel.SellerID, dateFrom, dateFrom.AddMonths(1));
                                    var allowanceItems = db.DataContext.GetAllowanceReport(viewModel.SellerID, dateFrom, dateFrom.AddMonths(1));

                                    foreach (var g in invoiceItems)
                                    {
                                        decimal? exchangeRate;
                                        if (g.CurrencyID == 0)
                                        {
                                            exchangeRate = 1;
                                        }
                                        else
                                        {
                                            exchangeRate = db.GetTable<InvoicePeriodExchangeRate>()
                                                .Where(v => v.PeriodID == g.PeriodID && v.CurrencyID == g.CurrencyID)
                                                .FirstOrDefault()?.ExchangeRate;
                                        }
                                        decimal? twTotalAmt = g.TotalAmount * exchangeRate;
                                        decimal? twAmtNoTax = null;
                                        if (twTotalAmt.HasValue)
                                        {
                                            twTotalAmt = Math.Round(twTotalAmt.Value);
                                            twAmtNoTax = Math.Round(twTotalAmt.Value / 1.05M);
                                        }
                                        writer.WriteLine($"35,{dateFrom.Year - 1911}{dateFrom.Month:00},{(g.CurrencyID == 0 ? "NTD" : g.AbbrevName)},{g.TotalAmount},{exchangeRate:.#####},{twTotalAmt},{twAmtNoTax},{twTotalAmt - twAmtNoTax},{g.RecordCount},{g.TrackCode},{g.StartNo:00000000},{g.EndNo:00000000},");
                                    }

                                    foreach (var g in allowanceItems)
                                    {
                                        decimal? exchangeRate;
                                        if (g.CurrencyID == 0)
                                        {
                                            exchangeRate = 1;
                                        }
                                        else
                                        {
                                            exchangeRate = db.GetTable<InvoicePeriodExchangeRate>()
                                                .Where(v => v.PeriodID == g.PeriodID && v.CurrencyID == g.CurrencyID)
                                                .FirstOrDefault()?.ExchangeRate;
                                        }
                                        decimal? twTotalAmt = g.TotalAmount * exchangeRate;
                                        decimal? twTaxAmt = g.TotalTaxAmount * exchangeRate;
                                        if (twTotalAmt.HasValue)
                                        {
                                            twTotalAmt = Math.Round(twTotalAmt.Value);
                                        }
                                        if (twTaxAmt.HasValue)
                                        {
                                            twTaxAmt = Math.Round(twTaxAmt.Value);
                                        }
                                        writer.WriteLine($"33,{dateFrom.Year - 1911}{dateFrom.Month:00},{(g.CurrencyID == 0 ? "NTD" : g.AbbrevName)},{g.TotalAmount},{exchangeRate:.#####},,,,{g.RecordCount},{g.TrackCode},{g.StartNo:00000000},{g.EndNo:00000000},{g.Year - 1911:000}{g.Month:00}");
                                    }

                                    dateFrom = dateFrom.AddMonths(1);
                                }
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
                    if (exception == null)
                    {
                        System.IO.File.Move(tmp, resultFile);
                    }

                }
                catch (Exception ex)
                {
                    CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                }
            });

        }


        public ActionResult Inquire(InquireInvoiceViewModel viewModel)
        {
            //ViewBag.HasQuery = true;
            ViewBag.PrintAction = "PrintResult";
            ViewBag.ViewModel = viewModel;

            var profile = HttpContext.GetUser();
            DataSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);
            DataSource.BuildQuery();

            return View("InquiryResult",DataSource.Inquiry);
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS })]
        public ActionResult InvoiceAttachment(InquireInvoiceViewModel viewModel)
        {
            //ViewBag.HasQuery = false;
            ViewBag.QueryAction = "InquireAttachment";
            ViewBag.ViewModel = viewModel;

            var profile = HttpContext.GetUser();
            DataSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);

            return View("InvoiceReport", DataSource.Inquiry);
        }

        public ActionResult InquireAttachment(InquireInvoiceViewModel viewModel)
        {
            //ViewBag.HasQuery = true;
            ViewBag.ViewModel = viewModel;

            var profile = HttpContext.GetUser();
            DataSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);
            DataSource.BuildQuery();

            return View("AttachmentResult", DataSource.Inquiry);
        }

        public ActionResult AttachmentGridPage(int index, int size, InquireInvoiceViewModel viewModel)
        {
            //ViewBag.HasQuery = true;
            ViewBag.ViewModel = viewModel;

            var profile = HttpContext.GetUser();
            DataSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);
            DataSource.BuildQuery();

            if (index > 0)
                index--;
            else
                index = 0;

            return View(DataSource.Items.OrderByDescending(d => d.InvoiceID)
                .Skip(index * size).Take(size));
        }


        public ActionResult GridPage(int index,int size, InquireInvoiceViewModel viewModel)
        {
            //ViewBag.HasQuery = true;
            ViewBag.ViewModel = viewModel;

            var profile = HttpContext.GetUser();
            DataSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);
            DataSource.BuildQuery();

            if (index > 0)
                index--;
            else
                index = 0;

            return View(DataSource.Items.OrderByDescending(d => d.InvoiceID)
                .Skip(index * size).Take(size));
        }

        public ActionResult DownloadCSV(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            var profile = HttpContext.GetUser();
            DataSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);
            DataSource.BuildQuery();

            var mediaType = new MediaTypeHeaderValue("application/octet-stream")
            {
                Encoding = Encoding.GetEncoding(950)
            };
            Response.ContentType = mediaType.ToString();

            return View(DataSource.Items);
        }

        public async Task<ActionResult> CreateXlsxAsync(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            var items = DataSource.Items.OrderBy(i => i.InvoiceID)
                .Select(i => new
                {
                    發票號碼 = i.TrackCode + i.No,
                    發票日期 = i.InvoiceDate,
                    附件檔名 = i.CDS_Document.Attachment.Count > 0 ? i.CDS_Document.Attachment.First().KeyName : null,
                    客戶ID = i.InvoiceBuyer.CustomerID,
                    序號 = i.InvoicePurchaseOrder != null ? i.InvoicePurchaseOrder.OrderNo : null,
                    發票開立人 = i.InvoiceSeller.CustomerName,
                    開立人統編 = i.InvoiceSeller.ReceiptNo,
                    未稅金額 = i.InvoiceAmountType.SalesAmount,
                    稅額 = i.InvoiceAmountType.TaxAmount,
                    含稅金額 = i.InvoiceAmountType.TotalAmount,
                    買受人名稱 = i.InvoiceBuyer.CustomerName,
                    買受人統編 = i.InvoiceBuyer.ReceiptNo,
                    連絡人名稱 = i.InvoiceBuyer.ContactName,
                    連絡人地址 = i.InvoiceBuyer.Address,
                    買受人EMail = i.InvoiceBuyer.EMail,
                    愛心碼 = i.InvoiceDonation.AgencyCode,
                    是否中獎 = i.InvoiceWinningNumber.UniformInvoiceWinningNumber.PrizeType,
                    載具類別 = i.InvoiceCarrier.CarrierType,
                    載具號碼 = i.InvoiceCarrier.CarrierNo,
                //備註 = String.Join("", i.InvoiceDetails.Select(t => t.InvoiceProduct.InvoiceProductItem.FirstOrDefault())
                //    .Select(p => p.Remark))
            });


            using (SqlCommand sqlCmd = (SqlCommand)models.GetCommand(items))
            {
                sqlCmd.Connection = (SqlConnection)models.DataContext.Connection;
                using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCmd))
                {
                    using (DataSet ds = new DataSet())
                    {
                        adapter.Fill(ds);
                        ds.Tables[0].TableName = ds.DataSetName = "發票資料明細";
                        await ds.SaveAsExcelAsync(Response, String.Format("attachment;filename={0}", HttpUtility.UrlEncode("發票資料明細.xlsx")));
                    }
                }
            }

            return new EmptyResult();
        }

        public ActionResult AssignDownload(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            var profile = HttpContext.GetUser();
            DataSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);
            DataSource.BuildQuery();

            String resultFile = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, Guid.NewGuid().ToString() + ".xlsx");
            _userProfile["assignDownload"] = resultFile;

            ThreadPool.QueueUserWorkItem(stateInfo => 
            {
                try
                {
                    SqlCommand sqlCmd = (SqlCommand)models.GetCommand(DataSource.Items);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCmd))
                    {
                        using (DataSet ds = new DataSet())
                        {
                            adapter.Fill(ds);
                            using(XLWorkbook xls = new XLWorkbook())
                            {
                                xls.Worksheets.Add(ds);
                                xls.SaveAs(resultFile);
                            }
                        }
                    }
                    models.Dispose();
                }
                catch(Exception ex)
                {
                    CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                }
            });

            return Content("下載資料請求已送出!!");
        }



        public ActionResult PrintResult(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            var profile = HttpContext.GetUser();
            DataSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);
            DataSource.BuildQuery();
            ((ModelSource<InvoiceItem>)models).ResultModel = Naming.DataResultMode.Print;

            return View(DataSource.Inquiry);
        }

        public async Task<ActionResult> DownloadInvoiceAttachmentAsync(DocumentQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            if (viewModel.KeyID != null)
            {
                viewModel.DocID = viewModel.DecryptKeyValue();
            }

            var items = models.GetTable<InvoiceItem>().Where(i => i.InvoiceID == viewModel.DocID);
            if (items.Any())
            {
                return zipAttachment(items);
            }
            else
            {
                Response.Clear();
                
                
                Response.Headers.Add("Cache-control", "max-age=1");
                Response.ContentType = "application/octet-stream";
                Response.Headers.Add("Content-Disposition", String.Format("attachment;filename={0}", HttpUtility.UrlEncode("DataNotFound.txt")));

                await Response.WriteAsync("file not found!!");

                return new EmptyResult();
            }
        }

        public ActionResult DownloadAttachment(String data)
        {
            String jsonData = data;
            if (!String.IsNullOrEmpty(jsonData))
            {
                int[]? invoiceID = JsonConvert.DeserializeObject<int[]>(jsonData);
                if (invoiceID != null)
                {
                    return zipAttachment(models.GetTable<InvoiceItem>().Where(i => invoiceID.Contains(i.InvoiceID)));
                }
            }
            return Content(jsonData);
        }

        public ActionResult DownloadAll(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            var profile = HttpContext.GetUser();
            DataSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);
            DataSource.BuildQuery();

            return zipAttachment(DataSource.Items);
        }

        private ActionResult zipAttachment(IEnumerable<InvoiceItem> items)
        {
            String temp = Startup.MapPath("~/temp");
            if (!Directory.Exists(temp))
            {
                Directory.CreateDirectory(temp);
            }
            String outFile = Path.Combine(temp, Guid.NewGuid().ToString() + ".zip");
            using (var zipOut = System.IO.File.Create(outFile))
            {
                using (ZipArchive zip = new ZipArchive(zipOut, ZipArchiveMode.Create))
                {
                    foreach (var item in items)
                    {
                        if (item.CDS_Document.Attachment.Count > 0)
                        {
                            for (int i = 0; i < item.CDS_Document.Attachment.Count; i++)
                            {
                                var attach = item.CDS_Document.Attachment[i];
                                if (System.IO.File.Exists(attach.StoredPath))
                                {
                                    ZipArchiveEntry entry = zip.CreateEntry(i == 0 ? item.TrackCode + item.No + ".pdf" : item.TrackCode + item.No + "-" + i + ".pdf");
                                    using (Stream outStream = entry.Open())
                                    {
                                        using (var inStream = System.IO.File.Open(attach.StoredPath, FileMode.Open))
                                        {
                                            inStream.CopyTo(outStream);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            var result = new VirtualFileResult(outFile, "application/octet-stream");
            result.FileDownloadName = "發票附件.zip";
            return result;

        }

        public ActionResult InvoiceSummary(InquireInvoiceViewModel viewModel)
        {
            //ViewBag.HasQuery = false;
            ViewBag.QueryAction = "InquireSummary";
            ViewBag.ViewModel = viewModel;

            var profile = HttpContext.GetUser();
            DataSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);

            return View("InvoiceReport", DataSource.Inquiry);
        }

        public static readonly int[] AvailableMemberCategory = new int[]
        {
            (int)CategoryDefinition.CategoryEnum.發票開立營業人,
            (int)CategoryDefinition.CategoryEnum.GoogleTaiwan,
            (int)CategoryDefinition.CategoryEnum.營業人發票自動配號,
            (int)CategoryDefinition.CategoryEnum.經銷商,
            (int)CategoryDefinition.CategoryEnum.境外電商,
        };

        public ActionResult InquireSummary(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (!viewModel.InvoiceDateFrom.HasValue)
            {
                ModelState.AddModelError("InvoiceDateFrom", "請輸入查詢起日");
            }

            if (!viewModel.InvoiceDateTo.HasValue)
            {
                ModelState.AddModelError("InvoiceDateTo", "請輸入查詢迄日");
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            var profile = HttpContext.GetUser();
            DataSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);
            DataSource.BuildQuery();

            var orgaCate = models.GetTable<OrganizationCategory>().Where(c => AvailableMemberCategory.Contains(c.CategoryID));
            IQueryable<Organization> sellerItems = models.GetTable<Organization>()
                    .Where(o => orgaCate.Any(c => c.CompanyID == o.CompanyID));

            sellerItems = models.FilterOrganizationByRole(profile, sellerItems);

            if (viewModel.SellerID.HasValue)
            {
                sellerItems = sellerItems.Where(o => o.CompanyID == viewModel.SellerID);
            }

            if (viewModel.AgentID.HasValue)
            {
                sellerItems = sellerItems
                    .Join(models.GetTable<InvoiceIssuerAgent>()
                            .Where(a => a.AgentID == viewModel.AgentID),
                        o => o.CompanyID, a => a.IssuerID, (o, a) => o);
            }

            viewModel.ResultView = "~/Views/InvoiceQuery/Module/InvoiceSummaryResult.cshtml";

            return PageResult(viewModel, sellerItems);

        }

        public async Task<ActionResult> CreateMonthlyReportXlsxAsync(InquireInvoiceViewModel viewModel)
        {
            ViewResult result = (ViewResult)InquireSummary(viewModel);
            IQueryable<Organization> items = result.Model as IQueryable<Organization>;

            if (items == null)
            {
                return result;
            }

            ProcessRequest processItem = new ProcessRequest
            {
                Sender = HttpContext.GetUser()?.UID,
                SubmitDate = DateTime.Now,
                ProcessStart = DateTime.Now,
                ResponsePath = System.IO.Path.Combine(Logger.LogDailyPath, Guid.NewGuid().ToString() + ".xlsx"),
            };
            models.GetTable<ProcessRequest>().InsertOnSubmit(processItem);
            models.SubmitChanges();

            _dbInstance = false;

            SqlCommand sqlCmd = (SqlCommand)models.GetCommand(items);
            SaveAsExcel(processItem, viewModel, items);

            return View("~/Views/Shared/Module/PromptCheckDownload.cshtml",
                    new AttachmentViewModel
                    {
                        TaskID = processItem.TaskID,
                        FileName = processItem.ResponsePath,
                        FileDownloadName = "開立發票月報表.xlsx",
                    });

        }

        private void SaveAsExcel(ProcessRequest taskItem, InquireInvoiceViewModel viewModel, IQueryable<Organization> items)
        {
            Task.Run(() =>
            {
                Exception exception = null;
                try
                {
                    using (DataSet ds = new DataSet())
                    {
                        IQueryable<InvoiceItem> dataItems = DataSource.Items;

                        DataTable table = new DataTable();
                        table.Columns.Add(new DataColumn("開立發票營業人", typeof(String)));
                        table.Columns.Add(new DataColumn("統編", typeof(String)));
                        table.Columns.Add(new DataColumn("上線日期", typeof(String)));
                        table.Columns.Add(new DataColumn("發票筆數", typeof(int)));
                        table.Columns.Add(new DataColumn("註記停用日期", typeof(String)));
                        table.TableName = $"發票資料統計({viewModel.InvoiceDateFrom:yyyy-MM-dd}~{viewModel.InvoiceDateTo:yyyy-MM-dd})";

                        ds.Tables.Add(table);

                        //var invoiceItems = items.GroupJoin(DataSource.Items,
                        //        o => o.CompanyID, i => i.SellerID, (o, i) => new { Seller = o, Items = i });

                        foreach (var item in items.OrderBy(o => o.ReceiptNo))
                        {
                            DataRow r = table.NewRow();

                            r[0] = item.CompanyName;
                            r[1] = item.ReceiptNo;
                            r[2] = $"{item.OrganizationExtension?.GoLiveDate:yyyy/MM/dd}";
                            r[3] = dataItems.Count(i => i.SellerID == item.CompanyID);
                            r[4] = $"{item.OrganizationExtension?.ExpirationDate:yyyy/MM/dd}";

                            table.Rows.Add(r);
                        }

                        foreach (var yy in dataItems.GroupBy(i => i.InvoiceDate.Value.Year))
                        {
                            foreach (var mm in yy.GroupBy(i => i.InvoiceDate.Value.Month))
                            {
                                table = new DataTable();
                                table.Columns.Add(new DataColumn("日期", typeof(String)));
                                table.Columns.Add(new DataColumn("未作廢總筆數", typeof(int)));
                                table.Columns.Add(new DataColumn("未作廢總金額", typeof(decimal)));
                                table.Columns.Add(new DataColumn("已作廢總筆數", typeof(int)));
                                table.Columns.Add(new DataColumn("已作廢總金額", typeof(decimal)));
                                table.TableName = $"月報表({yy.Key}-{mm.Key})";

                                ds.Tables.Add(table);

                                IEnumerable<InvoiceItem> v0, v1;
                                DataRow r;

                                foreach (var item in mm.GroupBy(i => i.InvoiceDate.Value.Day).OrderBy(g => g.Key))
                                {
                                    r = table.NewRow();
                                    r[0] = item.Key.ToString();
                                    v0 = item.Where(i => i.InvoiceCancellation == null);
                                    v1 = item.Where(i => i.InvoiceCancellation != null);
                                    r[1] = v0.Count();
                                    r[2] = v0.Sum(i => i.InvoiceAmountType.TotalAmount);
                                    r[3] = v1.Count();
                                    r[4] = v1.Sum(i => i.InvoiceAmountType.TotalAmount);
                                    table.Rows.Add(r);
                                }

                                v0 = mm.Where(i => i.InvoiceCancellation == null);
                                v1 = mm.Where(i => i.InvoiceCancellation != null);
                                r = table.NewRow();
                                r[0] = "總計";
                                r[1] = v0.Count();
                                r[2] = v0.Sum(i => i.InvoiceAmountType.TotalAmount);
                                r[3] = v1.Count();
                                r[4] = v1.Sum(i => i.InvoiceAmountType.TotalAmount);
                                table.Rows.Add(r);

                            }
                        }

                        using (var xls = ds.ConvertToExcel())
                        {
                            xls.SaveAs(taskItem.ResponsePath);
                        }
                    }

                }
                catch (Exception ex)
                {
                    exception = ex;
                    Logger.Error(ex);
                }

                if (exception != null)
                {
                    taskItem.ExceptionLog = new ExceptionLog
                    {
                        DataContent = exception.Message
                    };
                }

                taskItem.ProcessComplete = DateTime.Now;
                models.SubmitChanges();

                models.Dispose();

            });
        }

        public ActionResult InvoiceSummaryGridPage(int index, int size, InquireInvoiceViewModel viewModel)
        {
            //ViewBag.HasQuery = true;
            ViewBag.ViewModel = viewModel;

            var profile = HttpContext.GetUser();
            DataSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);
            DataSource.BuildQuery();

            if (index > 0)
                index--;
            else
                index = 0;

            ViewBag.PageIndex = index;
            ViewBag.PageSize = size;

            return View(DataSource.Items);
        }


        public ActionResult PrintInvoiceSummary(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            var profile = HttpContext.GetUser();
            DataSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);
            DataSource.BuildQuery();
            ((ModelSource<InvoiceItem>)models).ResultModel = Naming.DataResultMode.Print;

            return View(DataSource.Inquiry);
        }

        public ActionResult DataQueryIndex(DataQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/InvoiceQuery/DataQueryIndex.cshtml");
        }

        public ActionResult InquireData(DataQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            viewModel.CommandText = viewModel.CommandText.GetEfficientString();
            if (viewModel.CommandText == null)
            {
                ModelState.AddModelError("CommandText", "請輸入查詢指令!!");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ModelState = ModelState;
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            return View("~/Views/InvoiceQuery/DataAction/ExecuteDataQuery.cshtml");

        }



    }
}
