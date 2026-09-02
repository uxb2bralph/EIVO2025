using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using Microsoft.AspNetCore.Mvc;



using ClosedXML.Excel;
using CommonLib.Core.DataWork;
using WebHome.Helper;
using WebHome.Models;
using WebHome.Models.ViewModel;
using ModelCore.Models.ViewModel;
using ModelCore.DataEntity;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Helper;
using ModelCore.Resource;

using CommonLib.Utility;
using ModelCore.InvoiceManagement.InvoiceProcess;
using WebHome.Helper.Security.Authorization;
using System.Threading.Tasks;
using ModelCore.DataEntityWrapper;

namespace WebHome.Controllers
{
    public class InvoiceBusinessController : SampleController<InvoiceItem>
    {
        public InvoiceBusinessController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: InvoiceBusiness
        public ActionResult ApplyPOSDevice(int? id)
        {
            var item = models.GetTable<Organization>().Where(o => o.CompanyID == id).FirstOrDefault();
            if (item == null)
                return Content("營業人資料錯誤!!");

            return View("~/Views/InvoiceBusiness/POSDevice/POSDeviceList.cshtml", item);
        }

        public ActionResult CommitPOS(int? id, int? deviceID, String POSNo)
        {
            var orgItem = models.GetTable<Organization>().Where(o => o.CompanyID == id).FirstOrDefault();
            if (orgItem == null)
                return View("~/Views/Shared/AlertMessage.cshtml", model: "營業人資料錯誤!!");

            POSNo = POSNo.GetEfficientString();
            if (POSNo == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "POS機編號錯誤!!");
            }

