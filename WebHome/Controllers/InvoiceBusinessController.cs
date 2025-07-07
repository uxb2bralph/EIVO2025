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
using CommonLib.DataAccess;
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

            return View("POSDeviceList", item);
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

            return View("~/Views/InvoiceBusiness/POSDevice/DataItem.ascx", item);

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

            return View("~/Views/InvoiceBusiness/POSDevice/EditItem.ascx", item);

        }

        public ActionResult DataItem(int? id, int deviceID)
        {
            var item = models.GetTable<POSDevice>().Where(d => d.CompanyID == id && d.DeviceID == deviceID).FirstOrDefault();

            if (item == null)
            {
                return View("~/Views/Shared/AlertMessage.cshtml", model: "POS機編號錯誤!!");
            }

            return View("~/Views/InvoiceBusiness/POSDevice/DataItem.ascx", item);
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

            //using (TrackNoManager mgr = new TrackNoManager(new GenericManager<EIVOEntityDataContext>(models.DataContext), seller.CompanyID))
            //{
            //    if (!mgr.ApplyInvoiceDate(viewModel.InvoiceDate.Value))
            //    {
            //        return View("~/Views/Shared/AlertMessage.cshtml", model: String.Format(MessageResources.AlertNullTrackNoInterval, seller.ReceiptNo));
            //    }

            //    viewModel.TrackCode = mgr.InvoiceNoInterval.InvoiceTrackCodeAssignment.InvoiceTrackCode.TrackCode;
            //    viewModel.No = String.Format("{0:00000000}", mgr.PeekInvoiceNo());
            //}

            InvoiceViewModelValidator<InvoiceItem> validator = new InvoiceViewModelValidator<InvoiceItem>(this.DataSource, seller);
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

            InvoiceViewModelValidator<InvoiceItem> validator = new InvoiceViewModelValidator<InvoiceItem>(this.DataSource, seller);
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

                models!.GetTable<InvoiceItem>().InsertOnSubmit(newItem);
                newItem.CDS_Document.PushStepQueueOnSubmit(models, Naming.InvoiceStepDefinition.已開立, Naming.InvoiceProcessType.F0401);
                if (viewModel.Counterpart == true || !String.IsNullOrEmpty(viewModel.BuyerReceiptNo) || !String.IsNullOrEmpty(viewModel.EMail))
                {
                    newItem.CDS_Document.PushStepQueueOnSubmit(models, Naming.InvoiceStepDefinition.已接收資料待通知, Naming.InvoiceProcessType.F0401);
                }
                models.SubmitChanges();

                //EIVOTurnkeyFactory.Notify();

                viewModel.TrackCode = newItem.TrackCode;
                viewModel.No = newItem.No;


                return View("~/Views/InvoiceBusiness/Module/InvoiceCreated.ascx", newItem);
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

            A0401ViewModelValidator<InvoiceItem> validator = new A0401ViewModelValidator<InvoiceItem>(this.DataSource, seller);
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

            models.GetTable<InvoiceItem>().InsertOnSubmit(newItem);
            A0401Handler.PushStepQueueOnSubmit(models, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
            A0401Handler.PushStepQueueOnSubmit(models, newItem.CDS_Document, Naming.InvoiceStepDefinition.已開立);
            models.SubmitChanges();

            //EIVOTurnkeyFactory.Notify();

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

            A0101ViewModelValidator<InvoiceItem> validator = new A0101ViewModelValidator<InvoiceItem>(this.DataSource, seller);
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

            models!.GetTable<InvoiceItem>().InsertOnSubmit(newItem);
            newItem.CDS_Document.PushStepQueueOnSubmit(models, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.A0101);
            models.SubmitChanges();

            //EIVOTurnkeyFactory.Notify();

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
            models!.GetTable<InvoiceAllowance>().InsertOnSubmit(newItem);
            if (newItem.CDS_Document.ProcessType == (int)Naming.InvoiceProcessType.G0401)
            {
                newItem.CDS_Document.PushStepQueueOnSubmit(models, validator.Seller!.StepReadyToAllowanceMIG(), Naming.InvoiceProcessType.G0401);
                newItem.CDS_Document.PushStepQueueOnSubmit(models, Naming.InvoiceStepDefinition.已接收資料待通知, Naming.InvoiceProcessType.G0401);
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
                case Naming.InvoiceProcessType.A0401_Xlsx_Allocation_ByIssuer:
                case Naming.InvoiceProcessType.C0401_Xlsx_Allocation_ByIssuer:
                    ds = items.GetInvoiceDataForIssuer(models);
                    break;

                case Naming.InvoiceProcessType.C0401_Xlsx_Allocation_ByVAC:
                    ds = items.GetInvoiceDataForVAC(models);
                    break;

                case Naming.InvoiceProcessType.C0401_Xlsx_CBE:
                    ds = items.GetInvoiceDataForCBE(models);
                    break;

                case Naming.InvoiceProcessType.C0401_Xlsx:
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