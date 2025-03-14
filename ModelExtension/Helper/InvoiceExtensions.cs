using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using CommonLib.DataAccess;
using CommonLib.Security.UseCrypto;
using CommonLib.Utility;

using ModelCore.DataEntity;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Schema.TurnKey;

namespace ModelCore.Helper
{
    public static class InvoiceExtensions
    {
        //public static ModelCore.Schema.TurnKey.A0101.Invoice BuildA0101(this InvoiceItem item)
        //{
        //    Organization seller = item.Organization;
        //    InvoiceAmountType amount = item.InvoiceAmountType;

        //    var result = new ModelCore.Schema.TurnKey.A0101.Invoice
        //    {
        //        Main = new ModelCore.Schema.TurnKey.A0101.Main
        //        {
        //            InvoiceNumber = item.TrackCode + item.No,
        //            BuyerRemark =  (ModelCore.Schema.TurnKey.A0101.BuyerRemarkEnum?)item.BuyerRemark,
        //            BuyerRemarkSpecified = item.BuyerRemark.HasValue,
        //            Category = item.Category,
        //            CheckNumber = item.CheckNo,
        //            CustomsClearanceMark = (ModelCore.Schema.TurnKey.A0101.CustomsClearanceMarkEnum?)item.CustomsClearanceMark,
        //            CustomsClearanceMarkSpecified = item.CustomsClearanceMark.HasValue,
        //            DonateMark = item.InvoiceDonation==null ? Schema.TurnKey.A0101.DonateMarkEnum.Item0 : Schema.TurnKey.A0101.DonateMarkEnum.Item1,
        //            GroupMark = item.GroupMark,
        //            InvoiceDate = $"{item.InvoiceDate:yyyyMMdd}",
        //            InvoiceTime = item.InvoiceDate.Value,
        //            InvoiceType = (ModelCore.Schema.TurnKey.A0101.InvoiceTypeEnum)item.InvoiceType.Value,
        //            MainRemark = item.Remark,
        //            RelateNumber = item.RelateNumber,
        //            Seller = new ModelCore.Schema.TurnKey.A0101.MainSeller
        //            {
        //                Address = seller.Addr,
        //                //CustomerNumber = "",
        //                EmailAddress = seller.ContactEmail,
        //                FacsimileNumber = seller.Fax,
        //                Identifier = seller.ReceiptNo,
        //                Name = seller.CompanyName,
        //                //PersonInCharge = seller.UndertakerName,
        //                //RoleRemark = "",
        //                TelephoneNumber = seller.Phone
        //            }
        //        },
        //        Amount = new ModelCore.Schema.TurnKey.A0101.Amount
        //        {
        //            Currency = amount.CurrencyID.HasValue ? amount.CurrencyType.AbbrevName.ToEnumValue<ModelCore.Schema.TurnKey.A0101.CurrencyCodeEnum>() : null,
        //            CurrencySpecified = amount.CurrencyID.HasValue,
        //            DiscountAmount = amount.DiscountAmount.HasValue ? (long)amount.DiscountAmount.Value : 0,
        //            DiscountAmountSpecified = true,
        //            ExchangeRate = 1,
        //            ExchangeRateSpecified = false,
        //            OriginalCurrencyAmount = amount.OriginalCurrencyAmount.HasValue ? amount.OriginalCurrencyAmount.Value : 0,
        //            OriginalCurrencyAmountSpecified = false,
        //            SalesAmount = (int)amount.SalesAmount.Value,
        //            TaxAmount = amount.TaxAmount.HasValue ? (long)amount.TaxAmount.Value : 0,
        //            TaxRate = amount.TaxRate ?? 0.05m,
        //            TaxType = (ModelCore.Schema.TurnKey.A0101.TaxTypeEnum)amount.TaxType.Value,
        //            TotalAmount = (long)amount.TotalAmount.Value
        //        },
        //        Details = buildA0101Details(item)
        //    };

