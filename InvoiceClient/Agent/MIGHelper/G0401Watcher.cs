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
using ModelCore.Schema.TurnKey.Allowance;

namespace InvoiceClient.Agent.MIGHelper
{
    public class G0401Watcher : F0401Watcher
    {
        public G0401Watcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override XmlDocument prepareInvoiceDocument(string invoiceFile)
        {
            XmlDocument docInv = new XmlDocument();
            docInv.Load(invoiceFile);

            AllowanceRoot root = new AllowanceRoot { };

            if (docInv.DocumentElement != null)
            {
                if (docInv.DocumentElement.Attributes["xmlns"] != null)
                {
                    docInv.DocumentElement.SetAttribute("xmlns", "");
                    docInv.LoadXml(docInv.OuterXml);
                }

                var migItem = docInv.ConvertTo<Allowance>();
                root.Allowance =
                [
                    migItem.FromMIG()
                ];
            }

            return root.ConvertToXml();
        }

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            var result = invSvc.UploadAllowanceByClient(docInv, Settings.Default.ClientID, (int)_channelID).ConvertTo<Root>();
            return result;
        }

    }
}
