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
    public class D0401Watcher : C0401Watcher
    {
        public D0401Watcher(String fullPath)
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
                docInv.DocumentElement.SetAttribute("xmlns", "urn:GEINV:eInvoiceMessage:D0401:3.2");
                docInv.LoadXml(docInv.OuterXml);

                var D0401 = docInv.ConvertTo<ModelCore.Schema.TurnKey.D0401.Allowance>();
                root.Allowance = new AllowanceRootAllowance[]
                {
                        new AllowanceRootAllowance
                        {
                            SellerName = D0401.Main?.Seller?.Name,
                            SellerId = D0401.Main?.Seller?.Identifier,
                            BuyerId = D0401.Main?.Buyer?.Identifier,
                            BuyerName = D0401.Main?.Buyer?.Name,
                            AllowanceDate = D0401.Main?.AllowanceDate!=null && D0401.Main?.AllowanceDate.Length==8
                                ? $"{D0401.Main.AllowanceDate.Substring(0,4)}/{D0401.Main.AllowanceDate.Substring(4,2)}/{D0401.Main.AllowanceDate.Substring(6)}"
                                : null,
                            AllowanceNumber = D0401.Main?.AllowanceNumber,
                            AllowanceType = (byte?)D0401.Main?.AllowanceType ?? 1,
                            TaxAmount = D0401.Amount?.TaxAmount ?? 0,
                            TotalAmount = D0401.Amount?.TotalAmount ?? 0,
                            Contact = new AllowanceRootAllowanceContact
                            {
                                Address = D0401.Main?.Buyer?.Address,
                                Email = D0401.Main?.Buyer?.EmailAddress,
                                Name = D0401.Main?.Buyer?.Name,
                                TEL = D0401.Main?.Buyer?.TelephoneNumber,
                            },
                        }
                };

                short idx = 1;
                short seqNo = 1;
                bool result = false;
                bool parseSeqNo(String sequenceNumber)
                {
                    result = sequenceNumber != null && short.TryParse(sequenceNumber, out seqNo);
                    return result;
                }
                root.Allowance[0].AllowanceItem = D0401.Details.Select(d => new AllowanceRootAllowanceAllowanceItem 
                {
                    OriginalDescription = d.OriginalDescription,
                    OriginalInvoiceDate = d.OriginalInvoiceDate != null && d.OriginalInvoiceDate.Length == 8
                                ? $"{d.OriginalInvoiceDate.Substring(0, 4)}/{d.OriginalInvoiceDate.Substring(4, 2)}/{d.OriginalInvoiceDate.Substring(6)}"
                                : null,
                    OriginalInvoiceNumber = d.OriginalInvoiceNumber,
                    OriginalSequenceNumberSpecified = parseSeqNo(d.OriginalSequenceNumber),
                    OriginalSequenceNumber = result ? seqNo : (short?)null,
                    AllowanceSequenceNumber = idx++,
                    Tax = d.Tax,
                    TaxType = (byte)d.TaxType,
                    Quantity = d.Quantity,
                    Unit = d.Unit,
                    UnitPrice = d.UnitPrice,
                    Amount = d.Amount,
                }).ToArray();
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
