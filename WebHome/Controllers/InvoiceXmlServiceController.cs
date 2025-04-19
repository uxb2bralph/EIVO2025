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

using Business.Helper;
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
using ModelCore.Security.MembershipManagement;

using CommonLib.Core.Utility;
using CommonLib.DataAccess;
using WebHome.Published;
using Newtonsoft.Json;
using CommonLib.Utility;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace WebHome.Controllers
{
    public class InvoiceXmlServiceController : SampleController<InvoiceItem>
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;
        XmlDocument uploadData;

        public InvoiceXmlServiceController(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor) : base(serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: InvoiceService
        public ActionResult AcknowledgeLivingReport()
        {
            (new eInvoiceService(_httpContextAccessor!)).AcknowledgeLivingReport(uploadData);
            return new EmptyResult { };
        }

        public ActionResult AcknowledgeReceiving()
        {
            (new eInvoiceService(_httpContextAccessor!)).AcknowledgeReceiving(uploadData);
            return new EmptyResult { };
        }

        public ActionResult GetServiceInfo()
        {
            var result = (new eInvoiceService(_httpContextAccessor!)).GetServiceInfo(uploadData);
            return Content(result);
        }

        public async Task<ActionResult> UploadInvoiceCmsCSVAutoTrackNoAsync()
        {
            byte[] uploadData = await Request.GetRequestBytesAsync();
            var result = (new eInvoiceService(_httpContextAccessor!)).UploadInvoiceCmsCSVAutoTrackNo(uploadData); 
            return Content(result!.OuterXml, "text/xml"); 
        }

        public ActionResult AlertFailedTransaction()
        {
            (new eInvoiceService(_httpContextAccessor!)).AlertFailedTransaction(uploadData);
            return new EmptyResult { };
        }
        public ActionResult B2BReceiveA0501() { var result = (new eInvoiceService(_httpContextAccessor!)).B2BReceiveA0501(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult B2BReceiveB0501() { var result = (new eInvoiceService(_httpContextAccessor!)).B2BReceiveB0501(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult B2BUploadAllowance() { var result = (new eInvoiceService(_httpContextAccessor!)).B2BUploadAllowance(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult B2BUploadAllowanceCancellation() { var result = (new eInvoiceService(_httpContextAccessor!)).B2BUploadAllowanceCancellation(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult B2BUploadBuyerInvoice() { var result = (new eInvoiceService(_httpContextAccessor!)).B2BUploadBuyerInvoice(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult B2BUploadInvoice() { var result = (new eInvoiceService(_httpContextAccessor!)).B2BUploadInvoice(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult B2BUploadInvoiceCancellation() { var result = (new eInvoiceService(_httpContextAccessor!)).B2BUploadInvoiceCancellation(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult B2BUploadReceipt() { var result = (new eInvoiceService(_httpContextAccessor!)).B2BUploadReceipt(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult B2BUploadReceiptCancellation() { var result = (new eInvoiceService(_httpContextAccessor!)).B2BUploadReceiptCancellation(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult B2CUploadInvoice() { var result = (new eInvoiceService(_httpContextAccessor!)).B2CUploadInvoice(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult B2CUploadInvoiceCancellation() { var result = (new eInvoiceService(_httpContextAccessor!)).B2CUploadInvoiceCancellation(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult DeleteTempForReceivePDF(String pdfFile)
        {
            var result = (new eInvoiceService(_httpContextAccessor!)).DeleteTempForReceivePDF(uploadData,pdfFile);
            return Content(result.ConvertToXml().OuterXml, "text/xml");
        }
        public ActionResult GetCurrentYearInvoiceTrackCode() { var result = (new eInvoiceService(_httpContextAccessor!)).GetCurrentYearInvoiceTrackCode(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult GetCustomerIdListByAgent()
        {
            var result = (new eInvoiceService(_httpContextAccessor!)).GetCustomerIdListByAgent(uploadData);
            return Content(result.ConvertToXml().OuterXml, "text/xml");
        }
        public ActionResult GetIncomingAllowanceCancellations() { var result = (new eInvoiceService(_httpContextAccessor!)).GetIncomingAllowanceCancellations(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult GetIncomingAllowances() { var result = (new eInvoiceService(_httpContextAccessor!)).GetIncomingAllowances(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult GetIncomingInvoiceCancellations() { var result = (new eInvoiceService(_httpContextAccessor!)).GetIncomingInvoiceCancellations(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult GetIncomingInvoices() { var result = (new eInvoiceService(_httpContextAccessor!)).GetIncomingInvoices(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult GetIncomingWinningInvoices() { var result = (new eInvoiceService(_httpContextAccessor!)).GetIncomingWinningInvoices(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult GetInvoiceMailTracking(String clientID)
        {
            var result = (new eInvoiceService(_httpContextAccessor!)).GetInvoiceMailTracking(uploadData,clientID);
            return Content($"{result}");
        }
        public ActionResult GetInvoiceReturnedMail(String clientID)
        {
            var result = (new eInvoiceService(_httpContextAccessor!)).GetInvoiceReturnedMail(uploadData,clientID);
            return Content(result.ConvertToXml().OuterXml, "text/xml");
        }
        public ActionResult GetInvoicesMap() { var result = (new eInvoiceService(_httpContextAccessor!)).GetInvoicesMap(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult GetRegisteredMember() { var result = (new eInvoiceService(_httpContextAccessor!)).GetRegisteredMember(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult GetSignerCertificateContent(String activationKey)
        {
            var result = (new eInvoiceService(_httpContextAccessor!)).GetSignerCertificateContent(activationKey);
            return Content(result);
        }
        public ActionResult GetUpdatedWelfareAgenciesInfo(String sellerReceiptNo)
        {
            var result = (new eInvoiceService(_httpContextAccessor!)).GetUpdatedWelfareAgenciesInfo(sellerReceiptNo);
            return Content(result?.OuterXml, "text/xml");
        }
        public ActionResult GetVacantInvoiceNo(String receiptNo)
        {
            var result = (new eInvoiceService(_httpContextAccessor!)).GetVacantInvoiceNo(uploadData,receiptNo);
            return Content(result.ConvertToXml().OuterXml, "text/xml");
        }
        public ActionResult GetWelfareAgenciesInfo(String sellerReceiptNo) { var result = (new eInvoiceService(_httpContextAccessor!)).GetWelfareAgenciesInfo(sellerReceiptNo); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult NotifyCounterpartBusiness()
        {
            (new eInvoiceService(_httpContextAccessor!)).NotifyCounterpartBusiness(uploadData);
            return new EmptyResult { };
        }

        public ActionResult ReceiveContentAsPDF(String clientID)
        {
            var result = (new eInvoiceService(_httpContextAccessor!)).ReceiveContentAsPDF(uploadData,clientID);
            return Content(result.ConvertToXml().OuterXml, "text/xml");
        }
        public ActionResult UploadA0201() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadA0201(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadA0401() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadA0401(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadA0501() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadA0501(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadAllowance() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadAllowance(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadAllowanceCancellation() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadAllowanceCancellation(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadAllowanceCancellationV2() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadAllowanceCancellationV2(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadAllowanceV2() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadAllowanceV2(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadB0201() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadB0201(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadB0401() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadB0401(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadB0501() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadB0501(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadBranchTrack() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadBranchTrack(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadBranchTrackBlank() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadBranchTrackBlank(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadCounterpartBusiness() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadCounterpartBusiness(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadInvoice() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadInvoice(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadInvoiceAutoTrackNo() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadInvoiceAutoTrackNo(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadInvoiceAutoTrackNoByClient(String clientID,int channelID )
        {
            var result = (new eInvoiceService(_httpContextAccessor!)).UploadInvoiceAutoTrackNoByClient(uploadData,clientID,channelID);
            return Content(result.OuterXml, "text/xml");

        }
        public ActionResult UploadInvoiceAutoTrackNoV2() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadInvoiceAutoTrackNoV2(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadInvoiceCancellation() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadInvoiceCancellationV2(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadInvoiceCancellationV2() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadInvoiceCancellationV2(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadInvoiceCancellationV2_C0501() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadInvoiceCancellationV2_C0501(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadInvoiceEnterprise() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadInvoiceEnterprise(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadInvoiceV2() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadInvoiceV2(uploadData); return Content(result?.OuterXml, "text/xml"); }
        public ActionResult UploadInvoiceV2_C0401() { var result = (new eInvoiceService(_httpContextAccessor!)).UploadInvoiceV2_C0401(uploadData); return Content(result?.OuterXml, "text/xml"); }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            uploadData = new XmlDocument();
            uploadData.LoadXml(RequestBody);
        }

    }
}