        //    if (item.InvoiceBuyer != null)
        //    {
        //        InvoiceBuyer buyer = item.InvoiceBuyer;
        //        if (buyer.Organization != null)
        //        {
        //            Organization buyerOrg = buyer.Organization;
        //            result.Main.Buyer = new ModelCore.Schema.TurnKey.A0101.MainBuyer
        //            {
        //                Address = buyerOrg.Addr,
        //                //CustomerNumber = "",
        //                EmailAddress = buyerOrg.ContactEmail,
        //                FacsimileNumber = buyerOrg.Fax,
        //                Identifier = buyerOrg.ReceiptNo,
        //                Name = buyerOrg.CompanyName,
        //                //PersonInCharge = buyerOrg.UndertakerName,
        //                //RoleRemark = "",
        //                TelephoneNumber = buyerOrg.Phone
        //            };
        //        }
        //        else
        //        {
        //            result.Main.Buyer = new ModelCore.Schema.TurnKey.A0101.MainBuyer
        //            {
        //                Address = buyer.Address,
        //                //CustomerNumber = "",
        //                //EmailAddress = "",
        //                //FacsimileNumber = "",
        //                Identifier = String.IsNullOrEmpty(buyer.ReceiptNo) ? "0000000000" : buyer.ReceiptNo,
        //                Name = buyer.Name,
        //                //PersonInCharge = buyer.Name,
        //                //RoleRemark = "",
        //                //TelephoneNumber = ""
        //            };
        //        }
        //    }


        //    return result;
        //}

        //private static ModelCore.Schema.TurnKey.A0101.DetailsProductItem[] buildA0101Details(InvoiceItem item)
        //{
        //    List<ModelCore.Schema.TurnKey.A0101.DetailsProductItem> items = new List<ModelCore.Schema.TurnKey.A0101.DetailsProductItem>();
        //    foreach (var detailItem in item.InvoiceDetails)
        //    {
        //        foreach (var productItem in detailItem.InvoiceProduct.InvoiceProductItem)
        //        {
        //            items.Add(new ModelCore.Schema.TurnKey.A0101.DetailsProductItem
        //            {
        //                Amount = productItem.CostAmount.Value,
        //                Description = detailItem.InvoiceProduct.Brief,
        //                Quantity = productItem.Piece.Value,
        //                RelateNumber = productItem.RelateNumber,
        //                Remark = productItem.Remark,
        //                SequenceNumber = String.Format("{0:00}", productItem.No),
        //                Unit = productItem.PieceUnit,
        //                UnitPrice = productItem.UnitCost.HasValue ? productItem.UnitCost.Value : 0
        //            });
        //        }
        //    }
        //    return items.ToArray();
        //}

