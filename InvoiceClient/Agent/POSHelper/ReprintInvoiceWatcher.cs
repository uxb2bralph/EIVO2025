using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;

using InvoiceClient.Helper;
using InvoiceClient.Properties;

using ModelCore.Schema.EIVO;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TXN;
using Newtonsoft.Json;
using CommonLib.Core.Utility;
using CommonLib.Utility;

namespace InvoiceClient.Agent.POSHelper
{
    public class ReprintInvoiceWatcher : InvoiceProcessWatcher
    {
        public ReprintInvoiceWatcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override Dictionary<int, Exception> DoClientSideProcess(InvoiceRoot item, List<InvoiceRootInvoice> eventItems)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();

            if (item != null && item.Invoice != null && item.Invoice.Length > 0)
            {
                for (int idx = 0; idx < item.Invoice.Length; idx++)
                {
                    try
                    {
                        var invItem = item.Invoice[idx];
                        invItem.DataNumber = invItem.DataNumber.GetEfficientString();
                        if (invItem.DataNumber != null)
                        {
                            var printItem = Directory.EnumerateFiles(Logger.LogPath, $"{invItem.DataNumber}.htm", SearchOption.AllDirectories)
                                                .FirstOrDefault();
                            if (printItem != null)
                            {
                                String content = File.ReadAllText(printItem);
                                File.WriteAllText(printItem, content.Replace("<h2>電子發票證明聯</h2>", "<h2 style=\"font-size: x-large;\">電子發票證明聯補印</h2>"));
                                File.Move(printItem, Path.Combine(POSReady.Settings.PrintInvoice, Path.GetFileName(printItem)));
                                eventItems.Add(invItem);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        result.Add(idx, ex);
                    }
                }
            }

            return result;
        }

    }
}
