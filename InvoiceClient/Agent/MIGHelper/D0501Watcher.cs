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
    public class D0501Watcher : C0401Watcher
    {
        public D0501Watcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override XmlDocument prepareInvoiceDocument(string invoiceFile)
        {
            XmlDocument docInv = new XmlDocument();
            docInv.Load(invoiceFile);

            CancelAllowanceRoot root = new CancelAllowanceRoot { };

            if (docInv.DocumentElement != null)
            {
                docInv.DocumentElement.SetAttribute("xmlns", "urn:GEINV:eInvoiceMessage:D0501:3.2");
                docInv.LoadXml(docInv.OuterXml);

                var D0501 = docInv.ConvertTo<ModelCore.Schema.TurnKey.D0501.CancelAllowance>();
                root.CancelAllowance = new CancelAllowanceRootCancelAllowance[]
                {
                        new CancelAllowanceRootCancelAllowance
                        {
                            SellerId = D0501.SellerId,
                            BuyerId = D0501.BuyerId,
                            AllowanceDate = D0501.AllowanceDate!=null && D0501.AllowanceDate.Length==8 
                                ? $"{D0501.AllowanceDate.Substring(0,4)}/{D0501.AllowanceDate.Substring(4,2)}/{D0501.AllowanceDate.Substring(6)}" 
                                : null,
                            CancelDate = D0501.CancelDate!=null && D0501.CancelDate.Length==8
                                ? $"{D0501.CancelDate.Substring(0,4)}/{D0501.CancelDate.Substring(4,2)}/{D0501.CancelDate.Substring(6)}"
                                : null,
                            CancelTime = D0501.CancelTime.ToString("HH:mm:ss"),
                            CancelAllowanceNumber = D0501.CancelAllowanceNumber,
                            CancelReason = D0501.CancelReason,
                            Remark = D0501.Remark,
                        }
                };
            }

            return root.ConvertToXml();
        }

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            var result = invSvc.UploadAllowanceCancellationV2(docInv).ConvertTo<Root>();
            return result;
        }

    }
}
