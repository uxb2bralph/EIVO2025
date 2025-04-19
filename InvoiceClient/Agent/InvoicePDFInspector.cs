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

    public class InvoicePDFInspector : ServerInspector
    {
        protected DateTime _dateCounter;
        protected Task[] _tasks;
        protected String _prefix_name = "taiwan_uxb2b_scanned_gui_pdf_";

        public InvoicePDFInspector()
        {
            initializeCounter();
            if(Settings.Default.ProcessCount>0)
            {
                _tasks = new Task[Settings.Default.ProcessCount];
            }
        }

        private void initializeCounter()
        {
            _dateCounter = DateTime.Now;
        }

        public virtual String GetSaleInvoices(int? index = null)
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
                    Logger.Info($"retrieve PDF by ProcIdx:{token.Request.processIndex}/{token.Request.totalProcessCount}");
                }
                String storedPath = Settings.Default.DownloadDataInAbsolutePath ? Settings.Default.DownloadSaleInvoiceFolder : Path.Combine(Settings.Default.InvoiceTxnPath[0], Settings.Default.DownloadSaleInvoiceFolder);
                ValidityAgent.CheckAndCreatePath(storedPath);
                //storedPath = ValidityAgent.GetDateStylePath(storedPath);

                String tmpPath = Path.Combine(Logger.LogDailyPath, $"{(index ?? DateTime.Now.Ticks):0000}");

                bool hasNew = false;

                XmlDocument signedReq = token.ConvertToXml().Sign();
                string[] items = invSvc.ReceiveContentAsPDF(signedReq, Settings.Default.ClientID);
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

                    //if (Settings.Default.ProcessCount > 1)
                    //{
                    //    int start = 1;
                    //    while (start < items.Length)
                    //    {
                    //        int end = Math.Min(start + Settings.Default.ProcessCount, items.Length);
                    //        Parallel.For(start, end, (idx) =>
                    //        {
                    //            proc(idx);
                    //        });
                    //        start = end;
                    //    }
                    //}
                    //else
                    //{
                    //    for (int i = 1; i < items.Length; i++)
                    //    {
                    //        proc(i);
                    //    }
                    //}

                    Parallel.For(1, items.Length, (idx) =>
                    {
                        proc(idx);
                    });

                }

                if (hasNew)
                {
                    String args = String.Format("{0}{1:yyyyMMddHHmmssffff}{4:0000} \"{2}\" \"{3}\"", _prefix_name, DateTime.Now, tmpPath, storedPath, index ?? 0);
                    //if (index.HasValue)
                    //{
                    //    ZipPDFFactory.Notify(args);
                    //}
                    //else
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

        protected virtual void fetchPDF(string pdfFile, string url)
        {
            using (WebClientEx client = new WebClientEx { Timeout = 43200000 })
            {
                client.DownloadFile(url, pdfFile);
                Logger.Info($"finish PDF:{url}");
                url = $"{url}&ackDel={true}";
                client.DownloadString(url);
            }
        }

        public override void StartUp()
        {
            if (Settings.Default.IsAutoInvService && !_isRunning)
            {
                ThreadPool.QueueUserWorkItem(p =>
                {
                    while (Settings.Default.IsAutoInvService)
                    {
                        _isRunning = true;

                        ServerInspector.AcknowledgeServer();
                        String result = null;
                        if (_tasks != null)
                        {
                            do
                            {
                                result = null;
                                Logger.Info($"start retrieving PDF:{DateTime.Now}");
                                for (int i = 0; i < Settings.Default.ProcessCount; i++)
                                {
                                    int index = i;
                                    _tasks[i] = new Task(() =>
                                        {
                                            var r = GetSaleInvoices(index);
                                            if (r != null)
                                                result = r;
                                        })
                                    { };
                                    _tasks[i].Start();
                                }
                                var t = Task.Factory.ContinueWhenAll(_tasks, ts =>
                                {
                                    Logger.Info($"end retrieving PDF:{DateTime.Now}");
                                });
                                t.Wait();
                            } while (result != null);
                        }
                        else
                        {
                            while ((result = GetSaleInvoices()) != null) ;
                        }

                        Thread.Sleep(Settings.Default.AutoInvServiceInterval > 0 ? Settings.Default.AutoInvServiceInterval * 60 * 1000 : 1800000);
                    }
                    _isRunning = false;
                });
            }
        }

        public override Type UIConfigType
        {
            get { return typeof(InvoiceClient.MainContent.GoogleInvoiceServerConfig); }
        }

        public override void ExecutiveService(List<string> pathInfo)
        {
            if (!Settings.Default.IsAutoInvService)
            {
                String path;
                while ((path = this.GetSaleInvoices()) != null)
                {
                    pathInfo.Add(path);
                }
            }
            
            base.ExecutiveService(pathInfo);
        }
    }
}
