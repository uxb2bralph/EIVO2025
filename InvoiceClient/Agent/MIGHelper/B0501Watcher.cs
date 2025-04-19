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
    public class B0501Watcher : C0401Watcher
    {
        public B0501Watcher(String fullPath)
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
                docInv.DocumentElement.SetAttribute("xmlns", "urn:GEINV:eInvoiceMessage:B0501:3.2");
                docInv.LoadXml(docInv.OuterXml);

                var B0501 = docInv.ConvertTo<ModelCore.Schema.TurnKey.B0501.CancelAllowance>();
                root.CancelAllowance = new CancelAllowanceRootCancelAllowance[]
                {
                        new CancelAllowanceRootCancelAllowance
                        {
                            SellerId = B0501.SellerId,
                            BuyerId = B0501.BuyerId,
                            AllowanceDate = B0501.AllowanceDate!=null && B0501.AllowanceDate.Length==8 
                                ? $"{B0501.AllowanceDate.Substring(0,4)}/{B0501.AllowanceDate.Substring(4,2)}/{B0501.AllowanceDate.Substring(6)}" 
                                : null,
                            CancelDate = B0501.CancelDate!=null && B0501.CancelDate.Length==8
                                ? $"{B0501.CancelDate.Substring(0,4)}/{B0501.CancelDate.Substring(4,2)}/{B0501.CancelDate.Substring(6)}"
                                : null,
                            CancelTime = B0501.CancelTime.ToString("HH:mm:ss"),
                            CancelAllowanceNumber = B0501.CancelAllowanceNumber,
                            CancelReason = B0501.CancelReason,
                            Remark = B0501.Remark,
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
