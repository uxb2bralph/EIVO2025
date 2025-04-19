using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using InvoiceClient.Properties;
using System.Xml;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using InvoiceClient.Helper;
using ModelCore.Schema.EIVO;
using System.Threading;
using ModelCore.Schema.TXN;
using System.Diagnostics;
using System.Globalization;
using ModelCore.Locale;
using System.IO.Compression;
using System.Threading.Tasks;

namespace InvoiceClient.Agent
{
    public class FolderBuckleWatcher : InvoiceWatcher
    {
        private int _waitCycle;

        public FolderBuckleWatcher(String fullPath) : base(fullPath)
        {

        }

        protected override void prepareStorePath(string fullPath)
        {

        }

        public String BucklePrefix { get; set; }

        protected override void processBatchFiles(string[] files)
        {
            if (files.Length < Settings.Default.MaxFileCountInPDFZip)
            {
                if (_waitCycle < Settings.Default.MaxWaitingTurns)
                {
                    _waitCycle++;
                    Console.WriteLine($"fewer count:{files.Length}");
                    Task.Delay(Settings.Default.WaitForInvoicePDFInSeconds * 1000).Wait();
                    return;
                }

            }

            String tmpPath = $"{_requestPath}{DateTime.Now.Ticks}";
            tmpPath.CheckStoredPath();

            _waitCycle = 0;
            int count = 0;
            for (int i = 0; count < Settings.Default.MaxFileCountInPDFZip && i < files.Length; i++)
            {
                try
                {
                    File.Move(files[i], Path.Combine(tmpPath, Path.GetFileName(files[i])));
                    count++;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            if(count > 0)
            {
                String args = $"{BucklePrefix}{DateTime.Now:yyyyMMddHHmmssffff}_{count:0000} \"{tmpPath}\" \"{_ResponsedPath}\"";
                ZipPDFFactory.Notify(args);
            }

        }

        protected override void processComplete()
        {

        }

    }
}
