using System.Data;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc;


using WebHome.Helper;
using ModelCore.Models.ViewModel;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.Locale;
using ModelCore.DataExchange;
using ModelCore.Helper;
using CommonLib.Utility;
using Newtonsoft.Json;
using CommonLib.Core.Utility;
using CommonLib.DataAccess;

namespace WebHome.Controllers
{
    public class BusinessRelationshipController : SampleController<InvoiceItem>
    {
        public BusinessRelationshipController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public ActionResult MaintainRelationship(BusinessRelationshipQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/BusinessRelationship/MaintainRelationship.cshtml");
        }

        public ActionResult B2BIndex(BusinessRelationshipQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View();
        }

        public ActionResult ImportCounterpartBusinessXml(BusinessRelationshipQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/BusinessRelationship/ImportCounterpartBusinessXml.cshtml");
        }

        public ActionResult ImportCounterpartBusinessCsv(String message)
        {
            var userProfile = HttpContext.GetUser();
            var uploadMgr = userProfile["UploadManager"] as BusinessCounterpartUploadManager;
            if (uploadMgr != null)
                uploadMgr.Dispose();
            userProfile["UploadManager"] = null;

            ViewBag.Message = message;
            return View();
        }

        public ActionResult ImportCounterpartBusiness(String message)
        {
            var userProfile = HttpContext.GetUser();
            var uploadMgr = userProfile["UploadManager"] as BranchBusinessCounterpartUploadManager;
            if (uploadMgr != null)
                uploadMgr.Dispose();
            userProfile["UploadManager"] = null;

            ViewBag.Message = message;
            return View("~/Views/BusinessRelationship/ImportCounterpartBusiness.cshtml");
        }

        public ActionResult UploadCounterpartBusiness(BusinessRelationshipQueryViewModel viewModel,IEnumerable<IFormFile> excelFile)
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

            //if (!viewModel.BusinessID.HasValue)
            //{
            //    return Json(new { result = false, message = "請選擇營業人類別!!" });
            //}

            var profile = HttpContext.GetUser();
            List<String> available = null;

            if(!profile.IsSystemAdmin())
            {
                available = models.GetQueryByAgent(profile.CurrentUserRole.OrganizationCategory.CompanyID)
                        .Select(o => o.ReceiptNo).ToList();
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
                            .Where(t => t.TableName.Contains("相對營業人")).FirstOrDefault()) == null)
                    {
                        return Json(new { result = false, message = "Excel檔未包含【相對營業人】資料表!!" });
                    }

                    table.Columns.Add(new DataColumn("處理狀態", typeof(String)));
                    int colStatus = table.Columns.Count - 1;

                    foreach (DataRow r in table.Rows)
                    {
                        try
                        {
                            BusinessRelationshipViewModel item = new BusinessRelationshipViewModel
                            {
                                MasterName = r.GetString("營業人名稱"),
                                MasterNo = r.GetString("統一編號"),
                                CompanyName = r.GetString("相對營業人名稱"),
                                ReceiptNo = r.GetString("相對營業人統一編號"),
                                ContactEmail = r.GetString("聯絡人電子郵件"),
                                Addr = r.GetString("地址"),
                                Phone = r.GetString("電話"),
                                BusinessID = viewModel.BusinessID,
                                CustomerNo = r.GetString("客戶代碼"),
                                SettingInvoiceType = Naming.InvoiceTypeDefinition.一般稅額計算之電子發票,
                            };

                            ModelState.Clear();

                            if (available == null || available.Contains(item.MasterNo))
                            {
                                item.CommitBusinessRelationshipViewModel(models, ModelState);
                            }
                            else
                            {
                                ModelState.AddModelError("統一編號", "營業人錯誤");
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
                    File(fileName, "application/octet-stream", "相對營業人(回應).xlsx"));

            }
            catch (Exception ex)
            {
                CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                return Json(new { result = false, message = ex.Message });
            }
        }


