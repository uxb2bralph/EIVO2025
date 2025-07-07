using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelCore.Schema.TurnKey;
using ModelCore.Schema.TurnKey.Allowance;
using ModelCore.Schema.TurnKey.Invoice;

namespace ModelCore.Schema.EIVO
{
    public static class ExtensionMethods
    {
        public static InvoiceRootInvoice FromMIG(this Invoice migItem)
        {
            var item = new InvoiceRootInvoice
            {
                Address = migItem.Main?.Buyer?.Address,
                BuyerId = migItem.Main?.Buyer?.Identifier,
                BuyerMark = (byte?)migItem.Main?.BuyerRemark,
                BuyerName = migItem.Main?.Buyer?.Name,
                CarrierType = migItem.Main?.CarrierType,
                CarrierId1 = migItem.Main?.CarrierId1,
                CarrierId2 = migItem.Main?.CarrierId2,
                Contact = new InvoiceRootInvoiceContact
                {
                    Address = migItem.Main?.Buyer?.Address,
                    Email = migItem.Main?.Buyer?.EmailAddress,
                    Name = migItem.Main?.Buyer?.Name,
                    TEL = migItem.Main?.Buyer?.TelephoneNumber,
                },
                ContactName = migItem.Main?.Buyer?.Name,
                Currency = migItem.Amount?.Currency.ToString(),
                CustomsClearanceMark = (byte?)migItem.Main?.CustomsClearanceMark,
                CustomsClearanceMarkSpecified = migItem.Main?.CustomsClearanceMarkSpecified ?? false,
                CustomerID = migItem.Main?.Buyer?.CustomerNumber,
                DiscountAmountSpecified = migItem.Amount?.DiscountAmountSpecified ?? false,
                DiscountAmount = migItem.Amount?.DiscountAmount ?? 0m,
                DonateMark = $"{(int?)migItem.Main?.DonateMark}",
                EMail = migItem.Main?.Buyer?.EmailAddress,
                FreeTaxSalesAmount = migItem.Amount?.FreeTaxSalesAmount,
                FreeTaxSalesAmountSpecified = migItem.Amount?.FreeTaxSalesAmount != null,
                InvoiceDate = migItem.Main?.InvoiceDate,
                InvoiceTime = migItem.Main?.InvoiceTime,
                InvoiceNumber = migItem.Main?.InvoiceNumber,
                InvoiceType = $"{(int?)migItem.Main?.InvoiceType}",
                MainRemark = migItem.Main?.MainRemark,
                PrintMark = migItem.Main?.PrintMark,
                Phone = migItem.Main?.Buyer?.TelephoneNumber,
                NPOBAN = migItem.Main?.NPOBAN,
                SellerId = migItem.Main?.Seller?.Identifier,
                RandomNumber = migItem.Main?.RandomNumber,
                SalesAmount = migItem.Amount?.SalesAmount ?? 0m,
                TaxType = (byte)(migItem.Amount?.TaxType ?? TaxTypeEnum.Item1),
                TaxRate = migItem.Amount?.TaxRate ?? 0.05m,
                TaxRateSpecified = true,
                TaxAmount = migItem.Amount?.TaxAmount ?? 0,
                ZeroTaxSalesAmount = migItem.Amount?.ZeroTaxSalesAmount,
                ZeroTaxSalesAmountSpecified = migItem.Amount?.ZeroTaxSalesAmount != null,
                TotalAmount = migItem.Amount?.TotalAmount ?? 0,
            };

            short idx = 1;
            item.InvoiceItem = migItem.Details!.Select(d => new InvoiceRootInvoiceInvoiceItem
            {
                Description = d.Description,
                Item = d.RelateNumber,
                Quantity = d.Quantity ?? 0,
                Unit = d.Unit,
                UnitPrice = d.UnitPrice ?? 0,
                Amount = d.Amount ?? 0,
                Remark = d.Remark,
                SequenceNumber = idx++,
                TaxTypeSpecified = true,
                TaxType = (byte)d.TaxType,
            }).ToArray();

            return item;

        }

