using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Net;

using InvoiceClient.Properties;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using ModelCore.Schema.TXN;
using InvoiceClient.Helper;
using InvoiceClient.TransferManagement;
using System.Diagnostics;
using System.Threading.Tasks;


namespace InvoiceClient.Agent
{

    public class InvoicePDFGeneratorForGooglePlay : InvoicePDFGenerator
    {
        public InvoicePDFGeneratorForGooglePlay() : base()
        {

        }

        public override String GetSaleInvoices(int? index = null)
        {
            eInvoiceServiceClient invSvc = InvoiceWatcher.CreateInvoiceService();

            try
            {
                Root token = this.CreateMessageToken("下載電子發票PDF");
                if (index >= 0 && Settings.Default.ProcessCount > 0)
                {
                    token.Request.processIndexSpecified = token.Request.totalProcessCountSpecified = true;
                    token.Request.processIndex = index.Value + Settings.Default.ProcessArrayIndex;
                    token.Request.totalProcessCount = Math.Max(Settings.Default.ProcessArrayCount, Settings.Default.ProcessCount);
                }
                String storedPath = Settings.Default.DownloadDataInAbsolutePath ? Settings.Default.DownloadSaleInvoiceFolder : Path.Combine(Settings.Default.InvoiceTxnPath[0], Settings.Default.DownloadSaleInvoiceFolder);
                storedPath.CheckStoredPath();

                XmlDocument signedReq = token.ConvertToXml().Sign();
                string[] items = invSvc.ReceiveContentAsPDFForIssuer(signedReq, Settings.Default.ClientID);
                if (items != null && items.Length > 1)
                {
                    String serviceUrl = items[0];

                    List<InvoicePDFGeneratorForGooglePlayModel> logItems = new List<InvoicePDFGeneratorForGooglePlayModel>();

                    void proc(int i)
                    {
                        var item = items[i];
                        String[] paramValue = item.Split('\t');
                        String invNo = paramValue[0];

                        String pdfFile = Path.Combine(Settings.Default.PDFGeneratorOutput,
                            _prefix_name + paramValue[1] + "_" + invNo + ".pdf");
                        var url = $"{serviceUrl}?keyID={paramValue[2]}";
                        fetchPDF(pdfFile, url);

                        InvoicePDFGeneratorForGooglePlayModel logItem = new InvoicePDFGeneratorForGooglePlayModel
                        {
                            Date = DateTime.Now,
                            Path = pdfFile,
                            OrderNo = paramValue[1],
                            Url = url,
                        };

                        logItems.Add(logItem);
                    }

                    //Parallel.For(1, items.Length, (idx) =>
                    //{
                    //    proc(idx);
                    //});
                    for (int idx = 1; idx < items.Length; idx++)
                    {
                        proc(idx);
                    }

                    String filePath = Path.Combine(Logger.LogDailyPath, "InvoicePDFGeneratorForGooglePlay.csv");
                    using (StreamWriter writer = new StreamWriter(filePath, true))
                    {
                        using (CsvHelper.CsvWriter csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.CurrentCulture, true))
                        {
                            foreach (var item in logItems)
                            {
                                csv.WriteRecord<InvoicePDFGeneratorForGooglePlayModel>(item);
                                csv.NextRecord();
                            }
                        }
                    }

                    Logger.Debug($"fetch count:{items.Length - 1}");
                    return storedPath;
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return null;
        }        

        protected override void fetchPDF(string pdfFile, string url)
        {
            url = $"{url}&html={true}";
            url.ConvertHtmlToPDF(pdfFile, 1);
        }

        public override Type UIConfigType => typeof(InvoiceClient.MainContent.GoogleInvoiceServerConfigForPDFGeneratorGooglePlay);
    }
}
