using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

using InvoiceClient.Helper;
using InvoiceClient.Properties;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TXN;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using ModelCore.Schema.TurnKey.Invoice;

namespace InvoiceClient.Agent.MIGHelper
{
    public class F0501Watcher : F0401Watcher
    {
        public F0501Watcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override XmlDocument prepareInvoiceDocument(string invoiceFile)
        {
            XmlDocument docInv = new XmlDocument();
            docInv.Load(invoiceFile);

            CancelInvoiceRoot root = new CancelInvoiceRoot { };

            if (docInv.DocumentElement != null)
            {
                if (docInv.DocumentElement.Attributes["xmlns"] != null)
                {
                    docInv.DocumentElement.SetAttribute("xmlns", "");
                    docInv.LoadXml(docInv.OuterXml);
                }

                var migItem = docInv.ConvertTo<CancelInvoice>();
                root.CancelInvoice =
                [
                    migItem.FromMIG()
                ];
            }

            return root.ConvertToXml();
        }

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            var result = invSvc.UploadInvoiceCancellationV2(docInv).ConvertTo<Root>();
            return result;
        }

    }
}