            if (orgItem.POSDevice.Any(p => p.POSNo == POSNo && p.DeviceID != deviceID))
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "已存在相同的POS機編號!!");
            }

            var item = models.GetTable<POSDevice>().Where(p => p.CompanyID == id && p.DeviceID == deviceID).FirstOrDefault();
            if (item == null)
            {
                item = new POSDevice
                {
                    CompanyID = orgItem.CompanyID
                };
                orgItem.POSDevice.Add(item);
            }
            item.POSNo = POSNo;

            models.SubmitChanges();

            return View("~/Views/InvoiceBusiness/POSDevice/DataItem.cshtml", item);

        }

        public ActionResult DeletePOS(int? id, int deviceID)
        {
            var item = models.DeleteAny<POSDevice>(d => d.CompanyID == id && d.DeviceID == deviceID);

            if (item == null)
            {
                return Json(new { result = false, message = "POS機編號錯誤!!" });
            }

            return Json(new { result = true });

        }

        public ActionResult EditPOS(int? id, int deviceID)
        {
            var item = models.GetTable<POSDevice>().Where(d => d.CompanyID == id && d.DeviceID == deviceID).FirstOrDefault();

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "POS機編號錯誤!!");
            }

            return View("~/Views/InvoiceBusiness/POSDevice/EditItem.cshtml", item);

        }

        public ActionResult DataItem(int? id, int deviceID)
        {
            var item = models.GetTable<POSDevice>().Where(d => d.CompanyID == id && d.DeviceID == deviceID).FirstOrDefault();

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "POS機編號錯誤!!");
            }

            return View("~/Views/InvoiceBusiness/POSDevice/DataItem.cshtml", item);
        }

        public ActionResult GenerateGUID()
        {
            return Content(Guid.NewGuid().ToString());
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS, Naming.RoleID.ROLE_SELLER })]
        public ActionResult CreateInvoice(InvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/InvoiceBusiness/CreateInvoice.cshtml");
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS, Naming.RoleID.ROLE_SELLER })]
        public ActionResult EditInvoice(DocumentQueryViewModel viewModel)
        {

            int? invoiceID = viewModel.DocID;
            if (!String.IsNullOrEmpty(viewModel.KeyID))
            {
                invoiceID = viewModel.DecryptKeyValue();
            }

            var item = models!.GetTable<InvoiceItem>().Where(i => i.InvoiceID == invoiceID).FirstOrDefault();
            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "查無發票資料!!");
            }

            if (!CanModifyInvoice(HttpContext.GetUser(), item))
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "無權限修改此發票!!");
            }

            InvoiceViewModel editModel = BuildInvoiceViewModel(item);
            editModel.KeyID = item.InvoiceID.EncryptKey();  // 保留原發票識別，供 CommitEditInvoice 還原原發票

            ViewBag.ViewModel = editModel;
            ViewBag.CommitAction = "CommitEditInvoice";
            return View("~/Views/InvoiceBusiness/CreateInvoice.cshtml");
        }

        [RoleAuthorize(new Naming.RoleID[] { Naming.RoleID.ROLE_SYS, Naming.RoleID.ROLE_SELLER })]
        public ActionResult CommitEditInvoice(InvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (String.IsNullOrEmpty(viewModel.KeyID))
            {
                return Json(new { result = false, message = "發票資料錯誤!!" });
            }

            int originalID;
            try
            {
                originalID = viewModel.DecryptKeyValue();
            }
            catch
            {
                return Json(new { result = false, message = "發票資料錯誤!!" });
            }

            var original = models!.GetTable<InvoiceItem>().Where(i => i.InvoiceID == originalID).FirstOrDefault();
            if (original == null)
            {
                return Json(new { result = false, message = "查無原發票資料!!" });
            }

            if (!CanModifyInvoice(HttpContext.GetUser(), original))
            {
                return Json(new { result = false, message = "無權限修改此發票!!" });
            }

            var seller = models.GetTable<Organization>().Where(o => o.CompanyID == original.SellerID).FirstOrDefault();
            if (seller == null)
            {
                return Json(new { result = false, message = "發票開立人錯誤!!" });
            }

            viewModel.SellerID = seller.CompanyID;
            viewModel.SellerName = seller.CompanyName;
            viewModel.SellerReceiptNo = seller.ReceiptNo;

            // 沿用原發票號碼，避免驗證器另配新號（TrackCode/No 皆有值時不會呼叫 TrackNoManager）
            viewModel.TrackCode = original.TrackCode;
            viewModel.No = original.No;
            viewModel.InvoiceDate = viewModel.InvoiceDate ?? original.InvoiceDate;

            InvoiceViewModelValidator validator = new InvoiceViewModelValidator(new ModelSource(this.DataSource), seller);
            var exception = validator.Validate(viewModel);
            if (exception != null)
            {
                return Json(new { result = false, message = exception.Message });
            }

            // 原發票的號碼配置（若存在則轉移到新發票）
            var assignment = models.GetTable<InvoiceNoAssignment>()
                .Where(a => a.InvoiceID == originalID)
                .Select(a => new { a.IntervalID, a.InvoiceNo })
                .FirstOrDefault();

            var tran = models.EnterTransaction();
            try
            {
                // 1. 建立新發票（沿用原號），取得新的 InvoiceID
                InvoiceItem newItem = validator.InvoiceItem;
                newItem.CDS_Document.ProcessType = (int?)viewModel.InvoiceProcessType;
                if (original.Track != null)
                {
                    newItem.TrackID = original.TrackID;
                    original.Track = null;
                    models.SubmitChanges();
                }

                models!.GetTable<InvoiceItem>().Add(newItem);
                newItem.CDS_Document.PushStepQueueOnSubmit(models, Naming.InvoiceStepDefinition.已開立, Naming.InvoiceProcessType.F0401);
                models.SubmitChanges();

                // 2. 作廢原發票：ProcessVoidInvoiceRequest + 產出 F0701 至 F0701Outbound（於加星號前，確保作廢號碼正確）
                ModelExtension.Properties.AppSettings.Default.F0701Outbound.CheckStoredPath();
                models.ProcessVoidInvoiceRequest(Naming.VoidActionMode.註銷重開, null, original);
                original.CreateF0701().Save(System.IO.Path.Combine(
                    ModelExtension.Properties.AppSettings.Default.F0701Outbound,
                    "F0701_" + original.TrackCode + original.No + ".xml"));

                //// 3. 號碼配置轉移：先刪除原發票的 InvoiceNoAssignment（DeleteAny 立即送出，避免與唯一索引衝突），再以新 InvoiceID 重建一筆
                //if (assignment != null)
                //{
                //    models.DeleteAny<InvoiceNoAssignment>(a => a.InvoiceID == originalID);
                //    models.GetTable<InvoiceNoAssignment>().Add(new InvoiceNoAssignment
                //    {
                //        InvoiceID = newItem.InvoiceID,
                //        IntervalID = assignment.IntervalID,
                //        InvoiceNo = assignment.InvoiceNo,
                //    });
                //    models.SubmitChanges();
                //}

                //// 4. 原發票號結尾加星號，標記為已被取代
                //original.No = original.No + "*";
                //models.SubmitChanges();

                tran.Commit();

                viewModel.TrackCode = newItem.TrackCode;
                viewModel.No = newItem.No;
                return View("~/Views/InvoiceBusiness/Module/InvoiceCreated.cshtml", newItem);
            }
            catch (Exception ex)
            {
                tran.Rollback();
                CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                return Json(new { result = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 發票修改權限：ROLE_SYS 可修改所有發票；ROLE_SELLER 僅可修改本身營業人或代理項下營業人所開立之發票。
        /// </summary>
        private bool CanModifyInvoice(UserProfileWrapper profile, InvoiceItem item)
        {
            if (profile == null)
            {
                return false;
            }

            if (profile.IsSystemAdmin())
            {
                return true;
            }

            // InitializeOrganizationQuery 依角色類別回傳可存取的開立人範圍（自身 / 代理項下）
            return profile.InitializeOrganizationQuery(models!).Any(o => o.CompanyID == item.SellerID);
        }

        /// <summary>
        /// 由既有 InvoiceItem 還原為 InvoiceViewModel，供編輯畫面帶入。
        /// </summary>
        private InvoiceViewModel BuildInvoiceViewModel(InvoiceItem item)
        {
            var viewModel = new InvoiceViewModel
            {
                SellerID = item.SellerID,
                SellerName = item.InvoiceSeller?.Name,
                SellerReceiptNo = item.InvoiceSeller?.ReceiptNo,
                No = item.No,
                TrackCode = item.TrackCode,
                InvoiceDate = item.InvoiceDate,
                InvoiceType = item.InvoiceType,
                CustomsClearanceMark = item.CustomsClearanceMark,
                RandomNo = String.IsNullOrEmpty(item.RandomNo) ? String.Format("{0:0000}", (DateTime.Now.Ticks % 10000)) : item.RandomNo,
                DonateMark = item.InvoiceDonation == null ? "0" : "1",
                NPOBAN = item.InvoiceDonation?.AgencyCode,
                InvoiceProcessType = (Naming.InvoiceProcessType?)item.CDS_Document?.ProcessType,
            };

            var amount = item.InvoiceAmountType;
            if (amount != null)
            {
                viewModel.TaxType = amount.TaxType;
                viewModel.TaxRate = amount.TaxRate;
                viewModel.TaxAmount = amount.TaxAmount;
                viewModel.TotalAmount = amount.TotalAmount;
                viewModel.DiscountAmount = amount.DiscountAmount;
                viewModel.SalesAmount = (amount.SalesAmount ?? 0) + (amount.ZeroTaxSalesAmount ?? 0) + (amount.FreeTaxSalesAmount ?? 0);
            }

            var buyer = item.InvoiceBuyer;
            if (buyer != null)
            {
                viewModel.BuyerReceiptNo = buyer.ReceiptNo == "0000000000" ? null : buyer.ReceiptNo;
                viewModel.BuyerName = buyer.CustomerName ?? buyer.Name;
                viewModel.CustomerID = buyer.CustomerID;
                viewModel.Phone = buyer.Phone;
                viewModel.Address = buyer.Address;
                viewModel.EMail = buyer.EMail;
                viewModel.BuyerMark = (byte?)buyer.BuyerMark;
            }

            var carrier = item.InvoiceCarrier;
            if (carrier != null)
            {
                viewModel.CarrierType = carrier.CarrierType;
                viewModel.CarrierId1 = carrier.CarrierNo;
                viewModel.CarrierId2 = carrier.CarrierNo2;
            }

            var details = item.Product.ToList();
            viewModel.Brief = details.Select(d => (String?)d?.Brief).ToArray();
            viewModel.ItemNo = details.Select(d => d?.InvoiceProductItem.FirstOrDefault()?.ItemNo).ToArray();
            viewModel.ItemRemark = details.Select(d => d?.InvoiceProductItem.FirstOrDefault()?.Remark).ToArray();
            viewModel.Piece = details.Select(d => (int?)d?.InvoiceProductItem.FirstOrDefault()?.Piece).ToArray();
            viewModel.UnitCost = details.Select(d => d?.InvoiceProductItem.FirstOrDefault()?.UnitCost).ToArray();
            viewModel.CostAmount = details.Select(d => d?.InvoiceProductItem.FirstOrDefault()?.CostAmount).ToArray();

            return viewModel;
        }

        public ActionResult UploadData(InvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            return View("~/Views/InvoiceBusiness/UploadData.cshtml");
        }


        public ActionResult PreviewInvoice(InvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            var seller = models.GetTable<Organization>().Where(o => o.CompanyID == viewModel.SellerID).FirstOrDefault();
            if (seller == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "發票開立人錯誤!!");
            }

            viewModel.SellerName = seller.CompanyName;
            viewModel.SellerReceiptNo = seller.ReceiptNo;

            //using (TrackNoManager mgr = new TrackNoManager(new GenericDbContext<ApplicationDbContext>(models.DataContext), seller.CompanyID))
            //{
            //    if (!mgr.ApplyInvoiceDate(viewModel.InvoiceDate.Value))
            //    {
            //        return View("~/Views/Shared/AlertMessage.cshtml", model: String.Format(MessageResources.AlertNullTrackNoInterval, seller.ReceiptNo));
            //    }

            //    viewModel.TrackCode = mgr.InvoiceNoInterval.InvoiceTrackCodeAssignment.InvoiceTrackCode.TrackCode;
            //    viewModel.No = String.Format("{0:00000000}", mgr.PeekInvoiceNo());
            //}

            InvoiceViewModelValidator validator = new InvoiceViewModelValidator(new ModelSource(this.DataSource), seller);
            var exception = validator.Validate(viewModel);
            if (exception != null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: exception.Message);
            }

            InvoiceItem newItem = validator.InvoiceItem;
            viewModel.TrackCode = newItem.TrackCode;
            viewModel.No = newItem.No;
            viewModel.BuyerName = newItem.InvoiceBuyer.Name;
            viewModel.BuyerReceiptNo = newItem.InvoiceBuyer.ReceiptNo;

            ViewBag.Seller = seller;
            return View("PrintInvoice");
        }

        public ActionResult InitializeCommittingInvoice(InvoiceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            var seller = models.GetTable<Organization>().Where(o => o.CompanyID == viewModel.SellerID).FirstOrDefault();
            if (seller == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "發票開立人錯誤!!");
            }

            return View(seller);
        }


        public ActionResult CommitInvoice(InvoiceViewModel viewModel)
        {
            ViewResult result = (ViewResult)InitializeCommittingInvoice(viewModel);
            Organization seller = result.Model as Organization;
            if (seller == null)
            {
                return result;
            }

            viewModel.SellerName = seller.CompanyName;
            viewModel.SellerReceiptNo = seller.ReceiptNo;

            InvoiceViewModelValidator validator = new InvoiceViewModelValidator(new ModelSource(this.DataSource), seller);
            var exception = validator.Validate(viewModel);
            if (exception != null)
            {
                return Json(new { result = false, message = exception.Message });
            }

            try
            {
                InvoiceItem newItem = validator.InvoiceItem;
                newItem.CDS_Document.ProcessType = (int?)viewModel.InvoiceProcessType;

                if (viewModel.ForPreview == true)
                {
                    return View("~/Views/DataView/Module/InvoiceContent.cshtml", newItem);
                }

                models!.GetTable<InvoiceItem>().Add(newItem);
                newItem.CDS_Document.PushStepQueueOnSubmit(models, Naming.InvoiceStepDefinition.已開立, Naming.InvoiceProcessType.F0401);
                if (viewModel.Counterpart == true || !String.IsNullOrEmpty(viewModel.BuyerReceiptNo) || !String.IsNullOrEmpty(viewModel.EMail))
                {
                    //newItem.CDS_Document.PushStepQueueOnSubmit(models, Naming.InvoiceStepDefinition.已接收資料待通知, Naming.InvoiceProcessType.F0401);
                }
                models.SubmitChanges();

                //EIVONotificationFactory.Notify();

                viewModel.TrackCode = newItem.TrackCode;
                viewModel.No = newItem.No;


                return View("~/Views/InvoiceBusiness/Module/InvoiceCreated.cshtml", newItem);
            }
            catch (Exception ex)
            {
                CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                return Json(new { result = false, message = ex.Message });
            }


        }

        public ActionResult CommitA0401(InvoiceViewModel viewModel)
        {
            ViewResult result = (ViewResult)InitializeCommittingInvoice(viewModel);
            Organization seller = result.Model as Organization;
            if (seller == null)
            {
                return result;
            }

            viewModel.SellerName = seller.CompanyName;
            viewModel.SellerReceiptNo = seller.ReceiptNo;

            A0401ViewModelValidator validator = new A0401ViewModelValidator(new ModelSource(this.DataSource), seller);
            var exception = validator.Validate(viewModel);
            if (exception != null)
            {
                return Json(new { result = false, message = exception.Message });
            }

            InvoiceItem newItem = validator.InvoiceItem;
            newItem.CDS_Document.ProcessType = (int?)viewModel.InvoiceProcessType;
            if (viewModel.ForPreview == true)
            {
                return View("~/Views/DataView/Module/InvoiceContent.cshtml", newItem);
            }

            models!.GetTable<InvoiceItem>().Add(newItem);
            newItem.CDS_Document.PushStepQueueOnSubmit(models, Naming.InvoiceStepDefinition.已開立, Naming.InvoiceProcessType.F0401);
            //newItem.CDS_Document.PushStepQueueOnSubmit(models, Naming.InvoiceStepDefinition.已接收資料待通知, Naming.InvoiceProcessType.F0401);
            models.SubmitChanges();

            //EIVONotificationFactory.Notify();

            viewModel.TrackCode = newItem.TrackCode;
            viewModel.No = newItem.No;

            return View("~/Views/InvoiceBusiness/Module/A0401Created.cshtml", newItem);

        }

        public ActionResult CommitA0101(InvoiceViewModel viewModel)
        {
            ViewResult result = (ViewResult)InitializeCommittingInvoice(viewModel);
            Organization seller = result.Model as Organization;
            if (seller == null)
            {
                return result;
            }

            viewModel.SellerName = seller.CompanyName;
            viewModel.SellerReceiptNo = seller.ReceiptNo;

            A0101ViewModelValidator validator = new A0101ViewModelValidator(new ModelSource(this.DataSource), seller);
            var exception = validator.Validate(viewModel);
            if (exception != null)
            {
                return Json(new { result = false, message = exception.Message });
            }

            InvoiceItem newItem = validator.InvoiceItem;
            newItem.CDS_Document.ProcessType = (int?)viewModel.InvoiceProcessType;
            if (viewModel.ForPreview == true)
            {
                return View("~/Views/DataView/Module/InvoiceContent.cshtml", newItem);
            }

            models!.GetTable<InvoiceItem>().Add(newItem);
            newItem.CDS_Document.PushStepQueueOnSubmit(models, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.A0101);
            models.SubmitChanges();

            //EIVONotificationFactory.Notify();

            viewModel.TrackCode = newItem.TrackCode;
            viewModel.No = newItem.No;

            return View("~/Views/InvoiceBusiness/Module/A0401Created.cshtml", newItem);

        }

        public async Task<ActionResult> CommitAllowance([FromBody] AllowanceViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if(viewModel == null)
            {
                viewModel = await PrepareViewModelAsync<AllowanceViewModel>();
                ModelState.Clear();
            }

            if (viewModel.KeyID != null)
            {
                viewModel.SellerID = viewModel.DecryptKeyValue();
            }

            AllowanceViewModelValidator<InvoiceItem> validator = new AllowanceViewModelValidator<InvoiceItem>(this.DataSource, null);
            var exception = validator.Validate(viewModel);
            if (exception != null)
            {
                return Json(new { result = false, message = exception.Message });
            }

            InvoiceAllowance newItem = validator.Allowance;
            //newItem.CDS_Document.ProcessType = (int)viewModel.ProcessType;
            models!.GetTable<InvoiceAllowance>().Add(newItem);
            if (newItem.CDS_Document.ProcessType == (int)Naming.InvoiceProcessType.G0401)
            {
                newItem.CDS_Document.PushStepQueueOnSubmit(models, validator.Seller!.StepReadyToAllowanceMIG(), Naming.InvoiceProcessType.G0401);
                //newItem.CDS_Document.PushStepQueueOnSubmit(models, Naming.InvoiceStepDefinition.已接收資料待通知, Naming.InvoiceProcessType.G0401);
            }
            else
            {
                newItem.CDS_Document.PushStepQueueOnSubmit(models, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.B0101);
            }
            models.SubmitChanges();

            return View("~/Views/InvoiceBusiness/Module/AllowanceCreated.cshtml", newItem);

        }

        public ActionResult EncryptContent(String content,String key)
        {
            com.tradevan.qrutil.QREncrypter qrencrypter = new com.tradevan.qrutil.QREncrypter();
            return Content(qrencrypter.AESEncrypt(content, key));
        }

        public async Task<ActionResult> GetInvoiceRequestSampleAsync(InvoiceRequestViewModel viewModel)
        {

            var items = models.GetTable<InvoiceItem>().Where(i => false).Take(100);

            DataSet ds;
            switch (viewModel.ProcessType)
            {
                case Naming.InvoiceProcessType.A0101_Xlsx_Allocation_ByIssuer:
                case Naming.InvoiceProcessType.F0401_Xlsx_Allocation_ByIssuer:
                    ds = items.GetInvoiceDataForIssuer(models);
                    break;

                case Naming.InvoiceProcessType.F0401_Xlsx_Allocation_ByVAC:
                    ds = items.GetInvoiceDataForVAC(models);
                    break;

                case Naming.InvoiceProcessType.F0401_Xlsx_CBE:
                    ds = items.GetInvoiceDataForCBE(models);
                    break;

                case Naming.InvoiceProcessType.F0401_Xlsx:
                default:
                    ds = items.GetInvoiceData(models);
                    break;

            }

            using (var mgr = new InvoiceDataSetManager(models))
            {
                ds.Tables.Add(mgr.InitializeInvoiceResponseTable());
            }

            await ds.SaveAsExcelAsync(Response, String.Format("attachment;filename={0}", HttpUtility.UrlEncode("InvoiceRequestSample.xlsx")));
            ds.Dispose();

            return new EmptyResult();
        }

        public async Task<ActionResult> GetAllowanceRequestSampleAsync()
        {

            var items = models.GetTable<InvoiceAllowance>().Where(i => false).Take(100);
            using (DataSet ds = items.GetAllowanceData(models))
            {
                using (var mgr = new AllowanceDataSetManager(models))
                {
                    ds.Tables.Add(mgr.InitializeAllowanceResponseTable());
                }
                await ds.SaveAsExcelAsync(Response, String.Format("attachment;filename={0}", HttpUtility.UrlEncode("AllowanceRequestSample.xlsx")));
            }

            return new EmptyResult();
        }

        public async Task<ActionResult> GetVoidInvoiceRequestSampleAsync()
        {

            var items = models.GetTable<InvoiceCancellation>().Where(i => false).Take(100);

            using (DataSet ds = items.GetVoidInvoiceData(models))
            {
                using (var mgr = new VoidInvoiceDataSetManager(models))
                {
                    ds.Tables.Add(mgr.InitializeVoidInvoiceResponseTable());
                }

                await ds.SaveAsExcelAsync(Response, String.Format("attachment;filename={0}", HttpUtility.UrlEncode("VoidInvoiceRequestSample.xlsx")));
            }

            return new EmptyResult();
        }


        public async Task<ActionResult> GetVoidAllowanceRequestSampleAsync()
        {

            var items = models.GetTable<InvoiceAllowanceCancellation>().Where(i => false).Take(100);

            using (DataSet ds = items.GetVoidAllowanceData(models))
            {
                using (var mgr = new VoidAllowanceDataSetManager(models))
                {
                    ds.Tables.Add(mgr.InitializeVoidAllowanceResponseTable());
                }

                await ds.SaveAsExcelAsync(Response, String.Format("attachment;filename={0}", HttpUtility.UrlEncode("VoidAllowanceRequestSample.xlsx")));
            }

            return new EmptyResult();
        }

        public async Task<ActionResult> GetFullAllowanceRequestSampleAsync()
        {

            var items = models.GetTable<InvoiceItem>().Where(i => false).Take(100);
                        

            using (DataSet ds = items.GetInvoiceDataForFullAllowance(models))
            {
                using (var mgr = new AllowanceDataSetManager(models))
                {
                    ds.Tables.Add(mgr.InitializeAllowanceResponseTable());
                }
                await ds.SaveAsExcelAsync(Response, String.Format("attachment;filename={0}", HttpUtility.UrlEncode("FullAllowanceRequestSample.xlsx")));
            }

            return new EmptyResult();
        }

    }

}