using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModelCore.Schema.EIVO;
using ModelCore.DataEntity;
using CommonLib.Utility;
using ModelCore.Locale;

using ModelCore.Properties;
using CommonLib.DataAccess;

namespace ModelCore.Helper
{
    public static class B2CExtensionMethods
    {
        public static decimal ToFix(this decimal decVal, int decimals = 2)
        {
            return Math.Round(decVal, decimals);
        }

        public static decimal ToFix(this decimal? decVal, int decimals = 2)
        {
            return decVal.HasValue ? Math.Round(decVal.Value, decimals) : 0;
        }

        public static Object CreateInvoiceMIG(this InvoiceItem item, bool withExtension = false)
        {
            if(ModelExtension.Properties.AppSettings.Default.UseMIG40)
            {
                return item.CreateF0401(withExtension);
            }
            else
            {
                return item.CreateC0401(withExtension);
            }
        }

        public static ModelCore.Schema.TurnKey.C0401.Invoice CreateC0401(this InvoiceItem item, bool withExtension = false)
        {
            var result = new ModelCore.Schema.TurnKey.C0401.Invoice
            {
                Main = new Schema.TurnKey.C0401.Main
                {
                    Buyer = new Schema.TurnKey.C0401.MainBuyer
                    {
                        //選擇性欄位不提供給大平台
                        //Address = item.InvoiceBuyer.Address.GetEfficientStringMaxSize(0,100).InsteadOfNullOrEmpty(""),
                        //CustomerNumber = item.InvoiceBuyer.CustomerNumber.GetEfficientStringMaxSize(0, 20).InsteadOfNullOrEmpty(""),
                        //EmailAddress = item.InvoiceBuyer.EMail.GetEfficientStringMaxSize(0, 80).InsteadOfNullOrEmpty(""),
                        //FacsimileNumber = item.InvoiceBuyer.Fax.GetEfficientStringMaxSize(0, 26).InsteadOfNullOrEmpty(""),
                        Identifier = item.InvoiceBuyer.ReceiptNo,
                        Name = item.InvoiceBuyer.IsB2C()
                            ? Encoding.GetEncoding(950).GetBytes(item.InvoiceBuyer.Name.InsteadOfNullOrEmpty("")).Length == 4
                                ? item.InvoiceBuyer.Name : ValidityAgent.GenerateRandomCode(4)
                            : String.IsNullOrEmpty(item.InvoiceBuyer.Name)
                                ? item.InvoiceBuyer.ReceiptNo : item.InvoiceBuyer.Name,
                        //PersonInCharge = item.InvoiceBuyer.PersonInCharge.GetEfficientStringMaxSize(0, 30).InsteadOfNullOrEmpty(""),
                        //RoleRemark = item.InvoiceBuyer.RoleRemark.GetEfficientStringMaxSize(0, 40).InsteadOfNullOrEmpty(""),
                        //TelephoneNumber = item.InvoiceBuyer.Phone.GetEfficientStringMaxSize(0, 26).InsteadOfNullOrEmpty("")
                    },
                    BuyerRemark = (ModelCore.Schema.TurnKey.C0401.BuyerRemarkEnum?)item.BuyerRemark,
                    BuyerRemarkSpecified = item.BuyerRemark.HasValue,
                    Category = item.Category,
                    CheckNumber = item.CheckNo,
                    CustomsClearanceMark = (ModelCore.Schema.TurnKey.C0401.CustomsClearanceMarkEnum?)item.CustomsClearanceMark,
                    CustomsClearanceMarkSpecified = item.CustomsClearanceMark.HasValue,
                    InvoiceType = (ModelCore.Schema.TurnKey.C0401.InvoiceTypeEnum?)((int?)item.InvoiceType) ?? Schema.TurnKey.C0401.InvoiceTypeEnum.Item07,
                    //DonateMark = (Schema.TurnKey.C0401.DonateMarkEnum)(int.Parse(item.DonateMark)),
                    DonateMark = string.IsNullOrEmpty(item.DonateMark) ? Schema.TurnKey.C0401.DonateMarkEnum.Item0 : (Schema.TurnKey.C0401.DonateMarkEnum)(int.Parse(item.DonateMark)),
                    CarrierType = item.InvoiceCarrier != null ? item.InvoiceCarrier.CarrierType : "",
                    //CarrierTypeSpecified = item.InvoiceCarrier != null ? true : false,
                    CarrierId1 = item.InvoiceCarrier != null ? item.InvoiceCarrier.CarrierNo : "",
                    CarrierId2 = item.InvoiceCarrier != null ? item.InvoiceCarrier.CarrierNo2 : "",
                    //PrintMark = item.CDS_Document.DocumentPrintLogs.Any(l => l.TypeID == (int)Model.Locale.Naming.DocumentTypeDefinition.E_Invoice) ? "Y"  : "N"
                    PrintMark = item.PrintMark,
                    NPOBAN = item.InvoiceDonation != null ? item.InvoiceDonation.AgencyCode : "",
                    RandomNumber = item.RandomNo,
                    GroupMark = item.GroupMark,
                    InvoiceDate = String.Format("{0:yyyyMMdd}", item.InvoiceDate.Value),
                    InvoiceTime = item.InvoiceDate.Value,
                    //InvoiceTimeSpecified = false,
                    InvoiceNumber = String.Format("{0}{1}", item.TrackCode, item.No),
                    MainRemark = item.Remark.GetEfficientStringMaxSize(0, 200),
                    //PermitNumber = item.PermitNumber,
                    //PermitDate = item.PermitDate.HasValue ? String.Format("{0:yyyyMMdd}", item.PermitDate.Value) : null,
                    //PermitWord = item.PermitWord,
                    RelateNumber = item.RelateNumber,
                    //TaxCenter = item.TaxCenter,
                    Seller = new Schema.TurnKey.C0401.MainSeller
                    {
                        //選擇性欄位不提供給大平台
                        Address = item.InvoiceSeller.Address.GetEfficientStringMaxSize(0, 100).InsteadOfNullOrEmpty(""),
                        //CustomerNumber = item.InvoiceSeller.CustomerID.GetEfficientStringMaxSize(0, 20).InsteadOfNullOrEmpty(""),
                        //EmailAddress = item.InvoiceSeller.EMail.GetEfficientStringMaxSize(0, 80).InsteadOfNullOrEmpty(""),
                        FacsimileNumber = item.InvoiceSeller.Fax.GetEfficientStringMaxSize(0, 26).InsteadOfNullOrEmpty(""),
                        Identifier = item.InvoiceSeller.ReceiptNo,
                        Name = item.InvoiceSeller.CustomerName.GetEfficientStringMaxSize(0, 60).InsteadOfNullOrEmpty(""),
                        PersonInCharge = item.Organization.UndertakerName.GetEfficientStringMaxSize(0, 30).InsteadOfNullOrEmpty(""),
                        //RoleRemark = item.InvoiceSeller.RoleRemark.GetEfficientStringMaxSize(0, 40).InsteadOfNullOrEmpty(""),
                        TelephoneNumber = item.InvoiceSeller.Phone.GetEfficientStringMaxSize(0, 26).InsteadOfNullOrEmpty("")
                    },
                },
                Details = buildC0401Details(item),
                Amount = new Schema.TurnKey.C0401.Amount
                {
                    CurrencySpecified = false,
                    DiscountAmount = item.InvoiceAmountType.DiscountAmount.HasValue ? item.InvoiceAmountType.DiscountAmount.Value.ToFix(item.InvoiceAmountType.CurrencyType?.Decimals ?? 0) : 0,
                    DiscountAmountSpecified = item.InvoiceAmountType.DiscountAmount.HasValue,
                    ExchangeRateSpecified = false,
                    OriginalCurrencyAmountSpecified = false,
                    SalesAmount = item.InvoiceBuyer.IsB2C() && item.InvoiceAmountType.TaxType == (byte)Naming.TaxTypeDefinition.應稅 ? item.InvoiceAmountType.TotalAmount.ToFix(item.InvoiceAmountType.CurrencyType?.Decimals ?? 0) : item.InvoiceAmountType.SalesAmount.ToFix(item.InvoiceAmountType.CurrencyType?.Decimals ?? 0),
                    FreeTaxSalesAmount = item.InvoiceAmountType.FreeTaxSalesAmount.ToFix(item.InvoiceAmountType.CurrencyType?.Decimals ?? 0),
                    ZeroTaxSalesAmount = item.InvoiceAmountType.ZeroTaxSalesAmount.ToFix(item.InvoiceAmountType.CurrencyType?.Decimals ?? 0),
                    TaxAmount = item.InvoiceBuyer.IsB2C() ? 0 : item.InvoiceAmountType.TaxAmount.ToFix(item.InvoiceAmountType.CurrencyType?.Decimals ?? 0),
                    TaxRate = item.InvoiceAmountType.TaxRate.HasValue ? item.InvoiceAmountType.TaxRate.ToFix(2) : 0.05m,
                    TaxType = (Schema.TurnKey.C0401.TaxTypeEnum)((int)item.InvoiceAmountType.TaxType.Value),
                    TotalAmount = item.InvoiceAmountType.TotalAmount.ToFix(item.InvoiceAmountType.CurrencyType?.Decimals ?? 0)
                }
            };
            if (item.InvoiceAmountType.CurrencyID.HasValue)
            {
                result.Amount.CurrencySpecified = true;
                result.Amount.Currency = (Schema.TurnKey.C0401.CurrencyCodeEnum)Enum.Parse(typeof(Schema.TurnKey.C0401.CurrencyCodeEnum), item.InvoiceAmountType.CurrencyType.AbbrevName);
            }

            if (withExtension)
            {
                result.Main.DataNumber = item.InvoicePurchaseOrder?.OrderNo;
                result.TxnCode = Naming.GovTurnkeyTransaction.I.ToString();
                if (item.CDS_Document.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.MIG_C))
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.C.ToString();
                }
                else if (item.CDS_Document.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.MIG_E))
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.E.ToString();
                }
                else if (item.CDS_Document.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.已開立))
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.P.ToString();
                }

                //try
                //{
                //    using (TurnKey2DataContext turnkeyDB = new TurnKey2DataContext())
                //    {
                //        var log = turnkeyDB.GetTable<V_Invoice>()
                //                .Where(i => i.InvoiceNo == result.Main.InvoiceNumber)
                //                .Where(i => i.DocType == "C0401" || i.DocType == "A0401")
                //                .ToList()
                //                .OrderByDescending(i => i.MESSAGE_DTS).FirstOrDefault();
                //        if (log != null)
                //        {
                //            result.TxnCode = log.STATUS == "C"
                //                    ? Naming.GovTurnkeyTransaction.C.ToString()
                //                    : Naming.GovTurnkeyTransaction.E.ToString();
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Logger.Error(ex);
                //}
            }
            return result;
        }

        private static Schema.TurnKey.C0401.DetailsProductItem[] buildC0401Details(InvoiceItem item)
        {
            List<ModelCore.Schema.TurnKey.C0401.DetailsProductItem> items = new List<Schema.TurnKey.C0401.DetailsProductItem>();
            foreach (var detailItem in item.InvoiceDetails)
            {
                detailItem.InvoiceProduct.InvoiceProductItem.ToList();
                foreach (var productItem in detailItem.InvoiceProduct.InvoiceProductItem)
                {
                    items.Add(new ModelCore.Schema.TurnKey.C0401.DetailsProductItem
                    {
                        Amount = productItem.CostAmount ?? 0m,   //productItem.CostAmount.HasValue ? productItem.CostAmount.Value.ToFix(item.InvoiceAmountType.CurrencyType?.Decimals ?? 0) : 0m,
                        Description = detailItem.InvoiceProduct.Brief,
                        Quantity = productItem.Piece ?? 0m,  //productItem.Piece.HasValue ? productItem.Piece.Value.ToFix(0) : 0,
                        RelateNumber = productItem.RelateNumber,
                        Remark = productItem.Remark.GetEfficientStringMaxSize(0, 40),
                        SequenceNumber = String.Format("{0:00}", productItem.No),
                        Unit = productItem.PieceUnit?.Trim(),
                        UnitPrice = productItem.UnitCost ?? 0m, //productItem.UnitCost.HasValue ? productItem.UnitCost.Value.ToFix(item.InvoiceAmountType.CurrencyType?.Decimals ?? 0) : 0,
                    });
                }
            }
            return items.ToArray();
        }

        public static Object CreateInvoiceCancellationMIG(this InvoiceItem item, bool withExtension = false)
        {
            if (ModelExtension.Properties.AppSettings.Default.UseMIG40)
            {
                return item.CreateF0501(withExtension);
            }
            else
            {
                return item.CreateC0501(withExtension);
            }
        }

        public static ModelCore.Schema.TurnKey.C0501.CancelInvoice CreateC0501(this InvoiceItem item, bool withExtension = false)
        {
            InvoiceCancellation cancellation = item.InvoiceCancellation;
            if (cancellation == null)
            {
                return null;
            }

            var result = new ModelCore.Schema.TurnKey.C0501.CancelInvoice
            {
                CancelInvoiceNumber = cancellation.CancellationNo,
                InvoiceDate = String.Format("{0:yyyyMMdd}", item.InvoiceDate.Value),
                BuyerId = item.InvoiceBuyer.ReceiptNo,
                SellerId = item.Organization.ReceiptNo,
                CancelDate = String.Format("{0:yyyyMMdd}", cancellation.CancelDate.Value),
                CancelTime = cancellation.CancelDate.Value,
                CancelReason = cancellation.CancelReason.GetEfficientStringMaxSize(0, 20),
                ReturnTaxDocumentNumber = cancellation.ReturnTaxDocumentNo,
                Remark = cancellation.Remark,
            };

            if (withExtension)
            {
                result.TxnCode = Naming.GovTurnkeyTransaction.I.ToString();
                if (item.CDS_Document.ChildDocument.FirstOrDefault()?.CDS_Document?.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.MIG_C) == true)
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.C.ToString();
                }
                else if (item.CDS_Document.ChildDocument.FirstOrDefault()?.CDS_Document?.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.MIG_E) == true)
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.E.ToString();
                }
                else if (item.CDS_Document.ChildDocument.FirstOrDefault()?.CDS_Document?.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.已開立) == true)
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.P.ToString();
                }
                //try
                //{
                //    using (TurnKey2DataContext turnkeyDB = new TurnKey2DataContext())
                //    {
                //        var log = turnkeyDB.GetTable<V_Invoice>()
                //                .Where(i => i.InvoiceNo == result.CancelInvoiceNumber)
                //                .Where(i => i.DocType == "C0501" || i.DocType == "A0501")
                //                .ToList()
                //                .OrderByDescending(i => i.MESSAGE_DTS).FirstOrDefault();
                //        if (log != null)
                //        {
                //            result.TxnCode = log.STATUS == "C"
                //                    ? Naming.GovTurnkeyTransaction.C.ToString()
                //                    : Naming.GovTurnkeyTransaction.E.ToString();
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Logger.Error(ex);
                //}
            }

            return result;
        }

        public static Object CreateAllowanceMIG(this InvoiceAllowance item, GenericManager<EIVOEntityDataContext> models = null, bool withExtension = false)
        {
            if (ModelExtension.Properties.AppSettings.Default.UseMIG40)
            {
                return item.CreateG0401(models, withExtension);
            }
            else
            {
                return item.CreateD0401(models, withExtension);
            }
        }

        public static ModelCore.Schema.TurnKey.D0401.Allowance CreateD0401(this InvoiceAllowance item, GenericManager<EIVOEntityDataContext> models = null, bool withExtension = false)
        {
            //bool isCBM = models.IsCrossBorderMerchant(item.InvoiceAllowanceSeller.SellerID);

            var result = new Schema.TurnKey.D0401.Allowance
            {
                Main = new Schema.TurnKey.D0401.Main
                {
                    AllowanceNumber = item.TurnkeyAllowanceNo,
                    AllowanceDate = String.Format("{0:yyyyMMdd}", item.AllowanceDate),
                    AllowanceType = (Schema.TurnKey.D0401.AllowanceTypeEnum)((int)item.AllowanceType),
                    Buyer = new Schema.TurnKey.D0401.MainBuyer
                    {
                        //Address = string.IsNullOrEmpty(item.InvoiceAllowanceBuyer.Address) ?
                        //"" :
                        // item.InvoiceAllowanceBuyer.Address.Length > 100 ?
                        //  item.InvoiceAllowanceBuyer.Address.Substring(0, 100) :
                        //   item.InvoiceAllowanceBuyer.Address,
                        ////CustomerNumber = item.InvoiceAllowanceBuyer.CustomerName,
                        //EmailAddress = "",//item.InvoiceAllowanceBuyer.EMail,
                        //FacsimileNumber = String.IsNullOrEmpty(item.InvoiceAllowanceBuyer.Fax) ?
                        //"" :
                        // item.InvoiceAllowanceBuyer.Fax.Length > 26 ?
                        //  item.InvoiceAllowanceBuyer.Fax.Substring(0, 26) :
                        //   item.InvoiceAllowanceBuyer.Fax,
                        Identifier = item.InvoiceAllowanceBuyer.ReceiptNo,
                        Name = item.InvoiceAllowanceBuyer.IsB2C()
                            ? Encoding.GetEncoding(950).GetBytes(item.InvoiceAllowanceBuyer.Name.InsteadOfNullOrEmpty("")).Length == 4
                                ? item.InvoiceAllowanceBuyer.Name : ValidityAgent.GenerateRandomCode(4)
                            : String.IsNullOrEmpty(item.InvoiceAllowanceBuyer.Name)
                                ? item.InvoiceAllowanceBuyer.ReceiptNo : item.InvoiceAllowanceBuyer.Name,
                        //PersonInCharge = String.IsNullOrEmpty(item.InvoiceAllowanceBuyer.PersonInCharge) ?
                        //"" :
                        //item.InvoiceAllowanceBuyer.PersonInCharge.Length > 30 ?
                        //item.InvoiceAllowanceBuyer.PersonInCharge.Substring(0, 30) :
                        //item.InvoiceAllowanceBuyer.PersonInCharge,
                        //RoleRemark = item.InvoiceAllowanceBuyer.RoleRemark,
                        //TelephoneNumber = "",//item.InvoiceAllowanceBuyer.Phone,
                    },
                    Seller = new Schema.TurnKey.D0401.MainSeller
                    {
                        //Address = String.IsNullOrEmpty(item.InvoiceAllowanceSeller.Address) ?
                        //"" :
                        // item.InvoiceAllowanceSeller.Address.Length > 100 ?
                        //  item.InvoiceAllowanceSeller.Address.Substring(0, 100) :
                        //   item.InvoiceAllowanceSeller.Address,
                        ////CustomerNumber = item.InvoiceAllowanceSeller.CustomerName,
                        //EmailAddress = "",//item.InvoiceAllowanceSeller.EMail,
                        //FacsimileNumber = String.IsNullOrEmpty(item.InvoiceAllowanceSeller.Fax) ?
                        //"" :
                        //item.InvoiceAllowanceSeller.Fax.Length > 26 ?
                        //item.InvoiceAllowanceSeller.Fax.Substring(0, 26) :
                        //item.InvoiceAllowanceSeller.Fax,
                        Identifier = item.InvoiceAllowanceSeller.ReceiptNo,
                        Name = item.InvoiceAllowanceSeller.Name,
                        //PersonInCharge = String.IsNullOrEmpty(item.InvoiceAllowanceSeller.PersonInCharge) ?
                        //"" :
                        // item.InvoiceAllowanceSeller.PersonInCharge.Length > 30 ?
                        //  item.InvoiceAllowanceSeller.PersonInCharge.Substring(0, 30) :
                        //   item.InvoiceAllowanceSeller.PersonInCharge,
                        //RoleRemark = item.InvoiceAllowanceSeller.RoleRemark,
                        //TelephoneNumber = "",//item.InvoiceAllowanceSeller.Phone,
                    },
                },
                Amount = /*isCBM
                            ? new Schema.TurnKey.D0401.Amount
                            {
                                TaxAmount = 0,
                                TotalAmount = ((item.TotalAmount ?? 0) + (item.TaxAmount ?? 0)).ToFix(item.CurrencyType?.Decimals ?? 0),
                            }
                            : */
                            new Schema.TurnKey.D0401.Amount
                            {
                                TaxAmount = item.TaxAmount.HasValue
                                            ? item.TaxAmount.Value.ToFix(item.CurrencyType?.Decimals ?? 0)
                                            : 0,
                                TotalAmount = item.TotalAmount.HasValue ? item.TotalAmount.Value.ToFix(item.CurrencyType?.Decimals ?? 0) : 0,
                            },
            };

            int seqNo = 1;
            result.Details = item.InvoiceAllowanceDetails.Select(d => new Schema.TurnKey.D0401.DetailsProductItem
            {
                AllowanceSequenceNumber = (seqNo++).ToString(), //d.InvoiceAllowanceItem.No.ToString(),
                Amount = d.InvoiceAllowanceItem.Amount.HasValue ? d.InvoiceAllowanceItem.Amount.Value.ToFix(item.CurrencyType?.Decimals ?? 0) : 0m,
                OriginalDescription = d.InvoiceAllowanceItem.OriginalDescription,
                OriginalInvoiceDate = String.Format("{0:yyyyMMdd}", d.InvoiceAllowanceItem.InvoiceDate),
                OriginalInvoiceNumber = d.InvoiceAllowanceItem.InvoiceNo,
                OriginalSequenceNumber = d.InvoiceAllowanceItem.OriginalSequenceNo?.ToString() ?? "1",
                Quantity = (int)(d.InvoiceAllowanceItem.Piece ?? 0M),
                Tax = (int)(d.InvoiceAllowanceItem.Tax ?? 0),
                TaxType = (Schema.TurnKey.D0401.TaxTypeEnum?)d.InvoiceAllowanceItem.TaxType ?? Schema.TurnKey.D0401.TaxTypeEnum.Item1,
                Unit = d.InvoiceAllowanceItem.PieceUnit,
                UnitPrice = d.InvoiceAllowanceItem.UnitCost ?? 0,
            }).ToArray();

            if (withExtension)
            {
                foreach (var d in result.Details)
                {
                    String trackCode = d.OriginalInvoiceNumber.Substring(0, 2);
                    String no = d.OriginalInvoiceNumber.Substring(2);

                    var invItem = models.GetTable<InvoiceItem>()
                        .Where(i => i.TrackCode == trackCode && i.No == no)
                        .Where(i => i.SellerID == item.InvoiceAllowanceSeller.SellerID)
                        .FirstOrDefault();

                    d.DataNumber = invItem?.InvoicePurchaseOrder?.OrderNo;
                }

                result.TxnCode = Naming.GovTurnkeyTransaction.I.ToString();
                if (item.CDS_Document.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.MIG_C) == true)
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.C.ToString();
                }
                else if (item.CDS_Document.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.MIG_E) == true)
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.E.ToString();
                }
                else if (item.CDS_Document.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.已開立) == true)
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.P.ToString();
                }

                //try
                    //{
                    //    using (TurnKey2DataContext turnkeyDB = new TurnKey2DataContext())
                    //    {
                    //        var log = turnkeyDB.GetTable<V_Allowance>()
                    //                .Where(i => i.AllowanceNo == result.Main.AllowanceNumber)
                    //                .Where(i => i.DocType == "D0401" || i.DocType == "B0401")
                    //                .ToList()
                    //                .OrderByDescending(i => i.MESSAGE_DTS).FirstOrDefault();
                    //        if (log != null)
                    //        {
                    //            result.TxnCode = log.STATUS == "C"
                    //                    ? Naming.GovTurnkeyTransaction.C.ToString()
                    //                    : Naming.GovTurnkeyTransaction.E.ToString();
                    //        }
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    Logger.Error(ex);
                    //}
            }

            return result;
        }

        public static Object CreateAllowanceCancellationMIG(this InvoiceAllowance item, bool withExtension = false)
        {
            if (ModelExtension.Properties.AppSettings.Default.UseMIG40)
            {
                return item.CreateG0501(withExtension);
            }
            else
            {
                return item.CreateD0501(withExtension);
            }
        }

        public static ModelCore.Schema.TurnKey.D0501.CancelAllowance CreateD0501(this InvoiceAllowance item, bool withExtension = false)
        {
            InvoiceAllowanceCancellation cancelledItem = item.InvoiceAllowanceCancellation;
            if (cancelledItem == null)
            {
                return null;
            }

            var result = new ModelCore.Schema.TurnKey.D0501.CancelAllowance
            {
                CancelAllowanceNumber = item.TurnkeyAllowanceNo,
                AllowanceDate = String.Format("{0:yyyyMMdd}", item.AllowanceDate.Value),
                BuyerId = item.BuyerId,
                SellerId = item.SellerId,
                CancelDate = String.Format("{0:yyyyMMdd}", cancelledItem.CancelDate.Value),
                CancelTime = cancelledItem.CancelDate.Value,
                CancelReason = cancelledItem.CancelReason,
                Remark = cancelledItem.Remark,
            };

            if (withExtension)
            {
                result.TxnCode = Naming.GovTurnkeyTransaction.I.ToString();
                if (item.CDS_Document.ChildDocument.FirstOrDefault()?.CDS_Document?.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.MIG_C) == true)
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.C.ToString();
                }
                else if (item.CDS_Document.ChildDocument.FirstOrDefault()?.CDS_Document?.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.MIG_E) == true)
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.E.ToString();
                }
                else if (item.CDS_Document.ChildDocument.FirstOrDefault()?.CDS_Document?.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.已開立) == true)
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.P.ToString();
                }
                //try
                //{
                //    using (TurnKey2DataContext turnkeyDB = new TurnKey2DataContext())
                //    {
                //        var log = turnkeyDB.GetTable<V_Allowance>()
                //                .Where(i => i.AllowanceNo == result.CancelAllowanceNumber)
                //                .Where(i => i.DocType == "D0501" || i.DocType == "B0501")
                //                .ToList()
                //                .OrderByDescending(i => i.MESSAGE_DTS).FirstOrDefault();
                //        if (log != null)
                //        {
                //            result.TxnCode = log.STATUS == "C"
                //                    ? Naming.GovTurnkeyTransaction.C.ToString()
                //                    : Naming.GovTurnkeyTransaction.E.ToString();
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Logger.Error(ex);
                //}

            }

            return result;
        }

        public static ModelCore.Schema.TurnKey.E0402.BranchTrackBlank BuildE0402(this InvoiceTrackCodeAssignment item)
        {
            var result = new ModelCore.Schema.TurnKey.E0402.BranchTrackBlank
            {
                Main = new Schema.TurnKey.E0402.Main
                {
                    HeadBan = item.Organization.ReceiptNo,
                    BranchBan = item.Organization.ReceiptNo,
                    InvoiceType = (Schema.TurnKey.E0402.InvoiceTypeEnum)item.Organization.OrganizationStatus.SettingInvoiceType.Value,
                    YearMonth = String.Format("{0:000}{1:00}", item.InvoiceTrackCode.Year - 1911, item.InvoiceTrackCode.PeriodNo * 2),
                    InvoiceTrack = item.InvoiceTrackCode.TrackCode
                },
                Details = buildE0402Details(item)
            };
            return result;
        }

        private static Schema.TurnKey.E0402.DetailsBranchTrackBlankItem[] buildE0402Details(InvoiceTrackCodeAssignment item)
        {
            List<ModelCore.Schema.TurnKey.E0402.DetailsBranchTrackBlankItem> items = new List<Schema.TurnKey.E0402.DetailsBranchTrackBlankItem>();

            foreach (var detail in item.UnassignedInvoiceNo)
            {
                items.Add(new ModelCore.Schema.TurnKey.E0402.DetailsBranchTrackBlankItem
                {
                    InvoiceBeginNo = String.Format("{0:00000000}", detail.InvoiceBeginNo),
                    InvoiceEndNo = String.Format("{0:00000000}", detail.InvoiceEndNo)
                }
                );
            }
            return items.ToArray();
        }

        public static Object CreateVoidInvoiceMIG(this InvoiceItem item)
        {
            if (ModelExtension.Properties.AppSettings.Default.UseMIG40)
            {
                return item.CreateF0701();
            }
            else
            {
                return item.CreateC0701();
            }
        }

        public static ModelCore.Schema.TurnKey.C0701.VoidInvoice CreateC0701(this InvoiceItem item)
        {
            return new ModelCore.Schema.TurnKey.C0701.VoidInvoice
            {
                VoidInvoiceNumber = item.TrackCode + item.No,
                InvoiceDate = String.Format("{0:yyyyMMdd}", item.InvoiceDate),
                BuyerId = item.InvoiceBuyer.ReceiptNo,
                SellerId = item.InvoiceSeller.ReceiptNo,
                VoidDate = DateTime.Now.Date.ToString("yyyyMMdd"),
                VoidTime = DateTime.Now,
                VoidReason = "註銷重開",
                Remark = ""
            };
        }

        public static ModelCore.Schema.TurnKey.E0402.BranchTrackBlank BuildE0402(this InvoiceTrackCodeAssignment item, IQueryable<UnassignedInvoiceNoSummary> Summary)
        {
            var result = new ModelCore.Schema.TurnKey.E0402.BranchTrackBlank
            {
                Main = new Schema.TurnKey.E0402.Main
                {
                    HeadBan = item.Organization.ReceiptNo,
                    BranchBan = item.Organization.ReceiptNo,
                    InvoiceType = (Schema.TurnKey.E0402.InvoiceTypeEnum)(item.Organization.InvoiceItems.Where(i => i.InvoiceDate >= Convert.ToDateTime(String.Format("{0}/{1}/1", item.InvoiceTrackCode.Year, item.InvoiceTrackCode.PeriodNo * 2 - 1))).FirstOrDefault().InvoiceType),
                    YearMonth = String.Format("{0:000}{1:00}", item.InvoiceTrackCode.Year - 1911, item.InvoiceTrackCode.PeriodNo * 2),
                    InvoiceTrack = item.InvoiceTrackCode.TrackCode
                },
                Details = buildE0402Details(item, Summary)
            };
            return result;
        }

        private static Schema.TurnKey.E0402.DetailsBranchTrackBlankItem[] buildE0402Details(InvoiceTrackCodeAssignment item, IQueryable<UnassignedInvoiceNoSummary> Summary)
        {
            List<ModelCore.Schema.TurnKey.E0402.DetailsBranchTrackBlankItem> items = new List<Schema.TurnKey.E0402.DetailsBranchTrackBlankItem>();

            foreach (var detail in Summary.ToList())
            {
                items.Add(new ModelCore.Schema.TurnKey.E0402.DetailsBranchTrackBlankItem
                {
                    InvoiceBeginNo = String.Format("{0:00000000}", detail.StartNo),
                    InvoiceEndNo = String.Format("{0:00000000}", detail.EndNo)
                }
                );
            }
            return items.ToArray();
        }

        public static ModelCore.Schema.TurnKey.F0401.Invoice CreateF0401(this InvoiceItem item, bool withExtension = false)
        {
            var result = new ModelCore.Schema.TurnKey.F0401.Invoice
            {
                Main = new Schema.TurnKey.F0401.Main
                {
                    Buyer = new Schema.TurnKey.F0401.MainBuyer
                    {
                        //選擇性欄位不提供給大平台
                        //Address = item.InvoiceBuyer.Address.GetEfficientStringMaxSize(0,100).InsteadOfNullOrEmpty(""),
                        //CustomerNumber = item.InvoiceBuyer.CustomerNumber.GetEfficientStringMaxSize(0, 20).InsteadOfNullOrEmpty(""),
                        //EmailAddress = item.InvoiceBuyer.EMail.GetEfficientStringMaxSize(0, 80).InsteadOfNullOrEmpty(""),
                        //FacsimileNumber = item.InvoiceBuyer.Fax.GetEfficientStringMaxSize(0, 26).InsteadOfNullOrEmpty(""),
                        Identifier = item.InvoiceBuyer.ReceiptNo,
                        Name = item.InvoiceBuyer.IsB2C()
                            ? Encoding.GetEncoding(950).GetBytes(item.InvoiceBuyer.Name.InsteadOfNullOrEmpty("")).Length == 4
                                ? item.InvoiceBuyer.Name : ValidityAgent.GenerateRandomCode(4)
                            : String.IsNullOrEmpty(item.InvoiceBuyer.Name)
                                ? item.InvoiceBuyer.ReceiptNo : item.InvoiceBuyer.Name,
                        //PersonInCharge = item.InvoiceBuyer.PersonInCharge.GetEfficientStringMaxSize(0, 30).InsteadOfNullOrEmpty(""),
                        //RoleRemark = item.InvoiceBuyer.RoleRemark.GetEfficientStringMaxSize(0, 40).InsteadOfNullOrEmpty(""),
                        //TelephoneNumber = item.InvoiceBuyer.Phone.GetEfficientStringMaxSize(0, 26).InsteadOfNullOrEmpty("")
                    },
                    BuyerRemark = (ModelCore.Schema.TurnKey.F0401.BuyerRemarkEnum?)item.BuyerRemark,
                    BuyerRemarkSpecified = item.BuyerRemark.HasValue,
                    Category = item.Category,
                    //CheckNumber = item.CheckNo,
                    BondedAreaConfirm = item.BondedAreaConfirm?.ToString(),
                    CustomsClearanceMark = (ModelCore.Schema.TurnKey.F0401.CustomsClearanceMarkEnum?)item.CustomsClearanceMark,
                    CustomsClearanceMarkSpecified = item.CustomsClearanceMark.HasValue,
                    InvoiceType = (ModelCore.Schema.TurnKey.F0401.InvoiceTypeEnum?)((int?)item.InvoiceType) ?? Schema.TurnKey.F0401.InvoiceTypeEnum.Item07,
                    //DonateMark = (Schema.TurnKey.C0401.DonateMarkEnum)(int.Parse(item.DonateMark)),
                    DonateMark = string.IsNullOrEmpty(item.DonateMark) ? Schema.TurnKey.F0401.DonateMarkEnum.Item0 : (Schema.TurnKey.F0401.DonateMarkEnum)(int.Parse(item.DonateMark)),
                    CarrierType = item.InvoiceCarrier != null ? item.InvoiceCarrier.CarrierType : "",
                    //CarrierTypeSpecified = item.InvoiceCarrier != null ? true : false,
                    CarrierId1 = item.InvoiceCarrier != null ? item.InvoiceCarrier.CarrierNo : "",
                    CarrierId2 = item.InvoiceCarrier != null ? item.InvoiceCarrier.CarrierNo2 : "",
                    //PrintMark = item.CDS_Document.DocumentPrintLogs.Any(l => l.TypeID == (int)Model.Locale.Naming.DocumentTypeDefinition.E_Invoice) ? "Y"  : "N"
                    PrintMark = item.PrintMark,
                    NPOBAN = item.InvoiceDonation != null ? item.InvoiceDonation.AgencyCode : "",
                    RandomNumber = item.RandomNo,
                    GroupMark = item.GroupMark,
                    InvoiceDate = String.Format("{0:yyyyMMdd}", item.InvoiceDate.Value),
                    InvoiceTime = item.InvoiceDate.Value,
                    //InvoiceTimeSpecified = false,
                    InvoiceNumber = String.Format("{0}{1}", item.TrackCode, item.No),
                    MainRemark = item.Remark.GetEfficientStringMaxSize(0, 200),
                    //PermitNumber = item.PermitNumber,
                    //PermitDate = item.PermitDate.HasValue ? String.Format("{0:yyyyMMdd}", item.PermitDate.Value) : null,
                    //PermitWord = item.PermitWord,
                    RelateNumber = item.RelateNumber,
                    //TaxCenter = item.TaxCenter,
                    Seller = new Schema.TurnKey.F0401.MainSeller
                    {
                        //選擇性欄位不提供給大平台
                        Address = item.InvoiceSeller.Address.GetEfficientStringMaxSize(0, 100).InsteadOfNullOrEmpty(""),
                        //CustomerNumber = item.InvoiceSeller.CustomerID.GetEfficientStringMaxSize(0, 20).InsteadOfNullOrEmpty(""),
                        //EmailAddress = item.InvoiceSeller.EMail.GetEfficientStringMaxSize(0, 80).InsteadOfNullOrEmpty(""),
                        FacsimileNumber = item.InvoiceSeller.Fax.GetEfficientStringMaxSize(0, 26).InsteadOfNullOrEmpty(""),
                        Identifier = item.InvoiceSeller.ReceiptNo,
                        Name = item.InvoiceSeller.CustomerName.GetEfficientStringMaxSize(0, 60).InsteadOfNullOrEmpty(""),
                        PersonInCharge = item.Organization.UndertakerName.GetEfficientStringMaxSize(0, 30).InsteadOfNullOrEmpty(""),
                        //RoleRemark = item.InvoiceSeller.RoleRemark.GetEfficientStringMaxSize(0, 40).InsteadOfNullOrEmpty(""),
                        TelephoneNumber = item.InvoiceSeller.Phone.GetEfficientStringMaxSize(0, 26).InsteadOfNullOrEmpty("")
                    },
                },
                Details = buildF0401Details(item),
                Amount = new Schema.TurnKey.F0401.Amount
                {
                    CurrencySpecified = false,
                    DiscountAmount = item.InvoiceAmountType.DiscountAmount.HasValue ? item.InvoiceAmountType.DiscountAmount.Value.ToFix(item.InvoiceAmountType.CurrencyType?.Decimals ?? 0) : 0,
                    DiscountAmountSpecified = item.InvoiceAmountType.DiscountAmount.HasValue,
                    ExchangeRateSpecified = false,
                    OriginalCurrencyAmountSpecified = false,
                    SalesAmount = item.InvoiceBuyer.IsB2C() && item.InvoiceAmountType.TaxType == (byte)Naming.TaxTypeDefinition.應稅
                                    ? item.InvoiceAmountType.TotalAmount.ToFix(item.InvoiceAmountType.CurrencyType?.Decimals ?? 0)
                                    : item.InvoiceAmountType.SalesAmount.ToFix(item.InvoiceAmountType.CurrencyType?.Decimals ?? 0),
                    FreeTaxSalesAmount = item.InvoiceAmountType.FreeTaxSalesAmount.ToFix(item.InvoiceAmountType.CurrencyType?.Decimals ?? 0),
                    ZeroTaxSalesAmount = item.InvoiceAmountType.ZeroTaxSalesAmount.ToFix(item.InvoiceAmountType.CurrencyType?.Decimals ?? 0),
                    TaxAmount = item.InvoiceBuyer.IsB2C() ? 0 : item.InvoiceAmountType.TaxAmount.ToFix(0 /*item.InvoiceAmountType.CurrencyType?.Decimals ?? 0*/),
                    TaxRate = item.InvoiceAmountType.TaxRate.HasValue ? item.InvoiceAmountType.TaxRate.ToFix(2) : 0.05m,
                    TaxType = (Schema.TurnKey.F0401.TaxTypeEnum)((int)item.InvoiceAmountType.TaxType.Value),
                    TotalAmount = item.InvoiceAmountType.TotalAmount.ToFix(item.InvoiceAmountType.CurrencyType?.Decimals ?? 0)
                },
            };
            if (item.InvoiceAmountType.CurrencyID.HasValue)
            {
                result.Amount.CurrencySpecified = true;
                result.Amount.Currency = (Schema.TurnKey.F0401.CurrencyCodeEnum)Enum.Parse(typeof(Schema.TurnKey.C0401.CurrencyCodeEnum), item.InvoiceAmountType.CurrencyType.AbbrevName);
            }

            if (withExtension)
            {
                result.Main.DataNumber = item.InvoicePurchaseOrder?.OrderNo;
                result.TxnCode = Naming.GovTurnkeyTransaction.I.ToString();
                if (item.CDS_Document.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.MIG_C))
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.C.ToString();
                }
                else if (item.CDS_Document.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.MIG_E))
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.E.ToString();
                }
                else if (item.CDS_Document.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.已開立))
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.P.ToString();
                }
                //try
                //{
                //    using (TurnKey2DataContext turnkeyDB = new TurnKey2DataContext())
                //    {
                //        var log = turnkeyDB.GetTable<V_Invoice>()
                //                .Where(i => i.InvoiceNo == result.Main.InvoiceNumber)
                //                .Where(i => i.DocType == "C0401" || i.DocType == "A0401")
                //                .ToList()
                //                .OrderByDescending(i => i.MESSAGE_DTS).FirstOrDefault();
                //        if (log != null)
                //        {
                //            result.TxnCode = log.STATUS == "C"
                //                    ? Naming.GovTurnkeyTransaction.C.ToString()
                //                    : Naming.GovTurnkeyTransaction.E.ToString();
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Logger.Error(ex);
                //}
            }
            return result;
        }

        private static Schema.TurnKey.F0401.DetailsProductItem[] buildF0401Details(InvoiceItem item)
        {
            List<ModelCore.Schema.TurnKey.F0401.DetailsProductItem> items = new List<Schema.TurnKey.F0401.DetailsProductItem>();
            foreach (var detailItem in item.InvoiceDetails)
            {
                detailItem.InvoiceProduct.InvoiceProductItem.ToList();
                foreach (var productItem in detailItem.InvoiceProduct.InvoiceProductItem)
                {
                    items.Add(new ModelCore.Schema.TurnKey.F0401.DetailsProductItem
                    {
                        Amount = productItem.CostAmount ?? 0m,   //productItem.CostAmount.HasValue ? productItem.CostAmount.Value.ToFix(item.InvoiceAmountType.CurrencyType?.Decimals ?? 0) : 0m,
                        Description = detailItem.InvoiceProduct.Brief,
                        Quantity = productItem.Piece ?? 0m,  //productItem.Piece.HasValue ? productItem.Piece.Value.ToFix(0) : 0,
                        RelateNumber = productItem.RelateNumber,
                        Remark = productItem.Remark.GetEfficientStringMaxSize(0, 40),
                        SequenceNumber = String.Format("{0:00}", productItem.No),
                        Unit = productItem.PieceUnit,
                        UnitPrice = productItem.UnitCost ?? 0m, //productItem.UnitCost.HasValue ? productItem.UnitCost.Value.ToFix(item.InvoiceAmountType.CurrencyType?.Decimals ?? 0) : 0,
                        TaxType = (Schema.TurnKey.F0401.TaxTypeEnum?)productItem.TaxType ?? Schema.TurnKey.F0401.TaxTypeEnum.Item1,
                    });
                }
            }
            return items.ToArray();
        }

        public static ModelCore.Schema.TurnKey.F0501.CancelInvoice CreateF0501(this InvoiceItem item, bool withExtension = false)
        {
            InvoiceCancellation cancellation = item.InvoiceCancellation;
            if (cancellation == null)
            {
                return null;
            }

            var result = new ModelCore.Schema.TurnKey.F0501.CancelInvoice
            {
                CancelInvoiceNumber = cancellation.CancellationNo,
                InvoiceDate = String.Format("{0:yyyyMMdd}", item.InvoiceDate.Value),
                BuyerId = item.InvoiceBuyer.ReceiptNo,
                SellerId = item.Organization.ReceiptNo,
                CancelDate = String.Format("{0:yyyyMMdd}", cancellation.CancelDate.Value),
                CancelTime = cancellation.CancelDate.Value,
                CancelReason = cancellation.CancelReason.GetEfficientStringMaxSize(0, 20),
                ReturnTaxDocumentNumber = cancellation.ReturnTaxDocumentNo,
                Remark = cancellation.Remark,
            };

            if (withExtension)
            {
                result.TxnCode = Naming.GovTurnkeyTransaction.I.ToString();
                if (item.CDS_Document.ChildDocument.FirstOrDefault()?.CDS_Document?.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.MIG_C) == true)
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.C.ToString();
                }
                else if (item.CDS_Document.ChildDocument.FirstOrDefault()?.CDS_Document?.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.MIG_E) == true)
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.E.ToString();
                }
                else if (item.CDS_Document.ChildDocument.FirstOrDefault()?.CDS_Document?.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.已開立) == true)
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.P.ToString();
                }
                //try
                //{
                //    using (TurnKey2DataContext turnkeyDB = new TurnKey2DataContext())
                //    {
                //        var log = turnkeyDB.GetTable<V_Invoice>()
                //                .Where(i => i.InvoiceNo == result.CancelInvoiceNumber)
                //                .Where(i => i.DocType == "C0501" || i.DocType == "A0501")
                //                .ToList()
                //                .OrderByDescending(i => i.MESSAGE_DTS).FirstOrDefault();
                //        if (log != null)
                //        {
                //            result.TxnCode = log.STATUS == "C"
                //                    ? Naming.GovTurnkeyTransaction.C.ToString()
                //                    : Naming.GovTurnkeyTransaction.E.ToString();
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Logger.Error(ex);
                //}
            }

            return result;
        }

        public static ModelCore.Schema.TurnKey.F0701.VoidInvoice CreateF0701(this InvoiceItem item)
        {
            return new ModelCore.Schema.TurnKey.F0701.VoidInvoice
            {
                VoidInvoiceNumber = item.TrackCode + item.No,
                InvoiceDate = String.Format("{0:yyyyMMdd}", item.InvoiceDate),
                BuyerId = item.InvoiceBuyer.ReceiptNo,
                SellerId = item.InvoiceSeller.ReceiptNo,
                VoidDate = DateTime.Now.Date.ToString("yyyyMMdd"),
                VoidTime = DateTime.Now,
                VoidReason = "註銷重開",
                Remark = ""
            };
        }

        public static ModelCore.Schema.TurnKey.G0401.Allowance CreateG0401(this InvoiceAllowance item, GenericManager<EIVOEntityDataContext> models = null, bool withExtension = false)
        {
            //bool isCBM = models.IsCrossBorderMerchant(item.InvoiceAllowanceSeller.SellerID);

            var result = new Schema.TurnKey.G0401.Allowance
            {
                Main = new Schema.TurnKey.G0401.Main
                {
                    AllowanceNumber = item.TurnkeyAllowanceNo,
                    AllowanceDate = String.Format("{0:yyyyMMdd}", item.AllowanceDate),
                    AllowanceType = Schema.TurnKey.G0401.AllowanceTypeEnum.Item2,   //(Schema.TurnKey.G0401.AllowanceTypeEnum)((int)item.AllowanceType),
                    Buyer = new Schema.TurnKey.G0401.MainBuyer
                    {
                        //Address = string.IsNullOrEmpty(item.InvoiceAllowanceBuyer.Address) ?
                        //"" :
                        // item.InvoiceAllowanceBuyer.Address.Length > 100 ?
                        //  item.InvoiceAllowanceBuyer.Address.Substring(0, 100) :
                        //   item.InvoiceAllowanceBuyer.Address,
                        ////CustomerNumber = item.InvoiceAllowanceBuyer.CustomerName,
                        //EmailAddress = "",//item.InvoiceAllowanceBuyer.EMail,
                        //FacsimileNumber = String.IsNullOrEmpty(item.InvoiceAllowanceBuyer.Fax) ?
                        //"" :
                        // item.InvoiceAllowanceBuyer.Fax.Length > 26 ?
                        //  item.InvoiceAllowanceBuyer.Fax.Substring(0, 26) :
                        //   item.InvoiceAllowanceBuyer.Fax,
                        Identifier = item.InvoiceAllowanceBuyer.ReceiptNo,
                        Name = item.InvoiceAllowanceBuyer.IsB2C()
                            ? Encoding.GetEncoding(950).GetBytes(item.InvoiceAllowanceBuyer.Name.InsteadOfNullOrEmpty("")).Length == 4
                                ? item.InvoiceAllowanceBuyer.Name : ValidityAgent.GenerateRandomCode(4)
                            : String.IsNullOrEmpty(item.InvoiceAllowanceBuyer.Name)
                                ? item.InvoiceAllowanceBuyer.ReceiptNo : item.InvoiceAllowanceBuyer.Name,
                        //PersonInCharge = String.IsNullOrEmpty(item.InvoiceAllowanceBuyer.PersonInCharge) ?
                        //"" :
                        //item.InvoiceAllowanceBuyer.PersonInCharge.Length > 30 ?
                        //item.InvoiceAllowanceBuyer.PersonInCharge.Substring(0, 30) :
                        //item.InvoiceAllowanceBuyer.PersonInCharge,
                        //RoleRemark = item.InvoiceAllowanceBuyer.RoleRemark,
                        //TelephoneNumber = "",//item.InvoiceAllowanceBuyer.Phone,
                    },
                    Seller = new Schema.TurnKey.G0401.MainSeller
                    {
                        //Address = String.IsNullOrEmpty(item.InvoiceAllowanceSeller.Address) ?
                        //"" :
                        // item.InvoiceAllowanceSeller.Address.Length > 100 ?
                        //  item.InvoiceAllowanceSeller.Address.Substring(0, 100) :
                        //   item.InvoiceAllowanceSeller.Address,
                        ////CustomerNumber = item.InvoiceAllowanceSeller.CustomerName,
                        //EmailAddress = "",//item.InvoiceAllowanceSeller.EMail,
                        //FacsimileNumber = String.IsNullOrEmpty(item.InvoiceAllowanceSeller.Fax) ?
                        //"" :
                        //item.InvoiceAllowanceSeller.Fax.Length > 26 ?
                        //item.InvoiceAllowanceSeller.Fax.Substring(0, 26) :
                        //item.InvoiceAllowanceSeller.Fax,
                        Identifier = item.InvoiceAllowanceSeller.ReceiptNo,
                        Name = item.InvoiceAllowanceSeller.Name,
                        //PersonInCharge = String.IsNullOrEmpty(item.InvoiceAllowanceSeller.PersonInCharge) ?
                        //"" :
                        // item.InvoiceAllowanceSeller.PersonInCharge.Length > 30 ?
                        //  item.InvoiceAllowanceSeller.PersonInCharge.Substring(0, 30) :
                        //   item.InvoiceAllowanceSeller.PersonInCharge,
                        //RoleRemark = item.InvoiceAllowanceSeller.RoleRemark,
                        //TelephoneNumber = "",//item.InvoiceAllowanceSeller.Phone,
                    },
                },
                Amount = /*isCBM
                            ? new Schema.TurnKey.D0401.Amount
                            {
                                TaxAmount = 0,
                                TotalAmount = ((item.TotalAmount ?? 0) + (item.TaxAmount ?? 0)).ToFix(item.CurrencyType?.Decimals ?? 0),
                            }
                            : */
                            new Schema.TurnKey.G0401.Amount
                            {
                                TaxAmount = item.TaxAmount.HasValue
                                            ? item.TaxAmount.Value.ToFix(0 /*item.CurrencyType?.Decimals ?? 0*/)
                                            : 0,
                                TotalAmount = item.TotalAmount.HasValue ? item.TotalAmount.Value.ToFix(item.CurrencyType?.Decimals ?? 0) : 0,
                            },
            };

            int seqNo = 1;
            result.Details = item.InvoiceAllowanceDetails.Select(d => new Schema.TurnKey.G0401.DetailsProductItem
            {
                AllowanceSequenceNumber = (seqNo++).ToString(), //d.InvoiceAllowanceItem.No.ToString(),
                Amount = d.InvoiceAllowanceItem.Amount.HasValue ? d.InvoiceAllowanceItem.Amount.Value.ToFix(item.CurrencyType?.Decimals ?? 0) : 0m,
                OriginalDescription = d.InvoiceAllowanceItem.OriginalDescription,
                OriginalInvoiceDate = String.Format("{0:yyyyMMdd}", d.InvoiceAllowanceItem.InvoiceDate),
                OriginalInvoiceNumber = d.InvoiceAllowanceItem.InvoiceNo,
                OriginalSequenceNumber = d.InvoiceAllowanceItem.OriginalSequenceNo?.ToString() ?? "1",
                Quantity = (int)(d.InvoiceAllowanceItem.Piece ?? 0M),
                Tax = (int)(d.InvoiceAllowanceItem.Tax ?? 0),
                TaxType = (Schema.TurnKey.G0401.TaxTypeEnum?)d.InvoiceAllowanceItem.TaxType ?? Schema.TurnKey.G0401.TaxTypeEnum.Item1,
                Unit = d.InvoiceAllowanceItem.PieceUnit,
                UnitPrice = d.InvoiceAllowanceItem.UnitCost ?? 0,
            }).ToArray();

            if (withExtension)
            {
                foreach (var d in result.Details)
                {
                    String trackCode = d.OriginalInvoiceNumber.Substring(0, 2);
                    String no = d.OriginalInvoiceNumber.Substring(2);

                    var invItem = models.GetTable<InvoiceItem>()
                        .Where(i => i.TrackCode == trackCode && i.No == no)
                        .Where(i => i.SellerID == item.InvoiceAllowanceSeller.SellerID)
                        .FirstOrDefault();

                    d.DataNumber = invItem?.InvoicePurchaseOrder?.OrderNo;
                }

                result.TxnCode = Naming.GovTurnkeyTransaction.I.ToString();
                if (item.CDS_Document.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.MIG_C) == true)
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.C.ToString();
                }
                else if (item.CDS_Document.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.MIG_E) == true)
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.E.ToString();
                }
                else if (item.CDS_Document.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.已開立) == true)
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.P.ToString();
                }
                //try
                //{
                //    using (TurnKey2DataContext turnkeyDB = new TurnKey2DataContext())
                //    {
                //        var log = turnkeyDB.GetTable<V_Allowance>()
                //                .Where(i => i.AllowanceNo == result.Main.AllowanceNumber)
                //                .Where(i => i.DocType == "D0401" || i.DocType == "B0401")
                //                .ToList()
                //                .OrderByDescending(i => i.MESSAGE_DTS).FirstOrDefault();
                //        if (log != null)
                //        {
                //            result.TxnCode = log.STATUS == "C"
                //                    ? Naming.GovTurnkeyTransaction.C.ToString()
                //                    : Naming.GovTurnkeyTransaction.E.ToString();
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Logger.Error(ex);
                //}

            }

            return result;
        }

        public static ModelCore.Schema.TurnKey.G0501.CancelAllowance? CreateG0501(this InvoiceAllowance item, bool withExtension = false)
        {
            InvoiceAllowanceCancellation cancelledItem = item.InvoiceAllowanceCancellation;
            if (cancelledItem == null)
            {
                return null;
            }

            var result = new ModelCore.Schema.TurnKey.G0501.CancelAllowance
            {
                CancelAllowanceNumber = item.TurnkeyAllowanceNo,
                AllowanceDate = String.Format("{0:yyyyMMdd}", item.AllowanceDate!.Value),
                BuyerId = item.BuyerId,
                SellerId = item.SellerId,
                CancelDate = String.Format("{0:yyyyMMdd}", cancelledItem.CancelDate!.Value),
                CancelTime = cancelledItem.CancelDate.Value,
                CancelReason = cancelledItem.CancelReason,
                AllowanceType = (Schema.TurnKey.G0501.AllowanceTypeEnum?)item.AllowanceType ?? Schema.TurnKey.G0501.AllowanceTypeEnum.Item2,
                Remark = cancelledItem.Remark,
            };

            if (withExtension)
            {
                result.TxnCode = Naming.GovTurnkeyTransaction.I.ToString();
                if (item.CDS_Document.ChildDocument.FirstOrDefault()?.CDS_Document?.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.MIG_C) == true)
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.C.ToString();
                }
                else if (item.CDS_Document.ChildDocument.FirstOrDefault()?.CDS_Document?.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.MIG_E) == true)
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.E.ToString();
                }
                else if (item.CDS_Document.ChildDocument.FirstOrDefault()?.CDS_Document?.DataProcessLog.Any(d => d.StepID == (int)Naming.InvoiceStepDefinition.已開立) == true)
                {
                    result.TxnCode = Naming.GovTurnkeyTransaction.P.ToString();
                }
                //try
                //{
                //    using (TurnKey2DataContext turnkeyDB = new TurnKey2DataContext())
                //    {
                //        var log = turnkeyDB.GetTable<V_Allowance>()
                //                .Where(i => i.AllowanceNo == result.CancelAllowanceNumber)
                //                .Where(i => i.DocType == "D0501" || i.DocType == "B0501")
                //                .ToList()
                //                .OrderByDescending(i => i.MESSAGE_DTS).FirstOrDefault();
                //        if (log != null)
                //        {
                //            result.TxnCode = log.STATUS == "C"
                //                    ? Naming.GovTurnkeyTransaction.C.ToString()
                //                    : Naming.GovTurnkeyTransaction.E.ToString();
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Logger.Error(ex);
                //}

            }

            return result;
        }

    }
}
