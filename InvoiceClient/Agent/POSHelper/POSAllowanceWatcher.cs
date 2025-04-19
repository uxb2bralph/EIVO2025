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
using ModelCore.Schema.TXN;
using CommonLib.Core.Utility;
using CommonLib.Utility;

namespace InvoiceClient.Agent.POSHelper
{
    public class POSAllowanceWatcher : AllowanceWatcher
    {
        public POSAllowanceWatcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override XmlDocument prepareInvoiceDocument(string invoiceFile)
        {
            invoiceFile.ReviseXmlContent();
            var docInv = base.prepareInvoiceDocument(invoiceFile);

            String url = $"{POSReady._Settings.ServiceHost}{POSReady._Settings.VerifyAllowance}";
            using (WebClientEx client = new WebClientEx())
            {
                client.Timeout = 43200000;
                client.Headers[HttpRequestHeader.ContentType] = "application/xml";
                client.Encoding = Encoding.UTF8;

                using (Stream stream = client.OpenWrite(url))
                {
                    docInv.Save(stream);
                    docInv.Load(client.Response.GetResponseStream());
                }
                //String result = client.UploadString(url,
                //    docInv.OuterXml);

                //docInv.LoadXml(result);
            }

            return docInv;
        }

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            var result = invSvc.UploadAllowanceV2(docInv).ConvertTo<Root>();
            if (result.Result.value == 1)
            {
                docInv.TrimAll().ConvertTo<AllowanceRoot>()
                    .ConvertToXml().SaveDocumentWithEncoding(Path.Combine(POSReady._Settings.PreparedAllowance, $"{Guid.NewGuid()}.xml"));
            }
            return result;
        }

        protected override bool processError(IEnumerable<RootResponseInvoiceNo> rootInvoiceNo, XmlDocument docInv, string fileName)
        {
            if (rootInvoiceNo != null && rootInvoiceNo.Count() > 0)
            {
                IEnumerable<String> message = rootInvoiceNo.Select(i => String.Format("Allowance No:{0}=>{1}", i.Value, i.Description));
                Logger.Warn(String.Format("Transferring fault while uploading allowance file:[{0}], cause:\r\n{1}", fileName, String.Join("\r\n", message.ToArray())));

                AllowanceRoot invoice = docInv.TrimAll().ConvertTo<AllowanceRoot>();
                AllowanceRoot stored = new AllowanceRoot();

                var failedItems = rootInvoiceNo.Where(i => i.ItemIndexSpecified).Select(i => invoice.Allowance[i.ItemIndex]);
                stored.Allowance = failedItems.ToArray();

                stored.ConvertToXml().SaveDocumentWithEncoding(Path.Combine(_failedTxnPath, String.Format("{0}-{1:yyyyMMddHHmmssfff}.xml", Path.GetFileNameWithoutExtension(fileName), DateTime.Now)));

                if (invoice.Allowance.Length > failedItems.Count())
                {
                    stored = new AllowanceRoot
                    {
                        Allowance = invoice.Allowance.Except(failedItems).ToArray()
                    };
                    stored.ConvertToXml().SaveDocumentWithEncoding(Path.Combine(POSReady._Settings.PreparedAllowance, $"{Guid.NewGuid()}.xml"));
                }
            }

            return true;
        }
    }
}
