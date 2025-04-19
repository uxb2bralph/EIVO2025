using CommonLib.Core.Utility;
using CommonLib.Utility;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using Uxnet.Com.Properties;

namespace Uxnet.Com.Helper.DefaultTools
{
    public class Program
    {
        private WebView2 _webView;

        protected FileSystemWatcher _watcher;
        protected bool _running = true;
        protected Queue<String> _queue;
        private String _InProgress;
        private AutoResetEvent _queueIsReady;

        [STAThread()]
        public static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                Program prog = new Program();
                Logger.Info(args[0]);
                //prog.startUp(args[0]);
                prog.PrintUrl(args[0]);
            }
            else
            {
                (new Program()).StartUp();
            }

            //Application.Run();
        }

        public Program()
        {
            using(PrintDocument doc = new PrintDocument()) 
            {
                var printerName = doc.DefaultPageSettings.PrinterSettings.PrinterName;
                if (String.IsNullOrEmpty(printerName))
                {
                    var result = MessageBox.Show("未設定預設印表機，系統無法進行列印！！", "系統錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    Logger.Info(String.Format("使用預設印表機 => {0}", printerName));
                }
            }

            //if (Win32.Winspool.SetDefaultPrinter(Settings.Default.PdfPrinterName))
            //{
            //    Logger.Warn(String.Format("使用預設印表機 => {0}", Settings.Default.PdfPrinterName));
            //}
            //else
            //{
            //    String msg = String.Format("預設印表機設定失敗 => {0}", Settings.Default.PdfPrinterName);
            //    Logger.Warn(msg);
            //    var result = MessageBox.Show(msg, "系統錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    Application.Exit();
            //}

            _webView = new WebView2();
            _webView.NavigationCompleted += Wb_NavigationCompleted;
            _webView.NavigationStarting += Wb_NavigationStarting;
        }

        public int PrintTimeoutInSeconds { get; set; } = 30;


        public void StartUp()
        {
            Initialize();
            ProcessPrintRequest();

            while (_running)
            {
                try
                {
                    if(!_queue.Any())
                    {
                        _queueIsReady.WaitOne();
                    }

                    Logger.Info(String.Format("[{0}] 佇列中的工作件數:[{1}]", DateTime.Now, _queue.Count));
                    while (_queue.Any())
                    {
                        ProcessUrl(ReadIntentUrl(_queue.Dequeue()));
                        Logger.Info(String.Format("[{0}] 佇列中的工作件數:[{1}]", DateTime.Now, _queue.Count));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        private void Initialize()
        {
            Settings.Default.PDFWorkingQueue.CheckStoredPath();
            _InProgress = Path.Combine(Logger.LogPath,"InProgress").CheckStoredPath();
            _watcher = new FileSystemWatcher(Settings.Default.PDFWorkingQueue);
            Logger.Info($"工作佇列資料夾:{Settings.Default.PDFWorkingQueue}");
            _watcher.Created += new FileSystemEventHandler(_watcher_Created);
            _watcher.Filter = "*.txt";
            _watcher.EnableRaisingEvents = true;

            _queue = new Queue<string>();
            _queueIsReady = new AutoResetEvent(false);

            ThreadPool.QueueUserWorkItem(p =>
            {
                while (_running)
                {
                    var result = _watcher.WaitForChanged(WatcherChangeTypes.Created, 300000);
                    Thread.Sleep(100);
                    ProcessPrintRequest();
                }
            });


            Logger.Info("PDF列印服務啟動...");
        }

        private String ReadIntentUrl(String pathFile)
        {
            String url = File.ReadAllText(pathFile);
            File.Delete(pathFile);
            return url;
        }

        private void ProcessUrl(String url)
        {
            String outputFolder = Path.GetDirectoryName(url);

            Logger.Info(String.Format("[{0}] 處理檔案 => {1}", DateTime.Now, url));
            PrintUrl(url);
            Logger.Info(String.Format("[{0}] 處理完畢 => {1}", DateTime.Now, url));

            //if (File.Exists(Settings.Default.PdfOutput))
            //{
            //    String fileName = Path.GetFileNameWithoutExtension(fullPath);
            //    String outputPath = Path.Combine(outputFolder, String.Format("{0}.pdf", fileName));
            //    String pdfDest = Path.Combine(outputFolder, Path.GetFileName(Settings.Default.PdfOutput));
            //    try
            //    {
            //        if (File.Exists(outputPath))
            //        {
            //            File.Delete(outputPath);
            //        }

            //        Logger.Info(String.Format("[{0}] 搬移檔案 => {1} => {2}", DateTime.Now, Settings.Default.PdfOutput, outputPath));
            //        File.Move(Settings.Default.PdfOutput, pdfDest);
            //        File.Move(pdfDest, outputPath);
            //        Logger.Info(String.Format("[{0}] 搬移完畢 => {1}", DateTime.Now, Settings.Default.PdfOutput, outputPath));
            //    }
            //    catch (Exception ex)
            //    {
            //        Logger.Error(ex);
            //    }
            //}
        }

        private void ProcessPrintRequest()
        {
            IEnumerable<String> items;
            do
            {
                items = Directory.EnumerateFiles(Settings.Default.PDFWorkingQueue, "*.txt");
                foreach (var item in items)
                {
                    Logger.Info(String.Format("[{0}] 發現檔案 => {1}", DateTime.Now, item));
                    String destPath = Path.Combine(_InProgress,Path.GetFileName(item));
                    if (!File.Exists(destPath))
                    {
                        File.Move(item, destPath);
                        _queue.Enqueue(destPath);
                        Logger.Info(String.Format("[{0}] 放入新工作,佇列中的工作件數:[{1}]", DateTime.Now, _queue.Count));
                    }
                }
            } while(items.Any());

            if(_queue.Any())
            {
                _queueIsReady.Set();
            }
        }

        private void _watcher_Created(object sender, FileSystemEventArgs e)
        {

        }


        bool _printing = true;
        bool _readyToPrint = false;
        public bool PrintUrl(String url)
        {
            var target = new Uri(url);
            if(_webView.Source?.AbsoluteUri == target.AbsoluteUri)
            {
                _webView.Reload();
            }
            else
            {
                _webView.Source = target;
            }
            _printing = true;
            _readyToPrint = false;
            DateTime start = DateTime.Now;
            while (_printing)
            {
                System.Windows.Forms.Application.DoEvents();
                if (_readyToPrint)
                {
                    _readyToPrint = false;
                    PrintAsync();
                }

                if((DateTime.Now-start).TotalSeconds > PrintTimeoutInSeconds) 
                {
                    return false;
                }
            }

            return true;
        }

        private void Wb_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            WebView2 wb = (WebView2)sender;
            Logger.Info(String.Format("Navigating:{0}", e.Uri));
        }

        CoreWebView2 _coreWeb;
        private async void PrintAsync()
        {
            CoreWebView2PrintSettings printSettings = null;
            printSettings = _coreWeb.Environment.CreatePrintSettings();
            printSettings.ShouldPrintBackgrounds = true;
            printSettings.ShouldPrintHeaderAndFooter = false;
            CoreWebView2PrintStatus status  = await _coreWeb.PrintAsync(printSettings);
            Logger.Info($"document printed...{status}");
            _printing = false;
        }

        private void Wb_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            WebView2 wb = (WebView2)sender;
            _coreWeb = wb.CoreWebView2;
            _readyToPrint = true;
        }

    }
}
