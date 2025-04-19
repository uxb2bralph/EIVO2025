using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

using InvoiceClient.Helper;
using InvoiceClient.Properties;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TXN;
using Newtonsoft.Json.Linq;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using Win32;

namespace InvoiceClient.Agent.TurnkeyProcess
{
    public class SummaryResultWatcher : InvoiceWatcher
    {
        public SummaryResultWatcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override void processFile(String invFile)
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

            try
            {
                Console.WriteLine($"SummaryResult:{fullPath}");
                SummaryResult result = null;
                //using (FileStream stream = File.OpenRead(fullPath))
                //{
                //    result = stream.ConvertTo<SummaryResult>();
                //}
                XmlDocument doc = new XmlDocument();
                doc.Load(fullPath);
                doc.DocumentElement.SetAttribute("xmlns", "");
                doc.LoadXml(doc.OuterXml);
                result = doc.ConvertTo<SummaryResult>();

                if (result?.DetailList?.Any() == true)
                {
                    foreach (var item in result.DetailList)
                    {
                        ProcessSummary(item);
                    }
                }

                storeFile(fullPath, Path.Combine(Logger.LogDailyPath, fileName));

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
            }

        }

        private void ProcessSummary(SummaryResultMessage item)
        {
            if (TurnkeyProcessResultSettings.Default.ResultMessageType.Contains(item.Info.MessageType))
            {
                if (item.ResultType?.Good?.ResultDetailType?.Invoices?.Any() == true)
                {
                    String storedPath = Path.Combine(TurnkeyProcessResultSettings.Default.MessageResponseGood[item.Info.MessageType], $"{Guid.NewGuid()}.json");
                    File.WriteAllText(storedPath, item.ResultType.Good.ResultDetailType.Invoices.Select(x => x.ReferenceNumber).JsonStringify());
                }

                if (item.ResultType?.Failed?.ResultDetailType?.Invoices?.Any() == true)
                {
                    String storedPath = Path.Combine(TurnkeyProcessResultSettings.Default.MessageResponseFailed[item.Info.MessageType], $"{Guid.NewGuid()}.json");
                    File.WriteAllText(storedPath, item.ResultType.Failed.ResultDetailType.Invoices.Select(x => x.ReferenceNumber).JsonStringify());
                }
            }
        }
    }
}
