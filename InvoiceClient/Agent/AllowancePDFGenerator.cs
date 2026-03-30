using CommonLib.Core.Utility;
using CommonLib.Utility;
using InvoiceClient.Helper;
using InvoiceClient.Properties;
using InvoiceClient.TransferManagement;
using ModelCore.DataEntity;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Schema.TXN;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Win32;

namespace InvoiceClient.Agent
{

    public class AllowancePDFGenerator : InvoicePDFInspector
    {

        protected static AllowancePDFGeneratorSettings _Settings;

        static AllowancePDFGenerator()
        {
            _Settings = AppSettings.Default.AllowancePDFGeneratorSettings;
            if (_Settings == null)
            {
                _Settings = new AllowancePDFGeneratorSettings();
                AppSettings.Default.AllowancePDFGeneratorSettings = _Settings;
                AppSettings.Default.Save();
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
            var ret1 = retrieveFiles2024(index);
            return ret1;
        }

        protected override void fetchPDF(string pdfFile, string url)
        {
            url = $"{url}&html={true}";
            url.ConvertHtmlToPDF(pdfFile, 1);
        }

        private string retrieveFiles2024(int? index, Naming.ChannelIDType? channelID = null)
        {
            try
            {
                InvoiceManager models = new InvoiceManager();
                ///憑證資料檢查
                ///
                var token = models.GetTable<OrganizationToken>().Where(t => t.Thumbprint == AppSigner.SignerCertificate.Thumbprint).FirstOrDefault();
                if (token != null)//&& token.Organization.OrganizationStatus.EntrustToPrint == true
                {
                    String storedPath = Settings.Default.DownloadDataInAbsolutePath ? Settings.Default.DownloadSaleInvoiceFolder : Path.Combine(Settings.Default.InvoiceTxnPath[0], Settings.Default.DownloadSaleInvoiceFolder);
                    storedPath.CheckStoredPath();

                    IQueryable<InvoiceAllowance> queryItems = BuildQueryItems(models, token, channelID);

                    int count = 0;
                    InvoiceAllowance? item = queryItems.FirstOrDefault();

                    while (item != null)
                    {
                        if (models.ExecuteCommand("delete DocumentSubscriptionQueue where DocID = {0}", item.AllowanceID) > 0)
                        {
                            String pdfFile = Path.Combine(_Settings.AllowancePDFStore,
                                $"{_Settings.BucklePrefix}{item.AllowanceNumber}.pdf");

                            String allowanceUrl = String.Format(AppSettings.Default.AllowanceViewUrlPattern, item.AllowanceID);

                            Logger.Info($"Allowance PDF Url:{allowanceUrl}");
                            fetchPDF(pdfFile, allowanceUrl);

                            //models.ExecuteCommand(@"INSERT INTO [proc].DataProcessLog
                            //                                (DocID, LogDate, Status, StepID)
                            //                                VALUES          ({0},{1},{2},{3})",
                            //        item.InvoiceID, DateTime.Now, (int)Naming.DataProcessStatus.Done,
                            //        (int)Naming.InvoiceStepDefinition.PDF待傳輸);

                            count++;
                        }
                        else
                        {
                            continue;
                        }

                        if (count >= 1024)
                        {
                            count = 0;
                            models.Dispose();
                            models = new InvoiceManager();
                            queryItems = BuildQueryItems(models, token, channelID);
                        }
                        item = queryItems.FirstOrDefault();
                    }

                    models.Dispose();

                    return storedPath;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }

        private static IQueryable<InvoiceAllowance> BuildQueryItems(InvoiceManager models, OrganizationToken token, Naming.ChannelIDType? channelID)
        {
            IQueryable<CDS_Document> docItems = models.GetTable<CDS_Document>();
            if (Settings.Default.ClientID?.Length > 0)
            {
                docItems = docItems.Join(models.GetTable<DocumentOwner>().Where(o => o.ClientID == Settings.Default.ClientID), d => d.DocID, o => o.DocID, (d, o) => d);
            }

            if (channelID.HasValue)
            {
                docItems = docItems.Where(d => d.ChannelID == (int)channelID);
            }

            var items = models.GetTable<DocumentSubscriptionQueue>()
                .Join(docItems, s => s.DocID, d => d.DocID, (s, d) => d)
                .Join(models.GetTable<InvoiceAllowance>(), d => d.DocID, i => i.AllowanceID, (d, i) => i);
            var queryItems = models.GetAllowanceByAgent(items, token.CompanyID);

            return queryItems;
        }

        public override Type UIConfigType
        {
            get { return typeof(InvoiceClient.MainContent.GoogleInvoiceServerConfigForAllowancePDFGenerator); }
        }
    }
}