        public static ModelCore.Schema.TurnKey.B0101.Allowance BuildB0101(this InvoiceAllowance item)
        {
            var result = new ModelCore.Schema.TurnKey.B0101.Allowance
            {
                Main = new ModelCore.Schema.TurnKey.B0101.Main
                {
                    AllowanceDate = $"{item.AllowanceDate:yyyyMMdd}",
                    AllowanceNumber = item.AllowanceNumber,
                    AllowanceType = (ModelCore.Schema.TurnKey.B0101.AllowanceTypeEnum?)item.AllowanceType ?? Schema.TurnKey.B0101.AllowanceTypeEnum.Item1
                },
                Amount = new ModelCore.Schema.TurnKey.B0101.Amount
                {
                    TaxAmount = item.TaxAmount.HasValue ? (long)item.TaxAmount.Value : 0,
                    TotalAmount = item.TotalAmount.HasValue ? (long)item.TotalAmount.Value : 0
                }
            };

            result.Details = item.InvoiceAllowanceDetails.Select(d => new ModelCore.Schema.TurnKey.B0101.DetailsProductItem
            {
                AllowanceSequenceNumber = d.InvoiceAllowanceItem.No.ToString(),
                Amount = d.InvoiceAllowanceItem.Amount.Value,
                OriginalDescription = d.InvoiceAllowanceItem.OriginalDescription,
                OriginalInvoiceNumber = d.InvoiceAllowanceItem.InvoiceNo,
                Quantity = d.InvoiceAllowanceItem.Piece.HasValue ? d.InvoiceAllowanceItem.Piece.Value : 0.00000M,
                Tax = (long)d.InvoiceAllowanceItem.Tax.Value,
                TaxType = (ModelCore.Schema.TurnKey.B0101.TaxTypeEnum)d.InvoiceAllowanceItem.TaxType.Value,
                Unit = d.InvoiceAllowanceItem.PieceUnit,
                UnitPrice = d.InvoiceAllowanceItem.UnitCost.HasValue ? d.InvoiceAllowanceItem.UnitCost.Value : 0
            }).ToArray();

            if (item.InvoiceAllowanceSeller != null)
            {
                Organization seller = item.InvoiceAllowanceSeller.Organization;
                result.Main.Seller = new ModelCore.Schema.TurnKey.B0101.MainSeller
                {
                    Address = seller.Addr,
                    CustomerNumber = "",
                    EmailAddress = seller.ContactEmail,
                    FacsimileNumber = seller.Fax,
                    Identifier = seller.ReceiptNo,
                    Name = seller.CompanyName,
                    //PersonInCharge = seller.UndertakerName,
                    //RoleRemark = "",
                    TelephoneNumber = seller.Phone
                };
            }
            else
            {
                result.Main.Seller = new ModelCore.Schema.TurnKey.B0101.MainSeller
                {
                    Identifier = item.SellerId
                };
            }

            if (item.InvoiceAllowanceBuyer != null)
            {
                Organization buyerOrg = item.InvoiceAllowanceBuyer.Organization;
                if (buyerOrg != null)
                {
                    result.Main.Buyer = new ModelCore.Schema.TurnKey.B0101.MainBuyer
                    {
                        Address = buyerOrg.Addr,
                        CustomerNumber = "",
                        EmailAddress = buyerOrg.ContactEmail,
                        FacsimileNumber = buyerOrg.Fax,
                        Identifier = buyerOrg.ReceiptNo,
                        Name = buyerOrg.CompanyName,
                        //PersonInCharge = buyerOrg.UndertakerName,
                        //RoleRemark = "",
                        TelephoneNumber = buyerOrg.Phone
                    };
                }
                else
                {
                    result.Main.Buyer = new ModelCore.Schema.TurnKey.B0101.MainBuyer
                    {
                        Address = item.InvoiceAllowanceBuyer.Address,
                        CustomerNumber = "",
                        EmailAddress = item.InvoiceAllowanceBuyer.EMail,
                        FacsimileNumber = "",
                        Identifier = item.InvoiceAllowanceBuyer.ReceiptNo,
                        Name = item.InvoiceAllowanceBuyer.CustomerName,
                        //PersonInCharge = buyerOrg.UndertakerName,
                        //RoleRemark = "",
                        TelephoneNumber = item.InvoiceAllowanceBuyer.Phone
                    };
                }
            }
            else
            {
                result.Main.Buyer = new ModelCore.Schema.TurnKey.B0101.MainBuyer
                {
                    Identifier = item.BuyerId
                };
            }
            return result;
        }

        public static ModelCore.Schema.TurnKey.A0201.CancelInvoice BuildA0201(this InvoiceItem item)
        {
            Organization seller = item.Organization;
            InvoiceCancellation cancelItem = item.InvoiceCancellation;

            if (cancelItem == null)
                return null;

            var result = new ModelCore.Schema.TurnKey.A0201.CancelInvoice
            {
                CancelDate = $"{cancelItem.CancelDate:yyyyMMdd}",
                BuyerId =  item.InvoiceBuyer.BuyerID.HasValue ? item.InvoiceBuyer.Organization.ReceiptNo : item.InvoiceBuyer.ReceiptNo,
                CancelInvoiceNumber = cancelItem.CancellationNo,
                CancelTime = cancelItem.CancelDate.Value,
                InvoiceDate = $"{item.InvoiceDate:yyyyMMdd}",
                Remark = cancelItem.Remark,
                ReturnTaxDocumentNumber = cancelItem.ReturnTaxDocumentNo,
                SellerId = seller.ReceiptNo
            };

            return result;
        }

