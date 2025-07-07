using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ModelCore.DataEntity;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TurnKey.E0402;
using ModelCore.Schema.EIVO;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using ModelCore.Locale;
using CommonLib.Utility;
using ModelCore.Properties;


namespace ModelCore.Helper
{
    public static class B2BExtensionMethods
    {


        //public static string ByteSubStr(string a_SrcStr, int a_StartIndex, int a_Cnt)
        //{
        //    Encoding l_Encoding = Encoding.GetEncoding("utf-8", new EncoderExceptionFallback(), new DecoderReplacementFallback(""));
        //    byte[] l_byte = l_Encoding.GetBytes(a_SrcStr);// Encoding.UTF8.GetBytes(a_SrcStr);
        //    //byte[] l_byte = System.Text.Encoding.Default.GetBytes(a_SrcStr); 
        //    if (a_Cnt <= 0)
        //        return "";
        //    //若長度10 
        //    //若a_StartIndex傳入9 -> ok, 10 ->不行 
        //    if (a_StartIndex + 1 > l_byte.Length)
        //        return "";
        //    else
        //    {
        //        //若a_StartIndex傳入9 , a_Cnt 傳入2 -> 不行 -> 改成 9,1 
        //        if (a_StartIndex + a_Cnt > l_byte.Length)
        //            a_Cnt = l_byte.Length - a_StartIndex;
        //    }
        //    return l_Encoding.GetString(l_byte, a_StartIndex, a_Cnt); // System.Text.Encoding.Default.GetString(l_byte, a_StartIndex, a_Cnt); 
        //}

        public static ModelCore.Schema.EIVO.B2B.SellerInvoiceRoot CreateSellerInvoiceRoot(this IEnumerable<InvoiceItem> items)
        {
            var result = new ModelCore.Schema.EIVO.B2B.SellerInvoiceRoot
            {
                Invoice = buildSellerInvoiceRootInvoices(items)
            };

            return result;
        }

        private static Schema.EIVO.B2B.SellerInvoiceRootInvoice[] buildSellerInvoiceRootInvoices(IEnumerable<InvoiceItem> items)
        {
            return items.Select(item => new ModelCore.Schema.EIVO.B2B.SellerInvoiceRootInvoice
            {
                BuyerId = item.InvoiceBuyer.ReceiptNo,
                BuyerName = item.InvoiceBuyer.Name,
                SellerId = item.InvoiceSeller.ReceiptNo,
                InvoiceTime = "",
                InvoiceType = String.Format("{0:00}", item.InvoiceType.Value),
                InvoiceDate = String.Format("{0:yyyy/MM/dd}", item.InvoiceDate),
                InvoiceNumber = String.Format("{0}{1}", item.TrackCode, item.No),
                InvoiceItem = buildSellerInvoiceRootDetails(item),
                //DiscountAmount = item.InvoiceAmountType.DiscountAmount.HasValue ? item.InvoiceAmountType.DiscountAmount.Value : 0,
                SalesAmount = item.InvoiceAmountType.SalesAmount ?? 0,
                TaxAmount = item.InvoiceAmountType.TaxAmount ?? 0,
                TaxType = item.InvoiceAmountType.TaxType.Value,
                TotalAmount = item.InvoiceAmountType.TotalAmount ?? 0,
                //ExtraRemark = buildSellerInvoiceExtraRemark(item)
            }).ToArray();
        }

        private static Schema.EIVO.B2B.SellerInvoiceRootInvoiceInvoiceItem[] buildSellerInvoiceRootDetails(InvoiceItem item)
        {
            List<ModelCore.Schema.EIVO.B2B.SellerInvoiceRootInvoiceInvoiceItem> items = new List<Schema.EIVO.B2B.SellerInvoiceRootInvoiceInvoiceItem>();
            foreach (var detailItem in item.InvoiceDetails)
            {
                foreach (var productItem in detailItem.InvoiceProduct.InvoiceProductItem)
                {
                    items.Add(new ModelCore.Schema.EIVO.B2B.SellerInvoiceRootInvoiceInvoiceItem
                    {
                        Amount = productItem.CostAmount.HasValue ? Decimal.Parse(String.Format("{0:0.0000}", productItem.CostAmount.Value)) : 0m,
                        Amount2 = productItem.CostAmount2.HasValue ? Decimal.Parse(String.Format("{0:0.0000}", productItem.CostAmount2.Value)) : 0m,
                        Description = detailItem.InvoiceProduct.Brief,
                        Quantity = productItem.Piece.HasValue ? Decimal.Parse(String.Format("{0:0.0000}", productItem.Piece.Value)) : 0,
                        Quantity2 = productItem.Piece2.HasValue ? Decimal.Parse(String.Format("{0:0.0000}", productItem.Piece2.Value)) : 0,
                        Remark = productItem.Remark,
                        SequenceNumber = decimal.Parse(String.Format("{0:00}", productItem.No)) + 1,
                        Unit = productItem.PieceUnit,
                        Unit2 = productItem.PieceUnit2,
                        UnitPrice = productItem.UnitCost.HasValue ? Decimal.Parse(String.Format("{0:0.0000}", productItem.UnitCost.Value)) : 0,
                        UnitPrice2 = productItem.UnitCost2.HasValue ? Decimal.Parse(String.Format("{0:0.0000}", productItem.UnitCost2.Value)) : 0
                    });
                }
            }
            return items.ToArray();
        }

