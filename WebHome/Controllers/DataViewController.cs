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
using WebHome.Properties;
using ModelCore.Models.ViewModel;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Schema.TXN;

using CommonLib.Utility;
using CommonLib.Security.UseCrypto;
using ModelCore.Schema.EIVO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using CommonLib.Core.Utility;
using CommonLib.Core.Helper;
using Business.Helper.ProcessRequestProcessor;
using ModelCore.InvoiceManagement.Validator;

namespace WebHome.Controllers
{
    
    public class DataViewController : SampleController<InvoiceItem>
    {
        public DataViewController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: DataView
        public ActionResult ShowAllowancePageView(DocumentQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (viewModel.KeyID != null)
            {
                viewModel.id = viewModel.DecryptKeyValue();
            }

            var item = models.GetTable<InvoiceAllowance>().Where(a => a.AllowanceID == viewModel.id).FirstOrDefault();
            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "資料錯誤!!");
            }


            return View("~/Views/DataView/Module/Allowance.cshtml", item);
        }

        public ActionResult ShowAllowance(RenderStyleViewModel viewModel)
        {
            ViewResult result = (ViewResult)ShowAllowancePageView(viewModel);

            InvoiceAllowance? item = result.Model as InvoiceAllowance;
            if (item == null)
                return result;

            String[] useThermalPOSArgs;
            return View(getAllowanceViewPath(item, out useThermalPOSArgs, viewModel.PaperStyle), item);
        }

        protected String getInvoiceViewPath(InvoiceItem item, out String[]? useThermalPOSArgs, String? paperStyle = null, Naming.InvoiceProcessType? processType = null)
        {
            useThermalPOSArgs = null;
            if ((paperStyle == "B2B" || item.InvoiceBuyer.CustomerNumber?.Length > 4) && !item.InvoiceBuyer.IsB2C())
            {
                return "~/Views/DataView/A0401.cshtml";
            }
            else if (paperStyle == "POS")
            {
                useThermalPOSArgs = ThermalPOSPaper;
                return "~/Views/DataView/C0401_POS.cshtml";
            }
            else
            {
                return "~/Views/DataView/C0401_A4.cshtml";
            }
        }

        protected String getAllowanceViewPath(InvoiceAllowance? item,out String[]? useThermalPOSArgs, String? paperStyle = null)
        {
            useThermalPOSArgs = null;
            if (paperStyle == "B2B")
            {
                return "~/Views/DataView/B0401.cshtml";
            }
            else
            {
                useThermalPOSArgs = ThermalPOSPaper;
                return "~/Views/DataView/D0401.cshtml";
            }
        }

        public async Task<ActionResult> PrintSingleInvoiceAsPDFAsync(RenderStyleViewModel viewModel, InquireInvoiceViewModel queryModel)
        {
            ViewResult result = (ViewResult)ShowInvoice(viewModel, queryModel);
            InvoiceItem item = result.Model as InvoiceItem;
            if (item == null)
            {
                return new EmptyResult { };
            }

            this.TempData["viewModel"] = viewModel;
            String[] useThermalPOSArgs;
            String pdfFile = await this.CreateContentAsPDFAsync(getInvoiceViewPath(item, out useThermalPOSArgs, viewModel.PaperStyle), item, Properties.Settings.Default.SessionTimeoutInMinutes, useThermalPOSArgs);

            if (pdfFile != null)
            {
                if (viewModel.NameOnly == true)
                {
                    String outputFile = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, Path.GetFileName(pdfFile));
                    System.IO.File.Move(pdfFile, outputFile);

                    return Content(outputFile);
                }
                else
                {
                    return PhysicalFile(pdfFile, "application/pdf",$"{DateTime.Today:yyyy-MM-dd}.pdf");
                }
            }
            return new EmptyResult { };
        }

        private RouteValueDictionary Merge(object objA, object objB)
        {
            var routeValues = new RouteValueDictionary();

            // Add properties of ClassA to routeValues
            foreach (var prop in objA.GetType().GetProperties())
            {
                var v = prop.GetValue(objA);
                if (v != null)
                {
                    routeValues.Add(prop.Name, v);
                }
            }

            // Add properties of ClassB to routeValues
            foreach (var prop in objB.GetType().GetProperties())
            {
                var v = prop.GetValue(objB);
                if (v != null)
                {
                    if(routeValues.ContainsKey(prop.Name))
                    {
                        routeValues[prop.Name] = v;
                    }
                    else
                    {
                        routeValues.Add(prop.Name, v);
                    }
                }
            }

            return routeValues;
        }

        public ActionResult PrintInvoiceAsPDF(RenderStyleViewModel viewModel, InquireInvoiceViewModel queryModel)
        {
            String workUrl = $"{ModelExtension.Properties.AppSettings.Default.HostUrl}{WebHome.Properties.AppSettings.Default.ApplicationPath}/{Url.Action("ShowInvoice", "DataView", Merge(viewModel, queryModel))}";
            String pdfFile = Path.Combine(Logger.LogDailyPath, $"{Guid.NewGuid()}.pdf");

            workUrl.ConvertHtmlToPDF(pdfFile, 1);

            if (pdfFile.AssertFile())
            {
                var content = System.IO.File.ReadAllBytes(pdfFile);
                System.IO.File.Delete(pdfFile);
                return File(content, "application/pdf", $"{DateTime.Today:yyyy-MM-dd}.pdf");
            }

            return new EmptyResult { };

        }

        public async Task<ActionResult> GetCustomerInvoicePDFAsync(RenderStyleViewModel viewModel, bool? ackDel, bool? html)
        {
            ViewBag.ViewModel = viewModel;
            viewModel.UseCustomView = viewModel.UseCustomView ?? true;
            this.TempData["viewModel"] = viewModel;

            if (viewModel.KeyID != null)
            {
                viewModel.DocID = viewModel.DecryptKeyValue();
            }

            if (ackDel == true)
            {
                models.ExecuteCommand("delete DocumentSubscriptionQueue where DocID = {0}", viewModel.DocID);
                return Json(new { result = true });
            }
            else if (html == true)
            {
                models.ExecuteCommand("delete DocumentSubscriptionQueue where DocID = {0}", viewModel.DocID);
            }

            var item = models.GetTable<InvoiceItem>().Where(i => i.InvoiceID == viewModel.DocID).FirstOrDefault();

            if (item != null)
            {
                if (html == true)
                {
                    String[] useThermalPOSArgs;
                    return View(getInvoiceViewPath(item, out useThermalPOSArgs, viewModel.PaperStyle), item);
                }
                else
                {
                    String outputFile = await GetInvoicePDFAsync(item, viewModel);
                    //return PhysicalFile(outputFile, "application/pdf", $"{item.TrackCode}{item.No}.pdf");
                    if(System.IO.File.Exists(outputFile))
                    {
                        var buf = System.IO.File.ReadAllBytes(outputFile);
                        try
                        {
                            System.IO.File.Delete(outputFile);
                        }
                        catch(Exception ex)
                        {
                            Logger.Warn(ex.ToString());
                        }
                        return File(buf, "application/pdf", $"{item.TrackCode}{item.No}.pdf");
                    }
                }
            }

            return new EmptyResult { };

        }

        protected async Task<string> GetInvoicePDFAsync(InvoiceItem item, RenderStyleViewModel viewModel)
        {
            String outputFile = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogPath.GetDateStylePath(item.InvoiceDate.Value), String.Format("{0}{1}.pdf", item.TrackCode, item.No));
            if (viewModel.CreateNew != true && System.IO.File.Exists(outputFile))
            {

            }
            else
            {
                String[] useThermalPOSArgs;
                this.TempData["viewModel"] = viewModel;
                String pdfFile = await this.CreateContentAsPDFAsync(getInvoiceViewPath(item, out useThermalPOSArgs, viewModel.PaperStyle), item, Properties.Settings.Default.SessionTimeoutInMinutes, useThermalPOSArgs);
                if (pdfFile != null)
                {
                    if (System.IO.File.Exists(outputFile))
                    {
                        System.IO.File.Delete(outputFile);
                    }
                    System.IO.File.Move(pdfFile, outputFile);
                }
            }

            return outputFile;
        }

        protected async Task<string> GetInvoicePdfFileAsync(InvoiceItem item, RenderStyleViewModel viewModel)
        {
            String[] useThermalPOSArgs;
            this.TempData["viewModel"] = viewModel;
            return await this.CreateContentAsPDFAsync(getInvoiceViewPath(item, out useThermalPOSArgs, viewModel.PaperStyle, viewModel.ProcessType), item, ModelExtension.Properties.AppSettings.Default.SessionTimeout, useThermalPOSArgs);
        }

        public async Task<ActionResult> ZipInvoicePDFAsync(RenderStyleViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            var profile = HttpContext.GetUser();

            if (viewModel.ProcessModel == ProcessRequestViewModel.ProcessModelDefinition.ByTask)
            {
                viewModel.Message = "發票列印下載";

                ProcessRequest processItem = new ProcessRequest
                {
                    Sender = profile.UID,
                    SubmitDate = DateTime.Now,
                    ProcessStart = DateTime.Now,
                    ResponsePath = System.IO.Path.Combine(Logger.LogDailyPath, Guid.NewGuid().ToString() + ".zip"),
                    ViewModel = viewModel.JsonStringify(),
                };
                models.GetTable<ProcessRequest>().InsertOnSubmit(processItem);
                models.SubmitChanges();

                viewModel.TaskID = processItem.TaskID;
                viewModel.Push($"{DateTime.Now.Ticks}.json");

                Task.Run(() =>
                {
                    processItem.TaskID.ZipInvoicePDF();
                });

                return View("~/Views/Shared/Module/PromptCheckDownload.cshtml",
                        new AttachmentViewModel
                        {
                            TaskID = processItem.TaskID,
                            FileName = processItem.ResponsePath,
                            FileDownloadName = "發票列印下載.zip",
                        });
            }
            else
            {
                String outFile = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, Guid.NewGuid().ToString() + ".zip");

                var items = models.GetTable<DocumentPrintQueue>()
                    .Where(i => i.UID == profile.UID)
                    .Join(models.GetTable<CDS_Document>()
                            .Where(d => d.DocType == (int)Naming.DocumentTypeDefinition.E_Invoice),
                        i => i.DocID, d => d.DocID, (i, d) => i);
    
                //Response.Clear();
                //
                //
                viewModel.FileDownloadToken = viewModel.FileDownloadToken.GetEfficientString();
                if (viewModel.FileDownloadToken != null)
                {
                    Response.Cookies.Append("FileDownloadToken", viewModel.FileDownloadToken);
                }
                //Response.Headers.Add("Cache-control", "max-age=1");
                //Response.ContentType = "application/octet-stream";
                //Response.Headers.Add("Content-Disposition", String.Format("attachment;filename={0}({1:yyyy-MM-dd HH-mm-ss}).zip", HttpUtility.UrlEncode("電子發票下載"), DateTime.Now));
    
                using (var zipOut = System.IO.File.Create(outFile))
                {
                    using (ZipArchive zip = new ZipArchive(zipOut, ZipArchiveMode.Create))
                    {
                        foreach (var doc in items.ToArray())
                        {
                            InvoiceItem item = doc.CDS_Document.InvoiceItem;
                            var pdfFile = await GetInvoicePDFAsync(item, viewModel);
                            models.MarkPrintedLog(item, profile);
    
                            zip.CreateEntryFromFile(pdfFile, Path.GetFileName(pdfFile));
                        }
                    }
                }
    
                var result = new VirtualFileResult(outFile, "application/octet-stream");
                result.FileDownloadName = "發票列印下載.zip";
                return result;
            }
        }

        public async Task<ActionResult> GetCustomerAllowancePDFAsync(RenderStyleViewModel viewModel, bool? ackDel, bool? html)
        {
            ViewBag.ViewModel = viewModel;
            this.TempData["viewModel"] = viewModel;

            if (viewModel.KeyID != null)
            {
                viewModel.DocID = viewModel.DecryptKeyValue();
            }

            if (ackDel == true)
            {
                models.ExecuteCommand("delete DocumentSubscriptionQueue where DocID = {0}", viewModel.DocID);
                return Json(new { result = true });
            }
            else if (html == true)
            {
                models.ExecuteCommand("delete DocumentSubscriptionQueue where DocID = {0}", viewModel.DocID);
            }

            var item = models.GetTable<InvoiceAllowance>().Where(i => i.AllowanceID == viewModel.DocID).FirstOrDefault();

            if (item != null)
            {

                models.ExecuteCommand(@"INSERT INTO [proc].DataProcessLog
                                                            (DocID, LogDate, Status, StepID)
                                                            VALUES          ({0},{1},{2},{3})",
                        item.AllowanceID, DateTime.Now, (int)Naming.DataProcessStatus.Done,
                        (int)Naming.InvoiceStepDefinition.PDF待傳輸);

                if (html == true)
                {
                    String[] useThermalPOSArgs;
                    return View(getAllowanceViewPath(item, out useThermalPOSArgs, viewModel.PaperStyle), item);
                }
                else
                {
                    //String outputFile = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogPath.GetDateStylePath(item.AllowanceDate.Value), $"{item.AllowanceNumber}.pdf");
                    //if (System.IO.File.Exists(outputFile))
                    //{
                    //    return PhysicalFile(outputFile, "application/pdf");
                    //}
                    //else
                    //{
                    //    String[] useThermalPOSArgs;
                    //    String pdfFile = this.CreateContentAsPDF(getAllowanceViewPath(item, out useThermalPOSArgs), item, Properties.Settings.Default.SessionTimeoutInMinutes, useThermalPOSArgs);
                    //    if (pdfFile != null)
                    //    {
                    //        System.IO.File.Move(pdfFile, outputFile);
                    //        return PhysicalFile(outputFile, "application/pdf", $"{DateTime.Today:yyyy-MM-dd}.pdf");
                    //    }
                    //}
                    String[] useThermalPOSArgs;
                    String pdfFile = await this.CreateContentAsPDFAsync(getAllowanceViewPath(item, out useThermalPOSArgs, viewModel.PaperStyle), item, Properties.Settings.Default.SessionTimeoutInMinutes, useThermalPOSArgs);
                    if (pdfFile != null)
                    {
                        Response.Clear();
                        Response.Headers.Add("Cache-control", "max-age=1");
                        Response.ContentType = "application/octet-stream";
                        Response.Headers.Add("Content-Disposition", $"attachment;filename={DateTime.Today:yyyy-MM-dd}.pdf");

                        using (FileStream fs = System.IO.File.OpenRead(pdfFile))
                        {
                            await fs.CopyToAsync(Response.Body);
                            fs.Close();
                        }
                        await Response.Body.FlushAsync();

                        System.IO.File.Delete(pdfFile);
                    }
                }
            }

            return new EmptyResult { };

        }

        public ActionResult ShowInvoicePageView(DocumentQueryViewModel viewModel, InquireInvoiceViewModel queryModel)
        {
            ViewBag.ViewModel = viewModel;

            if (viewModel.KeyID != null)
            {
                viewModel.DocID = viewModel.DecryptKeyValue();
            }

            var item = models.GetTable<InvoiceItem>().Where(a => a.InvoiceID == viewModel.DocID).FirstOrDefault();
            if(item == null)
            {
                if(queryModel != null)
                {
                    if(queryModel.InvoiceDate.HasValue)
                    {
                        var seller = models.GetTable<Organization>().Where(s => s.ReceiptNo == queryModel.ReceiptNo).FirstOrDefault();
                        if(seller!=null)
                        {
                            queryModel.InvoiceNo = queryModel.InvoiceNo.GetEfficientString();
                            var match = queryModel.InvoiceNo.ParseInvoiceNo();
                            if (match.Success)
                            {
                                item = models.GetTable<InvoiceItem>()
                                    .Where(i => i.TrackCode == match.Groups[1].Value && i.No == match.Groups[2].Value)
                                    .Where(i => i.InvoiceDate >= queryModel.InvoiceDate && i.InvoiceDate < queryModel.InvoiceDate.Value.AddDays(1))
                                    .Where(i => i.SellerID == seller.CompanyID)
                                    .FirstOrDefault();
                            }
                        }

                    }
                }
            }
            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "資料錯誤!!");
            }

            return View("~/Views/DataView/ShowInvoicePageView.cshtml", item);
        }

        public ActionResult ShowInvoice(RenderStyleViewModel viewModel, InquireInvoiceViewModel queryModel)
        {
            ViewResult result = (ViewResult)ShowInvoicePageView(viewModel, queryModel);

            InvoiceItem item = result.Model as InvoiceItem;
            if (item == null)
                return result;
            String[] useThermalPOSArgs;
            return View(getInvoiceViewPath(item, out useThermalPOSArgs, viewModel.PaperStyle), item);
        }

        public ActionResult ShowInvoiceContent(DocumentQueryViewModel viewModel, InquireInvoiceViewModel queryModel)
        {
            ViewResult result = (ViewResult)ShowInvoicePageView(viewModel, queryModel);

            InvoiceItem item = result.Model as InvoiceItem;
            if (item == null)
                return result;

            return View("~/Views/DataView/Module/InvoiceContent.cshtml", item);
        }

        public ActionResult PrintSingleInvoice(RenderStyleViewModel viewModel, InquireInvoiceViewModel queryModel)
        {
            ViewResult result = (ViewResult)ShowInvoicePageView(viewModel, queryModel);

            InvoiceItem item = result.Model as InvoiceItem;
            if (item == null)
                return result;

            return View("~/Views/DataView/PrintSingleInvoice.cshtml", item);
        }

        [Authorize]
        public ActionResult PrintA0401(RenderStyleViewModel viewModel)
        {
            viewModel.PaperStyle = viewModel.PaperStyle ?? "B2B";
            return PrintInvoice(viewModel);
        }

        [Authorize]
        public ActionResult PrintB0401()
        {
            var profile = HttpContext.GetUser();

            var items = models.GetTable<DocumentPrintQueue>()
                .Where(i => i.UID == profile.UID)
                .Join(models.GetTable<CDS_Document>()
                        .Where(d => d.DocType == (int)Naming.DocumentTypeDefinition.E_Allowance),
                    i => i.DocID, d => d.DocID, (i, d) => i);

            return View("~/Views/DataView/PrintB0401.cshtml", items);
        }

        public async Task<ActionResult> PrintA0401AsPDFAsync(RenderStyleViewModel viewModel)
        {
            ViewResult result = (ViewResult)PrintA0401(viewModel);
            IQueryable<DocumentPrintQueue> items = result.Model as IQueryable<DocumentPrintQueue>;
            String pdfFile = await this.CreateContentAsPDFAsync("~/Views/DataView/PrintA0401.cshtml", items, Properties.Settings.Default.SessionTimeoutInMinutes);

            if (pdfFile != null)
            {
                return PhysicalFile(pdfFile, "application/pdf", $"{DateTime.Today:yyyy-MM-dd}.pdf");
            }
            else
            {
                ViewBag.CloseWindow = true;
                return View("~/Views/Shared/AlertMessage.cshtml", model: "資料錯誤!!");
            }
        }

        public async Task<ActionResult> PrintB0401AsPDFAsync()
        {
            ViewResult result = (ViewResult)PrintB0401();
            IQueryable<DocumentPrintQueue> items = result.Model as IQueryable<DocumentPrintQueue>;
            String pdfFile = await this.CreateContentAsPDFAsync("~/Views/DataView/PrintB0401.cshtml", items, Properties.Settings.Default.SessionTimeoutInMinutes);

            if (pdfFile != null)
            {
                return PhysicalFile(pdfFile, "application/pdf", $"{DateTime.Today:yyyy-MM-dd}.pdf");
            }
            else
            {
                ViewBag.CloseWindow = true;
                return View("~/Views/Shared/AlertMessage.cshtml", model: "資料錯誤!!");
            }
        }

        [Authorize]
        public ActionResult PrintInvoice(RenderStyleViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            var profile = HttpContext.GetUser();

            var items = models!.GetTable<DocumentPrintQueue>()
                .Where(i => i.UID == profile.UID)
                .Join(models.GetTable<CDS_Document>()
                    .Where(d => d.DocType == (int)Naming.DocumentTypeDefinition.E_Invoice),
                    i => i.DocID, d => d.DocID, (i, d) => i);

            if(viewModel.PaperStyle == "B2B")
                return View("~/Views/DataView/PrintA0401.cshtml", items);
            if (viewModel.PaperStyle == "A4")
                return View("~/Views/DataView/PrintC0401A4.cshtml", items);
            else
                return View("~/Views/DataView/PrintC0401POS.cshtml", items);
        }

        [Authorize]
        public ActionResult PrintC0401(RenderStyleViewModel viewModel)
        {
            return PrintInvoice(viewModel);
        }

        [Authorize]
        public ActionResult PrintD0401()
        {
            var profile = HttpContext.GetUser();

            var items = models.GetTable<DocumentPrintQueue>()
                .Where(i => i.UID == profile.UID)
                .Join(models.GetTable<CDS_Document>()
                        .Where(d => d.DocType == (int)Naming.DocumentTypeDefinition.E_Allowance),
                    i => i.DocID, d => d.DocID, (i, d) => i);

            return View("~/Views/DataView/PrintD0401.cshtml", items);
        }

        public static readonly String[] ThermalPOSPaper = new String[] { Settings.Default.ThermalPOS };

        public async Task<ActionResult> PrintC0401AsPDFAsync(RenderStyleViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            this.TempData["viewModel"] = viewModel;
            ViewResult result = (ViewResult)PrintC0401(viewModel);
            IQueryable<DocumentPrintQueue> items = result.Model as IQueryable<DocumentPrintQueue>;
            String pdfFile = viewModel.PaperStyle == "A4"
                    ? await this.CreateContentAsPDFAsync("~/Views/DataView/PrintC0401A4.cshtml", items, Properties.Settings.Default.SessionTimeoutInMinutes)
                    : await this.CreateContentAsPDFAsync("~/Views/DataView/PrintC0401POS.cshtml", items, Properties.Settings.Default.SessionTimeoutInMinutes, ThermalPOSPaper);

            if (pdfFile != null)
            {
                return PhysicalFile(pdfFile, "application/pdf", $"{DateTime.Today:yyyy-MM-dd}.pdf");
            }
            else
            {
                ViewBag.CloseWindow = true;
                return View("~/Views/Shared/AlertMessage.cshtml", model: "資料錯誤!!");
            }
        }

        public async Task<ActionResult> PrintD0401AsPDFAsync()
        {
            ViewResult result = (ViewResult)PrintD0401();
            IQueryable<DocumentPrintQueue> items = result.Model as IQueryable<DocumentPrintQueue>;
            String pdfFile = await this.CreateContentAsPDFAsync("~/Views/DataView/PrintD0401.cshtml", items, Properties.Settings.Default.SessionTimeoutInMinutes, ThermalPOSPaper);

            if (pdfFile != null)
            {
                return PhysicalFile(pdfFile, "application/pdf", $"{DateTime.Today:yyyy-MM-dd}.pdf");
            }
            else
            {
                ViewBag.CloseWindow = true;
                return View("~/Views/Shared/AlertMessage.cshtml", model: "資料錯誤!!");
            }
        }

        public async Task<ActionResult> GetLeftQRCodeAsync(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            IQueryable<InvoiceItem> items = models.GetTable<InvoiceItem>();
            if (viewModel.InvoiceID.HasValue)
            {
                items = items.Where(i => i.InvoiceID == viewModel.InvoiceID);
            }
            viewModel.InvoiceNo = viewModel.InvoiceNo.GetEfficientString();
            if (viewModel.InvoiceNo != null && viewModel.InvoiceNo.Length == 10)
            {
                items = items.Where(i => i.TrackCode == viewModel.InvoiceNo.Substring(0, 2) && i.No == viewModel.InvoiceNo.Substring(2));
            }

            var item = items.FirstOrDefault();
            if (item != null)
            {
                bool retry = false;
                String qrContent = null;
                try
                {
                    qrContent = item.GetQRCodeContent();
                    using (Bitmap qrcode = qrContent.CreateQRCode(width: 180, height: 180,qrVersion: 10))
                    {
                        Response.Clear();
                        Response.ContentType = "image/jpeg";
                        using (FileBufferingWriteStream output = new FileBufferingWriteStream())
                        {
                            qrcode.Save(output, ImageFormat.Jpeg);
                            ////output.Seek(0, SeekOrigin.Begin);
                            await output.DrainBufferAsync(Response.Body);
                            //await output.CopyToAsync(Response.Body);
                        }
                    }
                }
                catch(Exception ex)
                {
                    retry = true;
                    CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                    CommonLib.Core.Utility.FileLogger.Logger.Warn($"產生發票QR Code失敗 => {item.InvoiceID},\r\n{qrContent}\r\n{ex}");
                }

                if(retry)
                {
                    try
                    {
                        qrContent = $"{qrContent.Substring(0, 88)}:1:0:1:品項過長，詳列於發票明細:1:1:";
                        using (Bitmap qrcode = qrContent.CreateQRCode(width: 180, height: 180, qrVersion: 10))
                        {
                            Response.Clear();
                            Response.ContentType = "image/jpeg";
                            using (FileBufferingWriteStream output = new FileBufferingWriteStream())
                            {
                                qrcode.Save(output, ImageFormat.Jpeg);
                                ////output.Seek(0, SeekOrigin.Begin);
                                await output.DrainBufferAsync(Response.Body);
                                //await output.CopyToAsync(Response.Body);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                        CommonLib.Core.Utility.FileLogger.Logger.Warn($"產生發票QR Code失敗 => {item.InvoiceID},\r\n{qrContent}\r\n{ex}");
                    }
                }
            }

            return new EmptyResult();

        }

        public async Task<ActionResult> GetRightQRCodeAsync(InquireInvoiceViewModel viewModel)
        {

            using (Bitmap qrcode = "**".CreateQRCode(width: 180, height: 180, qrVersion: 10))
            {
                Response.Clear();
                Response.ContentType = "image/jpeg";
                using (FileBufferingWriteStream output = new FileBufferingWriteStream())
                {
                    qrcode.Save(output, ImageFormat.Jpeg);
                    ////output.Seek(0, SeekOrigin.Begin);
                    await output.DrainBufferAsync(Response.Body);
                    //await output.CopyToAsync(Response.Body);
                }
            }

            return new EmptyResult();

        }

        public async Task<ActionResult> PrintSingleAllowanceAsPDFAsync(RenderStyleViewModel viewModel)
        {
            ViewResult result = (ViewResult)ShowAllowance(viewModel);
            InvoiceAllowance? item = result.Model as InvoiceAllowance;
            if (item == null)
            {
                return new EmptyResult { };
            }

            String[] useThermalPOSArgs;
            String pdfFile = await this.CreateContentAsPDFAsync(getAllowanceViewPath(item, out useThermalPOSArgs, viewModel.PaperStyle), item, Properties.Settings.Default.SessionTimeoutInMinutes, useThermalPOSArgs);

            if (pdfFile != null)
            {
                if (viewModel.NameOnly == true)
                {
                    String outputFile = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, Path.GetFileName(pdfFile));
                    System.IO.File.Move(pdfFile, outputFile);

                    return Content(outputFile);
                }
                else
                {
                    return PhysicalFile(pdfFile, "application/pdf", $"{DateTime.Today:yyyy-MM-dd}.pdf");
                }
            }

            return new EmptyResult { };

        }

        public ActionResult ConvertDataToAllowance()
        {
            XmlDocument uploadData = new XmlDocument();
            Request.Body.Position = 0;
            uploadData.Load(Request.Body);
            AllowanceRoot allowance = uploadData.TrimAll().ConvertTo<AllowanceRoot>();
            return View(allowance);
        }

        public ActionResult ZipInvoicePackagePDF(RenderStyleViewModel viewModel, String jsonData)
        {
            var items = JsonConvert.DeserializeObject<MailTrackingCsvViewModel[]>(jsonData);
            if (items == null || items.Length == 0)
            {
                ViewBag.CloseWindow = true;
                ViewBag.Message = "請選擇郵寄項目!!";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }

            ViewBag.ViewModel = viewModel;
            Response.Cookies.Append("FileDownloadToken", viewModel.FileDownloadToken);

            ProcessRequest processItem = new ProcessRequest
            {
                Sender = HttpContext.GetUser()?.UID,
                SubmitDate = DateTime.Now,
                ProcessStart = DateTime.Now,
                ResponsePath = System.IO.Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, Guid.NewGuid().ToString() + ".zip"),
                ViewModel = viewModel.JsonStringify(),
            };
            models.GetTable<ProcessRequest>().InsertOnSubmit(processItem);
            models.SubmitChanges();

            viewModel.TaskID = processItem.TaskID;
            viewModel.Push($"{DateTime.Now.Ticks}.json");

            if (viewModel.ForMailingPackage == true)
            {
                Task.Run(() =>
                {
                    processItem.TaskID.ProcessInvoiceMailingPackage(items);
                });
            }
            else
            {
                Task.Run(() =>
                {
                    processItem.TaskID.ProcessInvoicePdfPackage(items);
                });
            }

            return View("~/Views/Shared/Module/PromptCheckDownload.cshtml",
                    new AttachmentViewModel
                    {
                        TaskID = processItem.TaskID,
                        FileName = processItem.ResponsePath,
                        FileDownloadName = "發票列印下載.zip",
                    });

//            String outFile = processItem.ResponsePath;
//            if (viewModel.ForMailingPackage == true)
//            {
//                ProcessInvoiceMailingPackageAsync(viewModel, items, outFile);
//            }
//            else
//            {
//                ProcessInvoicePdfPackageAsync(viewModel, items, outFile);
//            }
//
//            var result = new VirtualFileResult(outFile, "application/octet-stream");
//            result.FileDownloadName = "發票列印下載.zip";
//            return result;
        }

        private async Task ProcessInvoicePdfPackageAsync(RenderStyleViewModel viewModel, MailTrackingCsvViewModel[] items, string outFile)
        {
            using (var zipOut = System.IO.File.Create(outFile))
            {
                using (ZipArchive zip = new ZipArchive(zipOut, ZipArchiveMode.Create))
                {
                    int packageIdx = 1;
                    foreach (var g in items)
                    {
                        int idx = 1;
                        foreach (var v in g.InvoiceID)
                        {
                            InvoiceItem item = models.GetTable<InvoiceItem>().Where(i => i.InvoiceID == v).FirstOrDefault();
                            if (item == null)
                                continue;
                            var pdfFile = await GetInvoicePDFAsync(item, viewModel);

                            zip.CreateEntryFromFile(pdfFile, $"{packageIdx:000000}-{idx++:000}-{Path.GetFileName(pdfFile)}");

                            foreach (var attach in item.CDS_Document.Attachment)
                            {
                                if (System.IO.File.Exists(attach.StoredPath))
                                {
                                    zip.CreateEntryFromFile(attach.StoredPath, $"{packageIdx:000000}-{idx++:000}-{Path.GetFileName(attach.StoredPath)}");
                                }
                            }

                        }
                        packageIdx++;
                    }
                }
            }
        }

        private async Task ProcessInvoiceMailingPackageAsync(RenderStyleViewModel viewModel, MailTrackingCsvViewModel[] items, string outFile)
        {
            using (var zipOut = System.IO.File.Create(outFile))
            {
                using (ZipArchive zip = new ZipArchive(zipOut, ZipArchiveMode.Create))
                {
                    int packageIdx = 1;
                    List<String> pdfItems = new List<string>();
                    List<String> attachmentItems = new List<string>();

                    foreach (var g in items)
                    {
                        InvoiceItem item = null, idxItem = null;
                        String pdfPackage = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, $"{Guid.NewGuid()}.pdf");
                        bool isTemp = false;
                        pdfItems.Clear();
                        attachmentItems.Clear();

                        foreach (var v in g.InvoiceID)
                        {
                            item = models.GetTable<InvoiceItem>().Where(i => i.InvoiceID == v).FirstOrDefault();
                            if (item == null)
                                continue;

                            if (idxItem == null)
                            {
                                idxItem = item;
                            }

                            var pdfFile = await GetInvoicePDFAsync(item, viewModel);
                            pdfItems.Add(pdfFile);

                            foreach (var attach in item.CDS_Document.Attachment)
                            {
                                if (System.IO.File.Exists(attach.StoredPath))
                                {
                                    attachmentItems.Add(attach.StoredPath);
                                }
                            }
                        }

                        pdfItems.AddRange(attachmentItems);

                        if (pdfItems.Count > 0)
                        {
                            if (pdfItems.Count > 1)
                            {
                                pdfPackage.MergePDF(pdfItems);
                                isTemp = true;
                            }
                            else
                            {
                                pdfPackage = pdfItems[0];
                            }
                            zip.CreateEntryFromFile(pdfPackage, $"{packageIdx:000000}-{idxItem.TrackCode}{idxItem.No}-{item.InvoiceBuyer.CustomerName.EscapeFileNameCharacter('_')}.pdf");
                        }

                        packageIdx++;

                        if (isTemp)
                        {
                            try
                            {
                                System.IO.File.Delete(pdfPackage);
                            }
                            catch (Exception ex)
                            {
                                CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                            }
                        }
                    }
                }
            }
        }


        //private void ProcessInvoiceMailingPackage(RenderStyleViewModel viewModel, MailTrackingCsvViewModel[] items, string outFile)
        //{
        //    using (var zipOut = System.IO.File.Create(outFile))
        //    {
        //        using (ZipArchive zip = new ZipArchive(zipOut, ZipArchiveMode.Create))
        //        {
        //            int packageIdx = 1;
        //            List<String> pdfItems = new List<string>();

        //            foreach (var g in items)
        //            {
        //                InvoiceItem item = null,idxItem = null;
        //                String pdfPackage = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, $"{Guid.NewGuid()}.pdf");
        //                bool isTemp = false;
        //                pdfItems.Clear();

        //                foreach (var v in g.InvoiceID)
        //                {
        //                    item = models.GetTable<InvoiceItem>().Where(i => i.InvoiceID == v).FirstOrDefault();
        //                    if (item == null)
        //                        continue;

        //                    if (idxItem == null)
        //                    {
        //                        idxItem = item;
        //                    }

        //                    var pdfFile = GetInvoicePDF(item, viewModel);
        //                    pdfItems.Add(pdfFile);

        //                    foreach (var attach in item.CDS_Document.Attachment)
        //                    {
        //                        if (System.IO.File.Exists(attach.StoredPath))
        //                        {
        //                            pdfItems.Add(attach.StoredPath);
        //                        }
        //                    }
        //                }

        //                if (pdfItems.Count > 0)
        //                {
        //                    if (pdfItems.Count > 1)
        //                    {
        //                        pdfPackage.MergePDF(pdfItems);
        //                        isTemp = true;
        //                    }
        //                    else
        //                    {
        //                        pdfPackage = pdfItems[0];
        //                    }
        //                    zip.CreateEntryFromFile(pdfPackage, $"{packageIdx:000000}-{idxItem.TrackCode}{idxItem.No}-{item.InvoiceBuyer.CustomerName}.pdf");
        //                }

        //                packageIdx++;

        //                if(isTemp)
        //                {
        //                    try
        //                    {
        //                        System.IO.File.Delete(pdfPackage);
        //                    }
        //                    catch(Exception ex)
        //                    {
        //                        CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

    }
}