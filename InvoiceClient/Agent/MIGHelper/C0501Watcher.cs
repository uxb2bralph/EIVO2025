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

namespace InvoiceClient.Agent.MIGHelper
{
    public class C0501Watcher : C0401Watcher
    {
        public C0501Watcher(String fullPath)
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
                docInv.DocumentElement.SetAttribute("xmlns", "urn:GEINV:eInvoiceMessage:C0501:3.2");
                docInv.LoadXml(docInv.OuterXml);

                var C0501 = docInv.ConvertTo<ModelCore.Schema.TurnKey.C0501.CancelInvoice>();
                root.CancelInvoice = new CancelInvoiceRootCancelInvoice[]
                {
                        new CancelInvoiceRootCancelInvoice
                        {
                            SellerId = C0501.SellerId,
                            BuyerId = C0501.BuyerId,
                            InvoiceDate = C0501.InvoiceDate!=null && C0501.InvoiceDate.Length==8 
                                ? $"{C0501.InvoiceDate.Substring(0,4)}/{C0501.InvoiceDate.Substring(4,2)}/{C0501.InvoiceDate.Substring(6)}" 
                                : null,
                            CancelDate = C0501.CancelDate!=null && C0501.CancelDate.Length==8
                                ? $"{C0501.CancelDate.Substring(0,4)}/{C0501.CancelDate.Substring(4,2)}/{C0501.CancelDate.Substring(6)}"
                                : null,
                            CancelTime = C0501.CancelTime.ToString("HH:mm:ss"),
                            CancelInvoiceNumber = C0501.CancelInvoiceNumber,
                            CancelReason = C0501.CancelReason,
                            Remark = C0501.Remark,
                            ReturnTaxDocumentNumber = C0501.ReturnTaxDocumentNumber,
                        }
                };
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
