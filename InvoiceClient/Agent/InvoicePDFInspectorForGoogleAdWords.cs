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
using ModelCore.Locale;
using ModelCore.InvoiceManagement;
using ModelCore.DataEntity;
using ModelCore.Helper;
using System.Web;
using System.Security.Policy;

namespace InvoiceClient.Agent
{

    public class InvoicePDFInspectorForGoogleAdWords : InvoicePDFInspector
    {

        public InvoicePDFInspectorForGoogleAdWords() : base()
        {
                _tasks = null;
        }


        public override String GetSaleInvoices(int? index = null)
        {
            var ret1 = retrieveFiles2024(index, Naming.ChannelIDType.ForGoogleOnLine);
            var ret2 = retrieveFiles2024(index,Naming.ChannelIDType.ForGoogleTerms);
            return ret1 ?? ret2;
        }

        private string retrieveFiles(int? index,Naming.ChannelIDType? channelID = null)
        {
            eInvoiceServiceClient invSvc = InvoiceWatcher.CreateInvoiceService();

            try
            {
                Root token = this.CreateMessageToken("下載電子發票PDF");
                if(channelID.HasValue)
                {
                    token.Request.channelIDSpecified = true;
                    token.Request.channelID = (int)channelID;
                }
                String storedPath = Settings.Default.DownloadDataInAbsolutePath ? Settings.Default.DownloadSaleInvoiceFolder : Path.Combine(Settings.Default.InvoiceTxnPath[0], Settings.Default.DownloadSaleInvoiceFolder);
                storedPath.CheckStoredPath();
                //storedPath = ValidityAgent.GetDateStylePath(storedPath);

                String tmpPath = Path.Combine(Logger.LogDailyPath, $"{DateTime.Now.Ticks}");

                bool hasNew = false;

                XmlDocument signedReq = token.ConvertToXml().Sign();
                string[] items = invSvc.ReceiveContentAsPDFForSeller(signedReq, Settings.Default.ClientID);
                if (items != null && items.Length > 1)
                {
                    hasNew = true;
                    Directory.CreateDirectory(tmpPath);
                    String serviceUrl = items[0];

                    void proc(int i)
                    {
                        var item = items[i];
                        String[] paramValue = item.Split('\t');
                        String invNo = paramValue[0];

                        String pdfFile = Path.Combine(tmpPath,
                            _prefix_name + paramValue[1] + "_" + invNo + ".pdf");
                        //String pdfFile = Path.Combine(storedPath, string.Format("{0}{1}.pdf", "taiwan_uxb2b_scanned_gui_pdf_", invNo));

                        //token = this.CreateMessageToken("已下載電子發票PDF:" + invNo);// 
                        //signedReq = token.ConvertToXml().Sign();
                        var url = $"{serviceUrl}?keyID={paramValue[2]}";
                        Logger.Info($"retrieve PDF:{item}");
                        //var content = signedReq.UploadData(url, 43200000);
                        fetchPDF(pdfFile, url);
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

                if (hasNew)
                {
                    String args = String.Format("{0}{1:yyyyMMddHHmmssffff}-{4:0000}-AdWords{5} \"{2}\" \"{3}\"",
                        _prefix_name,
                        DateTime.Now,
                        tmpPath,
                        storedPath, index ?? 0,
                        channelID == Naming.ChannelIDType.ForGoogleOnLine
                        ? "-Online"
                        : channelID == Naming.ChannelIDType.ForGoogleTerms
                            ? "-Terms"
                            : null);
                    {
                        Logger.Info($"zip PDF:{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ZipPDF.bat")} {args}");
                        "ZipPDF.bat".RunBatch(args);
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
                    String tmpPath = Path.Combine(Logger.LogDailyPath, $"{DateTime.Now.Ticks}");
                    String storedPath = Settings.Default.DownloadDataInAbsolutePath ? Settings.Default.DownloadSaleInvoiceFolder : Path.Combine(Settings.Default.InvoiceTxnPath[0], Settings.Default.DownloadSaleInvoiceFolder);
                    storedPath.CheckStoredPath();

                    IQueryable<InvoiceItem> queryItems = BuildQueryItems(models, token, channelID);

                    int count = 0;
                    InvoiceItem item = queryItems.FirstOrDefault();
                    if (item != null)
                    {
                        tmpPath.CheckStoredPath();
                    }
                    while (item != null)
                    {
                        if (models.ExecuteCommand("delete DocumentSubscriptionQueue where DocID = {0}", item.InvoiceID) > 0)
                        {
                            String pdfFile = Path.Combine(tmpPath,
                                $"{_prefix_name}{item.InvoicePurchaseOrder.OrderNo}_{item.TrackCode}{item.No}.pdf");

                            String invoiceUrl = String.Format(AppSettings.Default.InvoiceViewUrlPattern, item.InvoiceID);

                            //var args = $"{pdfFile} \"{invoiceUrl}\"";

                            //Logger.Info($"Create PDF:{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CreatePDF.bat")} {args}");
                            //"CreatePDF.bat".RunBatch(args);
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
                            queryItems = BuildQueryItems(models, token, channelID);
                        }
                        item = queryItems.FirstOrDefault();
                    }

                    models.Dispose();

                    if (Directory.Exists(tmpPath))
                    {
                        String args = String.Format("{0}{1:yyyyMMddHHmmssffff}-{4:0000}-AdWords{5} \"{2}\" \"{3}\"",
                            _prefix_name,
                            DateTime.Now,
                            tmpPath,
                            storedPath, index ?? 0,
                            channelID == Naming.ChannelIDType.ForGoogleOnLine
                            ? "-Online"
                            : channelID == Naming.ChannelIDType.ForGoogleTerms
                                ? "-Terms"
                                : null);

                        Logger.Info($"zip PDF:{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ZipPDF.bat")} {args}");
                        "ZipPDF.bat".RunBatch(args);

                        return storedPath;
                    }
                }
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

        private static IQueryable<InvoiceItem> BuildQueryItems(InvoiceManager models, OrganizationToken token, Naming.ChannelIDType? channelID)
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
                //.Where(d => d.DocumentOwner.ClientID == clientID)
                .Join(models.GetTable<InvoiceItem>(), d => d.DocID, i => i.InvoiceID, (d, i) => i);
            IQueryable<InvoiceItem> queryItems = items.Where(i => i.SellerID == token.CompanyID);
            return queryItems;
        }

        public override Type UIConfigType
        {
            get { return typeof(InvoiceClient.MainContent.GoogleInvoiceServerConfigForPDFInspectorAdWords); }
        }

    }
}