        public static ModelCore.Schema.TurnKey.B0201.CancelAllowance BuildB0201(this InvoiceAllowance item)
        {
            InvoiceAllowanceCancellation cancelItem = item.InvoiceAllowanceCancellation;

            if (cancelItem == null)
                return null;

            var result = new ModelCore.Schema.TurnKey.B0201.CancelAllowance
            {
                AllowanceDate = $"{item.AllowanceDate:yyyyMMdd}",
                CancelDate = $"{cancelItem.CancelDate:yyyyMMdd}",
                CancelTime = cancelItem.CancelDate.Value,
                CancelAllowanceNumber = item.AllowanceNumber,
                Remark = cancelItem.Remark,
                BuyerId = item.BuyerId,
                SellerId = item.InvoiceAllowanceSeller.Organization.ReceiptNo
            };

            return result;
        }

        public static bool SignAndCheck(X509Certificate2 certificate, StringBuilder sb,Naming.CACatalogDefinition catalog,int docID,Naming.DocumentTypeDefinition typeID)
        {
            SignedCms signedCms;
            if (certificate != null)
            {
                ContentInfo content = new ContentInfo(Encoding.Default.GetBytes(sb.ToString()));
                signedCms = new SignedCms(content, false);
                CmsSigner signer = new CmsSigner(certificate);

                signedCms.ComputeSignature(signer, true);
            }
            else
            {
                signedCms = EIVOTurnkeyFactory.SignCms(sb.ToString());
                if (signedCms == null)
                    return false;
            }

            CryptoUtility crypto = new CryptoUtility();
            byte[] dataToSign;
            PKCS7Log log = crypto.PKCS7Log as PKCS7Log;
            if (log != null)
            {
                log.Crypto = crypto;
                log.Catalog = catalog;
                log.DocID = docID;
                log.TypeID = typeID;
            }

            return crypto.VerifyEnvelopedPKCS7(signedCms.Encode(), out dataToSign);
        }

        public static bool SignAndCheckToReceiveInvoiceItem(this InvoiceItem item, X509Certificate2 certificate, StringBuilder sb)
        {

            if (sb == null)
                sb = new StringBuilder();

            sb.Append("本發票由營業人指定由系統主動接收\r\n")
                .Append("營業人名稱:").Append(item.InvoiceBuyer.Organization.CompanyName).Append("\r\n")
                .Append("營業人統編:").Append(item.InvoiceBuyer.Organization.ReceiptNo).Append("\r\n")
                .Append("接收時間:").Append(DateTime.Now.ToString()).Append("\r\n")
                .Append("發票號碼:").Append(item.TrackCode).Append(item.No).Append("\r\n")
                .Append("發票日期:").Append(ValidityAgent.ConvertChineseDateString(item.InvoiceDate.Value)).Append("\r\n")
                .Append("發票開立人:").Append(item.InvoiceSeller.CustomerName).Append("\r\n");

            return SignAndCheck(certificate, sb,Naming.CACatalogDefinition.平台自動接收,item.InvoiceID,Naming.DocumentTypeDefinition.E_Invoice);
        }

        public static bool SignAndCheckToIssueInvoiceItem(this InvoiceItem item, X509Certificate2 certificate, StringBuilder sb)
        {

            if (sb == null)
                sb = new StringBuilder();

            sb.Append("本發票由營業人指定由系統主動開立\r\n")
                .Append("營業人名稱:").Append(item.InvoiceSeller.Organization.CompanyName).Append("\r\n")
                .Append("營業人統編:").Append(item.InvoiceSeller.Organization.ReceiptNo).Append("\r\n")
                .Append("開立時間:").Append(DateTime.Now.ToString()).Append("\r\n")
                .Append("發票號碼:").Append(item.TrackCode).Append(item.No).Append("\r\n")
                .Append("發票日期:").Append(ValidityAgent.ConvertChineseDateString(item.InvoiceDate.Value)).Append("\r\n")
                .Append("發票接收營業人:").Append(item.InvoiceBuyer.CustomerName).Append("\r\n");

            return SignAndCheck(certificate, sb, Naming.CACatalogDefinition.平台自動開立, item.InvoiceID, Naming.DocumentTypeDefinition.E_Invoice);

        }