        public static AllowanceRoot CreateAllowanceRoot(this IEnumerable<InvoiceAllowance> items)
        {
            return new AllowanceRoot()
            {
                Allowance = B2BExtensionMethods.buildAllowanceRootAllowance(items)
            };
        }

        private static AllowanceRootAllowance[] buildAllowanceRootAllowance(IEnumerable<InvoiceAllowance> items)
        {
            short itemIdx = 1;
            return items.Select<InvoiceAllowance, AllowanceRootAllowance>((Func<InvoiceAllowance, AllowanceRootAllowance>)(a =>
            {
                AllowanceRootAllowance allowanceRootAllowance = new AllowanceRootAllowance();
                allowanceRootAllowance.BuyerId = a.InvoiceAllowanceBuyer.ReceiptNo;
                allowanceRootAllowance.BuyerName = a.InvoiceAllowanceBuyer.CustomerName;
                allowanceRootAllowance.SellerId = a.InvoiceAllowanceSeller.ReceiptNo;
                allowanceRootAllowance.SellerName = a.InvoiceAllowanceSeller.CustomerName;
                allowanceRootAllowance.AllowanceItem = a.InvoiceAllowanceDetails.Select<InvoiceAllowanceDetail, AllowanceRootAllowanceAllowanceItem>((Func<InvoiceAllowanceDetail, AllowanceRootAllowanceAllowanceItem>)(d =>
                {
                    AllowanceRootAllowanceAllowanceItem allowanceAllowanceItem = new AllowanceRootAllowanceAllowanceItem();
                    allowanceAllowanceItem.AllowanceSequenceNumber = d.InvoiceAllowanceItem.No ?? itemIdx++;
                    allowanceAllowanceItem.Amount = d.InvoiceAllowanceItem.Amount.HasValue ? d.InvoiceAllowanceItem.Amount.Value : Decimal.Zero;
                    //allowanceAllowanceItem.Amount2 = d.InvoiceAllowanceItem.Amount2 ?? 0;
                    allowanceAllowanceItem.OriginalInvoiceDate = a.InvoiceItem == null ? string.Format("{0:yyyy/MM/dd}", (object)d.InvoiceAllowanceItem.InvoiceDate) : string.Format("{0:yyyy/MM/dd}", (object)a.InvoiceItem.InvoiceDate);
                    allowanceAllowanceItem.OriginalDescription = d.InvoiceAllowanceItem.OriginalDescription;
                    allowanceAllowanceItem.OriginalInvoiceNumber = a.InvoiceItem == null ? d.InvoiceAllowanceItem.InvoiceNo : string.Format("{0}{1}", (object)a.InvoiceItem.TrackCode, (object)a.InvoiceItem.No);

                    allowanceAllowanceItem.Quantity = d.InvoiceAllowanceItem.Piece ?? 0;

                    //allowanceAllowanceItem.Quantity2 = d.InvoiceAllowanceItem.Piece2 ?? 0;
                    allowanceAllowanceItem.Tax = d.InvoiceAllowanceItem.Tax ?? 0;
                    allowanceAllowanceItem.TaxType = d.InvoiceAllowanceItem.TaxType.Value;
                    allowanceAllowanceItem.Unit = d.InvoiceAllowanceItem.PieceUnit;
                    allowanceAllowanceItem.UnitPrice = d.InvoiceAllowanceItem.UnitCost ?? 0;
                    //allowanceAllowanceItem.Unit2 = d.InvoiceAllowanceItem.PieceUnit2;

                    //allowanceAllowanceItem.UnitPrice2 = d.InvoiceAllowanceItem.UnitCost2 ?? 0;
                    allowanceAllowanceItem.Remark = d.InvoiceAllowanceItem.Remark;
                    allowanceAllowanceItem.OriginalSequenceNumber = d.InvoiceAllowanceItem.OriginalSequenceNo ?? 1;
                    return allowanceAllowanceItem;
                })).ToArray<AllowanceRootAllowanceAllowanceItem>();
                allowanceRootAllowance.AllowanceDate = string.Format("{0:yyyy/MM/dd}", (object)a.AllowanceDate);
                Decimal? nullable1;
                Decimal zero4;
                if (!a.TaxAmount.HasValue)
                {
                    zero4 = Decimal.Zero;
                }
                else
                {
                    nullable1 = a.TaxAmount;
                    zero4 = nullable1.Value;
                }
                allowanceRootAllowance.TaxAmount = zero4;
                allowanceRootAllowance.AllowanceNumber = a.AllowanceNumber;
                nullable1 = a.TotalAmount;
                Decimal zero5;
                if (!nullable1.HasValue)
                {
                    zero5 = Decimal.Zero;
                }
                else
                {
                    nullable1 = a.TotalAmount;
                    zero5 = nullable1.Value;
                }
                allowanceRootAllowance.TotalAmount = zero5;
                allowanceRootAllowance.AllowanceType = a.AllowanceType.Value;
                //allowanceRootAllowance.ExtraRemark = (object)B2BExtensionMethods.buildAllowanceExtraRemark(a);
                return allowanceRootAllowance;
            })).ToArray<AllowanceRootAllowance>();
        }