        public ActionResult InquireBusinessRelationship(BusinessRelationshipQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            IQueryable<BusinessRelationship> items;
            IQueryable<Organization> masterItems, orgItems;
            viewModel.PromptBusinessRelationship(models, out items, out masterItems, out orgItems);

            var profile = HttpContext.GetUser();
            if (!profile.IsSystemAdmin())
            {
                items = items.Where(r => r.MasterID == profile.CurrentUserRole.OrganizationCategory.CompanyID);
            }

            viewModel.CompanyName = viewModel.CompanyName.GetEfficientString();
            if (viewModel.CompanyName!=null)
            {
                orgItems = orgItems.Where(i => i.CompanyName.Contains(viewModel.CompanyName) || i.OrganizationBranch.Any(b => b.BranchName.Contains(viewModel.CompanyName)));
            }

            if (viewModel.CompanyStatus.HasValue)
            {
                orgItems = orgItems.Where(i => i.OrganizationStatus.CurrentLevel == viewModel.CompanyStatus);
            }

            viewModel.BranchNo = viewModel.BranchNo.GetEfficientString();
            if (viewModel.BranchNo!=null)
            {
                ViewBag.ByBranchNo = viewModel.BranchNo;
                orgItems = orgItems.Where(i => i.OrganizationBranch.Any(b => b.BranchNo == viewModel.BranchNo));
            }

            //主動列印
            if (viewModel.EntrustToPrint.HasValue)
            {
                if (viewModel.EntrustToPrint == true)
                {
                    orgItems = orgItems.Where(i => i.OrganizationStatus.EntrustToPrint == true);
                }
                else
                {
                    orgItems = orgItems.Where(i => i.OrganizationStatus.EntrustToPrint == false || !i.OrganizationStatus.EntrustToPrint.HasValue);
                }
            }
            //自動接收
            if (viewModel.Entrusting.HasValue)
            {
                if (viewModel.Entrusting == true)
                {
                    orgItems = orgItems.Where(i => i.OrganizationStatus.Entrusting == true);
                }
                else
                {
                    orgItems = orgItems.Where(i => i.OrganizationStatus.Entrusting == false || !i.OrganizationStatus.Entrusting.HasValue);
                }
            }

            if (viewModel.BusinessType.HasValue)
            {
                items = items.Where(r => r.BusinessID == viewModel.BusinessType);
            }

            items = items
                    .Join(masterItems, b => b.MasterID, o => o.CompanyID, (b, o) => b)
                    .Join(orgItems, b => b.RelativeID, o => o.CompanyID, (b, o) => b);

            //if (viewModel.PageIndex.HasValue)
            //{
            //    viewModel.PageIndex--;
            //    return View("~/Views/BusinessRelationship/Module/ItemList.cshtml", items);
            //}
            //else
            //{
            //    viewModel.PageIndex = 0;
            //    return View("~/Views/BusinessRelationship/Module/QueryResult.cshtml", items);
            //}

            viewModel.ResultView = "~/Views/BusinessRelationship/DataQuery/BusinessRelationshipList.cshtml";
            viewModel.ResultAction = "~/Views/BusinessRelationship/DataAction/QueryResultAction.cshtml";
            return PageResult(viewModel, items);

        }

        public ActionResult ProcessDataItem(BusinessRelationshipQueryViewModel viewModel)
        {

            ViewResult result = (ViewResult)InquireBusinessRelationship(viewModel);
            result.ViewName = "~/Views/BusinessRelationship/DataQuery/BusinessRelationshipList.cshtml";
            if (viewModel.DisplayType == Naming.FieldDisplayType.DataItem)
            {
                IQueryable<BusinessRelationship> items = (IQueryable<BusinessRelationship>)result.Model;
                if (items.Count() == 0)
                {
                    viewModel.DisplayType = Naming.FieldDisplayType.Create;
                }
            }

            ViewBag.DisplayType = viewModel.DisplayType;

            return result;
        }

        public ActionResult DeleteItem(BusinessRelationshipQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (viewModel.KeyID != null)
            {
                viewModel = JsonConvert.DeserializeObject<BusinessRelationshipQueryViewModel>(viewModel.KeyID.DecryptData());
            }

            int count = models.ExecuteCommand("delete center.BusinessRelationship where MasterID = {0} and RelativeID = {1} and BusinessID = {2}",
                    viewModel.MasterID, viewModel.RelativeID, (int?)viewModel.BusinessID);

            if (count == 0)
            {
                return Json(new { result = false, message = "資料錯誤" });
            }

            return Json(new { result = true });

        }

