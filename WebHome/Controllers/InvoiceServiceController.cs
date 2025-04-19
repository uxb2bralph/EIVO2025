using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

using WebHome.Helper;
using ModelCore.Models.ViewModel;
using WebHome.Properties;
using WebHome.Published;

using ModelCore.DataEntity;
using ModelCore.DocumentManagement;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TurnKey;
using ModelCore.Schema.TXN;
using ModelCore.InvoiceManagement.ErrorHandle;
using CommonLib.Security.UseCrypto;
using CommonLib.Utility;
using CommonLib.Core.Utility;
using ModelCore.Notification;
using ModelCore.Service;
using ModelCore.InvoiceManagement.InvoiceProcess;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebHome.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Consumes("application/xml", "text/xml")]
    //[Produces("text/xml")]
    public class InvoiceServiceController : Controller
    {
        private readonly eInvoiceService _invoiceService;
        private XmlDocument? uploadData;
        public InvoiceServiceController(IHttpContextAccessor httpContextAccessor)
        {
            _invoiceService = new eInvoiceService(httpContextAccessor);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            if (context.HttpContext.Request.ContentType != null)
            {
                var contentType = context.HttpContext.Request.ContentType.ToLower();
                if (contentType.Contains("text/xml") || contentType.Contains("application/xml"))
                {
                    var task = GetXmlFromRequestAsync();
                    task.Wait();
                    uploadData = task.Result;
                }
            }
        }
        // Helper method to process XML content from request body
        private async Task<XmlDocument> GetXmlFromRequestAsync()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var xmlContent = await reader.ReadToEndAsync();
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlContent);
                return xmlDoc;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse XML request body", ex);
            }
        }

        [HttpPost("UploadInvoice")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadInvoice()
        {
            var result = _invoiceService.UploadInvoice(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadAllowance")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadAllowance()
        {
            
            var result = _invoiceService.UploadAllowance(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadB0401")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadB0401()
        {
            
            var result = _invoiceService.UploadB0401(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadInvoiceAutoTrackNo")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadInvoiceAutoTrackNo()
        {
            
            var result = _invoiceService.UploadInvoiceAutoTrackNo(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadInvoiceCmsCSVAutoTrackNo")]
        public XmlDocument UploadInvoiceCmsCSVAutoTrackNo([FromBody] byte[] uploadData)
        {
            return _invoiceService.UploadInvoiceCmsCSVAutoTrackNo(uploadData!);
        }

        [HttpPost("UploadInvoiceCancellationCmsCSV")]
        public XmlDocument UploadInvoiceCancellationCmsCSV([FromBody] byte[] uploadData)
        {
            return _invoiceService.UploadInvoiceCancellationCmsCSV(uploadData!);
        }

        [HttpPost("UploadAllowanceCancellationCmsCSV")]
        public XmlDocument UploadAllowanceCancellationCmsCSV([FromBody] byte[] uploadData)
        {
            return _invoiceService.UploadAllowanceCancellationCmsCSV(uploadData!);
        }

        [HttpPost("UploadInvoiceCancellation")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadInvoiceCancellation()
        {
            
            var result = _invoiceService.UploadInvoiceCancellation(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadAllowanceCancellation")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadAllowanceCancellation()
        {
            
            var result = _invoiceService.UploadAllowanceCancellation(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadB0501")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadB0501()
        {
            
            var result = _invoiceService.UploadB0501(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpGet("GetUpdatedWelfareAgenciesInfo/{sellerReceiptNo}")]
        public XmlDocument GetUpdatedWelfareAgenciesInfo(string sellerReceiptNo)
        {
            return _invoiceService.GetUpdatedWelfareAgenciesInfo(sellerReceiptNo);
        }

        [HttpGet("GetWelfareAgenciesInfo/{sellerReceiptNo}")]
        public XmlDocument GetWelfareAgenciesInfo(string sellerReceiptNo)
        {
            return _invoiceService.GetWelfareAgenciesInfo(sellerReceiptNo);
        }

        [HttpPost("GetIncomingInvoices")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult GetIncomingInvoices()
        {
            
            var result = _invoiceService.GetIncomingInvoices(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("GetInvoicesMap")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult GetInvoicesMap()
        {
            
            var result = _invoiceService.GetInvoicesMap(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("GetIncomingInvoiceCancellations")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult GetIncomingInvoiceCancellations()
        {
            
            var result = _invoiceService.GetIncomingInvoiceCancellations(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("GetIncomingAllowances")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult GetIncomingAllowances()
        {
            
            var result = _invoiceService.GetIncomingAllowances(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("GetIncomingAllowanceCancellations")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult GetIncomingAllowanceCancellations()
        {
            
            var result = _invoiceService.GetIncomingAllowanceCancellations(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("AcknowledgeLivingReport")]
        [Consumes("application/xml", "text/xml")]
        public void AcknowledgeLivingReport()
        {
            _invoiceService.AcknowledgeLivingReport(uploadData!);
        }

        [HttpPost("GetIncomingWinningInvoices")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult GetIncomingWinningInvoices()
        {
            
            var result = _invoiceService.GetIncomingWinningInvoices(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpGet("GetSignerCertificateContent/{activationKey}")]
        public string GetSignerCertificateContent(string activationKey)
        {
            return _invoiceService.GetSignerCertificateContent(activationKey);
        }

        [HttpPost("GetRegisteredMember")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult GetRegisteredMember()
        {
            
            var result = _invoiceService.GetRegisteredMember(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadA0401")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadA0401()
        {
            
            var result = _invoiceService.UploadA0401(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadA0201")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadA0201()
        {
            
            var result = _invoiceService.UploadA0201(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadA0501")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadA0501()
        {
            
            var result = _invoiceService.UploadA0501(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadB0201")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadB0201()
        {
            
            var result = _invoiceService.UploadB0201(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("B2CUploadInvoice")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult B2CUploadInvoice()
        {
            
            var result = _invoiceService.B2CUploadInvoice(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("B2CUploadInvoiceCancellation")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult B2CUploadInvoiceCancellation()
        {
            
            var result = _invoiceService.B2CUploadInvoiceCancellation(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("ReceiveContentAsPDFForSeller")]
        [Consumes("application/xml", "text/xml")]
        public string[] ReceiveContentAsPDFForSeller( [FromQuery] string clientID)
        {
            return _invoiceService.ReceiveContentAsPDFForSeller(uploadData!, clientID);
        }

        [HttpPost("ReceiveContentAsPDFForIssuer")]
        [Consumes("application/xml", "text/xml")]
        public string[] ReceiveContentAsPDFForIssuer( [FromQuery] string clientID)
        {
            return _invoiceService.ReceiveContentAsPDFForIssuer(uploadData!, clientID);
        }

        [HttpPost("ReceiveContentAsPDF")]
        [Consumes("application/xml", "text/xml")]
        public string[] ReceiveContentAsPDF( [FromQuery] string clientID)
        {
            return _invoiceService.ReceiveContentAsPDF(uploadData!, clientID);
        }

        [HttpPost("ReceiveAllowancePDF")]
        [Consumes("application/xml", "text/xml")]
        public string[] ReceiveAllowancePDF( [FromQuery] string clientID)
        {
            return _invoiceService.ReceiveAllowancePDF(uploadData!, clientID);
        }

        [HttpPost("DeleteTempForReceivePDF")]
        [Consumes("application/xml", "text/xml")]
        public bool DeleteTempForReceivePDF( [FromQuery] string pdfFile)
        {
            return _invoiceService.DeleteTempForReceivePDF(uploadData!, pdfFile);
        }

        [HttpPost("GetInvoiceMailTracking")]
        [Consumes("application/xml", "text/xml")]
        public string[] GetInvoiceMailTracking( [FromQuery] string clientID)
        {
            return _invoiceService.GetInvoiceMailTracking(uploadData!, clientID);
        }

        [HttpPost("GetInvoiceReturnedMail")]
        [Consumes("application/xml", "text/xml")]
        public string[] GetInvoiceReturnedMail( [FromQuery] string clientID)
        {
            return _invoiceService.GetInvoiceReturnedMail(uploadData!, clientID);
        }

        [HttpPost("AlertFailedTransaction")]
        [Consumes("application/xml", "text/xml")]
        public void AlertFailedTransaction()
        {
            _invoiceService.AlertFailedTransaction(uploadData!);
        }

        [HttpPost("B2BUploadReceipt")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult B2BUploadReceipt()
        {
            
            var result = _invoiceService.B2BUploadReceipt(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("B2BUploadReceiptCancellation")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult B2BUploadReceiptCancellation()
        {
            
            var result = _invoiceService.B2BUploadReceiptCancellation(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("B2BUploadInvoice")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult B2BUploadInvoice()
        {
            
            var result = _invoiceService.B2BUploadInvoice(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("B2BUploadAllowance")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult B2BUploadAllowance()
        {
            
            var result = _invoiceService.B2BUploadAllowance(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("B2BUploadAllowanceCancellation")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult B2BUploadAllowanceCancellation()
        {
            
            var result = _invoiceService.B2BUploadAllowanceCancellation(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("B2BUploadInvoiceCancellation")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult B2BUploadInvoiceCancellation()
        {
            
            var result = _invoiceService.B2BUploadInvoiceCancellation(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("B2BUploadBuyerInvoice")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult B2BUploadBuyerInvoice()
        {
            
            var result = _invoiceService.B2BUploadBuyerInvoice(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadBranchTrackBlank")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadBranchTrackBlank()
        {
            
            var result = _invoiceService.UploadBranchTrackBlank(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("B2BReceiveA0501")]
        [Consumes("application/xml", "text/xml")]
        public XmlDocument B2BReceiveA0501()
        {
            return _invoiceService.B2BReceiveA0501(uploadData!);
        }

        [HttpPost("B2BReceiveB0501")]
        [Consumes("application/xml", "text/xml")]
        public XmlDocument B2BReceiveB0501()
        {
            return _invoiceService.B2BReceiveB0501(uploadData!);
        }

        [HttpPost("ReceiveContentAsPDFB2B")]
        [Consumes("application/xml", "text/xml")]
        public string[] ReceiveContentAsPDFB2B()
        {
            return _invoiceService.ReceiveContentAsPDFB2B(uploadData!);
        }

        [HttpPost("DeleteTempForReceivePDFB2B")]
        [Consumes("application/xml", "text/xml")]
        public bool DeleteTempForReceivePDFB2B( [FromQuery] int docID)
        {
            return _invoiceService.DeleteTempForReceivePDFB2B(uploadData!, docID);
        }

        [HttpPost("UploadCounterpartBusiness")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadCounterpartBusiness()
        {
            
            var result = _invoiceService.UploadCounterpartBusiness(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("GetServiceInfo")]
        [Consumes("application/xml", "text/xml")]
        public string GetServiceInfo()
        {
            return _invoiceService.GetServiceInfo(uploadData!);
        }

        [HttpPost("AcknowledgeReceiving")]
        [Consumes("application/xml", "text/xml")]
        public void AcknowledgeReceiving()
        {
            _invoiceService.AcknowledgeReceiving(uploadData!);
        }

        [HttpPost("NotifyCounterpartBusiness")]
        [Consumes("application/xml", "text/xml")]
        public void NotifyCounterpartBusiness()
        {
            _invoiceService.NotifyCounterpartBusiness(uploadData!);
        }

        #region Methods from eInvoiceService_201612
        [HttpPost("UploadInvoiceBuyerCmsCSV")]
        public XmlDocument UploadInvoiceBuyerCmsCSV([FromBody] byte[] uploadData)
        {
            return _invoiceService.UploadInvoiceBuyerCmsCSV(uploadData!);
        }

        [HttpPost("UploadInvoiceCmsCSV")]
        public XmlDocument UploadInvoiceCmsCSV([FromBody] byte[] uploadData)
        {
            return _invoiceService.UploadInvoiceCmsCSV(uploadData!);
        }
        #endregion

        #region Methods from eInvoiceService_201311
        [HttpPost("UploadInvoiceAutoTrackNoByClient")]
        [Consumes("application/xml", "text/xml")]
        public XmlDocument UploadInvoiceAutoTrackNoByClient( [FromQuery] string clientID, [FromQuery] int channelID)
        {
            return _invoiceService.UploadInvoiceAutoTrackNoByClient(uploadData!, clientID, channelID);
        }

        [HttpPost("UploadInvoiceByClient")]
        [Consumes("application/xml", "text/xml")]
        public XmlDocument UploadInvoiceByClient( [FromQuery] string clientID, [FromQuery] int channelID, [FromQuery] bool autoTrackNo, [FromQuery] int processType)
        {
            return _invoiceService.UploadInvoiceByClient(uploadData!, clientID, channelID, autoTrackNo, processType);
        }

        [HttpPost("UploadAllowanceByClient")]
        [Consumes("application/xml", "text/xml")]
        public XmlDocument UploadAllowanceByClient( [FromQuery] string clientID, [FromQuery] int channelID)
        {
            return _invoiceService.UploadAllowanceByClient(uploadData!, clientID, channelID);
        }

        [HttpPost("UploadInvoiceAutoTrackNoV2")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadInvoiceAutoTrackNoV2()
        {
            
            var result = _invoiceService.UploadInvoiceAutoTrackNoV2(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadInvoiceCancellationV2")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadInvoiceCancellationV2()
        {
            
            var result = _invoiceService.UploadInvoiceCancellationV2(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadAllowanceV2")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadAllowanceV2()
        {
            
            var result = _invoiceService.UploadAllowanceV2(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadAllowanceCancellationV2")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadAllowanceCancellationV2()
        {
            
            var result = _invoiceService.UploadAllowanceCancellationV2(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadInvoiceV2")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadInvoiceV2()
        {

            var result = _invoiceService.UploadInvoiceV2(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadInvoiceCmsCSVAutoTrackNoV2")]
        public XmlDocument UploadInvoiceCmsCSVAutoTrackNoV2([FromBody] byte[] uploadData)
        {
            return _invoiceService.UploadInvoiceCmsCSVAutoTrackNoV2(uploadData!);
        }

        [HttpPost("UploadAllowanceCmsCSV")]
        public XmlDocument UploadAllowanceCmsCSV([FromBody] byte[] uploadData)
        {
            return _invoiceService.UploadAllowanceCmsCSV(uploadData!);
        }

        [HttpPost("UploadBranchTrack")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadBranchTrack()
        {
            
            var result = _invoiceService.UploadBranchTrack(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadInvoiceV2_C0401")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadInvoiceV2_C0401()
        {
            
            var result = _invoiceService.UploadInvoiceV2_C0401(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadAllowance_D0401")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadAllowance_D0401()
        {
            
            var result = _invoiceService.UploadAllowance_D0401(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadInvoiceCancellationV2_C0501")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadInvoiceCancellationV2_C0501()
        {
            
            var result = _invoiceService.UploadInvoiceCancellationV2_C0501(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadAllowanceCancellationV2_D0501")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadAllowanceCancellationV2_D0501()
        {
            
            var result = _invoiceService.UploadAllowanceCancellationV2_D0501(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("UploadInvoiceEnterprise")]
        [Consumes("application/xml", "text/xml")]
        public IActionResult UploadInvoiceEnterprise()
        {
            
            var result = _invoiceService.UploadInvoiceEnterprise(uploadData!);
            return Content(result.OuterXml, "text/xml");
        }

        [HttpPost("GetCurrentYearInvoiceTrackCode")]
        [Consumes("application/xml", "text/xml")]
        public XmlDocument GetCurrentYearInvoiceTrackCode()
        {
            return _invoiceService.GetCurrentYearInvoiceTrackCode(uploadData!);
        }

        [HttpPost("GetCustomerIdListByAgent")]
        [Consumes("application/xml", "text/xml")]
        public string[] GetCustomerIdListByAgent()
        {
            return _invoiceService.GetCustomerIdListByAgent(uploadData!);
        }

        [HttpPost("GetVacantInvoiceNo")]
        [Consumes("application/xml", "text/xml")]
        [Produces("text/xml")]
        public XmlDocument GetVacantInvoiceNo( [FromQuery] string receiptNo)
        {
            return _invoiceService.GetVacantInvoiceNo(uploadData!, receiptNo);
        }
        #endregion

        [HttpGet("GetStrings")]
        public string[] GetStrings([FromQuery] string clientID)
        {
            return ["aaa", "bbb", DateTime.Now.ToString(), clientID];
        }

        [HttpGet("GetBoolean")]
        public bool GetBoolean([FromQuery] string clientID)
        {
            return clientID == "uxb2b";
        }



    }
}