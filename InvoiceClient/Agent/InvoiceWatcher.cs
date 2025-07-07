using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using InvoiceClient.Helper;
using InvoiceClient.Properties;
using InvoiceClient.TransferManagement;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.TXN;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using System.ServiceModel;
using CommonLib.Helper;


namespace InvoiceClient.Agent
{
    public class InvoiceWatcher : IDisposable
    {
        protected String _failedTxnPath;
        protected String _inProgressPath;
        protected String _ResponsedPath;
        protected String _requestPath;
        protected bool _retryConnection;
        protected Naming.ChannelIDType _channelID = Naming.ChannelIDType.ForGoogleOnLine;

        public Naming.InvoiceProcessType? PreferredProcessType { get; set; }
        public String TxnPath { get; set; }

        public ITransferManager TransferManager { get; set; }

        public static readonly HttpClient TheHttpClient = new HttpClient()
        {
            Timeout = Timeout.InfiniteTimeSpan,
        };

        protected static List<String> __FailedTxnPath;
        static CookieContainer __CookieContainer;

        static InvoiceWatcher()
        {
            __CookieContainer = new CookieContainer();
            __CookieContainer.Add(new Uri(Settings.Default.InvoiceClient_WS_Invoice_eInvoiceServiceClient), new Cookie("cLang", Settings.Default.AppCulture));

            if (!Settings.Default.AutoRetry && Settings.Default.EnableFailedUploadAlert)
            {
                __FailedTxnPath = new List<string>();
                Task.Delay(10000).ContinueWith(ts1 =>
                {
                    __SendFailedTxnAlert();
                });

                ((Action)(() =>
                {
                    __SendFailedTxnAlert();
                })).RunForever(30 * 60 * 1000, true);
            }
        }

        private QueuedProcessHandler _handler;

        public InvoiceWatcher(String fullPath)
        {
            Logger.Info($"watching path:{fullPath}");
            fullPath.CheckStoredPath();
            _requestPath = fullPath;

            _retryConnection = Settings.Default.RetryOnConnectException;
            prepareStorePath(fullPath);

            _handler = new QueuedProcessHandler(FileLogger.Logger)
            {
                Process = () =>
                {
                    if (_dependentWatcher != null)
                        _dependentWatcher.InvokeProcess();
                    else
                        doProcess();

                    if (Settings.Default.AutoRetry)
                    {
                        Retry();
                    }
                },
                MilliSecondsWait = 10000,
                MaxWaitingCount = 2,
            };

        }

