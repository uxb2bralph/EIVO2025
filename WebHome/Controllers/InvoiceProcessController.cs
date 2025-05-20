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
using ModelCore.Models.ViewModel;
using WebHome.Properties;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;

using CommonLib.Utility;
using CommonLib.DataAccess;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using CommonLib.Core.Utility;
using WebHome.Helper.Security.Authorization;
using ModelCore.Resource;
using DocumentFormat.OpenXml.Spreadsheet;

namespace WebHome.Controllers
{
    [Authorize]
    public class InvoiceProcessController : SampleController<InvoiceItem>
    {
        protected UserProfile _userProfile;

        public InvoiceProcessController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public static ModelSourceInquiry<InvoiceItem> CreateInvoiceItemInquiry(Controller controller, InquireInvoiceViewModel? viewModel)
        {
            var profile = controller.HttpContext.GetUser();
            return viewModel.CreateInvoiceInquiry(profile);
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS, Naming.RoleID.ROLE_SELLER })]
        public ActionResult Index(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ResultAction = "Common";
            //DataLoadOptions ops = new DataLoadOptions();
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceBuyer);
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceCancellation);
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceAmountType);
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceSeller);
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceWinningNumber);
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceCarrier);
            //ops.LoadWith<InvoiceItem>(i => i.InvoicePurchaseOrder);
            //models.DataContext.LoadOptions = ops;

            ViewBag.ViewModel = viewModel;

            DataSource.Inquiry = CreateInvoiceItemInquiry(this, viewModel);

            var profile = HttpContext.GetUser();

            switch ((Naming.CategoryID)profile.CurrentUserRole.OrganizationCategory.CategoryID)
            {
                case Naming.CategoryID.COMP_E_INVOICE_B2C_SELLER:
                case Naming.CategoryID.COMP_INVOICE_AGENT:
                    ViewBag.InquiryView = "~/Views/InvoiceProcess/InvoiceQueryBySeller.cshtml";
                    break;
                case Naming.CategoryID.COMP_E_INVOICE_B2C_BUYER:
                    ViewBag.InquiryView = "~/Views/InvoiceProcess/InvoiceQueryByBuyer.cshtml";
                    break;
                default:
                    ViewBag.InquiryView = "~/Views/InvoiceProcess/Module/InvoiceQuery.cshtml";
                    break;
            }

            return View("~/Views/InvoiceProcess/Index2023.cshtml", DataSource.Inquiry);

        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS, Naming.RoleID.ROLE_SELLER })]
        public ActionResult Index2023(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ResultAction = "Common";
            ViewBag.ViewModel = viewModel;
            DataSource.Inquiry = CreateInvoiceItemInquiry(this, viewModel);

            return View("~/Views/InvoiceProcess/Index2023.cshtml", DataSource.Inquiry);

        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS, Naming.RoleID.ROLE_SELLER })]
        public ActionResult IssuingNotice(InquireInvoiceViewModel viewModel)
        {
            ViewResult result = (ViewResult)Index(viewModel);
            result.ViewName = "Index2023";

            ViewBag.InquiryView = "~/Views/InvoiceProcess/InvoiceQueryForNotice.cshtml";
            return result;
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS, Naming.RoleID.ROLE_SELLER })]
        public ActionResult InquireToCancel(InquireInvoiceViewModel viewModel)
        {
            ViewResult result = (ViewResult)Index(viewModel);
            viewModel.ActionTitle = "作廢電子發票";
            viewModel.CommitAction = "CancelInvoice";
            result.ViewName = "Index2023";
            viewModel.ResultAction = "CancelInvoice";
            ViewBag.InquiryView = "~/Views/InvoiceProcess/InvoiceQueryForAction.cshtml";
            return result;
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS, Naming.RoleID.ROLE_SELLER })]
        public ActionResult InquireToIssueAllowance(InquireInvoiceViewModel viewModel)
        {
            ViewResult result = (ViewResult)Index(viewModel);
            viewModel.ActionTitle = "開立折讓證明";
            viewModel.CommitAction = "IssueAllowance";
            result.ViewName = "Index2023";
            viewModel.ResultAction = "IssueAllowance";
            ViewBag.InquiryView = "~/Views/InvoiceProcess/InvoiceQueryForAction.cshtml";
            return result;
        }


        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS, Naming.RoleID.ROLE_SELLER })]
        public ActionResult InquireToMIG(InquireInvoiceViewModel viewModel)
        {
            ViewResult result = (ViewResult)Index(viewModel);
            result.ViewName = "Index2023";
            viewModel.ResultAction = "CreateMIG";
            ViewBag.InquiryView = "~/Views/InvoiceProcess/InvoiceQueryForMIG.cshtml";
            return result;
        }

        public ActionResult InquireForIncoming(InquireInvoiceViewModel viewModel)
        {
            ViewResult result = (ViewResult)Index(viewModel);
            result.ViewName = "Index2023";
            ViewBag.ResultAction = "Incoming";
            ViewBag.InquiryView = "~/Views/InvoiceProcess/InvoiceQueryByBuyer.cshtml";
            return result;
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS, Naming.RoleID.ROLE_SELLER })]
        public ActionResult InquireToAuthorize(InquireInvoiceViewModel viewModel)
        {
            ViewResult result = (ViewResult)Index(viewModel);
            result.ViewName = "Index2023";
            viewModel.ResultAction = "Authorize";
            ViewBag.Title = "核准重印發票";
            //ViewBag.InquiryView = "~/Views/InvoiceProcess/InvoiceQueryForNotice.cshtml";

            return result;
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS, Naming.RoleID.ROLE_SELLER })]
        public ActionResult InquireToVoid(InquireInvoiceViewModel viewModel)
        {
            ViewResult result = (ViewResult)Index(viewModel);
            result.ViewName = "Index2023";
            viewModel.ResultAction = "Void";
            ViewBag.Title = "註銷發票(F0701)";
            //ViewBag.InquiryView = "~/Views/InvoiceProcess/InvoiceQueryForNotice.cshtml";

            return result;
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS })]
        public ActionResult AllowToVoid(InquireInvoiceViewModel viewModel)
        {
            ViewResult result = (ViewResult)Index(viewModel);
            result.ViewName = "Index2023";
            ViewBag.ResultAction = "Allow";
            ViewBag.Title = "核准註銷發票(C0701)";
            ViewBag.InquiryView = "~/Views/InvoiceProcess/InvoiceQueryForAllowingToVoid.cshtml";

            return result;
        }


        public ActionResult Inquire(InquireInvoiceViewModel viewModel)
        {
            //ViewBag.HasQuery = true;
            //DataLoadOptions ops = new DataLoadOptions();
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceBuyer);
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceCancellation);
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceAmountType);
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceSeller);
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceWinningNumber);
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceCarrier);
            //ops.LoadWith<InvoiceItem>(i => i.InvoicePurchaseOrder);
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceDetails);
            //ops.LoadWith<InvoiceDetail>(i => i.InvoiceProduct);
            ////ops.LoadWith<InvoiceProduct>(i => i.InvoiceProductItem);

            //models.DataContext.LoadOptions = ops;

            ViewBag.ViewModel = viewModel;
            var profile = HttpContext.GetUser();
            DataSource.BuildInvoiceQuery(viewModel, profile, viewModel?.ResultAction);

            ViewBag.DataItemView = "~/Views/InvoiceProcess/DataQuery/PrepareInvoiceQueryDataItem.cshtml";

            if (viewModel!.PageIndex.HasValue)
            {
                viewModel.PageIndex--;
                return View("~/Views/InvoiceProcess/DataQuery/InvoiceQueryList.cshtml", DataSource.Items);
            }
            else
            {
                viewModel.PageIndex = 0;
                //checkQueryAction(viewModel.ResultAction);
                return View("~/Views/InvoiceProcess/Module/InvoiceQueryResult.cshtml", DataSource.Items);
            }
        }

        public ActionResult Inquire2023(InquireInvoiceViewModel viewModel)
        {
            bool hasPaging = viewModel.PageIndex.HasValue;
            ViewResult result = (ViewResult)Inquire(viewModel);
            IQueryable<InvoiceItem> items = result.Model as IQueryable<InvoiceItem>;
            viewModel.RecordCount = items?.Count();

            if (hasPaging)
            {
                result.ViewName = "~/Views/InvoiceProcess/DataQuery/InvoiceQueryList.cshtml";
            }
            else
            {
                result.ViewName = "~/Views/InvoiceProcess/Module/InvoiceQueryResult.cshtml";
            }
            return result;
        }


        private void checkQueryAction(String resultAction)
        {
            switch (resultAction)
            {
                case "Authorize":
                    ViewBag.DataItemView = "~/Views/InvoiceProcess/Module/AuthorizeDataItemToPrint.ascx";
                    return;
                case "Incoming":
                    ViewBag.DataItemView = "~/Views/InvoiceProcess/Buyer/DataItem.ascx";
                    return;
                case "Allow":
                    ViewBag.DataItemView = "~/Views/InvoiceProcess/Module/AuthorizeDataItemToVoid.ascx";
                    return;
                default:
                    return;
            }
        }

        //public ActionResult InvoiceAttachment()
        //{
        //    //ViewBag.HasQuery = false;
        //    ViewBag.QueryAction = "InquireAttachment";
        //    ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>();
        //    TempData.SetModelSource(models);
        //    DataSource.Inquiry = CreateInvoiceItemInquiry(this);

        //    return View("InvoiceReport", DataSource.Inquiry);
        //}

        //public ActionResult InquireAttachment()
        //{
        //    //ViewBag.HasQuery = true;
        //    ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>();
        //    TempData.SetModelSource(models);
        //    DataSource.Inquiry = CreateInvoiceItemInquiry(this);
        //    DataSource.BuildQuery();

        //    return View("AttachmentResult", DataSource.Inquiry);
        //}

        //public ActionResult AttachmentGridPage(int index, int size)
        //{
        //    //ViewBag.HasQuery = true;
        //    ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>();
        //    TempData.SetModelSource(models);
        //    DataSource.Inquiry = CreateInvoiceItemInquiry(this);
        //    DataSource.BuildQuery();

        //    if (index > 0)
        //        index--;
        //    else
        //        index = 0;

        //    return View(DataSource.Items.OrderByDescending(d => d.InvoiceID)
        //        .Skip(index * size).Take(size)
        //        .ToArray());
        //}


        //public ActionResult GridPage(int index,int size)
        //{
        //    //ViewBag.HasQuery = true;
        //    ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>();
        //    TempData.SetModelSource(models);
        //    DataSource.Inquiry = CreateInvoiceItemInquiry(this);
        //    DataSource.BuildQuery();

        //    if (index > 0)
        //        index--;
        //    else
        //        index = 0;

        //    return View(DataSource.Items.OrderByDescending(d => d.InvoiceID)
        //        .Skip(index * size).Take(size)
        //        .ToArray());
        //}

        public async Task<ActionResult> CreateXlsxAsync(InquireInvoiceViewModel viewModel)
        {

            ViewResult result = (ViewResult)Inquire(viewModel);
            IQueryable<InvoiceItem> model = result.Model as IQueryable<InvoiceItem>;

            if (model == null)
                return result;


            var items = model.OrderBy(i => i.TrackCode).ThenBy(i => i.No)
                .Select(i => new
                {
                    InvoiceID = i.InvoiceID,
                    發票號碼 = i.TrackCode + i.No,
                    發票日期 = i.InvoiceDate,
                    附件檔名 = i.CDS_Document.Attachment.Count > 0 ? i.CDS_Document.Attachment.First().KeyName : null,
                    附件檔頁數 = 0,
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
                    是否中獎 = i.InvoiceWinningNumber.PrizeType,
                    載具類別 = i.InvoiceCarrier.CarrierType,
                    載具號碼 = i.InvoiceCarrier.CarrierNo,
                    備註 = i.InvoiceDetails.First().InvoiceProduct.InvoiceProductItem.First().Remark
                    //備註 = String.Join("", i.InvoiceDetails.Select(t => t.InvoiceProduct.InvoiceProductItem.FirstOrDefault())
                    //    .Select(p => p.Remark))
                });


            using (DataSet ds = new DataSet())
            {
                DataTable table = new DataTable("發票資料明細");
                ds.Tables.Add(table);
                table.Columns.Add("InvoiceID",typeof(int));
                table.Columns.Add("發票號碼");
                table.Columns.Add("發票日期");
                table.Columns.Add("附件檔名");
                table.Columns.Add("附件檔頁數");
                table.Columns.Add("客戶ID");
                table.Columns.Add("序號");
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
                table.Columns.Add("愛心碼");
                table.Columns.Add("是否中獎");
                table.Columns.Add("載具類別");
                table.Columns.Add("載具號碼");
                table.Columns.Add("備註");

                //void InvoiceXls_RowChanged(object sender, DataRowChangeEventArgs e)
                //{

                //}

                DataSource.GetDataSetResult(items, table);

                if (viewModel.Attachment != 0)
                {
                    foreach(DataRow row in table.Rows)
                    {
                        row["附件檔頁數"] = ((int)row["InvoiceID"]).GetAttachedPdfPageCount(models);
                    }
                }

                table.Columns.RemoveAt(0);

                //DataTable table = items.ToDataTable();
                //ds.Tables.Add(table);

                //foreach (var r in table.Select("買受人統編 = '0000000000'"))
                //{
                //    r["買受人統編"] = "";
                //}

                foreach (DataRow r in table.Rows)
                {
                    if ("0000000000".Equals(r["買受人統編"]))
                    {
                        r["買受人統編"] = "";
                    }
                }

                await ds.SaveAsExcelAsync(Response, String.Format("attachment;filename={0}", HttpUtility.UrlEncode("發票資料明細.xlsx")));
            }

            return new EmptyResult();
        }

        static void saveAsExcel(int taskID, String resultFile, SqlCommand sqlCmd, bool hasAttachment)
        {
            Task.Run(() =>
            {
                try
                {
                    using (DataSet ds = new DataSet())
                    {
                        DataTable table = new DataTable("發票資料明細");
                        ds.Tables.Add(table);
                        table.Columns.Add("InvoiceID", typeof(int));
                        table.Columns.Add("發票號碼");
                        table.Columns.Add("發票日期");
                        table.Columns.Add("附件檔名");
                        table.Columns.Add("附件檔頁數");
                        table.Columns.Add("客戶ID");
                        table.Columns.Add("序號");
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
                        table.Columns.Add("愛心碼");
                        table.Columns.Add("是否中獎");
                        table.Columns.Add("載具類別");
                        table.Columns.Add("載具號碼");
                        table.Columns.Add("備註");

                        //void InvoiceXls_RowChanged(object sender, DataRowChangeEventArgs e)
                        //{

                        //}
                        using (ModelSource<InvoiceItem> db = new ModelSource<InvoiceItem>())
                        {
                            Exception exception = null;

                            try
                            {
                                db.GetDataSetResult(sqlCmd, table);
                                if (hasAttachment)
                                {
                                    foreach (DataRow row in table.Rows)
                                    {
                                        row["附件檔頁數"] = ((int)row["InvoiceID"]).GetAttachedPdfPageCount(db);
                                    }
                                }

                                table.Columns.RemoveAt(0);

                                //DataTable table = items.ToDataTable();
                                //ds.Tables.Add(table);

                                //foreach (var r in table.Select("買受人統編 = '0000000000'"))
                                //{
                                //    r["買受人統編"] = "";
                                //}

                                foreach (DataRow r in table.Rows)
                                {
                                    if ("0000000000".Equals(r["買受人統編"]))
                                    {
                                        r["買受人統編"] = "";
                                    }
                                }

                                using (var xls = ds.ConvertToExcel())
                                {
                                    xls.SaveAs(resultFile);
                                }
                            }
                            catch(Exception ex)
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

        public ActionResult CreateXlsx2021(InquireInvoiceViewModel viewModel)
        {
            this.models = new ModelSource<InvoiceItem>();

            ViewResult result = (ViewResult)Inquire(viewModel);
            IQueryable<InvoiceItem> model = result.Model as IQueryable<InvoiceItem>;

            if (model == null)
                return result;

            IQueryable<dynamic> items;
            var profile = HttpContext.GetUser();
            if (profile.IsSystemAdmin())
            {
                items = model.OrderBy(i => i.TrackCode).ThenBy(i => i.No)
                                .Select(i => new
                                {
                                    InvoiceID = i.InvoiceID,
                                    發票號碼 = i.TrackCode + i.No,
                                    發票日期 = i.InvoiceDate,
                                    //附件檔名 = i.CDS_Document.Attachment.Any() ? i.CDS_Document.Attachment.First().KeyName : null,
                                    //附件檔頁數 = 0,
                                    客戶ID = i.InvoiceBuyer.CustomerID,
                                    序號 = i.InvoicePurchaseOrder != null ? i.InvoicePurchaseOrder.OrderNo : null,
                                    發票開立人 = i.InvoiceSeller.CustomerName,
                                    開立人統編 = i.InvoiceSeller.ReceiptNo,
                                    營業人店別 = i.Organization.OrganizationExtension.CustomerNo,
                                    未稅金額 = i.InvoiceAmountType.SalesAmount,
                                    稅額 = i.InvoiceAmountType.TaxAmount,
                                    含稅金額 = i.InvoiceAmountType.TotalAmount,
                                    幣別 = i.InvoiceAmountType.CurrencyID.HasValue ? i.InvoiceAmountType.CurrencyType.AbbrevName : null,
                                    買受人名稱 = i.InvoiceBuyer.CustomerName,
                                    買受人統編 = i.InvoiceBuyer.ReceiptNo,
                                    連絡人名稱 = i.InvoiceBuyer.ContactName,
                                    連絡人地址 = i.InvoiceBuyer.Address,
                                    買受人EMail = i.InvoiceBuyer.EMail,
                                    愛心碼 = i.InvoiceDonation.AgencyCode,
                                    是否中獎 = i.InvoiceWinningNumber.PrizeType,
                                    備註 = i.Remark,
                                    發票狀態 = i.InvoiceCancellation != null ? "已作廢" : null,
                                    載具類別 = i.InvoiceCarrier.CarrierType,
                                    載具號碼 = i.InvoiceCarrier.CarrierNo,
                                    //備註 =  i.InvoiceDetails.First().InvoiceProduct.InvoiceProductItem.First().Remark
                                    //備註 = String.Join("", i.InvoiceDetails.Select(t => t.InvoiceProduct.InvoiceProductItem.FirstOrDefault())
                                    //    .Select(p => p.Remark))
                                });
            }
            else
            {
                items = model.OrderBy(i => i.TrackCode).ThenBy(i => i.No)
                                .Select(i => new
                                {
                                    InvoiceID = i.InvoiceID,
                                    發票號碼 = i.TrackCode + i.No,
                                    發票日期 = i.InvoiceDate,
                                    //附件檔名 = i.CDS_Document.Attachment.Any() ? i.CDS_Document.Attachment.First().KeyName : null,
                                    //附件檔頁數 = 0,
                                    客戶ID = i.InvoiceBuyer.CustomerID,
                                    序號 = i.InvoicePurchaseOrder != null ? i.InvoicePurchaseOrder.OrderNo : null,
                                    發票開立人 = i.InvoiceSeller.CustomerName,
                                    開立人統編 = i.InvoiceSeller.ReceiptNo,
                                    營業人店別 = i.Organization.OrganizationExtension.CustomerNo,
                                    未稅金額 = i.InvoiceAmountType.SalesAmount,
                                    稅額 = i.InvoiceAmountType.TaxAmount,
                                    含稅金額 = i.InvoiceAmountType.TotalAmount,
                                    幣別 = i.InvoiceAmountType.CurrencyID.HasValue ? i.InvoiceAmountType.CurrencyType.AbbrevName : null,
                                    買受人名稱 = i.InvoiceBuyer.CustomerName,
                                    買受人統編 = i.InvoiceBuyer.ReceiptNo,
                                    //連絡人名稱 = i.InvoiceBuyer.ContactName,
                                    //連絡人地址 = i.InvoiceBuyer.Address,
                                    //買受人EMail = i.InvoiceBuyer.EMail,
                                    愛心碼 = i.InvoiceDonation.AgencyCode,
                                    是否中獎 = i.InvoiceWinningNumber.PrizeType,
                                    備註 = i.Remark,
                                    發票狀態 = i.InvoiceCancellation != null ? "已作廢" : null,
                                    載具類別 = i.InvoiceCarrier.CarrierType,
                                    載具號碼 = i.InvoiceCarrier.CarrierNo,
                                    //備註 =  i.InvoiceDetails.First().InvoiceProduct.InvoiceProductItem.First().Remark
                                    //備註 = String.Join("", i.InvoiceDetails.Select(t => t.InvoiceProduct.InvoiceProductItem.FirstOrDefault())
                                    //    .Select(p => p.Remark))
                                });
            }


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
                        FileDownloadName = "發票資料明細.xlsx",
                    });

        }

        static void saveAsExcel(int taskID, String resultFile, GenericManager<EIVOEntityDataContext> db, IQueryable<dynamic> items, bool hasAttachment)
        {
            Task.Run(() =>
            {
                try
                {
                    using (DataSet ds = new DataSet())
                    {

                        Exception exception = null;

                        try
                        {
                            DataTable table = items.ToDataTable();
                            ds.Tables.Add(table);

                            if (hasAttachment)
                            {
                                foreach (DataRow row in table.Rows)
                                {
                                    row["附件檔頁數"] = ((int)row["InvoiceID"]).GetAttachedPdfPageCount(db);
                                }
                            }

                            table.Columns.RemoveAt(0);

                            //DataTable table = items.ToDataTable();
                            //ds.Tables.Add(table);

                            //foreach (var r in table.Select("買受人統編 = '0000000000'"))
                            //{
                            //    r["買受人統編"] = "";
                            //}

                            foreach (DataRow r in table.Rows)
                            {
                                if ("0000000000".Equals(r["買受人統編"]))
                                {
                                    r["買受人統編"] = "";
                                }
                            }

                            using (var xls = ds.ConvertToExcel())
                            {
                                xls.SaveAs(resultFile);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"{ex}\r\n{items}");
                            exception = ex;
                        }

                        ProcessRequest taskItem = db.GetTable<ProcessRequest>()
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
                                db.GetTable<ExceptionLog>().InsertOnSubmit(logItem);
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
                    Logger.Error(ex);
                }
                finally
                {
                    db.Dispose();
                }
            });

        }

        public ActionResult CreateXlsx2024(InquireInvoiceViewModel viewModel)
        {
            //this.models = new ModelSource<InvoiceItem>();
            _dbInstance = false;
            ViewResult result = (ViewResult)Inquire(viewModel);
            IQueryable<InvoiceItem> items = result.Model as IQueryable<InvoiceItem>;
            viewModel.RecordCount = items?.Count();

            result.ViewName = "~/Views/InvoiceProcess/Module/CreateXlsx2024.cshtml";
            return result;
        }

        public async Task<ActionResult> ExportInvoiceBuyerAsync(InquireInvoiceViewModel viewModel)
        {
            //DataLoadOptions ops = new DataLoadOptions();
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceBuyer);
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceCancellation);
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceAmountType);
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceSeller);
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceWinningNumber);
            //ops.LoadWith<InvoiceItem>(i => i.InvoiceCarrier);
            //ops.LoadWith<InvoiceItem>(i => i.InvoicePurchaseOrder);
            //models.DataContext.LoadOptions = ops;

            ViewBag.ViewModel = viewModel;

            DataSource.Inquiry = CreateInvoiceItemInquiry(this, viewModel);
            DataSource.BuildQuery();

            var items = DataSource.Items.OrderBy(i => i.InvoiceID)
                .Select(i => new
                {
                    發票號碼 = i.TrackCode + i.No,
                    營業人名稱 = i.InvoiceBuyer.CustomerName,
                    收件人姓名 = i.InvoiceBuyer.ContactName,
                    地址 = i.InvoiceBuyer.Address,
                    電話 = i.InvoiceBuyer.Phone,
                    EMail = i.InvoiceBuyer.EMail,
                });

            using (DataSet ds = new DataSet())
            {
                DataTable table = new DataTable("買受人");
                ds.Tables.Add(table);
                table.Columns.Add("發票號碼");
                table.Columns.Add("營業人名稱");
                table.Columns.Add("收件人姓名");
                table.Columns.Add("地址");
                table.Columns.Add("電話");
                table.Columns.Add("EMail");

                DataSource.GetDataSetResult(items, table);

                await ds.SaveAsExcelAsync(Response, String.Format("attachment;filename={0}", HttpUtility.UrlEncode("發票買受人資料.xlsx")));
            }

            return new EmptyResult();
        }


        public ActionResult AssignDownload(InquireInvoiceViewModel viewModel)
        {
            ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>();
            DataSource.Inquiry = CreateInvoiceItemInquiry(this, viewModel);
            DataSource.BuildQuery();

            String resultFile = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, Guid.NewGuid().ToString() + ".xlsx");
            var profile = HttpContext.GetUser();
            profile["assignDownload"] = resultFile;

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
                            using (XLWorkbook xls = new XLWorkbook())
                            {
                                xls.Worksheets.Add(ds);
                                xls.SaveAs(resultFile);
                            }
                        }
                    }
                    models.Dispose();
                }
                catch (Exception ex)
                {
                    CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                }
            });

            return Content("下載資料請求已送出!!");
        }



        public ActionResult Print(int[] chkItem, RenderStyleViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            var profile = HttpContext.GetUser();

            if (viewModel.ProcessType == Naming.InvoiceProcessType.A0401)
            {
                profile.EnqueueInvoicePrint(models, chkItem);
                //printUrl = "~/DataView/PrintA0401AsPDF";
                //ViewBag.PrintView = "~/DataView/PrintA0401";
            }
            else
            {
                String keyCodeFile = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogPath, "ORCodeKey.txt");
                if (System.IO.File.Exists(keyCodeFile))
                {
                    if (!String.IsNullOrEmpty(System.IO.File.ReadAllText(keyCodeFile)))
                    {
                        String reason;
                        if (profile.EnqueueInvoicePrint(models, chkItem, out reason))
                        {
                            if (viewModel.PaperStyle == "A4")
                            {
                                //printUrl = "~/SAM/NewPrintInvoiceAsPDF.aspx?printBack=" + Request["printBack"];
                                //ViewBag.PrintView = "~/SAM/NewPrintInvoicePage.aspx?printBack=" + Request["printBack"];
                            }
                            else if (viewModel.PaperStyle == "POS")
                            {
                                //printUrl = "~/SAM/NewPrintInvoicePOSAsPDF.aspx?printBuyerAddr=" + Request["printBuyerAddr"];
                                //ViewBag.PrintView = "~/SAM/NewPrintInvoicePOSPage.aspx?printBuyerAddr=" + Request["printBuyerAddr"];
                            }
                        }
                        else
                        {
                            //ViewBag.Message = "發票暫時不可列印，請檢查載具資訊及列印註記!!";
                            return Json(new { result = false, message = reason });
                        }
                    }
                    else
                    {
                        return Json(new { result = false, message = "QRCode金鑰檔無內容，無法列印!!" });
                    }
                }
                else
                {
                    return Json(new { result = false, message = "無QRCode金鑰檔，無法列印!!" });
                }
            }
            return View("~/Views/InvoiceProcess/Module/PrintResult.cshtml");
        }

        public ActionResult IssueInvoiceNotice(int[] chkItem, bool? cancellation,Naming.InvoiceProcessType? processType,String mailTo)
        {
            if (chkItem != null && chkItem.Count() > 0)
            {
                if (cancellation == true)
                {
                    chkItem.NotifyIssuedInvoiceCancellation();
                }
                else
                {
                    if (processType == Naming.InvoiceProcessType.A0401)
                    {
                        chkItem.NotifyIssuedA0401(mailTo);
                    }
                    else
                    {
                        chkItem.NotifyIssuedInvoice(true, mailTo);
                    }
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

        public ActionResult IssueWinningNotice(InquireInvoiceViewModel viewModel)
        {
            List<int> items = new List<int>();
            if (viewModel.ChkItem != null && viewModel.ChkItem.Length > 0)
            {
                items.AddRange(viewModel.ChkItem);
            }

            if (viewModel.KeyID != null)
            {
                items.Add(viewModel.DecryptKeyValue());
            }

            if (items.Count() > 0)
            {
                items.NotifyWinningInvoice(false);
                ViewBag.Message = "Email通知已重送!!";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }
            else
            {
                ViewBag.Message = "請選擇重送資料!!";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }

        }

        public ActionResult CancelInvoice(int[] chkItem, Naming.InvoiceProcessType? proceType)
        {
            if (chkItem != null && chkItem.Count() > 0)
            {
                InvoiceManager mgr = new InvoiceManager(models);
                mgr.VoidInvoice(chkItem);
                if (mgr.EventItems != null && mgr.EventItems.Count > 0)
                {
                    ViewBag.Message = "下列發票已作廢完成!!\r\n" + String.Join("\r\n", mgr.EventItems.Select(i => i.TrackCode + i.No));
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

        public async Task<ActionResult> IssueAllowanceAsync([FromBody] QueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            if(viewModel == null)
            {
                viewModel = await PrepareViewModelAsync<QueryViewModel>();
                ModelState.Clear();
            }

            if (viewModel.ChkItem?.Length>0)
            {
                var profile = HttpContext.GetUser();
                var items = models.GetTable<InvoiceItem>().Where(i => viewModel.ChkItem.Contains(i.InvoiceID));
                items = models.FilterInvoiceByRole(profile, items);

                if (items.Count() > 1)
                {
                    var g = items.GroupBy(i => i.InvoiceBuyer.ReceiptNo);
                    if (g.Count() > 1)
                    {
                        ViewBag.Message = "請選擇相同買受人之發票資料!!";
                        return View("~/Views/Shared/AlertMessage.cshtml");
                    }

                    var g2 = items.Join(models.GetTable<CDS_Document>(), i => i.InvoiceID, d => d.DocID, (i, d) => d.ProcessType)
                        .GroupBy(t => t);

                    if (g2.Count() > 1)
                    {
                        ViewBag.Message = "請選擇相同類型之發票資料!!";
                        return View("~/Views/Shared/AlertMessage.cshtml");
                    }
                }

                return View("~/Views/InvoiceProcess/Module/IssueAllowance.cshtml", items);
            }
            else
            {
                ViewBag.Message = "請選擇發票資料!!";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }
        }

        public ActionResult AuthorizeToPrint(int[] chkItem)
        {
            if (chkItem != null && chkItem.Count() > 0)
            {
                var items = models.GetTable<InvoiceItem>().Where(i => chkItem.Contains(i.InvoiceID))
                        .Where(i => i.CDS_Document.DocumentPrintLog.Any() && i.CDS_Document.DocumentAuthorization == null)
                        .Select(i => i.InvoiceID).ToList()
                        .Select(i => new DocumentAuthorization
                        {
                            DocID = i
                        }).ToList();

                models.GetTable<DocumentAuthorization>().InsertAllOnSubmit(items);
                models.SubmitChanges();

                ViewBag.Message = "下列發票已核准重印!!\r\n" + String.Join("\r\n", items.Select(i => i.CDS_Document.InvoiceItem.TrackCode + i.CDS_Document.InvoiceItem.No));
                return View("~/Views/Shared/AlertMessage.cshtml");
            }
            else
            {
                ViewBag.Message = "請選擇核准重印資料!!";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }

        }

        public ActionResult DesireToVoidInvoice(int[] chkItem, bool? allow)
        {
            if (chkItem != null && chkItem.Count() > 0)
            {
                if (allow == true)
                {
                    var items = models.GetTable<InvoiceItem>()
                        .Where(i => i.AuthorizeToVoid != null && i.AuthorizeToVoid.VoidMode == (int)Naming.VoidActionMode.註銷作廢)
                        .Where(i => chkItem.Contains(i.InvoiceID));
                    if (items.Count() > 0)
                    {
                        doVoidInvoice(items, Naming.VoidActionMode.註銷作廢);
                    }

                    items = models.GetTable<InvoiceItem>()
                                            .Where(i => i.AuthorizeToVoid != null && i.AuthorizeToVoid.VoidMode == (int)Naming.VoidActionMode.註銷重開)
                                            .Where(i => chkItem.Contains(i.InvoiceID));
                    if (items.Count() > 0)
                    {
                        doVoidInvoice(items, Naming.VoidActionMode.註銷重開);
                    }
                }

                foreach (var item in chkItem)
                {
                    models.ExecuteCommand("delete AuthorizeToVoid where InvoiceID = {0}", item);
                }

                //models.DeleteAny<AuthorizeToVoid>(i => chkItem.Contains(i.InvoiceID));

                return View("~/Views/InvoiceProcess/ResultAction/VoidDone.cshtml");
            }
            else
            {
                ViewBag.Message = "請選擇核准註銷資料!!";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }

        }

        public async Task<ActionResult> VoidInvoiceAsync([FromBody] ReviseInvoiceViewModel viewModel)
        {
            if (viewModel == null)
            {
                viewModel = await PrepareViewModelAsync<ReviseInvoiceViewModel>();
                ModelState.Clear();
            }

            ViewBag.ViewModel = viewModel;
            IQueryable<InvoiceItem>? items = null;
            if (viewModel?.ChkItem?.Length > 0)
            {

                var profile = HttpContext.GetUser();
                //if (profile.IsSystemAdmin())
                //{
                items = models!.GetTable<InvoiceItem>().Where(i => viewModel.ChkItem.Contains(i.InvoiceID));

                if(!items.Any())
                {
                    ModelState.AddModelError("Message", "註銷發票不存在!!");
                }
                else
                {
                    if (viewModel.Mode == Naming.VoidActionMode.修正)
                    {
                        if (viewModel?.ReviseContent?.ReceiptNo.CheckRegno() != true)
                        {
                            ModelState.AddModelError("Message", "公司統一編號錯誤!!");
                        }
                    }
                }
            }
            else
            {
                ModelState.AddModelError("Message", "請選擇註銷資料!!");
                ViewBag.Message = "請選擇註銷資料!!";
                return View("~/Views/Shared/AlertMessage.cshtml");
            }

            if(ModelState.IsValid)
            {
                doVoidInvoice(items, viewModel?.Mode, viewModel);
                return View("~/Views/InvoiceProcess/ResultAction/VoidDone.cshtml");
            }
            else
            {
                return View("~/Views/Shared/AlertMessage.cshtml", ModelState.ErrorMessage());
            }

        }

        //private void doVoidInvoice(IEnumerable<InvoiceItem> items, Naming.VoidActionMode? mode)
        //{
        //    ModelExtension.Properties.AppSettings.Default.C0701Outbound.CheckStoredPath();

        //    foreach (var item in items)
        //    {
        //        item.CreateF0701().ConvertToXml().Save(System.IO.Path.Combine(ModelExtension.Properties.AppSettings.Default.C0701Outbound, "INV0701_" + item.TrackCode + item.No + ".xml"));
        //    }

        //    if (mode == Naming.VoidActionMode.註銷作廢 
        //        || mode == Naming.VoidActionMode.索取紙本)
        //    {
        //        String storedPath = Path.Combine(Logger.LogPath, "C0401(Outbound)").CheckStoredPath();
        //        var profile = HttpContext.GetUser();
        //        foreach (var item in items)
        //        {
        //            if (mode == Naming.VoidActionMode.索取紙本
        //                && item.InvoiceCancellation == null)
        //            {
        //                item.PrintMark = "Y";
        //                models.DeleteAnyOnSubmit<InvoiceCarrier>(c => c.InvoiceID == item.InvoiceID);
        //                models.SubmitChanges();
        //            }

        //            var c0401 = item.CreateF0401().ConvertToXml();
        //            c0401.Save(System.IO.Path.Combine(storedPath, $"INV0401_{item.TrackCode}{item.No}_{DateTime.Now.Ticks}.xml"));

        //            models.GetTable<ExceptionLog>().InsertOnSubmit(new ExceptionLog
        //            {
        //                DataContent = c0401.OuterXml,
        //                CompanyID = item.SellerID,
        //                LogTime = DateTime.Now,
        //                TypeID = (int)Naming.DocumentTypeDefinition.E_InvoiceVoid,
        //                Message = $"發票註銷({item.TrackCode}{item.No}),UID:{profile?.UID},PID:{profile?.PID}"
        //            });
        //            models.SubmitChanges();

        //            if (mode == Naming.VoidActionMode.註銷作廢)
        //            {
        //                models.ExecuteCommand(@"DELETE FROM CDS_Document
        //                FROM    DerivedDocument INNER JOIN
        //                        CDS_Document ON DerivedDocument.DocID = CDS_Document.DocID
        //                WHERE   (DerivedDocument.SourceID = {0})", item.InvoiceID);
        //                models.DeleteAny<InvoiceCancellation>(d => d.InvoiceID == item.InvoiceID);
        //            }
        //        }

        //        ThreadPool.QueueUserWorkItem(t =>
        //        {
        //            Thread.Sleep(10 * 60000);
        //            String[] files = Directory.GetFiles(storedPath);
        //            if (files != null && files.Length > 0)
        //            {
        //                ModelExtension.Properties.AppSettings.Default.C0401Outbound.CheckStoredPath();

        //                foreach (var f in files)
        //                {
        //                    try
        //                    {
        //                        System.IO.File.Move(f, Path.Combine(ModelExtension.Properties.AppSettings.Default.C0401Outbound, Path.GetFileName(f)));
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Logger.Error(ex);
        //                    }
        //                }
        //            }
        //        });
        //    }
        //    else if (mode == Naming.VoidActionMode.註銷重開)
        //    {
        //        var profile = HttpContext.GetUser();
        //        String storedPath = Path.Combine(Logger.LogPath, "Archive").CheckStoredPath();
        //        foreach (var item in items)
        //        {
        //            var c0401 = item.CreateF0401().ConvertToXml();
        //            c0401.Save(System.IO.Path.Combine(storedPath, $"INV0401_{item.TrackCode}{item.No}_{DateTime.Now.Ticks}.xml"));

        //            models.GetTable<ExceptionLog>().InsertOnSubmit(new ExceptionLog
        //            {
        //                DataContent = c0401.OuterXml,
        //                CompanyID = item.SellerID,
        //                LogTime = DateTime.Now,
        //                TypeID = (int)Naming.DocumentTypeDefinition.E_InvoiceVoid,
        //                Message = $"發票註銷({item.TrackCode}{item.No}),UID:{profile?.UID},PID:{profile?.PID}"
        //            });
        //            models.SubmitChanges();

        //            models.ExecuteCommand(@"DELETE FROM CDS_Document
        //                FROM    DerivedDocument INNER JOIN
        //                        CDS_Document ON DerivedDocument.DocID = CDS_Document.DocID
        //                WHERE   (DerivedDocument.SourceID = {0})", item.InvoiceID);
        //            models.ExecuteCommand("delete CDS_Document where DocID={0}", item.InvoiceID);
        //        }
        //    }
        //}

        private void doVoidInvoice(IEnumerable<InvoiceItem> items, Naming.VoidActionMode? mode, ReviseInvoiceViewModel? viewModel = null)
        {
            ModelExtension.Properties.AppSettings.Default.F0701Outbound.CheckStoredPath();

            foreach (var item in items)
            {
                lock(typeof(InvoiceProcessController))
                {
                    models!.ProcessVoidInvoiceRequest(mode, viewModel, item);
                }

                item.CreateF0701().ConvertToXml().Save(System.IO.Path.Combine(ModelExtension.Properties.AppSettings.Default.F0701Outbound, "INV0701_" + item.TrackCode + item.No + ".xml"));
            }
        }


        private void authorizeToVoid(IEnumerable<InvoiceItem> items, Naming.VoidActionMode? mode)
        {
            foreach (var item in items)
            {
                item.AuthorizeToVoid = new AuthorizeToVoid
                {
                    VoidMode = (int?)mode
                };
            }

            models.SubmitChanges();

        }

        public ActionResult DownloadC0401(int[] chkItem)
        {
            if (chkItem != null && chkItem.Count() > 0)
            {
                var items = models.GetTable<InvoiceItem>().Where(i => chkItem.Contains(i.InvoiceID));
                return zipItems(items, i => i.CreateF0401().ConvertToXml(), "INV0401");
            }
            else
            {
                ViewBag.Message = "請選擇下載資料!!";
                return View("~/Views/Shared/ShowMessage.aspx");
            }

        }

        public ActionResult DownloadC0701(int[] chkItem)
        {
            if (chkItem != null && chkItem.Count() > 0)
            {
                var items = models.GetTable<InvoiceItem>().Where(i => chkItem.Contains(i.InvoiceID));
                return zipItems(items, i => i.CreateF0701().ConvertToXml(), "INV0701");
            }
            else
            {
                ViewBag.Message = "請選擇下載資料!!";
                return View("~/Views/Shared/ShowMessage.aspx");
            }
        }

        public ActionResult DownloadC0501(int[] chkItem)
        {
            if (chkItem != null && chkItem.Count() > 0)
            {
                var items = models.GetTable<InvoiceItem>().Where(i => chkItem.Contains(i.InvoiceID));
                return zipItems(items, i => i.CreateF0501().ConvertToXml(), "INV0501");
            }
            else
            {
                ViewBag.Message = "請選擇下載資料!!";
                return View("~/Views/Shared/ShowMessage.aspx");
            }

        }

        private ActionResult zipItems(IEnumerable<InvoiceItem> items, Func<InvoiceItem, XmlDocument> convertTo, String docName)
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
                        var docItem = convertTo(item);
                        ZipArchiveEntry entry = zip.CreateEntry(String.Format("{0}_{1}{2}.xml", docName, item.TrackCode, item.No));
                        using (Stream outStream = entry.Open())
                        {
                            docItem.Save(outStream);
                        }
                    }
                }
            }

            var result = new VirtualFileResult(outFile, "message/rfc822");
            result.FileDownloadName = docName + ".zip";
            return result;
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS, Naming.RoleID.ROLE_SELLER })]
        public ActionResult InvoiceSummary(InquireInvoiceViewModel viewModel)
        {
            //ViewBag.HasQuery = false;
            ViewBag.QueryAction = "InquireSummary";
            DataSource.Inquiry = CreateInvoiceItemInquiry(this, viewModel);

            return View("InvoiceReport", DataSource.Inquiry);
        }

        public ActionResult InquireSummary(InquireInvoiceViewModel viewModel)
        {
            //ViewBag.HasQuery = true;
            ViewBag.PrintAction = "PrintInvoiceSummary";
            ViewBag.ViewModel = viewModel;

            DataSource.Inquiry = CreateInvoiceItemInquiry(this, viewModel);
            DataSource.BuildQuery();

            return View("InvoiceSummaryResult", DataSource.Inquiry);
        }

        public ActionResult InvoiceSummaryGridPage(int index, int size, InquireInvoiceViewModel viewModel)
        {
            //ViewBag.HasQuery = true;
            DataSource.Inquiry = CreateInvoiceItemInquiry(this, viewModel);
            DataSource.BuildQuery();

            if (index > 0)
                index--;
            else
                index = 0;

            ((ModelSource<InvoiceItem>)models).InquiryPageIndex = index;
            ((ModelSource<InvoiceItem>)models).InquiryPageSize = size;

            return View(DataSource.Items);
        }

        public async Task<ActionResult> CreateInvoiceSummaryXlsxAsync(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            var items = DataSource.Items.GroupBy(i => i.SellerID)
                //.Select(d => new
                //{
                //    SellerID = d.Key,
                //    RecordCount = d.Count(),
                //    InvoiceDate = d.OrderBy(i => i.InvoiceID).First().InvoiceDate
                //})
                .Join(models.GetTable<Organization>(), i => i.Key, o => o.CompanyID,
                    (i, o) => new
                    {
                        Seller = o,
                        Items = i
                    //SellerName = o.CompanyName,
                    //SellerReceiptNo = o.ReceiptNo,
                    //o.InvoiceItems.OrderBy(n => n.InvoiceDate).First().InvoiceDate,
                    //i.RecordCount
                });


            var details = items
                .Select(item => new
                {
                    開立發票營業人 = item.Seller.CompanyName,
                    統編 = item.Seller.ReceiptNo,
                    上線日期 = item.Seller.InvoiceItems.OrderBy(i => i.InvoiceDate).First().InvoiceDate,
                    發票筆數 = item.Items.Count(),
                });

            using (DataSet ds = new DataSet())
            {
                DataTable table = details.ToDataTable();
                table.TableName = "發票資料統計";
                ds.Tables.Add(table);

                await ds.SaveAsExcelAsync(Response, String.Format("attachment;filename={0}", HttpUtility.UrlEncode("發票資料統計.xlsx")));
            }


            return new EmptyResult();
        }

        public ActionResult PrintInvoiceSummary(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            DataSource.Inquiry = CreateInvoiceItemInquiry(this, viewModel);
            DataSource.BuildQuery();
            ((ModelSource<InvoiceItem>)models).ResultModel = Naming.DataResultMode.Print;

            return View(DataSource.Inquiry);
        }

        public ActionResult LoadInvoiceBuyer(InvoiceBuyerViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            InvoiceBuyer item = null;

            if (viewModel.KeyID != null)
            {
                viewModel.InvoiceID = viewModel.DecryptKeyValue();
                item = models.GetTable<InvoiceBuyer>().Where(u => u.InvoiceID == viewModel.InvoiceID).FirstOrDefault();
            }

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "買受人資料錯誤!!");
            }

            return View("~/Views/InvoiceProcess/Module/EditInvoiceBuyer.cshtml", item);

        }


        public ActionResult EditInvoiceBuyer(InvoiceBuyerViewModel viewModel)
        {
            ViewResult result = (ViewResult)LoadInvoiceBuyer(viewModel);
            InvoiceBuyer item = result.Model as InvoiceBuyer;

            if (item == null)
            {
                return result;
            }

            if (item != null)
            {
                viewModel.InvoiceID = item.InvoiceID;
                viewModel.ReceiptNo = item.ReceiptNo;
                viewModel.PostCode = item.PostCode;
                viewModel.Address = item.Address;
                viewModel.Name = item.Name;
                viewModel.BuyerID = item.BuyerID;
                viewModel.CustomerID = item.CustomerID;
                viewModel.ContactName = item.ContactName;
                viewModel.Phone = item.Phone;
                viewModel.EMail = item.EMail;
                viewModel.CustomerName = item.CustomerName;
                viewModel.Fax = item.Fax;
                viewModel.PersonInCharge = item.PersonInCharge;
                viewModel.RoleRemark = item.RoleRemark;
                viewModel.CustomerNumber = item.CustomerNumber;
                viewModel.BuyerMark = item.BuyerMark;
            }

            return View("~/Views/InvoiceProcess/Module/EditInvoiceBuyer.cshtml", item);

        }

        public ActionResult CommitInvoiceBuyer(InvoiceBuyerViewModel viewModel)
        {

            ViewResult result = (ViewResult)LoadInvoiceBuyer(viewModel);
            InvoiceBuyer item = result.Model as InvoiceBuyer;

            if (item == null)
                return result;

            item.ReceiptNo = viewModel.ReceiptNo;
            item.PostCode = viewModel.PostCode;
            item.Address = viewModel.Address;
            item.CustomerID = viewModel.CustomerID;
            item.ContactName = viewModel.ContactName;
            item.Phone = viewModel.Phone;
            item.EMail = viewModel.EMail;
            item.CustomerName = viewModel.CustomerName;

            models.SubmitChanges();

            return Json(new { result = true });
        }

        public ActionResult InquireToProcess(InquireInvoiceViewModel viewModel)
        {
            ViewResult result = (ViewResult)Index(viewModel);
            result.ViewName = "Index2023";
            viewModel.ResultAction = ViewBag.ResultAction = "Process";
            ViewBag.Title = "發票處理作業";
            //ViewBag.InquiryView = "~/Views/InvoiceProcess/InvoiceQueryForNotice.cshtml";

            return result;
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS })]
        public ActionResult InquireToNotifyWinning(InquireInvoiceViewModel viewModel)
        {
            ViewResult result = (ViewResult)Index(viewModel);
            result.ViewName = "Index2023";
            viewModel.ResultAction = ViewBag.ResultAction = "NotifyWinning";
            ViewBag.Title = "發票中獎通知";

            return result;
        }


        public ActionResult UploadAttachment(AttachmentViewModel viewModel, IFormFile theFile)
        {
            if (theFile == null)
            {
                return Json(new { result = false, message = "未選取檔案或檔案上傳失敗" });
            }

            try
            {
                String fileName = Path.GetFileName(theFile.FileName);
                String keyName = $"{Path.GetFileNameWithoutExtension(fileName)}{DateTime.Now.Ticks}";
                String fullPath = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, $"{keyName}{Path.GetExtension(fileName)}");
                theFile.SaveAs(fullPath);

                Attachment item =  new Attachment
                    {
                        KeyName = keyName,
                        StoredPath = fullPath,
                        DocID = viewModel.DocID,
                    };
                models.GetTable<Attachment>().InsertOnSubmit(item);

                models.SubmitChanges();

                return Json(new { result = true, message = keyName });

            }
            catch (Exception ex)
            {
                CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                return Json(new { result = false, message = ex.Message });
            }
        }


        public ActionResult DeleteAttachment(AttachmentViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            AttachmentViewModel tmp = viewModel;

            if(viewModel.KeyID!=null)
            {
                tmp = JsonConvert.DeserializeObject<AttachmentViewModel>(viewModel.KeyID.DecryptData());
            }

            try
            {
                var result = models.ExecuteCommand("delete Attachment where KeyName = {0} and DocID = {1}", tmp.KeyName, tmp.DocID);
                if (result > 0)
                {
                    return Json(new { result = true });
                }
                else
                {
                    return Json(new { result = false,message="資料錯誤!!" });
                }
            }
            catch (Exception ex)
            {
                CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                return Json(new { result = false, message = ex.Message });
            }
        }

        public ActionResult ArrangeAttachment(InquireInvoiceViewModel viewModel)
        {
            ViewResult result = (ViewResult)LoadInvoiceItem(viewModel);
            InvoiceItem item = result.Model as InvoiceItem;

            if (item == null)
                return result;

            return View("~/Views/InvoiceProcess/Module/ArrangeAttachment.cshtml", item);

        }

        public ActionResult LoadInvoiceItem(InquireInvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            InvoiceItem item = null;

            if (viewModel.KeyID != null)
            {
                viewModel.InvoiceID = viewModel.DecryptKeyValue();
                item = models.GetTable<InvoiceItem>().Where(u => u.InvoiceID == viewModel.InvoiceID).FirstOrDefault();
            }

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "資料錯誤!!");
            }

            return View("~/Views/InvoiceProcess/DataQuery/PrepareInvoiceQueryDataItem.cshtml", item);
        }
    }
}
