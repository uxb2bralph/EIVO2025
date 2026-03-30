using CommonLib.Core.Utility;
using CommonLib.Utility;
using InvoiceClient.Helper;
using InvoiceClient.Properties;
using InvoiceClient.TransferManagement;
using ModelCore.DataEntity;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Helper;
using ModelCore.Schema.TXN;
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


namespace InvoiceClient.Agent
{

    public class InvoicePDFGeneratorForGooglePlay : InvoicePDFInspectorForGoogleAdWords
    {
        public InvoicePDFGeneratorForGooglePlay() : base()
        {

        }

        public override String GetSaleInvoices(int? index = null)
        {
            return retrieveFiles2026(index);
        }

        private string retrieveFiles2026(int? index, Naming.ChannelIDType? channelID = null)
        {
            try
            {
                InvoiceManager models = new InvoiceManager();
                ///憑證資料檢查
                ///
                var token = models.GetTable<OrganizationToken>().Where(t => t.Thumbprint == AppSigner.SignerCertificate.Thumbprint).FirstOrDefault();
                if (token != null)//&& token.Organization.OrganizationStatus.EntrustToPrint == true
                {
                    IQueryable<InvoiceItem> queryItems = models.InquireInvoiceSubscription(token.CompanyID, null, channelID, Settings.Default.ClientID, true);

                    int count = 0;
                    InvoiceItem? item = queryItems.FirstOrDefault();
                    
                    while (item != null)
                    {
                        if (models.ExecuteCommand("delete DocumentSubscriptionQueue where DocID = {0}", item.InvoiceID) > 0)
                        {
                            String pdfFile = Path.Combine(Settings.Default.PDFGeneratorOutput,
                                $"{_prefix_name}{item.InvoicePurchaseOrder?.OrderNo}_{item.TrackCode}{item.No}.pdf");

                            String invoiceUrl = String.Format(AppSettings.Default.InvoiceViewUrlPattern, item.InvoiceID);

                            Logger.Info($"Invoice PDF Url:{invoiceUrl}");
                            fetchPDF(pdfFile, invoiceUrl);

                            models.ExecuteCommand(@"INSERT INTO [proc].DataProcessLog
                                                            (DocID, LogDate, Status, StepID)
                                                            VALUES          ({0},{1},{2},{3})",
                                    item.InvoiceID, DateTime.Now, (int)Naming.DataProcessStatus.Done,
                                    (int)Naming.InvoiceStepDefinition.PDF待傳輸);

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
                            queryItems = models.InquireInvoiceSubscription(token.CompanyID, null, channelID, Settings.Default.ClientID, true);
                        }
                        item = queryItems.FirstOrDefault();
                    }

                    models.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return Settings.Default.PDFGeneratorOutput;
        }

        public override Type UIConfigType => typeof(InvoiceClient.MainContent.GoogleInvoiceServerConfigForPDFGeneratorGooglePlay);
    }
}
