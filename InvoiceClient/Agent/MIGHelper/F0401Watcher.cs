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
using ModelCore.Schema.TXN;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using ModelCore.Schema.TurnKey.Invoice;

namespace InvoiceClient.Agent.MIGHelper
{
    public class F0401Watcher : InvoiceWatcher
    {
        public F0401Watcher(String fullPath)
            : base(fullPath)
        {
            PreferredProcessType = Naming.InvoiceProcessType.F0401;
        }

        protected override XmlDocument prepareInvoiceDocument(string invoiceFile)
        {
            XmlDocument docInv = new XmlDocument();
            docInv.Load(invoiceFile);

            InvoiceRoot root = new InvoiceRoot
            {
                NotificationSpecified = true,
                Notification = invoiceFile.Contains("_A")
                                ? (short)Naming.NotificationIndication.Deferred
                                : invoiceFile.Contains("_C")
                                    ? (short)Naming.NotificationIndication.None
                                    : (short)Naming.NotificationIndication.Immediate,
                ProcessType = $"{PreferredProcessType}",
            };

            if (docInv.DocumentElement != null)
            {
                if (docInv.DocumentElement.Attributes["xmlns"] != null)
                {
                    docInv.DocumentElement.SetAttribute("xmlns", "");
                    docInv.LoadXml(docInv.OuterXml);
                }

                var migItem = docInv.ConvertTo<Invoice>();
                root.Invoice =
                [
                    migItem.FromMIG()
                ];

            }

            return root.ConvertToXml();
        }

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            var result = invSvc.UploadInvoiceByClient(docInv, Settings.Default.ClientID, (int)_channelID, false, (int)PreferredProcessType!).ConvertTo<Root>();
            return result;
        }

        protected override bool processError(IEnumerable<RootResponseInvoiceNo> rootInvoiceNo, XmlDocument docInv, string fileName)
        {
            if (rootInvoiceNo != null && rootInvoiceNo.Count() > 0)
            {
                IEnumerable<String> message = rootInvoiceNo.Select(i => String.Format("Invoice No:{0}=>{1}", i.Value, i.Description));
                Logger.Warn(String.Format("Transferring fault while uploading request file:[{0}], cause:\r\n{1}", fileName, String.Join("\r\n", message.ToArray())));
            }
            return false;
        }


    }
}