        public static CancelInvoiceRootCancelInvoice FromMIG(this CancelInvoice migItem)
        {
            return new CancelInvoiceRootCancelInvoice
            {
                SellerId = migItem.SellerId,
                BuyerId = migItem.BuyerId,
                InvoiceDate = migItem.InvoiceDate != null && migItem.InvoiceDate.Length == 8
                                            ? $"{migItem.InvoiceDate.Substring(0, 4)}/{migItem.InvoiceDate.Substring(4, 2)}/{migItem.InvoiceDate.Substring(6)}"
                                            : null,
                CancelDate = migItem.CancelDate != null && migItem.CancelDate.Length == 8
                                            ? $"{migItem.CancelDate.Substring(0, 4)}/{migItem.CancelDate.Substring(4, 2)}/{migItem.CancelDate.Substring(6)}"
                                            : null,
                CancelTime = migItem.CancelTime.ToString("HH:mm:ss"),
                CancelInvoiceNumber = migItem.CancelInvoiceNumber,
                CancelReason = migItem.CancelReason,
                Remark = migItem.Remark,
                ReturnTaxDocumentNumber = migItem.ReturnTaxDocumentNumber,
            };
        }

        public static AllowanceRootAllowance FromMIG(this Allowance migItem)
        {
            var item = new AllowanceRootAllowance
            {
                SellerName = migItem.Main?.Seller?.Name,
                SellerId = migItem.Main?.Seller?.Identifier,
                BuyerId = migItem.Main?.Buyer?.Identifier,
                BuyerName = migItem.Main?.Buyer?.Name,
                AllowanceDate = migItem.Main?.AllowanceDate != null && migItem.Main?.AllowanceDate.Length == 8
                                            ? $"{migItem.Main.AllowanceDate.Substring(0, 4)}/{migItem.Main.AllowanceDate.Substring(4, 2)}/{migItem.Main.AllowanceDate.Substring(6)}"
                                            : null,
                AllowanceNumber = migItem.Main?.AllowanceNumber,
                AllowanceType = (byte?)migItem.Main?.AllowanceType ?? 1,
                TaxAmount = migItem.Amount?.TaxAmount ?? 0,
                TotalAmount = migItem.Amount?.TotalAmount ?? 0,
                Contact = new AllowanceRootAllowanceContact
                {
                    Address = migItem.Main?.Buyer?.Address,
                    Email = migItem.Main?.Buyer?.EmailAddress,
                    Name = migItem.Main?.Buyer?.Name,
                    TEL = migItem.Main?.Buyer?.TelephoneNumber,
                },
            };

            short idx = 1;
            short seqNo = 1;
            bool result = false;
            bool parseSeqNo(String? sequenceNumber)
            {
                result = sequenceNumber != null && short.TryParse(sequenceNumber, out seqNo);
                return result;
            }

            item.AllowanceItem = migItem.Details?.Select(d => new AllowanceRootAllowanceAllowanceItem
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


            return item;
        }

        public static CancelAllowanceRootCancelAllowance FromMIG(this CancelAllowance migItem)
        {
            return new CancelAllowanceRootCancelAllowance
            {
                SellerId = migItem.SellerId,
                BuyerId = migItem.BuyerId,
                AllowanceDate = migItem.AllowanceDate != null && migItem.AllowanceDate.Length == 8
                                            ? $"{migItem.AllowanceDate.Substring(0, 4)}/{migItem.AllowanceDate.Substring(4, 2)}/{migItem.AllowanceDate.Substring(6)}"
                                            : null,
                CancelDate = migItem.CancelDate != null && migItem.CancelDate.Length == 8
                                            ? $"{migItem.CancelDate.Substring(0, 4)}/{migItem.CancelDate.Substring(4, 2)}/{migItem.CancelDate.Substring(6)}"
                                            : null,
                CancelTime = migItem.CancelTime.ToString("HH:mm:ss"),
                CancelAllowanceNumber = migItem.CancelAllowanceNumber,
                CancelReason = migItem.CancelReason,
                Remark = migItem.Remark,
            };
        }

    }
}