        private static void __SendFailedTxnAlert()
        {
            if (__FailedTxnPath?.Count > 0)
            {
                try
                {
                    var items = __FailedTxnPath.Select(f => new KeyValuePair<String, int>(f, Directory.GetFiles(f).Length))
                        .Where(v => v.Value > 0).ToArray();
                    if (items.Length > 0)
                    {
                        eInvoiceServiceClient invSvc = InvoiceWatcher.CreateInvoiceService();
                        Root token = invSvc.CreateMessageToken(String.Join("\r\n", items.Select(r => r.Key + " => " + r.Value + "筆")));
                        invSvc.AlertFailedTransaction(token.ConvertToXml().Sign());
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }


        protected virtual void prepareStorePath(String fullPath)
        {
            _failedTxnPath = fullPath + "(Failure)";
            _failedTxnPath.CheckStoredPath();

            if (__FailedTxnPath != null)
            {
                __FailedTxnPath.Add(_failedTxnPath);
            }


            _inProgressPath = Path.Combine(fullPath + "(Processing)", $"{Process.GetCurrentProcess().Id}");
            _inProgressPath.CheckStoredPath();

            if (Settings.Default.ResponseUpload)
            {
                _ResponsedPath = fullPath + "(Response)";
                _ResponsedPath.CheckStoredPath();
            }
        }

        public String ResponsePath
        {
            get
            {
                return _ResponsedPath;
            }
            set
            {
                _ResponsedPath = value.GetEfficientString();
                if (_ResponsedPath != null)
                    _ResponsedPath.CheckStoredPath();
            }
        }


        protected InvoiceWatcher _dependentWatcher;
        protected List<InvoiceWatcher> _reponseTo;

        public void InitializeDependency(InvoiceWatcher dependentWatcher)
        {
            _dependentWatcher = dependentWatcher;
            dependentWatcher.addResponse(this);
        }

        private void addResponse(InvoiceWatcher response)
        {
            if (_reponseTo == null)
                _reponseTo = new List<InvoiceWatcher>();
            if (!_reponseTo.Contains(response))
            {
                _reponseTo.Add(response);
            }
        }

        private void doProcess()
        {
            try
            {
                //Thread.Sleep(Settings.Default.WatcherProcessDelayInSeconds * 1000);
                String[] files;
                bool done = false;
                do
                {
                    files = Directory.GetFiles(/*_watcher.Path*/_requestPath);
                    if (files != null && files.Count() > 0)
                    {
                        done = true;

                        processBatchFiles(files);

                        //if (Settings.Default.FileWatcherProcessCount > 1)
                        //{
                        //    int start = 0;
                        //    while (start < files.Length)
                        //    {
                        //        int end = Math.Min(start + Settings.Default.FileWatcherProcessCount, files.Length);
                        //        Parallel.For(start, end, (idx) =>
                        //          {
                        //              Logger.Info($"Upload Invoide[{idx}]:{files[idx]}");
                        //              processFile(files[idx]);
                        //          });
                        //        start = end;
                        //    }
                        //}
                        //else
                        //{
                        //    foreach (String fullPath in files)
                        //    {
                        //        processFile(fullPath);
                        //    }
                        //}
                    }
                } while (files != null && files.Count() > 0);

                if (AppSettings.Default.WatchSubDirectories)
                {
                    bool hasFile;
                    IEnumerable<String> items;
                    do
                    {
                        hasFile = false;
                        items = Directory.EnumerateFiles(/*_watcher.Path*/_requestPath, "*.*", SearchOption.AllDirectories);
                        foreach (String fullPath in items)
                        {
                            hasFile = true;
                            done = true;
                            processFile(fullPath);
                        }
                    } while (hasFile);
                }

                if (done)
                {
                    processComplete();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            if (_reponseTo != null)
            {
                foreach (var response in _reponseTo)
                {
                    response.doProcess();
                }
            }
        }

        protected virtual void processBatchFiles(string[] files)
        {
            if (Settings.Default.FileWatcherProcessCount > 0)
            {
                //IEnumerable<String> workingList;
                //var mode = DateTime.Now.Ticks % Settings.Default.FileWatcherProcessCount + 1;
                //workingList = files
                //    .OrderBy(f => Path.GetFileName(f).Sum(c => (int)c) % mode)
                //    .Take(Settings.Default.FileWatcherProcessCount);

                int start = 0;
                while (start < files.Length)
                {
                    int end = Math.Min(start + Settings.Default.FileWatcherProcessCount, files.Length);
                    Parallel.For(start, end, (idx) =>
                    {
                        Logger.Info($"process file[{idx}]:{files[idx]}");
                        processFile(files[idx]);
                    });
                    start = end;
                }

                //Parallel.ForEach(workingList, fullPath =>
                //{
                //    Logger.Info($"process file:{fullPath}");
                //    processFile(fullPath);
                //});

            }
            else
            {
                foreach (String fullPath in files)
                {
                    processFile(fullPath);
                }
            }
        }

        public void InvokeProcess()
        {
            _handler.Notify();
        }

        protected virtual void processComplete()
        {
            eInvoiceServiceClient invSvc = InvoiceWatcher.CreateInvoiceService();

            try
            {
                Root token = this.CreateMessageToken("資料已傳送完成－通知相對營業人");
                invSvc.NotifyCounterpartBusiness(token.ConvertToXml().Sign());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

        }

        protected virtual void processFile(String invFile)
        {
            if (!File.Exists(invFile))
                return;

            String fileName = Path.GetFileName(invFile);
            String fullPath = Path.Combine(_inProgressPath, fileName);
            try
            {
                File.Move(invFile, fullPath);
            }
            catch (Exception ex)
            {
                Logger.Error($"while processing move {invFile} => {fullPath}\r\n{ex}");
                return;
            }

            eInvoiceServiceClient invSvc = CreateInvoiceService();
            Root result = new Root();
            try
            {
                XmlDocument docInv = prepareInvoiceDocument(fullPath);

                docInv.Sign();
                result = processUpload(invSvc, docInv);

                if (result.Result.value != 1)
                {
                    if (result.Response != null && result.Response.InvoiceNo != null && result.Response.InvoiceNo.Length > 0)
                    {
                        if (processError(result.Response.InvoiceNo, docInv, fileName))
                        {
                            storeFile(fullPath, Path.Combine(Logger.LogDailyPath, fileName));
                        }
                        else
                        {
                            storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
                        }
                    }
                    else
                    {
                        processError(result.Result.message, docInv, fileName);
                        storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
                    }
                }
                else
                {
                    storeFile(fullPath, Path.Combine(Logger.LogDailyPath, fileName));
                }
            }
            catch (System.Net.WebException ex)
            {
                if (_retryConnection)
                {
                    storeFile(fullPath, Path.Combine(_requestPath, fileName));
                }
                else
                {
                    Logger.Error(ex);
                    storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
            }
            finally
            {
                if (Settings.Default.ResponseUpload && result.Automation != null)
                {
                    Automation auto = new Automation { Item = result.Automation };
                    auto.ConvertToXml()
                        .SaveDocumentWithEncoding(
                            Path.Combine(_ResponsedPath, 
                                fileName.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase) 
                                    ? fileName : $"{fileName}.xml"));
                }
            }

        }

        protected virtual XmlDocument prepareInvoiceDocument(String invoiceFile)
        {
            XmlDocument docInv = new XmlDocument();
            int retryCount = 0;

            void loadInvoiceDocument()
            {
                try
                {
                    docInv.Load(invoiceFile);
                    retryCount = 100;
                }
                catch (Exception ex)
                {
                    if (retryCount < 10)
                    {
                        Logger.Warn($"fail to load xml: {invoiceFile}");
                        Logger.Warn(ex);
                        retryCount++;
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }

            do
            {
                loadInvoiceDocument();
            }
            while (retryCount < 10);

            ///去除"N/A"資料
            ///
            var nodes = docInv.SelectNodes("//*[text()='N/A']");
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes.Item(i);
                node.RemoveChild(node.SelectSingleNode("text()"));
            }
            ///
            return docInv;
        }

        protected virtual void processError(string message, XmlDocument docInv, string fileName)
        {
            Logger.Warn(String.Format("在上傳發票檔({0})時,傳送失敗!!原因如下:\r\n{1}", fileName, message));
            if (docInv?.DocumentElement?["SourceLog"] != null && docInv?.DocumentElement?["ToFail"] != null)
            {
                ToFail(docInv.DocumentElement["SourceLog"].InnerText, docInv.DocumentElement["ToFail"].InnerText);
            }
        }

        private void ToFail(string logItem, string failedPath)
        {
            var items = logItem.Split(';')
                .Select(i => i.GetEfficientString())
                .Where(i => i != null).ToArray();

            List<String> failedItems = new List<string>();
            foreach (var item in items)
            {
                String failedItem = Path.Combine(failedPath, Path.GetFileName(item));
                if (File.Exists(item))
                {
                    storeFile(item, failedItem);
                }
                failedItems.Add(failedItem);
            }

            MainForm.Alert($"檔案：\r\n{String.Join("\r\n", failedItems)}", "資料處理發生錯誤", transferManager: this.TransferManager);
        }

        public static eInvoiceServiceClient CreateInvoiceService()
        {
            //var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            //binding.OpenTimeout = binding.SendTimeout = binding.CloseTimeout = TimeSpan.FromMilliseconds(Settings.Default.WS_TimeoutInMilliSeconds);
            //binding.AllowCookies = true;
            eInvoiceServiceClient invSvc = new eInvoiceServiceClient();
            //eInvoiceServiceClientClient invSvc = new eInvoiceServiceClientClient();
            return invSvc;
        }

        protected virtual Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            var result = invSvc.UploadInvoice(docInv).ConvertTo<Root>();
            return result;
        }

        protected void storeFile(String srcName, String destName)
        {
            try
            {
                if (File.Exists(destName))
                {
                    File.Delete(destName);
                }
                File.Move(srcName, destName);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        protected virtual bool processError(IEnumerable<RootResponseInvoiceNo> rootInvoiceNo, XmlDocument docInv, string fileName)
        {
            if (rootInvoiceNo != null && rootInvoiceNo.Count() > 0)
            {
                IEnumerable<String> message = rootInvoiceNo.Select(i => String.Format("發票號碼:{0}=>{1}", i.Value, i.Description));
                Logger.Warn(String.Format("在上傳發票檔({0})時,傳送失敗!!原因如下:\r\n{1}", fileName, String.Join("\r\n", message.ToArray())));

                InvoiceRoot invoice = docInv.TrimAll().ConvertTo<InvoiceRoot>();
                InvoiceRoot stored = docInv.TrimAll().ConvertTo<InvoiceRoot>();
                stored.Invoice = rootInvoiceNo.Where(i => i.ItemIndexSpecified).Select(i => invoice.Invoice[i.ItemIndex]).ToArray();

                stored.ConvertToXml().SaveDocumentWithEncoding(Path.Combine(_failedTxnPath, Path.GetFileName(fileName)));
            }

            return true;
        }

        public String ContentName { get; set; } = "發票資料";
        public virtual String ReportError()
        {
            int count = Directory.GetFiles(_failedTxnPath).Length;
            return count > 0 ? String.Format("{0}筆{1}作業失敗!!\r\n", count, ContentName) : null;
        }

        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion

        public void Retry()
        {
            foreach (String fileName in Directory.GetFiles(_failedTxnPath))
            {
                File.Move(fileName, Path.Combine(/*_watcher.Path*/_requestPath, Path.GetFileName(fileName)));
            }
        }

        public void StartUp()
        {
            //foreach (String filePath in Directory.GetFiles(/*_watcher.Path*/_requestPath))
            //{
            //    processFile(filePath);
            //}
            InvokeProcess();
        }
    }
}
