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

namespace InvoiceClient.Agent.MIGReviser
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

            docInv.DocumentElement!.SetAttribute("xmlns", ModelCore.Properties.AppSettings.Default.MIG.F0401);
            docInv.LoadXml(docInv.OuterXml);

            var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.F0401Outbound, Path.GetFileName(invoiceFile));
            docInv.Save(fileName);

            return docInv!;
        }

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            Root result = new Root
            {
                UXB2B = "電子發票系統",
                Result = new RootResult
                {
                    timeStamp = DateTime.Now,
                    value = 1
                }
            };
            return result;
        }
    }
}
