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
    public class PreparedInvoiceWatcher : InvoiceProcessWatcher
    {
        public PreparedInvoiceWatcher(String fullPath)
            : base(fullPath)
        {

        }

        public String ConvertPrintFormUrl { get; set; } = POSReady.Settings.PrintC0401;

        protected override bool processError(IEnumerable<RootResponseInvoiceNo> rootInvoiceNo, XmlDocument docInv, string fileName)
        {
            if (rootInvoiceNo != null && rootInvoiceNo.Count() > 0)
            {
                InvoiceRoot invoice = docInv.TrimAll().ConvertTo<InvoiceRoot>();
                InvoiceRoot stored = docInv.TrimAll().ConvertTo<InvoiceRoot>();
                stored.Invoice = rootInvoiceNo.Where(i => i.ItemIndexSpecified).Select(i => invoice.Invoice[i.ItemIndex]).ToArray();

                stored.ConvertToXml().SaveDocumentWithEncoding(Path.Combine(_requestPath, $"{Guid.NewGuid()}.xml"));
            }
            return true;
        }

        protected override Dictionary<int, Exception> DoClientSideProcess(InvoiceRoot item, List<InvoiceRootInvoice> eventItems)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();

            if (item != null && item.Invoice != null && item.Invoice.Length > 0)
            {
                using (WebClientEx client = new WebClientEx { Timeout = 43200000 })
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Encoding = Encoding.UTF8;
                    for (int idx = 0; idx < item.Invoice.Length; idx++)
                    {
                        try
                        {
                            var invItem = item.Invoice[idx];
                            String tmpHtml = Path.Combine(POSReady._Settings.PrintInvoice, $"{invItem.DataNumber ?? invItem.InvoiceNumber}.htm");
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
