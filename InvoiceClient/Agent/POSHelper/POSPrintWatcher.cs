using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using InvoiceClient.Helper;
using InvoiceClient.Properties;

using ModelCore.Resource;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TXN;
using CommonLib.Core.Utility;
using CommonLib.Utility;

namespace InvoiceClient.Agent.POSHelper
{
    public class POSPrintWatcher : InvoiceWatcher
    {
        private Uxnet.Com.Helper.DefaultTools.Program _printAgent;
        public POSPrintWatcher(String fullPath)
            : base(fullPath)
        {
            _printAgent = new Uxnet.Com.Helper.DefaultTools.Program();
        }

        delegate void PrintContent();
        PrintContent doPrint;

        protected override void processFile(String invFile)
        {
            if (!File.Exists(invFile))
                return;

            if (String.IsNullOrEmpty(POSReady.Settings.DefaultPOSPrinter)
                || !Win32.Winspool.SetDefaultPrinter(POSReady.Settings.DefaultPOSPrinter))
            {
                return;
            }

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

            try
            {
                doPrint = () =>
                {
                    if (_printAgent.PrintUrl(fullPath))
                    {
                        storeFile(fullPath, Path.Combine(Logger.LogDailyPath, fileName));
                    }
                    else
                    {
                        storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
                    }
                };
                //Application.OpenForms["MainForm"]?.Invoke(doPrint);
                MainForm.AppMainForm.Invoke(doPrint);

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
            }
        }

    }
}
