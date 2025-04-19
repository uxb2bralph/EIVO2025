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
    public class A0501Watcher : C0401Watcher
    {
        public A0501Watcher(String fullPath)
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
                docInv.DocumentElement.SetAttribute("xmlns", "urn:GEINV:eInvoiceMessage:A0501:3.2");
                docInv.LoadXml(docInv.OuterXml);

                var A0501 = docInv.ConvertTo<ModelCore.Schema.TurnKey.A0501.CancelInvoice>();
                root.CancelInvoice = new CancelInvoiceRootCancelInvoice[]
                {
                        new CancelInvoiceRootCancelInvoice
                        {
                            SellerId = A0501.SellerId,
                            BuyerId = A0501.BuyerId,
                            InvoiceDate = A0501.InvoiceDate!=null && A0501.InvoiceDate.Length==8 
                                ? $"{A0501.InvoiceDate.Substring(0,4)}/{A0501.InvoiceDate.Substring(4,2)}/{A0501.InvoiceDate.Substring(6)}" 
                                : null,
                            CancelDate = A0501.CancelDate!=null && A0501.CancelDate.Length==8
                                ? $"{A0501.CancelDate.Substring(0,4)}/{A0501.CancelDate.Substring(4,2)}/{A0501.CancelDate.Substring(6)}"
                                : null,
                            CancelTime = A0501.CancelTime.ToString("HH:mm:ss"),
                            CancelInvoiceNumber = A0501.CancelInvoiceNumber,
                            CancelReason = A0501.CancelReason,
                            Remark = A0501.Remark,
                            ReturnTaxDocumentNumber = A0501.ReturnTaxDocumentNumber,
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
