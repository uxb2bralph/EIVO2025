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
    public class ReprintAllowanceWatcher : AllowanceProcessWatcher
    {
        public ReprintAllowanceWatcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override Dictionary<int, Exception> DoClientSideProcess(AllowanceRoot item, List<AllowanceRootAllowance> eventItems)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();

            if (item != null && item.Allowance != null && item.Allowance.Length > 0)
            {
                for (int idx = 0; idx < item.Allowance.Length; idx++)
                {
                    try
                    {
                        var allowance = item.Allowance[idx];
                        allowance.AllowanceNumber = allowance.AllowanceNumber.GetEfficientString();
                        if (allowance.AllowanceNumber != null)
                        {
                            var printItem = Directory.EnumerateFiles(Logger.LogPath, $"{allowance.AllowanceNumber.EscapeFileNameCharacter('_')}.htm", SearchOption.AllDirectories)
                                                .FirstOrDefault();
                            if (printItem != null)
                            {
                                File.Move(printItem, Path.Combine(POSReady.Settings.PrintInvoice, Path.GetFileName(printItem)));
                                eventItems.Add(allowance);
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
