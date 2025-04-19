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
    public class PreparedAllowanceWatcher : InvoiceWatcher
    {
        public PreparedAllowanceWatcher(String fullPath)
            : base(fullPath)
        {

        }

        public String ConvertPrintFormUrl { get; set; } = POSReady.Settings.PrintD0401;

        protected override bool processError(IEnumerable<RootResponseInvoiceNo> rootInvoiceNo, XmlDocument docInv, string fileName)
        {
            if (rootInvoiceNo != null && rootInvoiceNo.Count() > 0)
            {
                AllowanceRoot invoice = docInv.TrimAll().ConvertTo<AllowanceRoot>();
                AllowanceRoot stored = docInv.TrimAll().ConvertTo<AllowanceRoot>();
                stored.Allowance = rootInvoiceNo.Where(i => i.ItemIndexSpecified).Select(i => invoice.Allowance[i.ItemIndex]).ToArray();

                stored.ConvertToXml().SaveDocumentWithEncoding(Path.Combine(_requestPath, $"{Guid.NewGuid()}.xml"));
            }
            return true;
        }

        private Root processUploadCore(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            DateTime ts = DateTime.Now;
            Console.WriteLine($"start converting xml to object at {ts}");
            AllowanceRoot invoice = docInv.TrimAll().ConvertTo<AllowanceRoot>();

            Root result = new Root
            {
                UXB2B = "電子發票系統",
                Result = new RootResult
                {
                    timeStamp = DateTime.Now,
                    value = 0
                }
            };

            List<AllowanceRootAllowance> eventItems = new List<AllowanceRootAllowance>();
            var items = createPrintFormat(invoice, eventItems);
            if (items.Count > 0)
            {
                result.Response = new RootResponse
                {
                    InvoiceNo =
                    items.Select(d => new RootResponseInvoiceNo
                    {
                        Value = invoice.Allowance[d.Key].AllowanceNumber,
                        Description = d.Value.Message,
                        ItemIndexSpecified = true,
                        ItemIndex = d.Key,
                    }).ToArray()
                };
            }
            else
            {
                result.Result.value = 1;
            }
            Console.WriteLine($"total seconds: {(DateTime.Now - ts).TotalSeconds}");

            return result;
        }


        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            return processUploadCore(invSvc, docInv);
        }

        private Dictionary<int, Exception> createPrintFormat(AllowanceRoot item, List<AllowanceRootAllowance> eventItems)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();

            if (item != null && item.Allowance != null && item.Allowance.Length > 0)
            {
                using (WebClientEx client = new WebClientEx { Timeout = 43200000 })
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Encoding = Encoding.UTF8;
                    for (int idx = 0; idx < item.Allowance.Length; idx++)
                    {
                        try
                        {
                            var invItem = item.Allowance[idx];
                            String tmpHtml = Path.Combine(POSReady._Settings.PrintInvoice, $"{invItem.AllowanceNumber.EscapeFileNameCharacter('_')}.htm");
                            File.WriteAllText(tmpHtml, client.UploadString(ConvertPrintFormUrl, JsonConvert.SerializeObject(invItem)));
                            eventItems.Add(invItem);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            result.Add(idx, ex);
                        }
                    }
                }
            }

            return result;
        }


    }
}