        public static bool SignAndCheckToReceiveInvoiceAllowance(this InvoiceAllowance item, X509Certificate2 certificate, StringBuilder sb)
        {

            if (sb == null)
                sb = new StringBuilder();

            sb.Append("本發票折讓證明單由營業人指定由系統主動接收\r\n")
                .Append("營業人名稱:").Append(item.InvoiceAllowanceSeller.Organization.CompanyName).Append("\r\n")
                .Append("營業人統編:").Append(item.InvoiceAllowanceSeller.Organization.ReceiptNo).Append("\r\n")
                .Append("接收時間:").Append(DateTime.Now.ToString()).Append("\r\n")
                .Append("折讓單號碼:").Append(item.AllowanceNumber).Append("\r\n")
                .Append("折讓單日期:").Append(ValidityAgent.ConvertChineseDateString(item.AllowanceDate.Value)).Append("\r\n")
                .Append("折讓單開立人:").Append(item.InvoiceAllowanceBuyer.CustomerName).Append("\r\n");

            return SignAndCheck(certificate, sb, Naming.CACatalogDefinition.平台自動接收, item.AllowanceID, Naming.DocumentTypeDefinition.E_Allowance);

        }

        public static bool SignAndCheckToIssueInvoiceAllowance(this InvoiceAllowance item, X509Certificate2 certificate, StringBuilder sb)
        {
            if (sb == null)
                sb = new StringBuilder();

            sb.Append("本發票折讓證明單由營業人指定由系統主動開立\r\n")
                .Append("營業人名稱:").Append(item.InvoiceAllowanceBuyer.Organization.CompanyName).Append("\r\n")
                .Append("營業人統編:").Append(item.InvoiceAllowanceBuyer.Organization.ReceiptNo).Append("\r\n")
                .Append("開立時間:").Append(DateTime.Now.ToString()).Append("\r\n")
                .Append("折讓單號碼:").Append(item.AllowanceNumber).Append("\r\n")
                .Append("折讓單日期:").Append(ValidityAgent.ConvertChineseDateString(item.AllowanceDate.Value)).Append("\r\n")
                .Append("折讓單接收營業人:").Append(item.InvoiceAllowanceSeller.CustomerName).Append("\r\n");

            return SignAndCheck(certificate, sb, Naming.CACatalogDefinition.平台自動開立, item.AllowanceID, Naming.DocumentTypeDefinition.E_Allowance);
        }

        public static bool SignAndCheckToReceiveInvoiceCancellation(this InvoiceItem item, X509Certificate2 certificate, StringBuilder sb,int docID)
        {

            if (sb == null)
                sb = new StringBuilder();

            sb.Append("本作廢發票由營業人指定由系統主動接收\r\n")
                .Append("營業人名稱:").Append(item.InvoiceBuyer.Organization.CompanyName).Append("\r\n")
                .Append("營業人統編:").Append(item.InvoiceBuyer.Organization.ReceiptNo).Append("\r\n")
                .Append("接收時間:").Append(DateTime.Now.ToString()).Append("\r\n")
                .Append("作廢發票號碼:").Append(item.InvoiceCancellation.CancellationNo).Append("\r\n")
                .Append("作廢發票日期:").Append(ValidityAgent.ConvertChineseDateString(item.InvoiceCancellation.CancelDate.Value)).Append("\r\n")
                .Append("發票開立人:").Append(item.InvoiceSeller.CustomerName).Append("\r\n");

            return SignAndCheck(certificate, sb, Naming.CACatalogDefinition.平台自動接收, docID, Naming.DocumentTypeDefinition.E_InvoiceCancellation);
        }

