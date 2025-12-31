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
    public class G0501Watcher : F0401Watcher
    {
        public G0501Watcher(String fullPath)
            : base(fullPath)
        {
            PreferredProcessType = Naming.InvoiceProcessType.G0501;
        }

        protected override XmlDocument prepareInvoiceDocument(string invoiceFile)
        {
            XmlDocument docInv = new XmlDocument();
            docInv.Load(invoiceFile);

            docInv.DocumentElement!.SetAttribute("xmlns", ModelCore.Properties.AppSettings.Default.MIG.G0501);
            docInv.LoadXml(docInv.OuterXml);

            var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.G0501Outbound, Path.GetFileName(invoiceFile));
            docInv.Save(fileName);

            return docInv!;
        }
    }
}
