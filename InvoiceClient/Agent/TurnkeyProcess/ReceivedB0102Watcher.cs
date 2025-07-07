using CommonLib.Core.Utility;
using CommonLib.Utility;
using InvoiceClient.Helper;
using InvoiceClient.Properties;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.InvoiceManagement.InvoiceProcess;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TXN;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;

namespace InvoiceClient.Agent.TurnkeyProcess
{
    public class ReceivedB0102Watcher : InvoiceWatcher
    {
        public ReceivedB0102Watcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override void processFile(String invFile)
        {
            if (!File.Exists(invFile))
                return;

            String fileName = Path.GetFileName(invFile);
            String fullPath = Path.Combine(_inProgressPath, fileName);
            String backupPath = Path.Combine(Logger.LogDailyPath, fileName);

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
                XmlDocument doc = new XmlDocument();
                doc.Load(fullPath);

                using (B2BInvoiceManager models = new B2BInvoiceManager { ProcessType = Naming.InvoiceProcessType.B0101 })
                {
                    models.ReceiveB0102(doc);
                    storeFile(fullPath, backupPath);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
            }

        }

        protected override void processComplete()
        {

        }
    }
}