        public static bool SignAndCheckToIssueInvoiceCancellation(this InvoiceItem item, X509Certificate2 certificate, StringBuilder sb,int docID)
        {

            if (sb == null)
                sb = new StringBuilder();

            sb.Append("本作廢發票由營業人指定由系統主動開立\r\n")
                .Append("營業人名稱:").Append(item.InvoiceSeller.Organization.CompanyName).Append("\r\n")
                .Append("營業人統編:").Append(item.InvoiceSeller.Organization.ReceiptNo).Append("\r\n")
                .Append("開立時間:").Append(DateTime.Now.ToString()).Append("\r\n")
                .Append("作廢發票號碼:").Append(item.InvoiceCancellation.CancellationNo).Append("\r\n")
                .Append("作廢發票日期:").Append(ValidityAgent.ConvertChineseDateString(item.InvoiceCancellation.CancelDate.Value)).Append("\r\n")
                .Append("發票接收營業人:").Append(item.InvoiceBuyer.CustomerName).Append("\r\n");

            return SignAndCheck(certificate, sb, Naming.CACatalogDefinition.平台自動開立, docID, Naming.DocumentTypeDefinition.E_InvoiceCancellation);

        }

        public static bool SignAndCheckToReceiveAllowanceCancellation(this InvoiceAllowance item, X509Certificate2 certificate, StringBuilder sb,int docID)
        {

            if (sb == null)
                sb = new StringBuilder();

            sb.Append("本作廢發票折讓證明單由營業人指定由系統主動接收\r\n")
                .Append("營業人名稱:").Append(item.InvoiceAllowanceSeller.Organization.CompanyName).Append("\r\n")
                .Append("營業人統編:").Append(item.InvoiceAllowanceSeller.Organization.ReceiptNo).Append("\r\n")
                .Append("接收時間:").Append(DateTime.Now.ToString()).Append("\r\n")
                .Append("折讓單號碼:").Append(item.AllowanceNumber).Append("\r\n")
                .Append("折讓單日期:").Append(ValidityAgent.ConvertChineseDateString(item.AllowanceDate.Value)).Append("\r\n")
                .Append("折讓單開立人:").Append(item.InvoiceAllowanceBuyer.CustomerName).Append("\r\n");

            return SignAndCheck(certificate, sb, Naming.CACatalogDefinition.平台自動接收, docID, Naming.DocumentTypeDefinition.E_AllowanceCancellation);

        }

        public static bool SignAndCheckToIssueAllowanceCancellation(this InvoiceAllowance item, X509Certificate2 certificate, StringBuilder sb,int docID)
        {
            if (sb == null)
                sb = new StringBuilder();

            sb.Append("本作廢發票折讓證明單由營業人指定由系統主動開立\r\n")
                .Append("營業人名稱:").Append(item.InvoiceAllowanceBuyer.Organization.CompanyName).Append("\r\n")
                .Append("營業人統編:").Append(item.InvoiceAllowanceBuyer.Organization.ReceiptNo).Append("\r\n")
                .Append("開立時間:").Append(DateTime.Now.ToString()).Append("\r\n")
                .Append("折讓單號碼:").Append(item.AllowanceNumber).Append("\r\n")
                .Append("折讓單日期:").Append(ValidityAgent.ConvertChineseDateString(item.AllowanceDate.Value)).Append("\r\n")
                .Append("折讓單接收營業人:").Append(item.InvoiceAllowanceSeller.CustomerName).Append("\r\n");

            return SignAndCheck(certificate, sb, Naming.CACatalogDefinition.平台自動開立, docID, Naming.DocumentTypeDefinition.E_AllowanceCancellation);
        }

        public static bool SignAndCheckToReceiveReceipt(this ReceiptItem item, X509Certificate2 certificate, StringBuilder sb)
        {

            if (sb == null)
                sb = new StringBuilder();

            sb.Append("本收據由營業人指定由系統主動接收\r\n")
                .Append("營業人名稱:").Append(item.Buyer.CompanyName).Append("\r\n")
                .Append("營業人統編:").Append(item.Buyer.ReceiptNo).Append("\r\n")
                .Append("接收時間:").Append(DateTime.Now.ToString()).Append("\r\n")
                .Append("收據號碼:").Append(item.No).Append("\r\n")
                .Append("收據開立日期:").Append(ValidityAgent.ConvertChineseDateString(item.ReceiptDate)).Append("\r\n")
                .Append("收據開立人:").Append(item.Seller.CompanyName).Append("\r\n");

            return SignAndCheck(certificate, sb, Naming.CACatalogDefinition.平台自動接收, item.ReceiptID, Naming.DocumentTypeDefinition.E_Receipt);

        }

