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
using Newtonsoft.Json;

namespace InvoiceClient.Agent
{

    public class AllowancePDFGenerator : InvoicePDFInspector
    {

        private static AllowancePDFGenerator.LocalSettings _Settings;

        static AllowancePDFGenerator()
        {
            string path = Path.Combine(Logger.LogPath, $"{typeof(AllowancePDFGenerator).Name}.json");
            LocalSettings defaultSettings = new LocalSettings { };
            if (File.Exists(path))
            {
                _Settings = JsonConvert.DeserializeObject<LocalSettings>(File.ReadAllText(path));
            }
            else
            {
                _Settings = new LocalSettings();
                File.WriteAllText(path, JsonConvert.SerializeObject((object)_Settings));
            }

            _Settings.AllowancePDFStore = _Settings.AllowancePDFStore.GetEfficientString();
            if (_Settings.AllowancePDFStore == null)
            {
                _Settings.AllowancePDFStore = defaultSettings.AllowancePDFStore;
            }
            _Settings.AllowancePDFStore.CheckStoredPath();

            FolderBuckleWatcher processor = new FolderBuckleWatcher(_Settings.AllowancePDFStore)
            {
                BucklePrefix = _Settings.BucklePrefix,
                ResponsePath = Settings.Default.DownloadDataInAbsolutePath ? Settings.Default.DownloadSaleInvoiceFolder : Path.Combine(Settings.Default.InvoiceTxnPath[0], Settings.Default.DownloadSaleInvoiceFolder),
            };
            processor.StartUp();
        }

        public AllowancePDFGenerator()
        {
            
        }

        public override String GetSaleInvoices(int? index = null)
        {
            eInvoiceServiceClient invSvc = InvoiceWatcher.CreateInvoiceService();

            try
            {
                Root token = this.CreateMessageToken("下載電子折讓PDF");
                if (index >= 0 && Settings.Default.ProcessCount > 0)
                {
                    token.Request.processIndexSpecified = token.Request.totalProcessCountSpecified = true;
                    token.Request.processIndex = index.Value + Settings.Default.ProcessArrayIndex;
                    token.Request.totalProcessCount = Math.Max(Settings.Default.ProcessArrayCount, Settings.Default.ProcessCount);
                    Logger.Info($"retrieve PDF by ProcIdx:{token.Request.processIndex}/{token.Request.totalProcessCount}");
                }

                String storedPath = Settings.Default.DownloadDataInAbsolutePath ? Settings.Default.DownloadSaleInvoiceFolder : Path.Combine(Settings.Default.InvoiceTxnPath[0], Settings.Default.DownloadSaleInvoiceFolder);
                storedPath.CheckStoredPath();

                bool hasNew = false;

                XmlDocument signedReq = token.ConvertToXml().Sign();
                string[] items = invSvc.ReceiveAllowancePDF(signedReq, Settings.Default.ClientID);
                if (items != null && items.Length > 1)
                {
                    hasNew = true;
                    String serviceUrl = items[0];

                    void proc(int i)
                    {
                        var item = items[i];
                        String[] paramValue = item.Split('\t');
                        String allowanceNo = paramValue[0];

                        String pdfFile = Path.Combine(_Settings.AllowancePDFStore,
                            _Settings.BucklePrefix + allowanceNo + ".pdf");

                        var url = $"{serviceUrl}?keyID={paramValue[1]}";
                        Logger.Info($"retrieve PDF:{item}");
                        try
                        {
                            fetchPDF(pdfFile, url);
                        }
                        catch(Exception ex)
                        {
                            Logger.Error(ex);
                            Logger.Info($"fail to retrieve PDF:{item}");
                        }
                    }

                    //Parallel.For(1, items.Length, (idx) =>
                    //{
                    //    proc(idx);
                    //});

                    for (int idx = 1; idx < items.Length; idx++)
                    {
                        proc(idx);
                    }

                }

                return hasNew ? storedPath : null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return null;
        }

        protected override void fetchPDF(string pdfFile, string url)
        {
            String workUrl = $"{url}&html={true}";
            //if (File.Exists(pdfFile))
            //{
            //    File.Delete(pdfFile);
            //}
            workUrl.ConvertHtmlToPDF(pdfFile, 1);

            if (pdfFile.AssertFile())
            {
                Logger.Info($"finish PDF:{workUrl}");

                using (WebClientEx client = new WebClientEx { Timeout = 43200000 })
                {
                    url = $"{url}&ackDel={true}";
                    client.DownloadString(url);
                }
            }
        }

        public override Type UIConfigType
        {
            get { return typeof(InvoiceClient.MainContent.GoogleInvoiceServerConfigForAllowancePDFGenerator); }
        }

        private class LocalSettings
        {
            public String AllowancePDFStore { get; set; } = Path.Combine(Logger.LogPath, "AllowancePDF");
            public String BucklePrefix { get; set; } = "taiwan_uxb2b_scanned_sac_pdf_";
        }

    }
}
