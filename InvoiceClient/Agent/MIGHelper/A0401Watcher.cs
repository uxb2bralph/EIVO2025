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
    public class A0401Watcher : C0401Watcher
    {
        public A0401Watcher(String fullPath)
            : base(fullPath)
        {

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
                                    : (short)Naming.NotificationIndication.Immediate
            };

            if (docInv.DocumentElement != null)
            {
                docInv.DocumentElement.SetAttribute("xmlns", "urn:GEINV:eInvoiceMessage:A0401:3.2");
                docInv.LoadXml(docInv.OuterXml);

                var a0401 = docInv.ConvertTo<ModelCore.Schema.TurnKey.A0401.Invoice>();
                root.Invoice = new InvoiceRootInvoice[]
                {
                        new InvoiceRootInvoice
                        {
                            Address = a0401.Main?.Buyer?.Address,
                            BuyerId = a0401.Main?.Buyer?.Identifier,
                            BuyerMark = (byte?)a0401.Main?.BuyerRemark,
                            BuyerName = a0401.Main?.Buyer?.Name,
                            Contact = new InvoiceRootInvoiceContact
                            {
                                Address = a0401.Main?.Buyer?.Address,
                                Email = a0401.Main?.Buyer?.EmailAddress,
                                Name = a0401.Main?.Buyer?.Name,
                                TEL = a0401.Main?.Buyer?.TelephoneNumber,
                            },
                            ContactName = a0401.Main?.Buyer?.Name,
                            Currency = a0401.Amount.CurrencySpecified ? a0401.Amount.Currency.ToString() : null,
                            CustomsClearanceMark = (byte?)a0401.Main?.CustomsClearanceMark,
                            CustomsClearanceMarkSpecified = a0401.Main?.CustomsClearanceMarkSpecified ?? false,
                            CustomerID = a0401.Main?.Buyer?.CustomerNumber,
                            DiscountAmountSpecified = a0401.Amount?.DiscountAmountSpecified ?? false,
                            DiscountAmount = a0401.Amount?.DiscountAmount ?? 0m,
                            DonateMark = $"{(int?)a0401.Main?.DonateMark}",
                            EMail = a0401.Main?.Buyer?.EmailAddress,
                            FreeTaxSalesAmountSpecified = false,
                            InvoiceDate = a0401.Main?.InvoiceDate!=null && a0401.Main?.InvoiceDate.Length==8
                                ? $"{a0401.Main.InvoiceDate.Substring(0,4)}/{a0401.Main.InvoiceDate.Substring(4,2)}/{a0401.Main.InvoiceDate.Substring(6)}"
                                : null,
                            InvoiceTime = a0401.Main?.InvoiceTime.ToString("HH:mm:ss"),
                            InvoiceNumber = a0401.Main?.InvoiceNumber,
                            InvoiceType = $"{(int?)a0401.Main?.InvoiceType}",
                            MainRemark = a0401.Main?.MainRemark,
                            PrintMark = "Y",
                            Phone = a0401.Main?.Buyer?.TelephoneNumber,
                            SellerId = a0401.Main?.Seller?.Identifier,
                            SalesAmount = a0401.Amount?.SalesAmount ?? 0m,
                            TaxType = (byte)a0401.Amount?.TaxType,
                            TaxRate = a0401.Amount?.TaxRate ?? 0.05m,
                            TaxRateSpecified = true,
                            TaxAmount = a0401.Amount?.TaxAmount ?? 0,
                            TotalAmount = a0401.Amount?.TotalAmount ?? 0,
                            BondedAreaConfirm = (byte?)a0401.Main?.BondedAreaConfirm,
                        }
                };

                short idx = 1;
                root.Invoice[0].InvoiceItem = a0401.Details.Select(d => new InvoiceRootInvoiceInvoiceItem 
                {
                    Description = d.Description,
                    Item = d.RelateNumber,
                    Quantity = d.Quantity,
                    Unit = d.Unit,
                    UnitPrice = d.UnitPrice,
                    Amount = d.Amount,
                    Remark = d.Remark,
                    SequenceNumber = idx++,
                    TaxTypeSpecified = false,
                }).ToArray();
            }

            return root.ConvertToXml();
        }

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            var result = invSvc.UploadInvoiceByClient(docInv, Settings.Default.ClientID, (int)_channelID, false, (int)Naming.InvoiceProcessType.A0401).ConvertTo<Root>();
            return result;
        }

    }
}