        public ActionResult Deactivate(int businessID, int masterID, int relativeID)
        {
            var item = models.GetTable<BusinessRelationship>().Where(m => m.MasterID == masterID && m.RelativeID == relativeID && m.BusinessID == businessID).FirstOrDefault();
            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "營業人資料錯誤!!");
            }

            item.CurrentLevel = (int)Naming.MemberStatusDefinition.Mark_To_Delete;
            models.SubmitChanges();

            return View("~/Views/BusinessRelationship/Module/DataItem.cshtml", item);
        }

        public ActionResult Activate(int businessID, int masterID, int relativeID)
        {
            var item = models.GetTable<BusinessRelationship>().Where(m => m.MasterID == masterID && m.RelativeID == relativeID && m.BusinessID == businessID).FirstOrDefault();
            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "營業人資料錯誤!!");
            }

            item.CurrentLevel = (int)Naming.MemberStatusDefinition.Checked;
            models.SubmitChanges();

            return View("~/Views/BusinessRelationship/Module/DataItem.cshtml", item);
        }

        public ActionResult SetEntrusting(int businessID, int masterID, int relativeID, bool status)
        {
            var item = models.GetTable<BusinessRelationship>().Where(m => m.MasterID == masterID && m.RelativeID == relativeID && m.BusinessID == businessID).FirstOrDefault();
            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "營業人資料錯誤!!");
            }

            item.Counterpart.OrganizationStatus.Entrusting = status;
            models.SubmitChanges();

            return View("~/Views/BusinessRelationship/Module/DataItem.cshtml", item);
        }

        public ActionResult SetEntrustToPrint(int businessID, int masterID, int relativeID, bool status)
        {
            var item = models.GetTable<BusinessRelationship>().Where(m => m.MasterID == masterID && m.RelativeID == relativeID && m.BusinessID == businessID).FirstOrDefault();
            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "營業人資料錯誤!!");
            }

            item.Counterpart.OrganizationStatus.EntrustToPrint = status;
            models.SubmitChanges();

            return View("~/Views/BusinessRelationship/Module/DataItem.cshtml", item);
        }

        public ActionResult CommitItem(BusinessRelationshipQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            viewModel.CompanyName = viewModel.CompanyName.GetEfficientString();
            if (viewModel.CompanyName == null)
            {
                ModelState.AddModelError("CompanyName", "營業人名稱格式錯誤");
            }

            //if (viewModel.ReceiptNo == null || !Regex.IsMatch(viewModel.ReceiptNo, "\\d{8}"))
            //{
            //    ModelState.AddModelError("ReceiptNo", "統編格式錯誤");
            //}

            //if (string.IsNullOrEmpty(viewModel.ContactEmail) || !viewModel.ContactEmail.Contains('@'))
            //{
            //    ModelState.AddModelError("ContactEmail", "聯絡人電子郵件格式錯誤");
            //}

            //if (string.IsNullOrEmpty(viewModel.Addr))
            //{
            //    ModelState.AddModelError("Addr", "地址格式錯誤");
            //}

            if (!ModelState.IsValid)
            {
                ViewBag.ModelState = ModelState;
                return View("~/Views/Shared/AlertMessage.cshtml", model: null);
            }

            IQueryable<BusinessRelationship> sourceItems;
            IQueryable<Organization> masterItems, relativeItems;
            IQueryable<BusinessRelationship> items = viewModel.PromptBusinessRelationship(models, out sourceItems, out masterItems, out relativeItems);

            BusinessRelationship item = items?.FirstOrDefault();
            if (item != null)
            {
                item.CompanyName = viewModel.CompanyName;
                item.ContactEmail = viewModel.ContactEmail.GetEfficientString();
                item.Addr = viewModel.Addr.GetEfficientString();
                item.Phone = viewModel.Phone.GetEfficientString();
                item.CustomerNo = viewModel.CustomerNo.GetEfficientString();

                models.SubmitChanges();
                return Json(new { result = true, viewModel.KeyID });
            }
            else
            {
                return Json(new { result = false, message = "資料錯誤!!" });
            }

        }

        public ActionResult ImportCsv(BusinessRelationshipViewModel viewModel)
        {
            var userProfile = HttpContext.GetUser();

            if (!viewModel.CompanyID.HasValue)
            {
                ModelState.AddModelError("CompanyID", "請先建立集團成員!!");
            }
            else if (!viewModel.BusinessType.HasValue)
            {
                ModelState.AddModelError("BusinessType", "請選擇相對營業人類別!!");
            }
            else if (Request.Form.Files.Count == 0)
            {
                ModelState.AddModelError("csvFile", "請匯入檔案!!");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ModelState = ModelState;
                ViewBag.Message = ModelState.ErrorMessage();
                return View("~/Views/BusinessRelationship/ImportCounterpartBusiness.cshtml");
            }

            var file = Request.Form.Files[0];
            String fileName = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, DateTime.Now.Ticks + "_" + Path.GetFileName(file.FileName));
            file.SaveAs(fileName);

            var mgr = new BranchBusinessCounterpartUploadManager();
            mgr.BusinessType = (Naming.InvoiceCenterBusinessType?)viewModel.BusinessType ?? Naming.InvoiceCenterBusinessType.銷項;
            mgr.MasterID = viewModel.CompanyID;
            viewModel.Encoding=viewModel.Encoding.GetEfficientString();
            mgr.ParseData(userProfile, fileName, viewModel.Encoding != null ? Encoding.GetEncoding(viewModel.Encoding) : Encoding.Default);

            userProfile["UploadManager"] = mgr;

            return View("~/Views/BusinessRelationship/ImportCounterpartBusiness.cshtml");

        }

        public ActionResult ImportXml(BusinessRelationshipViewModel viewModel)
        {
            var userProfile = HttpContext.GetUser();

            if (!viewModel.CompanyID.HasValue)
            {
                ModelState.AddModelError("CompanyID", "請先建立集團成員!!");
            }
            else if (!viewModel.BusinessType.HasValue)
            {
                ModelState.AddModelError("BusinessType", "請選擇相對營業人類別!!");
            }
            else if (Request.Form.Files.Count == 0)
            {
                ModelState.AddModelError("theFile", "請匯入檔案!!");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ModelState = ModelState;
                return View("~/Views/Shared/ReportInputError.cshtml");
            }

            var file = Request.Form.Files[0];
            String fileName = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, DateTime.Now.Ticks + "_" + Path.GetFileName(file.FileName));
            file.SaveAs(fileName);

            CounterpartBusinessExchange exchange = new CounterpartBusinessExchange();
            exchange.ExchangeData(viewModel, fileName);

            ViewBag.ImportFile = fileName;

            return View("~/Views/BusinessRelationship/Module/CounterpartBusinessXmlResult.cshtml", exchange);

        }

        public ActionResult ImportBusinessCsv(BusinessRelationshipViewModel viewModel)
        {
            var userProfile = HttpContext.GetUser();

            if (!viewModel.CompanyID.HasValue)
            {
                ModelState.AddModelError("CompanyID", "請先建立集團成員!!");
            }
            else if (!viewModel.BusinessType.HasValue)
            {
                ModelState.AddModelError("BusinessType", "請選擇相對營業人類別!!");
            }
            else if (Request.Form.Files.Count == 0)
            {
                ModelState.AddModelError("csvFile", "請匯入檔案!!");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ModelState = ModelState;
                ViewBag.Message = ModelState.ErrorMessage();
                return View("~/Views/BusinessRelationship/ImportCounterpartBusinessCsv.cshtml");
            }

            var file = Request.Form.Files[0];
            String fileName = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, DateTime.Now.Ticks + "_" + Path.GetFileName(file.FileName));
            file.SaveAs(fileName);

            var mgr = new BusinessCounterpartUploadManager();
            mgr.BusinessType = (Naming.InvoiceCenterBusinessType?)viewModel.BusinessType ?? Naming.InvoiceCenterBusinessType.銷項;
            mgr.MasterID = viewModel.CompanyID;
            viewModel.Encoding = viewModel.Encoding.GetEfficientString();
            mgr.ParseData(userProfile, fileName, viewModel.Encoding!=null ? Encoding.GetEncoding(viewModel.Encoding) : Encoding.Default);

            userProfile["UploadManager"] = mgr;

            return View("~/Views/BusinessRelationship/ImportCounterpartBusinessCsv.cshtml");

        }

        public ActionResult ImportCounterpartBusinessList(int? pageIndex, int? filterMode)
        {

            if (pageIndex.HasValue)
            {
                ViewBag.PageIndex = pageIndex - 1;
            }
            else
            {
                ViewBag.PageIndex = 0;
            }

            ViewBag.FilterMode = filterMode;

            var profile = HttpContext.GetUser();
            var mgr = (BranchBusinessCounterpartUploadManager)profile["UploadManager"];

            if (mgr == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "連線逾時，請重新匯入資料!!");
            }

            return View("~/Views/BusinessRelationship/Module/ImportCounterpartBusinessList.cshtml");

        }

        public ActionResult ImportCounterpartBusinessCsvList(int? pageIndex, int? filterMode)
        {

            if (pageIndex.HasValue)
            {
                ViewBag.PageIndex = pageIndex - 1;
            }
            else
            {
                ViewBag.PageIndex = 0;
            }

            ViewBag.FilterMode = filterMode;

            var profile = HttpContext.GetUser();
            var mgr = (BusinessCounterpartUploadManager)profile["UploadManager"];

            if (mgr == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "連線逾時，請重新匯入資料!!");
            }

            return View("~/Views/BusinessRelationship/Module/ImportCounterpartBusinessCsvList.cshtml");

        }

        public ActionResult ImportCounterpartBusinessXmlList(int? pageIndex, int? filterMode)
        {

            if (pageIndex.HasValue)
            {
                ViewBag.PageIndex = pageIndex - 1;
            }
            else
            {
                ViewBag.PageIndex = 0;
            }

            ViewBag.FilterMode = filterMode;

            var profile = HttpContext.GetUser();
            var mgr = (BusinessCounterpartXmlUploadManager)profile["UploadManager"];

            if (mgr == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "連線逾時，請重新匯入資料!!");
            }

            return View("~/Views/BusinessRelationship/Module/ImportCounterpartBusinessXmlList.cshtml");

        }

        public ActionResult CommitImport()
        {
            var userProfile = HttpContext.GetUser();
            var uploadMgr = (BranchBusinessCounterpartUploadManager)userProfile["UploadManager"];
            if (uploadMgr.IsValid)
            {
                try
                {
                    uploadMgr.Save();
                    return Json(new { result = true });
                }
                catch (Exception ex)
                {
                    CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                    return Json(new { result = false, message = ex.Message });
                }
            }
            else
            {
                return Json(new { result = true, message = "資料檔有錯,無法匯入!!" });
            }

        }

        public ActionResult CommitImportXml()
        {
            var userProfile = HttpContext.GetUser();
            var uploadMgr = (BusinessCounterpartXmlUploadManager)userProfile["UploadManager"];
            if (uploadMgr.IsValid)
            {
                try
                {
                    uploadMgr.Save();
                    return Json(new { result = true });
                }
                catch (Exception ex)
                {
                    CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                    return Json(new { result = false, message = ex.Message });
                }
            }
            else
            {
                return Json(new { result = true, message = "資料檔有錯,無法匯入!!" });
            }

        }

        public ActionResult CommitImportBusinessCsv()
        {
            var userProfile = HttpContext.GetUser();
            var uploadMgr = (BusinessCounterpartUploadManager)userProfile["UploadManager"];
            if (uploadMgr.IsValid)
            {
                try
                {
                    uploadMgr.Save();
                    return Json(new { result = true });
                }
                catch (Exception ex)
                {
                    CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                    return Json(new { result = false, message = ex.Message });
                }
            }
            else
            {
                return Json(new { result = true, message = "資料檔有錯,無法匯入!!" });
            }

        }

        public ActionResult CancelImport()
        {
            var userProfile = HttpContext.GetUser();
            IDisposable uploadMgr = userProfile["UploadManager"] as BranchBusinessCounterpartUploadManager;
            if (uploadMgr == null)
            {
                uploadMgr = userProfile["UploadManager"] as BusinessCounterpartUploadManager;
            }
            if (uploadMgr != null)
            {
                uploadMgr.Dispose();
            }
            userProfile["UploadManager"] = null;

            return Json(new { result = true });

        }

        public async Task<ActionResult> GetCounterpartBusinessSampleAsync()
        {
            CounterpartBusinessExchange exchange = new CounterpartBusinessExchange();
            using (var xls = exchange.GetSample())
            {
                await xls.SaveAsExcelAsync(Response, $"attachment;filename={HttpUtility.UrlEncode("相對營業人")}.xlsx");
            }

            return new EmptyResult { };
        }

        //public ActionResult CreateXlsx(BusinessRelationshipQueryViewModel viewModel)
        //{

        //    ViewResult result = (ViewResult)InquireBusinessRelationship(viewModel);
        //    IQueryable<BusinessRelationship> model = result.Model as IQueryable<BusinessRelationship>;

        //    if (model == null)
        //        return result;

        //    var items = model.OrderBy(i => i.MasterID).ThenBy(i => i.RelativeID)
        //        .Select(o => new
        //        {
        //            營業人名稱 = o.BusinessMaster.CompanyName,
        //            統一編號 = o.BusinessMaster.ReceiptNo,
        //            相對營業人名稱 = o.CompanyName,
        //            相對營業人統一編號 = o.Counterpart.ReceiptNo,
        //            聯絡人電子郵件 = o.ContactEmail,
        //            地址 = o.Addr,
        //            電話 = o.Phone,
        //            客戶代碼 = o.CustomerNo,
        //        });

        //    DataTable table = new DataTable("相對營業人");
        //    items.BuildDataColumns(table);

        //    ProcessRequest processItem = new ProcessRequest
        //    {
        //        Sender = HttpContext.GetUser()?.UID,
        //        SubmitDate = DateTime.Now,
        //        ProcessStart = DateTime.Now,
        //        ResponsePath = System.IO.Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, Guid.NewGuid().ToString() + ".xlsx"),
        //    };
        //    models.GetTable<ProcessRequest>().InsertOnSubmit(processItem);
        //    models.SubmitChanges();

        //    SqlCommand sqlCmd = (SqlCommand)models.GetCommand(items);
        //    sqlCmd.SaveAsExcel(table, processItem.ResponsePath, processItem.TaskID);

        //    return View("~/Views/Shared/Module/PromptCheckDownload.cshtml",
        //            new AttachmentViewModel
        //            {
        //                TaskID = processItem.TaskID,
        //                FileName = processItem.ResponsePath,
        //                FileDownloadName = "相對營業人.xlsx",
        //            });

        //}

        public ActionResult CreateXlsx(BusinessRelationshipQueryViewModel viewModel)
        {

            ViewResult result = (ViewResult)InquireBusinessRelationship(viewModel);
            IQueryable<BusinessRelationship> model = result.Model as IQueryable<BusinessRelationship>;

            if (model == null)
                return result;


            var items = model.OrderBy(i => i.MasterID).ThenBy(i => i.RelativeID)
                .Select(o => new
                {
                    營業人名稱 = o.BusinessMaster.CompanyName,
                    統一編號 = o.BusinessMaster.ReceiptNo,
                    相對營業人名稱 = o.CompanyName,
                    相對營業人統一編號 = o.Counterpart.ReceiptNo,
                    聯絡人電子郵件 = o.ContactEmail,
                    地址 = o.Addr,
                    電話 = o.Phone,
                    客戶代碼 = o.CustomerNo,
                });

            return CreateExcelDownloadResult(items, "相對營業人", "相對營業人.xlsx");

        }

    }
}