        public static bool SignAndCheckToIssueReceipt(this ReceiptItem item, X509Certificate2 certificate, StringBuilder sb)
        {
            if (sb == null)
                sb = new StringBuilder();

            sb.Append("本收據由營業人指定由系統主動開立\r\n")
                .Append("營業人名稱:").Append(item.Seller.CompanyName).Append("\r\n")
                .Append("營業人統編:").Append(item.Seller.ReceiptNo).Append("\r\n")
                .Append("開立時間:").Append(DateTime.Now.ToString()).Append("\r\n")
                .Append("收據號碼:").Append(item.No).Append("\r\n")
                .Append("收據開立日期:").Append(ValidityAgent.ConvertChineseDateString(item.ReceiptDate)).Append("\r\n")
                .Append("收據接收營業人:").Append(item.Buyer.CompanyName).Append("\r\n");

            return SignAndCheck(certificate, sb, Naming.CACatalogDefinition.平台自動開立, item.ReceiptID, Naming.DocumentTypeDefinition.E_Receipt);
        }

        public static bool SignAndCheckToReceiveReceiptCancellation(this ReceiptItem item, X509Certificate2 certificate, StringBuilder sb,int docID)
        {

            if (sb == null)
                sb = new StringBuilder();

            sb.Append("本作廢收據由營業人指定由系統主動接收\r\n")
                .Append("營業人名稱:").Append(item.Buyer.CompanyName).Append("\r\n")
                .Append("營業人統編:").Append(item.Buyer.ReceiptNo).Append("\r\n")
                .Append("接收時間:").Append(DateTime.Now.ToString()).Append("\r\n")
                .Append("作廢收據號碼:").Append(item.ReceiptCancellation.CancellationNo).Append("\r\n")
                .Append("作廢收據開立日期:").Append(ValidityAgent.ConvertChineseDateString(item.ReceiptCancellation.CancelDate)).Append("\r\n")
                .Append("作廢收據開立人:").Append(item.Seller.CompanyName).Append("\r\n");

            return SignAndCheck(certificate, sb, Naming.CACatalogDefinition.平台自動接收, docID, Naming.DocumentTypeDefinition.E_ReceiptCancellation);

        }


        public static bool SignAndCheckToIssueReceiptCancellation(this ReceiptCancellation cancelItem, ReceiptItem item, X509Certificate2 certificate, StringBuilder sb,int docID)
        {
            if (sb == null)
                sb = new StringBuilder();

            sb.Append("本作廢收據由營業人指定由系統主動開立\r\n")
                .Append("營業人名稱:").Append(item.Seller.CompanyName).Append("\r\n")
                .Append("營業人統編:").Append(item.Seller.ReceiptNo).Append("\r\n")
                .Append("開立時間:").Append(DateTime.Now.ToString()).Append("\r\n")
                .Append("作廢收據號碼:").Append(cancelItem.CancellationNo).Append("\r\n")
                .Append("作廢收據開立日期:").Append(ValidityAgent.ConvertChineseDateString(cancelItem.CancelDate)).Append("\r\n")
                .Append("作廢收據接收營業人:").Append(item.Buyer.CompanyName).Append("\r\n");

            return SignAndCheck(certificate, sb, Naming.CACatalogDefinition.平台自動開立, docID, Naming.DocumentTypeDefinition.E_ReceiptCancellation);
        }

        public static void PushLogOnSubmit(this CDS_Document docItem, GenericManager<EIVOEntityDataContext> models, Naming.InvoiceStepDefinition stepID, Naming.DataProcessStatus status,String content = null)
        {
            models.GetTable<DataProcessLog>().InsertOnSubmit(
                new DataProcessLog
                {
                    CDS_Document = docItem,
                    LogDate = DateTime.Now,
                    StepID = (int)stepID,
                    Status = (int)status,
                    Content = content,
                });
        }

        public static String CheckB2CMIGName(this String name)
        {
            return name != null && Regex.IsMatch(name, "^[A-Za-z0-9]{4}$")
                                ? name != "0000"
                                    ? name 
                                    : "AAAA"
                                : ((int)4).CreateRandomStringCode();
        }

    }
}
