using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;

using WebHome.Models.ViewModel;
using ClosedXML.Excel;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Helper;

using ModelCore.DataExchange;
using CommonLib.Utility;
using Newtonsoft.Json;
using System.Threading.Tasks;
using WebHome.Helper;
using ModelCore.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using WebHome.Helper.Security.Authorization;
using CommonLib.Core.Utility;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Data.Linq;

namespace WebHome.Controllers
{
    [Authorize]
    public class DataExchangeController : SampleController<InvoiceItem>
    {
        public DataExchangeController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: DataExchange
        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS })]
        public ActionResult Index()
        {
            return View("~/Views/DataExchange/Index.cshtml");
        }

        public ActionResult UpdateBuyer(bool? issueNotification)
        {
            var profile = HttpContext.GetUser();

            try
            {
                var buyerDetails = Request.Form.Files["InvoiceBuyer"];
                if (buyerDetails != null)
                {
                    using var input = buyerDetails.OpenReadStream();
                    using(XLWorkbook xlwb = new XLWorkbook(input))
                    {
                        InvoiceBuyerExchange exchange = new InvoiceBuyerExchange();
                        switch ((Naming.RoleID)profile.CurrentUserRole.RoleID)
                        {
                            case Naming.RoleID.ROLE_SYS:
                                exchange.ExchangeData(xlwb);
                                break;
                            case Naming.RoleID.ROLE_SELLER:
                            case Naming.RoleID.ROLE_NETWORKSELLER:
                                exchange.ExchangeData(xlwb, item =>
                                {
                                    return item.SellerID == profile.CurrentUserRole.OrganizationCategory.CompanyID
                                        || item.CDS_Document.DocumentOwner.OwnerID == profile.CurrentUserRole.OrganizationCategory.CompanyID;
                                });
                                break;
                            default:
                                break;
                        }

                        if (issueNotification == true && exchange.EffectiveItems.Count > 0)
                        {
                            exchange.EffectiveItems.Select(i => i.InvoiceID)
                                .NotifyIssuedInvoice(true);
                        }

                        String result = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, Guid.NewGuid().ToString() + ".xslx");
                        xlwb.SaveAs(result);
                        return File(result, "application/octet-stream", "修改買受人資料(回應).xlsx");
                    }
                }
                ViewBag.AlertMessage = "檔案錯誤!!";
            }
            catch(Exception ex)
            {
                CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                ViewBag.AlertMessage = ex.ToString();
            }
            return View("Index");
        }

        public ActionResult UpdateBuyerInfo(bool? issueNotification)
        {
            ActionResult result = UpdateBuyer(issueNotification);
            if(result is VirtualFileResult)
            {
                return View("~/Views/DataExchange/Module/UpdateBuyerInfo.cshtml", result);
            }
            else
            {
                return View("~/Views/Shared/AlertMessage.cshtml");
            }
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
                        return File(result, "application/octet-stream", "修改發票字軌(回應).xlsx");
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

        public ActionResult GetResource(String path)
        {
            if (path != null)
            {
                String filePath = path.DecryptData();
                if (System.IO.File.Exists(filePath))
                {
                    return File(filePath, "application/octet-stream", Path.GetFileName(filePath));
                }
            }
            return new EmptyResult { };
        }

        public ActionResult DownloadResource(AttachmentViewModel viewModel)
        {
            if (viewModel.KeyID != null)
            {
                viewModel = JsonConvert.DeserializeObject<AttachmentViewModel>(viewModel.KeyID.DecryptData());
            }

            ViewBag.ViewModel = viewModel;

            if (System.IO.File.Exists(viewModel.FileName))
            {
                return PhysicalFile(viewModel.FileName,
                    viewModel.ContentType ?? "application/octet-stream",
                    viewModel.FileDownloadName ?? Path.GetFileName(viewModel.FileName));
            }
            else
            {
                return new EmptyResult { };
            }
        }

        public ActionResult CheckResource(AttachmentViewModel viewModel)
        {
            if (viewModel.KeyID != null)
            {
                viewModel = JsonConvert.DeserializeObject<AttachmentViewModel>(viewModel.KeyID.DecryptData())!;
            }

            ViewBag.ViewModel = viewModel;

            if (viewModel!.FileName?.CanReadFile() == true)
            {
                return Json(new { result = true, viewModel.KeyID });
            }
            else if (viewModel.TaskID.HasValue)
            {
                var taskItem = models!.GetTable<ProcessRequest>().Where(p => p.TaskID == viewModel.TaskID).FirstOrDefault();
                if (taskItem != null)
                {
                    if (taskItem.ResponsePath != null && System.IO.File.Exists(taskItem.ResponsePath))
                    {
                        return Json(new { result = true, KeyID = viewModel.JsonStringify().EncryptData() });
                    }

                    return Json(new { result = false, KeyID = viewModel.JsonStringify().EncryptData(), message = taskItem.ExceptionLog?.DataContent });

                }
            }

            return Json(new { result = false, KeyID = viewModel.JsonStringify().EncryptData() });
        }

        public ActionResult CheckProcessRequest(AttachmentViewModel viewModel)
        {
            if (viewModel.KeyID != null)
            {
                viewModel.TaskID = viewModel.DecryptKeyValue();
            }

            ViewBag.ViewModel = viewModel;
            var item = models!.GetTable<ProcessRequest>().Where(p => p.TaskID == viewModel.TaskID).FirstOrDefault();
            return Json(new { result = item?.ProcessComplete.HasValue, item?.ProcessComplete });

        }


        public async Task<ActionResult> ImportInvoice()
        {
            var userProfile = HttpContext.GetUser();

            if (Request.Form.Files.Count <= 0)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "請選擇匯入檔!!");
            }

            var file = Request.Form.Files[0];
            String fileName = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, DateTime.Now.Ticks + "_" + Path.GetFileName(file.FileName));
            ViewBag.ImportFile = fileName;
            String content;
            using var input =Request.Form.Files[0].OpenReadStream();
            using (StreamReader reader = new StreamReader(input))
            {
                content = reader.ReadToEnd();
            }

            InvoiceEntity[] items = await Task.Run<InvoiceEntity[]>(() =>
                {
                    InvoiceEntity[] result = null;
                    try
                    {
                        result = JsonConvert.DeserializeObject<InvoiceEntity[]>(content);
                        if (result != null && result.Length > 0)
                        {
                            Parallel.ForEach(result, item =>
                            {
                                try
                                {
                                    item.Save();
                                }
                                catch (Exception ex)
                                {
                                    CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                                    item.Status = Naming.UploadStatusDefinition.匯入失敗;
                                    item.Reason = ex.Message;
                                }
                            });

                            System.IO.File.WriteAllText(fileName, JsonConvert.SerializeObject(result));
                        }
                    }
                    catch (Exception ex)
                    {
                        CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                    }
                    return result;
                });

            return View("~/Views/DataExchange/Module/ImportInvoiceResult.cshtml", items);

        }

        public ActionResult MaintainData(DataTableQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/DataExchange/MaintainData.cshtml");
        }

        Type PrepareDataTable(DataTableQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            viewModel.TableName = viewModel.TableName.GetEfficientString();
            if (viewModel.TableName == null)
            {
                ModelState.AddModelError("Message", "請選擇資料表!!");
                return null;
            }

            String typeName = typeof(EIVOEntityDataContext).AssemblyQualifiedName.Replace("EIVOEntityDataContext", viewModel.TableName);
            var type = Type.GetType(typeName);
            if (type == null)
            {
                ModelState.AddModelError("Message", "資料表錯誤!!");
                return null;
            }

            return type;
        }


        public ActionResult ShowDataTable(DataTableQueryViewModel viewModel)
        {
            var type = PrepareDataTable(viewModel);
            if (type == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: ModelState.ErrorMessage());
            }

            ViewBag.TableType = type;

            IQueryable items = models.DataContext.GetTable(type);
            if (viewModel.DataItem != null && viewModel.DataItem.Length > 0)
            {
                items = BuildQuery(viewModel.DataItem, type, items);
            }
            viewModel.RecordCount = items?.Count();

            if (viewModel.PageIndex.HasValue)
            {
                viewModel.PageIndex--;
                return View("~/Views/DataExchange/Module/DataItemList.cshtml", items);
            }
            else
            {
                viewModel.PageIndex = 0;
                return View("~/Views/DataExchange/Module/DataTableQueryResult.cshtml", items);
            }

        }

        private static IQueryable BuildQuery(DataTableColumn[] fields, Type type, IQueryable items)
        {
            foreach (DataTableColumn field in fields)
            {
                PropertyInfo? propertyInfo = type.GetProperty(field.Name);
                var columnAttribute = propertyInfo?.GetColumnAttribute();
                if (columnAttribute != null)
                {
                    String fieldValue = field.Value.GetEfficientString();
                    if (fieldValue == null)
                    {
                        continue;
                    }
                    var t = propertyInfo!.PropertyType;
                    if (t == typeof(String))
                    {
                        items = items.Where($"{propertyInfo.Name}.StartsWith(@0)", fieldValue);
                    }
                    else
                    {
                        items = items.Where($"{propertyInfo.Name} == @0", Convert.ChangeType(fieldValue, propertyInfo.PropertyType));
                    }
                }
            }

            return items;
        }

        public ActionResult DataItem(DataTableQueryViewModel viewModel)
        {
            var type = PrepareDataTable(viewModel);
            if (type == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: ModelState.ErrorMessage());
            }

            ViewBag.TableType = type;
            IQueryable items = ViewBag.DataTable = models.DataContext.GetTable(type);
            //var items = dataTable.Cast<dynamic>(); 

            dynamic dataItem;
            if (viewModel.KeyItem?.Any(k => k.Name != null && k.Value != null) == true)
            {
                items = BuildQuery(viewModel.KeyItem, type, items);
            }
            else
            {
                items = items.Where(" 1 = 0");
            }
            //var key = viewModel.KeyItem?.Where(k => k.Name != null && k.Value != null);
            //if (key?.Any() == true)
            //{
            //    int idx = 0;
            //    String sqlCmd = String.Concat(items.ToString(),
            //                        " where ",
            //                        String.Join(" and ", key.Select(k => $"{k.Name} = {{{idx++}}}")));
            //    var paramValues = key.Select(k => k.Value).ToArray();
            //    dataItem = ((IEnumerable<dynamic>)models.DataContext.ExecuteQuery(type, sqlCmd, paramValues))
            //                    .FirstOrDefault();
            //}
            //else
            //{
            //    dataItem = items.FirstOrDefault();
            //}

            dataItem = items.FirstOrDefault();

            return View("~/Views/DataExchange/Module/DataItem.cshtml", dataItem);
        }

        public ActionResult EditItem(DataTableQueryViewModel viewModel)
        {
            ViewResult result = (ViewResult)DataItem(viewModel);
            result.ViewName = "~/Views/DataExchange/Module/EditItem.cshtml";
            return result;
        }

        public ActionResult CommitItem(DataTableQueryViewModel viewModel)
        {
            ViewResult result = (ViewResult)DataItem(viewModel);
            Type type = ViewBag.TableType as Type;
            if (type == null)
            {
                return result;
            }

            ITable dataTable = ViewBag.DataTable as ITable;
            dynamic dataItem = result.Model;

            if (dataItem == null)
            {
                dataItem = Activator.CreateInstance(type);
                dataTable.InsertOnSubmit(dataItem);
            }

            if (viewModel.DataItem != null)
            {
                foreach (DataTableColumn field in viewModel.DataItem)
                {
                    PropertyInfo propertyInfo = type.GetProperty(field.Name);
                    if (propertyInfo != null && propertyInfo.CanWrite)
                    {
                        object value = field.Value != null
                                        ? Convert.ChangeType(field.Value, propertyInfo.PropertyType)
                                        : null;
                        propertyInfo.SetValue(dataItem, value, null);
                    }
                }
            }

            models.SubmitChanges();
            return View("~/Views/DataExchange/Module/DataItem.cshtml", dataItem);
        }

        public ActionResult DeleteItem(DataTableQueryViewModel viewModel)
        {
            var type = PrepareDataTable(viewModel);
            if (type == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: ModelState.ErrorMessage());
            }

            ViewBag.TableType = type;
            ITable dataTable = ViewBag.DataTable = models.DataContext.GetTable(type);

            if (viewModel.KeyItems != null)
            {
                foreach (var keyItem in viewModel.KeyItems)
                {
                    DataTableColumn[] keyData = JsonConvert.DeserializeObject<DataTableColumn[]>(keyItem.DecryptData());
                    dynamic item = BuildQuery(keyData, type, (IQueryable)dataTable).FirstOrDefault();
                    if (item != null)
                    {
                        dataTable.DeleteOnSubmit(item);
                        models.SubmitChanges();
                    }
                }
            }

            return Json(new { result = true });
        }

    }
}