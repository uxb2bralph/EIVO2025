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
    public class C0401Watcher : InvoiceWatcher
    {
        public C0401Watcher(String fullPath)
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
                docInv.DocumentElement.SetAttribute("xmlns", "urn:GEINV:eInvoiceMessage:C0401:3.2");
                docInv.LoadXml(docInv.OuterXml);

                var c0401 = docInv.ConvertTo<ModelCore.Schema.TurnKey.C0401.Invoice>();
                root.Invoice = new InvoiceRootInvoice[]
                {
                        new InvoiceRootInvoice
                        {
                            Address = c0401.Main?.Buyer?.Address,
                            BuyerId = c0401.Main?.Buyer?.Identifier,
                            BuyerMark = (byte?)c0401.Main?.BuyerRemark,
                            BuyerName = c0401.Main?.Buyer?.Name,
                            CarrierType = c0401.Main?.CarrierType,
                            CarrierId1 = c0401.Main?.CarrierId1,
                            CarrierId2 = c0401.Main?.CarrierId2,
                            Contact = new InvoiceRootInvoiceContact
                            {
                                Address = c0401.Main?.Buyer?.Address,
                                Email = c0401.Main?.Buyer?.EmailAddress,
                                Name = c0401.Main?.Buyer?.Name,
                                TEL = c0401.Main?.Buyer?.TelephoneNumber,
                            },
                            ContactName = c0401.Main?.Buyer?.Name,
                            Currency = c0401.Amount.CurrencySpecified ? c0401.Amount.Currency.ToString() : null,
                            CustomsClearanceMark = (byte?)c0401.Main?.CustomsClearanceMark,
                            CustomsClearanceMarkSpecified = c0401.Main?.CustomsClearanceMarkSpecified ?? false,
                            CustomerID = c0401.Main?.Buyer?.CustomerNumber,
                            DiscountAmountSpecified = c0401.Amount?.DiscountAmountSpecified ?? false,
                            DiscountAmount = c0401.Amount?.DiscountAmount ?? 0m,
                            DonateMark = $"{(int?)c0401.Main?.DonateMark}",
                            EMail = c0401.Main?.Buyer?.EmailAddress,
                            FreeTaxSalesAmount = c0401.Amount?.FreeTaxSalesAmount,
                            FreeTaxSalesAmountSpecified = c0401.Amount?.FreeTaxSalesAmount != null,
                            InvoiceDate = c0401.Main?.InvoiceDate!=null && c0401.Main?.InvoiceDate.Length==8 
                                ? $"{c0401.Main.InvoiceDate.Substring(0,4)}/{c0401.Main.InvoiceDate.Substring(4,2)}/{c0401.Main.InvoiceDate.Substring(6)}" 
                                : null,
                            InvoiceTime = c0401.Main?.InvoiceTime.ToString("HH:mm:ss"),
                            InvoiceNumber = c0401.Main?.InvoiceNumber,
                            InvoiceType = $"{(int?)c0401.Main?.InvoiceType}",
                            MainRemark = c0401.Main?.MainRemark,
                            PrintMark = c0401.Main?.PrintMark,
                            Phone = c0401.Main?.Buyer?.TelephoneNumber,
                            NPOBAN = c0401.Main?.NPOBAN,
                            SellerId = c0401.Main?.Seller?.Identifier,
                            RandomNumber = c0401.Main?.RandomNumber,
                            SalesAmount = c0401.Amount?.SalesAmount ?? 0m,
                            TaxType = (byte)c0401.Amount?.TaxType,
                            TaxRate = c0401.Amount?.TaxRate ?? 0.05m,
                            TaxRateSpecified = true,
                            TaxAmount = c0401.Amount?.TaxAmount ?? 0,
                            ZeroTaxSalesAmount = c0401.Amount?.ZeroTaxSalesAmount,
                            ZeroTaxSalesAmountSpecified = c0401.Amount?.ZeroTaxSalesAmount!=null,
                            TotalAmount = c0401.Amount?.TotalAmount ?? 0,
                        }
                };

                short idx = 1;
                root.Invoice[0].InvoiceItem = c0401.Details.Select(d => new InvoiceRootInvoiceInvoiceItem 
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
            var result = invSvc.UploadInvoiceByClient(docInv, Settings.Default.ClientID, (int)_channelID, false, (int)Naming.InvoiceProcessType.C0401).ConvertTo<Root>();
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
