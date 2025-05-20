using InvoiceClient.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace InvoiceClient.Helper
{
    public class eInvoiceServiceClient : IDisposable
    {
        private HttpClient _client;
        private String _serviceUrl = Settings.Default.InvoiceClient_WS_Invoice_eInvoiceServiceClient;
        public eInvoiceServiceClient()
        {
            _client = new HttpClient()
            {
                Timeout = TimeSpan.FromMilliseconds(Settings.Default.WS_TimeoutInMilliSeconds),
            };
        }

        private HttpResponseMessage PostData(String actionName, HttpContent content,String? query = null)
        {
            Task<HttpResponseMessage> task = _client.PostAsync($"{_serviceUrl}/{actionName}{query}", content);
            task.Wait();
            HttpResponseMessage response = task.Result;
            response.EnsureSuccessStatusCode();
            return response;
        }

        private HttpResponseMessage GetData(String url)
        {
            Task<HttpResponseMessage> task = _client.GetAsync(url);
            task.Wait();
            HttpResponseMessage response = task.Result;
            response.EnsureSuccessStatusCode();
            return response;
        }

        private XmlDocument PostData(String actionName, XmlDocument uploadData, String? query = null)
        {
            StringContent content = new StringContent(uploadData.OuterXml, Encoding.UTF8, "text/xml");
            return PostXmlData(actionName, content, query);
        }

        private XmlDocument PostXmlData(string actionName, HttpContent content, string? query = null)
        {
            HttpResponseMessage response = PostData(actionName, content, query);
            Task<String> data = response.Content.ReadAsStringAsync();
            data.Wait();
            var doc = new XmlDocument();
            if(data.Result.StartsWith('<'))
            {
                doc.LoadXml(data.Result);
            }
            else
            {
                doc.LoadXml($"<Response>{data.Result}</Response>");
            }
            return doc;
            //return new XmlDocument() { InnerXml = data.Result };
        }

        private XmlDocument PostData(String actionName, String uploadData)
        {
            return new XmlDocument() { InnerXml = PostString(actionName, uploadData) };
        }

        private String PostString(String actionName, String uploadData)
        {
            StringContent content = new StringContent(uploadData);
            HttpResponseMessage response = PostData(actionName, content);
            Task<String> data = response.Content.ReadAsStringAsync();
            data.Wait();
            return data.Result;
        }

        private XmlDocument PostData(String actionName, byte[] uploadData)
        {
            ByteArrayContent content = new ByteArrayContent(uploadData);
            return PostXmlData(actionName, content);
        }

        public XmlDocument UploadInvoice(XmlDocument uploadData)
        {
            return PostData("UploadInvoice", uploadData);
        }

        public XmlDocument UploadAllowance(XmlDocument uploadData)
        {
            return PostData("UploadAllowance", uploadData);
        }

        public XmlDocument UploadB0401(XmlDocument uploadData)
        {
            return PostData("UploadB0401", uploadData);
        }

        public XmlDocument UploadInvoiceAutoTrackNo(XmlDocument uploadData)
        {
            return PostData("UploadInvoiceAutoTrackNo", uploadData);
        }

        public XmlDocument UploadInvoiceCmsCSVAutoTrackNo(byte[] uploadData)
        {
            return PostData("UploadInvoiceCmsCSVAutoTrackNo", uploadData);
        }

        public XmlDocument UploadInvoiceCancellationCmsCSV(byte[] uploadData)
        {
            return PostData("UploadInvoiceCancellationCmsCSV", uploadData);
        }

        public XmlDocument UploadAllowanceCancellationCmsCSV(byte[] uploadData)
        {
            return PostData("UploadAllowanceCancellationCmsCSV", uploadData);
        }

        public XmlDocument UploadInvoiceCancellation(XmlDocument uploadData)
        {
            return PostData("UploadInvoiceCancellation", uploadData);
        }

        public XmlDocument UploadAllowanceCancellation(XmlDocument uploadData)
        {
            return PostData("UploadAllowanceCancellation", uploadData);
        }

        public XmlDocument UploadB0501(XmlDocument uploadData)
        {
            return PostData("UploadB0501", uploadData);
        }

        public XmlDocument GetUpdatedWelfareAgenciesInfo(string sellerReceiptNo)
        {
            return PostData("GetUpdatedWelfareAgenciesInfo", sellerReceiptNo);
        }

        public XmlDocument GetWelfareAgenciesInfo(string sellerReceiptNo)
        {
            return PostData("GetWelfareAgenciesInfo", sellerReceiptNo);
        }

        public XmlDocument GetIncomingInvoices(XmlDocument sellerInfo)
        {
            return PostData("GetIncomingInvoices", sellerInfo);
        }

        public XmlDocument GetInvoicesMap(XmlDocument sellerInfo)
        {
            return PostData("GetInvoicesMap", sellerInfo);
        }

        public XmlDocument GetIncomingInvoiceCancellations(XmlDocument sellerInfo)
        {
            return PostData("GetIncomingInvoiceCancellations", sellerInfo);
        }

        public XmlDocument GetIncomingAllowances(XmlDocument sellerInfo)
        {
            return PostData("GetIncomingAllowances", sellerInfo);
        }

        public XmlDocument GetIncomingAllowanceCancellations(XmlDocument sellerInfo)
        {
            return PostData("GetIncomingAllowanceCancellations", sellerInfo);
        }

        public void AcknowledgeLivingReport(XmlDocument sellerInfo)
        {
            StringContent content = new StringContent(sellerInfo.OuterXml, Encoding.UTF8, "text/xml");
            HttpResponseMessage response = PostData("AcknowledgeLivingReport", content);
            Task<String> data = response.Content.ReadAsStringAsync();
            data.Wait();
        }

        public XmlDocument GetIncomingWinningInvoices(XmlDocument sellerInfo)
        {
            return PostData("GetIncomingWinningInvoices", sellerInfo);
        }

        public string GetSignerCertificateContent(string activationKey)
        {
            String url = $"{_serviceUrl}/GetSignerCertificateContent/{activationKey}";
            HttpResponseMessage response = GetData(url);
            Task<String> data = response.Content.ReadAsStringAsync();
            data.Wait();
            return data.Result;
        }

        public XmlDocument GetRegisteredMember(XmlDocument sellerInfo)
        {
            return PostData("GetRegisteredMember", sellerInfo);
        }

        public XmlDocument UploadA0401(XmlDocument uploadData)
        {
            return PostData("UploadA0401", uploadData);
        }

        public XmlDocument UploadA0201(XmlDocument uploadData)
        {
            return PostData("UploadA0201", uploadData);
        }

        public XmlDocument UploadA0501(XmlDocument uploadData)
        {
            return PostData("UploadA0501", uploadData);
        }

        public XmlDocument UploadB0201(XmlDocument uploadData)
        {
            return PostData("UploadB0201", uploadData);
        }

        public XmlDocument B2CUploadInvoice(XmlDocument uploadData)
        {
            return PostData("B2CUploadInvoice", uploadData);
        }

        public XmlDocument B2CUploadInvoiceCancellation(XmlDocument uploadData)
        {
            return PostData("B2CUploadInvoiceCancellation", uploadData);
        }

        public string[]? ReceiveContentAsPDFForSeller(XmlDocument sellerInfo, string clientID)
        {
            StringContent content = new StringContent(sellerInfo.OuterXml, Encoding.UTF8, "text/xml");
            HttpResponseMessage response = PostData("ReceiveContentAsPDFForSeller", content, $"?clientID={clientID}");
            Task<String> data = response.Content.ReadAsStringAsync();
            data.Wait();
            return JsonConvert.DeserializeObject<String[]>(data.Result);
        }

        public string[]? ReceiveContentAsPDFForIssuer(XmlDocument sellerInfo, string clientID)
        {
            StringContent content = new StringContent(sellerInfo.OuterXml, Encoding.UTF8, "text/xml");
            HttpResponseMessage response = PostData("ReceiveContentAsPDFForIssuer", content, $"?clientID={clientID}");
            Task<String> data = response.Content.ReadAsStringAsync();
            data.Wait();
            return JsonConvert.DeserializeObject<String[]>(data.Result);
        }

        public string[]? ReceiveContentAsPDF(XmlDocument sellerInfo, string clientID)
        {
            StringContent content = new StringContent(sellerInfo.OuterXml, Encoding.UTF8, "text/xml");
            HttpResponseMessage response = PostData("ReceiveContentAsPDF", content, $"?clientID={clientID}");
            Task<String> data = response.Content.ReadAsStringAsync();
            data.Wait();
            return JsonConvert.DeserializeObject<String[]>(data.Result);
        }

        public string[]? ReceiveAllowancePDF(XmlDocument sellerInfo, string clientID)
        {
            StringContent content = new StringContent(sellerInfo.OuterXml, Encoding.UTF8, "text/xml");
            HttpResponseMessage response = PostData("ReceiveAllowancePDF", content, $"?clientID={clientID}");
            Task<String> data = response.Content.ReadAsStringAsync();
            data.Wait();
            return JsonConvert.DeserializeObject<String[]>(data.Result);
        }

        public bool DeleteTempForReceivePDF(XmlDocument sellerInfo, string pdfFile)
        {
            StringContent content = new StringContent(sellerInfo.OuterXml, Encoding.UTF8, "text/xml");
            HttpResponseMessage response = PostData("DeleteTempForReceivePDF", content, $"?pdfFile={pdfFile}");
            Task<String> data = response.Content.ReadAsStringAsync();
            data.Wait();
            return JsonConvert.DeserializeObject<bool>(data.Result);
        }

        public string[]? GetInvoiceMailTracking(XmlDocument sellerInfo, string clientID)
        {
            StringContent content = new StringContent(sellerInfo.OuterXml, Encoding.UTF8, "text/xml");
            HttpResponseMessage response = PostData("GetInvoiceMailTracking", content, $"?clientID={clientID}");
            Task<String> data = response.Content.ReadAsStringAsync();
            data.Wait();
            return JsonConvert.DeserializeObject<String[]>(data.Result);
        }

        public string[]? GetInvoiceReturnedMail(XmlDocument sellerInfo, string clientID)
        {
            StringContent content = new StringContent(sellerInfo.OuterXml, Encoding.UTF8, "text/xml");
            HttpResponseMessage response = PostData("GetInvoiceReturnedMail", content, $"?clientID={clientID}");
            Task<String> data = response.Content.ReadAsStringAsync();
            data.Wait();
            return JsonConvert.DeserializeObject<String[]>(data.Result);
        }

        public void AlertFailedTransaction(XmlDocument sellerInfo)
        {
            PostData("AlertFailedTransaction", sellerInfo);
        }

        public XmlDocument B2BUploadReceipt(XmlDocument uploadData)
        {
            return PostData("B2BUploadReceipt", uploadData);
        }

        public XmlDocument B2BUploadReceiptCancellation(XmlDocument uploadData)
        {
            return PostData("B2BUploadReceiptCancellation", uploadData);
        }

        public XmlDocument B2BUploadInvoice(XmlDocument uploadData)
        {
            return PostData("B2BUploadInvoice", uploadData);
        }

        public XmlDocument B2BUploadAllowance(XmlDocument uploadData)
        {
            return PostData("B2BUploadAllowance", uploadData);
        }

        public XmlDocument B2BUploadAllowanceCancellation(XmlDocument uploadData)
        {
            return PostData("B2BUploadAllowanceCancellation", uploadData);
        }

        public XmlDocument B2BUploadInvoiceCancellation(XmlDocument uploadData)
        {
            return PostData("B2BUploadInvoiceCancellation", uploadData);
        }

        public XmlDocument B2BUploadBuyerInvoice(XmlDocument uploadData)
        {
            return PostData("B2BUploadBuyerInvoice", uploadData);
        }

        public XmlDocument UploadBranchTrackBlank(XmlDocument uploadData)
        {
            return PostData("UploadBranchTrackBlank", uploadData);
        }

        public void AcknowledgeReceiving(XmlDocument uploadData)
        {
            PostData("AcknowledgeReceiving", uploadData);
        }

        public void NotifyCounterpartBusiness(XmlDocument uploadData)
        {
            PostData("NotifyCounterpartBusiness", uploadData);
        }

        public XmlDocument B2BReceiveA0501(XmlDocument sellerInfo)
        {
            return PostData("B2BReceiveA0501", sellerInfo);
        }

        public XmlDocument B2BReceiveB0501(XmlDocument sellerInfo)
        {
            return PostData("B2BReceiveB0501", sellerInfo);
        }

        public string[]? ReceiveContentAsPDFB2B(XmlDocument sellerInfo)
        {
            StringContent content = new StringContent(sellerInfo.OuterXml, Encoding.UTF8, "text/xml");
            HttpResponseMessage response = PostData("ReceiveContentAsPDFB2B", content);
            Task<String> data = response.Content.ReadAsStringAsync();
            data.Wait();
            return JsonConvert.DeserializeObject<String[]>(data.Result);
        }

        public bool DeleteTempForReceivePDFB2B(XmlDocument sellerInfo, int docID)
        {
            StringContent content = new StringContent(sellerInfo.OuterXml, Encoding.UTF8, "text/xml");
            HttpResponseMessage response = PostData("DeleteTempForReceivePDFB2B", content, $"?docID={docID}");
            Task<String> data = response.Content.ReadAsStringAsync();
            data.Wait();
            return JsonConvert.DeserializeObject<bool>(data.Result);
        }

        public XmlDocument UploadCounterpartBusiness(XmlDocument uploadData)
        {
            return PostData("UploadCounterpartBusiness", uploadData);
        }

        public string GetServiceInfo(XmlDocument sellerInfo)
        {
            StringContent content = new StringContent(sellerInfo.OuterXml, Encoding.UTF8, "text/xml");
            HttpResponseMessage response = PostData("GetServiceInfo", content);
            Task<String> data = response.Content.ReadAsStringAsync();
            data.Wait();
            return data.Result;
        }

        public XmlDocument UploadInvoiceAutoTrackNoByClient(XmlDocument uploadData, string clientID, int channelID)
        {
            return PostData("UploadInvoiceAutoTrackNoByClient", uploadData,$"?clientID={clientID}&channelID={channelID}");
        }

        public XmlDocument UploadInvoiceByClient(XmlDocument uploadData, string clientID, int channelID, bool autoTrackNo, int processType)
        {
            return PostData("UploadInvoiceByClient", uploadData, $"?clientID={clientID}&channelID={channelID}&autoTrackNo={autoTrackNo}&processType={processType}");
        }

        public XmlDocument UploadAllowanceByClient(XmlDocument uploadData, string clientID, int channelID)
        {
            return PostData("UploadAllowanceByClient", uploadData, $"?clientID={clientID}&channelID={channelID}");
        }

        public XmlDocument UploadInvoiceAutoTrackNoV2(XmlDocument uploadData)
        {
            return PostData("UploadInvoiceAutoTrackNoV2", uploadData);
        }

        public XmlDocument UploadInvoiceCancellationV2(XmlDocument uploadData)
        {
            return PostData("UploadInvoiceCancellationV2", uploadData);
        }

        public XmlDocument UploadAllowanceV2(XmlDocument uploadData)
        {
            return PostData("UploadAllowanceV2", uploadData);
        }

        public XmlDocument UploadAllowanceCancellationV2(XmlDocument uploadData)
        {
            return PostData("UploadAllowanceCancellationV2", uploadData);
        }

        public XmlDocument UploadInvoiceV2(XmlDocument uploadData)
        {
            return PostData("UploadInvoiceV2", uploadData);
        }

        public XmlDocument UploadInvoiceCmsCSVAutoTrackNoV2(byte[] uploadData)
        {
            return PostData("UploadInvoiceCmsCSVAutoTrackNoV2", uploadData);
        }

        public XmlDocument UploadAllowanceCmsCSV(byte[] uploadData)
        {
            return PostData("UploadAllowanceCmsCSV", uploadData);
        }

        public XmlDocument UploadBranchTrack(XmlDocument uploadData)
        {
            return PostData("UploadBranchTrack", uploadData);
        }

        public XmlDocument UploadInvoiceV2_C0401(XmlDocument uploadData)
        {
            return PostData("UploadInvoiceV2_C0401", uploadData);
        }

        public XmlDocument UploadAllowance_D0401(XmlDocument uploadData)
        {
            return PostData("UploadAllowance_D0401", uploadData);
        }

        public XmlDocument UploadInvoiceCancellationV2_C0501(XmlDocument uploadData)
        {
            return PostData("UploadInvoiceCancellationV2_C0501", uploadData);
        }

        public XmlDocument UploadAllowanceCancellationV2_D0501(XmlDocument uploadData)
        {
            return PostData("UploadAllowanceCancellationV2_D0501", uploadData);
        }

        public XmlDocument UploadInvoiceEnterprise(XmlDocument uploadData)
        {
            return PostData("UploadInvoiceEnterprise", uploadData);
        }

        public XmlDocument GetCurrentYearInvoiceTrackCode(XmlDocument sellerInfo)
        {
            return PostData("GetCurrentYearInvoiceTrackCode", sellerInfo);
        }

        public string[]? GetCustomerIdListByAgent(XmlDocument sellerInfo)
        {
            StringContent content = new StringContent(sellerInfo.OuterXml, Encoding.UTF8, "text/xml");
            HttpResponseMessage response = PostData("GetCustomerIdListByAgent", content);
            Task<String> data = response.Content.ReadAsStringAsync();
            data.Wait();
            return JsonConvert.DeserializeObject<String[]>(data.Result);
        }

        public XmlDocument GetVacantInvoiceNo(XmlDocument sellerInfo, string receiptNo)
        {
            return PostData("GetVacantInvoiceNo", sellerInfo, $"?receiptNo={receiptNo}");
        }

        public XmlDocument UploadInvoiceBuyerCmsCSV(byte[] uploadData)
        {
            return PostData("UploadInvoiceBuyerCmsCSV", uploadData);
        }

        public XmlDocument UploadInvoiceCmsCSV(byte[] uploadData)
        {
            return PostData("UploadInvoiceCmsCSV", uploadData);
        }


        #region IDisposable 成員
        bool _bDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_bDisposed)
            {
                if (disposing)
                {
                    _client?.Dispose();
                }

                _bDisposed = true;
            }
        }

        ~eInvoiceServiceClient()
        {
            Dispose(false);
        }

        #endregion

    }

}