        //private static XmlNode[] buildAllowanceExtraRemark(InvoiceAllowance item)
        //{
        //    if (item.InvoiceAllowanceItemExtension == null)
        //        return (XmlNode[])null;
        //    XmlDocument xmlDocument = new XmlDocument();
        //    xmlDocument.LoadXml(item.InvoiceAllowanceItemExtension.ExtraRemark.ToString());
        //    return xmlDocument.DocumentElement.ChildNodes.Cast<XmlNode>().ToArray<XmlNode>();
        //}

        public static CancelAllowanceRoot CreateCancelAllowanceRoot(this IEnumerable<CDS_Document> items)
        {
            return new CancelAllowanceRoot()
            {
                CancelAllowance = B2BExtensionMethods.buildCancelAllowanceRoot(items.Select<CDS_Document, InvoiceAllowance>((Func<CDS_Document, InvoiceAllowance>)(d => d.DerivedDocument.ParentDocument.InvoiceAllowance)))
            };
        }

        private static CancelAllowanceRootCancelAllowance[] buildCancelAllowanceRoot(IEnumerable<InvoiceAllowance> items)
        {
            return items.Select<InvoiceAllowance, CancelAllowanceRootCancelAllowance>((Func<InvoiceAllowance, CancelAllowanceRootCancelAllowance>)(item => new CancelAllowanceRootCancelAllowance()
            {
                BuyerId = item.InvoiceAllowanceBuyer.ReceiptNo,
                SellerId = item.InvoiceAllowanceSeller.ReceiptNo,
                AllowanceDate = string.Format("{0:yyyy/MM/dd}", (object)item.AllowanceDate),
                CancelDate = string.Format("{0:yyyy/MM/dd}", (object)item.InvoiceAllowanceCancellation.CancelDate),
                CancelAllowanceNumber = item.TurnkeyAllowanceNo,
                CancelTime = string.Format("{0:HH:mm:ss}", (object)item.InvoiceAllowanceCancellation.CancelDate),
                CancelReason = item.InvoiceAllowanceCancellation.Remark,
                Remark = ""
            })).ToArray<CancelAllowanceRootCancelAllowance>();
        }

        public static CancelInvoiceRoot CreateCancelInvoiceRoot(this IEnumerable<CDS_Document> items)
        {
            return new CancelInvoiceRoot()
            {
                CancelInvoice = B2BExtensionMethods.buildCancelInvoiceRootCancelInvoices(items.Select<CDS_Document, InvoiceItem>((Func<CDS_Document, InvoiceItem>)(d => d.DerivedDocument.ParentDocument.InvoiceItem)))
            };
        }

        private static CancelInvoiceRootCancelInvoice[] buildCancelInvoiceRootCancelInvoices(IEnumerable<InvoiceItem> items)
        {
            return items.Select<InvoiceItem, CancelInvoiceRootCancelInvoice>((Func<InvoiceItem, CancelInvoiceRootCancelInvoice>)(i => new CancelInvoiceRootCancelInvoice()
            {
                BuyerId = i.InvoiceBuyer.ReceiptNo,
                SellerId = i.InvoiceSeller.ReceiptNo,
                InvoiceDate = string.Format("{0:yyyy/MM/dd}", (object)i.InvoiceDate),
                CancelDate = string.Format("{0:yyyy/MM/dd}", (object)i.InvoiceCancellation.CancelDate),
                CancelInvoiceNumber = i.InvoiceCancellation.CancellationNo,
                CancelTime = string.Format("{0:HH:mm:ss}", (object)i.InvoiceCancellation.CancelDate),
                CancelReason = i.InvoiceCancellation.CancelReason + i.InvoiceCancellation.Remark,
                Remark = "",
                ReturnTaxDocumentNumber = i.InvoiceCancellation.ReturnTaxDocumentNo
            })).ToArray<CancelInvoiceRootCancelInvoice>();
        }

        public static X509Certificate2 PrepareSignerCertificate(this Organization item)
        {
            var useSigner = item.OrganizationStatus.Entrusting == true;
            var signerToken = item.OrganizationStatus.UserToken;
            if (useSigner && signerToken != null)
            {
                return new X509Certificate2(Convert.FromBase64String(signerToken.PKCS12), signerToken.Token.ToString().Substring(0, 8));
            }
            return null;
        }

    }
}
