using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Core.Utility;
using CommonLib.DataAccess;
using CommonLib.Utility;
using ModelCore.DataEntity;
using ModelCore.InvoiceManagement.InvoiceProcess;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;

namespace ModelCore.Helper
{
    public static class InvoiceExtensionMethods
    {

        public static void GetInvoiceNo(this String invoiceNumber, out String invNo, out String trackCode)
        {
            if (!String.IsNullOrEmpty(invoiceNumber) && invoiceNumber.Length >= 10)
            {
                trackCode = invoiceNumber.Substring(0, 2);
                invNo = invoiceNumber.Substring(2);
            }
            else
            {
                trackCode = null;
                invNo = invoiceNumber;
            }
        }

        public static InvoiceItem ConvertToInvoiceItem(this GenericManager<EIVOEntityDataContext> models, ModelCore.Schema.TurnKey.A0401.Invoice invoice, OrganizationToken owner)
        {
            Organization buyer = models.GetTable<Organization>().Where(o => o.ReceiptNo == invoice.Main.Buyer.Identifier).FirstOrDefault();
            if (buyer == null)
            {
                buyer = new Organization
                {
                    Addr = invoice.Main.Buyer.Address,
                    CompanyName = invoice.Main.Buyer.Name,
                    UndertakerName = invoice.Main.Buyer.PersonInCharge,
                    Phone = invoice.Main.Buyer.TelephoneNumber,
                    Fax = invoice.Main.Buyer.FacsimileNumber,
                    ContactEmail = invoice.Main.Buyer.EmailAddress,
                    ReceiptNo = invoice.Main.Buyer.Identifier,
                    OrganizationStatus = new OrganizationStatus
                    {
                        IronSteelIndustry = false
                    }
                };
            }

            Organization seller = models.GetTable<Organization>().Where(o => o.ReceiptNo == invoice.Main.Seller.Identifier).FirstOrDefault();
            if (seller == null)
            {
                seller = new Organization
                {
                    Addr = invoice.Main.Seller.Address,
                    CompanyName = invoice.Main.Seller.Name,
                    UndertakerName = invoice.Main.Seller.PersonInCharge,
                    Phone = invoice.Main.Seller.TelephoneNumber,
                    Fax = invoice.Main.Seller.FacsimileNumber,
                    ContactEmail = invoice.Main.Seller.EmailAddress,
                    ReceiptNo = invoice.Main.Seller.Identifier,
                    OrganizationStatus = new OrganizationStatus
                    {
                        IronSteelIndustry = false
                    }
                };
            }
            else if (seller.OrganizationStatus == null)
            {
                seller.OrganizationStatus = new OrganizationStatus
                {
                    IronSteelIndustry = false
                };
            }

            String invNo, trackCode;
            invoice.Main.InvoiceNumber.GetInvoiceNo(out invNo, out trackCode);

            InvoiceItem newItem = new InvoiceItem
            {
                CDS_Document = new CDS_Document
                {
                    DocDate = DateTime.Now,
                    DocType = (int)Naming.DocumentTypeDefinition.E_Invoice,
                },
                InvoiceBuyer = new InvoiceBuyer
                {
                    BuyerMark = invoice.Main.BuyerRemarkSpecified ? (int)invoice.Main.BuyerRemark : (int?)null,
                    Name = invoice.Main.Buyer.Name,
                    ReceiptNo = invoice.Main.Buyer.Identifier,
                    ContactName = invoice.Main.Buyer.PersonInCharge,
                    Address = invoice.Main.Buyer.Address,
                    CustomerID = invoice.Main.Buyer.CustomerNumber,
                    CustomerName = invoice.Main.Buyer.Name,
                    EMail = invoice.Main.Buyer.EmailAddress,
                    Fax = invoice.Main.Buyer.FacsimileNumber,
                    PersonInCharge = invoice.Main.Buyer.PersonInCharge,
                    Phone = invoice.Main.Buyer.TelephoneNumber,
                    RoleRemark = invoice.Main.Buyer.RoleRemark,
                    Organization = buyer
                },
                InvoiceSeller = new InvoiceSeller
                {
                    Name = invoice.Main.Seller.Name,
                    ReceiptNo = invoice.Main.Seller.Identifier,
                    ContactName = invoice.Main.Seller.PersonInCharge,
                    Address = invoice.Main.Seller.Address,
                    CustomerID = invoice.Main.Seller.CustomerNumber,
                    CustomerName = invoice.Main.Seller.Name,
                    EMail = invoice.Main.Seller.EmailAddress,
                    Fax = invoice.Main.Seller.FacsimileNumber,
                    PersonInCharge = invoice.Main.Seller.PersonInCharge,
                    Phone = invoice.Main.Seller.TelephoneNumber,
                    RoleRemark = invoice.Main.Seller.RoleRemark,
                    Organization = seller
                },
                InvoiceDate = DateTime.ParseExact(String.Format("{0}", invoice.Main.InvoiceDate), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture).Add(invoice.Main.InvoiceTime.TimeOfDay),
                InvoiceType = (byte)((int)invoice.Main.InvoiceType),
                No = invNo,
                TrackCode = trackCode,
                Organization = seller,
                BuyerRemark = (byte?)invoice.Main.BuyerRemark,
                Category = invoice.Main.Category,
                CheckNo = invoice.Main.CheckNumber,
                DonateMark = ((int)invoice.Main.DonateMark).ToString(),
                CustomsClearanceMark = (byte?)invoice.Main.CustomsClearanceMark,
                GroupMark = invoice.Main.GroupMark,
                //PermitDate = String.IsNullOrEmpty(invoice.Main.PermitDate) ? (DateTime?)null : DateTime.ParseExact(invoice.Main.PermitDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture),
                //PermitNumber = invoice.Main.PermitNumber,
                //PermitWord = invoice.Main.PermitWord,
                RelateNumber = invoice.Main.RelateNumber,
                Remark = invoice.Main.MainRemark,
                //TaxCenter = invoice.Main.TaxCenter,
                InvoiceAmountType = new InvoiceAmountType
                {
                    DiscountAmount = invoice.Amount.DiscountAmountSpecified ? invoice.Amount.DiscountAmount : (decimal?)null,
                    SalesAmount = invoice.Amount.SalesAmount,
                    TaxAmount = invoice.Amount.TaxAmount,
                    TaxType = (byte)((int)invoice.Amount.TaxType),
                    TotalAmount = invoice.Amount.TotalAmount,
                    TotalAmountInChinese = ValidityAgent.MoneyShow(invoice.Amount.TotalAmount),
                    TaxRate = invoice.Amount.TaxRate,
                    CurrencyID = invoice.Amount.CurrencySpecified ? (int)invoice.Amount.Currency : (int?)null,
                    ExchangeRate = invoice.Amount.ExchangeRateSpecified ? invoice.Amount.ExchangeRate : (decimal?)null,
                    OriginalCurrencyAmount = invoice.Amount.OriginalCurrencyAmountSpecified ? invoice.Amount.OriginalCurrencyAmount : (decimal?)null
                },
                PrintMark = "Y",
            };

            if (owner != null)
            {
                newItem.CDS_Document.DocumentOwner = new DocumentOwner
                {
                    OwnerID = owner.CompanyID
                };
            }

            short seqNo = 1;

            var productItems = invoice.Details.Select(i => new InvoiceProductItem
            {
                InvoiceProduct = new InvoiceProduct { Brief = i.Description },
                CostAmount = i.Amount,
                ItemNo = i.SequenceNumber,
                Piece = i.Quantity,
                PieceUnit = i.Unit,
                UnitCost = i.UnitPrice,
                Remark = i.Remark,
                TaxType = newItem.InvoiceAmountType.TaxType,
                RelateNumber = i.RelateNumber,
                No = (seqNo++)
            });

            newItem.InvoiceDetails.AddRange(productItems.Select(p => new InvoiceDetail
            {
                InvoiceProduct = p.InvoiceProduct
            }));
            return newItem;
        }

        public static InvoiceItem ConvertToInvoiceItem(this GenericManager<EIVOEntityDataContext> models, InvoiceEntity invoice)
        {
            InvoiceItem item = invoice.MainItem;
            if (item.InvoiceBuyer == null)
            {
                invoice.Status = Naming.UploadStatusDefinition.匯入失敗;
                invoice.Reason = "InvoiceBuyer is null";
                return null;
            }
            Organization buyer = models.GetTable<Organization>().Where(o => o.ReceiptNo == item.InvoiceBuyer.ReceiptNo).FirstOrDefault();
            if (buyer == null)
            {
                buyer = new Organization
                {
                    Addr = item.InvoiceBuyer.Address,
                    CompanyName = item.InvoiceBuyer.Name,
                    UndertakerName = item.InvoiceBuyer.PersonInCharge,
                    Phone = item.InvoiceBuyer.Phone,
                    Fax = item.InvoiceBuyer.Fax,
                    ContactEmail = item.InvoiceBuyer.EMail,
                    ReceiptNo = item.InvoiceBuyer.ReceiptNo,
                    OrganizationStatus = new OrganizationStatus
                    {
                        IronSteelIndustry = false
                    }
                };
            }

            item.InvoiceBuyer.Organization = buyer;

            if (item.InvoiceSeller == null)
            {
                invoice.Status = Naming.UploadStatusDefinition.匯入失敗;
                invoice.Reason = "InvoiceSeller is null";
                return null;
            }

            Organization seller = models.GetTable<Organization>().Where(o => o.ReceiptNo == item.InvoiceSeller.ReceiptNo).FirstOrDefault();
            if (seller == null)
            {
                seller = new Organization
                {
                    Addr = item.InvoiceSeller.Address,
                    CompanyName = item.InvoiceSeller.Name,
                    UndertakerName = item.InvoiceSeller.PersonInCharge,
                    Phone = item.InvoiceSeller.Phone,
                    Fax = item.InvoiceSeller.Fax,
                    ContactEmail = item.InvoiceSeller.EMail,
                    ReceiptNo = item.InvoiceSeller.ReceiptNo,
                    OrganizationStatus = new OrganizationStatus
                    {
                        IronSteelIndustry = false
                    }
                };
            }
            else if (seller.OrganizationStatus == null)
            {
                seller.OrganizationStatus = new OrganizationStatus
                {
                    IronSteelIndustry = false
                };
            }
            item.Organization = seller;

            item.InvoiceSeller.Organization = seller;
            item.CDS_Document = new CDS_Document
            {
                DocDate = DateTime.Now,
                DocType = (int)Naming.DocumentTypeDefinition.E_Invoice,
            };

            //foreach(var p in invoice.ItemDetails)
            //{
            //    p.InvoiceProduct = new InvoiceProduct
            //    {
            //        Brief = p.InvoiceProduct.Brief
            //    };
            //}

            item.InvoiceDetails.Clear();
            item.InvoiceDetails.AddRange(invoice.ItemDetails.Select(p => new InvoiceDetail
            {
                InvoiceProduct = p
            }));


            return item;
        }

        public static DataSet GetInvoiceData(this IQueryable<InvoiceItem> items, GenericManager<EIVOEntityDataContext> models)
        {
            var productItems = items.Join(models.GetTable<InvoiceDetail>(), i => i.InvoiceID, d => d.InvoiceID, (i, d) => d)
                                    .SelectMany(d => d.InvoiceProduct.InvoiceProductItem);

            var dataItems = items.ToArray().Select(i => new
            {
                Invoice_No = $"{i.TrackCode}{i.No}",
                Invoice_Date = i.InvoiceDate,
                Data_ID = i.InvoicePurchaseOrder?.OrderNo,
                Data_Date = i.InvoicePurchaseOrder?.PurchaseDate,
                Seller_ID = i.InvoiceSeller.ReceiptNo,
                Buyer_Name = i.InvoiceBuyer.CustomerName,
                Buyer_ID = i.InvoiceBuyer.ReceiptNo,
                Buyer_Mark = i.BuyerRemark,
                Customs_Clearance_Mark = i.CustomsClearanceMark,
                Customer_ID = i.InvoiceBuyer.CustomerID,
                Contact_Name = i.InvoiceBuyer.ContactName,
                EMail = i.InvoiceBuyer.EMail,
                Address = i.InvoiceBuyer.Address,
                Phone = i.InvoiceBuyer.Phone,
                Sales_Amount = i.InvoiceAmountType.SalesAmount,
                Free_Tax_Sales_Amount = (decimal?)null,
                Zero_Tax_Sales_Amount = (decimal?)null,
                Invoice_Type = (int?)i.InvoiceType,
                Tax_Type = (int?)i.InvoiceAmountType.TaxType,
                Tax_Rate = i.InvoiceAmountType.TaxRate,
                Tax_Amount = i.InvoiceAmountType.TaxAmount,
                Total_Amount = i.InvoiceAmountType.TotalAmount,
                Currency = i.InvoiceAmountType.CurrencyType?.AbbrevName,
                Print_Mark = i.PrintMark,
                Carrier_Type = i.InvoiceCarrier?.CarrierType,
                Carrier_Id1 = i.InvoiceCarrier?.CarrierNo,
                Carrier_Id2 = i.InvoiceCarrier?.CarrierNo2,
                Donate_Mark = i.DonateMark,
                NPOBAN = i.InvoiceDonation?.AgencyCode,
                Random_Number = i.RandomNo,
                Main_Remark = i.Remark,
                //Process_Type = ((Naming.InvoiceProcessType?)i.CDS_Document.ProcessType).ToString(),
            });

            var detailItems = productItems.ToArray().Select(d => new
            {
                Invoice_No = $"{d.InvoiceProduct.InvoiceDetails.First().InvoiceItem.TrackCode}{d.InvoiceProduct.InvoiceDetails.First().InvoiceItem.No}",
                Data_ID = d.InvoiceProduct.InvoiceDetails.First().InvoiceItem.InvoicePurchaseOrder?.OrderNo,
                Description = d.InvoiceProduct.Brief,
                Quantity = d.Piece,
                Unit = d.PieceUnit,
                Unit_Price = d.UnitCost,
                Amount = d.CostAmount,
                Item_Tax_Type = d.TaxType,
                Remark = d.Remark,
            });

            DataSet ds = new DataSet();

            DataTable table = dataItems.ToDataTable();
            table.TableName = "Invoice";
            ds.Tables.Add(table);

            table = detailItems.ToDataTable();
            table.TableName = "Details";
            ds.Tables.Add(table);
            return ds;

        }

        public static DataSet GetAllowanceData(this IQueryable<InvoiceAllowance> items, GenericManager<EIVOEntityDataContext> models)
        {
            var productItems = items.Join(models.GetTable<InvoiceAllowanceDetail>(), i => i.AllowanceID, d => d.AllowanceID, (i, d) => d)
                                    .Select(d => d.InvoiceAllowanceItem);

            var dataItems = items.ToArray().Select(i => new
            {
                Allowance_No = i.AllowanceNumber,
                Allowance_Date = i.AllowanceDate,
                Seller_ID = i.InvoiceAllowanceSeller.ReceiptNo,
                //Customer_ID = i.InvoiceAllowanceBuyer.CustomerID,
                //Buyer_Name = i.InvoiceAllowanceBuyer.CustomerName,
                //Buyer_ID = i.InvoiceAllowanceBuyer.ReceiptNo,
                //Allowance_Type = i.AllowanceType,
                Contact_Name = i.InvoiceAllowanceBuyer.ContactName,
                EMail = i.InvoiceAllowanceBuyer.EMail,
                Address = i.InvoiceAllowanceBuyer.Address,
                Phone = i.InvoiceAllowanceBuyer.Phone,
                Tax_Amount = i.TaxAmount,
                Total_Amount = i.TotalAmount,
                Currency = i.CurrencyType?.AbbrevName,
            });

            var detailItems = productItems.ToArray().Select(d => new
            {
                Allowance_No = d.InvoiceAllowanceDetail.First().InvoiceAllowance.AllowanceNumber,
                Original_Invoice_Date = d.InvoiceDate,
                Original_Invoice_No = d.InvoiceNo,
                Original_Sequence_No = d.OriginalSequenceNo,
                Original_Description = d.OriginalDescription,
                Quantity = d.Piece,
                Unit_Price = d.UnitCost,
                Amount = d.Amount,
                Tax = d.Tax,
                Allowance_Sequence_No = d.No,
                Item_Tax_Type = d.TaxType,
            });

            DataSet ds = new DataSet();

            DataTable table = dataItems.ToDataTable();
            table.TableName = "Allowance";
            ds.Tables.Add(table);

            table = detailItems.ToDataTable();
            table.TableName = "Details";
            ds.Tables.Add(table);
            return ds;

        }

        public static DataSet GetInvoiceDataForVAC(this IQueryable<InvoiceItem> items, GenericManager<EIVOEntityDataContext> models)
        {
            var productItems = items.Join(models.GetTable<InvoiceDetail>(), i => i.InvoiceID, d => d.InvoiceID, (i, d) => d)
                                    .SelectMany(d => d.InvoiceProduct.InvoiceProductItem);

            var dataItems = items.ToArray().Select(i => new
            {
                Data_ID = i.InvoicePurchaseOrder?.OrderNo,
                Data_Date = i.InvoicePurchaseOrder?.PurchaseDate,
                Seller_ID = i.InvoiceSeller.ReceiptNo,
                Buyer_Name = i.InvoiceBuyer.CustomerName,
                Buyer_ID = i.InvoiceBuyer.ReceiptNo,
                Buyer_Mark = i.BuyerRemark,
                Customs_Clearance_Mark = i.CustomsClearanceMark,
                Customer_ID = i.InvoiceBuyer.CustomerID,
                Contact_Name = i.InvoiceBuyer.ContactName,
                EMail = i.InvoiceBuyer.EMail,
                Address = i.InvoiceBuyer.Address,
                Phone = i.InvoiceBuyer.Phone,
                Sales_Amount = i.InvoiceAmountType.SalesAmount,
                Free_Tax_Sales_Amount = (decimal?)null,
                Zero_Tax_Sales_Amount = (decimal?)null,
                Invoice_Type = (int?)i.InvoiceType,
                Tax_Type = (int?)i.InvoiceAmountType.TaxType,
                Tax_Rate = i.InvoiceAmountType.TaxRate,
                Tax_Amount = i.InvoiceAmountType.TaxAmount,
                Total_Amount = i.InvoiceAmountType.TotalAmount,
                Currency = i.InvoiceAmountType.CurrencyType?.AbbrevName,
                Print_Mark = i.PrintMark,
                Carrier_Type = i.InvoiceCarrier?.CarrierType,
                Carrier_Id1 = i.InvoiceCarrier?.CarrierNo,
                Carrier_Id2 = i.InvoiceCarrier?.CarrierNo2,
                Donate_Mark = i.DonateMark,
                NPOBAN = i.InvoiceDonation?.AgencyCode,
                Random_Number = i.RandomNo,
                Main_Remark = i.Remark,
                //Process_Type = ((Naming.InvoiceProcessType?)i.CDS_Document.ProcessType).ToString(),
            });

            var detailItems = productItems.ToArray().Select(d => new
            {
                Data_ID = d.InvoiceProduct.InvoiceDetails.First().InvoiceItem.InvoicePurchaseOrder?.OrderNo,
                Description = d.InvoiceProduct.Brief,
                Quantity = d.Piece,
                Unit = d.PieceUnit,
                Unit_Price = d.UnitCost,
                Amount = d.CostAmount,
                Item_Tax_Type = d.TaxType,
                Remark = d.Remark,
            });

            DataSet ds = new DataSet();

            DataTable table = dataItems.ToDataTable();
            table.TableName = "Invoice";
            ds.Tables.Add(table);

            table = detailItems.ToDataTable();
            table.TableName = "Details";
            ds.Tables.Add(table);
            return ds;

        }

        public static DataSet GetInvoiceDataForIssuer(this IQueryable<InvoiceItem> items, GenericManager<EIVOEntityDataContext> models)
        {
            var productItems = items.Join(models.GetTable<InvoiceDetail>(), i => i.InvoiceID, d => d.InvoiceID, (i, d) => d)
                                    .SelectMany(d => d.InvoiceProduct.InvoiceProductItem);

            var dataItems = items.ToArray().Select(i => new
            {
                Invoice_No = $"{i.TrackCode}{i.No}",
                Invoice_Date = i.InvoiceDate,
                Data_ID = i.InvoicePurchaseOrder?.OrderNo,
                Seller_ID = i.InvoiceSeller.ReceiptNo,
                Buyer_Name = i.InvoiceBuyer.CustomerName,
                Buyer_ID = i.InvoiceBuyer.ReceiptNo,
                Buyer_Mark = i.BuyerRemark,
                Customs_Clearance_Mark = i.CustomsClearanceMark,
                Customer_ID = i.InvoiceBuyer.CustomerID,
                Contact_Name = i.InvoiceBuyer.ContactName,
                EMail = i.InvoiceBuyer.EMail,
                Address = i.InvoiceBuyer.Address,
                Phone = i.InvoiceBuyer.Phone,
                Sales_Amount = i.InvoiceAmountType.SalesAmount,
                Free_Tax_Sales_Amount = (decimal?)null,
                Zero_Tax_Sales_Amount = (decimal?)null,
                Invoice_Type = (int?)i.InvoiceType,
                Tax_Type = (int?)i.InvoiceAmountType.TaxType,
                Tax_Rate = i.InvoiceAmountType.TaxRate,
                Tax_Amount = i.InvoiceAmountType.TaxAmount,
                Total_Amount = i.InvoiceAmountType.TotalAmount,
                Currency = i.InvoiceAmountType.CurrencyType?.AbbrevName,
                Print_Mark = i.PrintMark,
                Carrier_Type = i.InvoiceCarrier?.CarrierType,
                Carrier_Id1 = i.InvoiceCarrier?.CarrierNo,
                Carrier_Id2 = i.InvoiceCarrier?.CarrierNo2,
                Donate_Mark = i.DonateMark,
                NPOBAN = i.InvoiceDonation?.AgencyCode,
                Random_Number = i.RandomNo,
                Main_Remark = i.Remark,
                //Process_Type = ((Naming.InvoiceProcessType?)i.CDS_Document.ProcessType).ToString(),
            });

            var detailItems = productItems.ToArray().Select(d => new
            {
                Invoice_No = $"{d.InvoiceProduct.InvoiceDetails.First().InvoiceItem.TrackCode}{d.InvoiceProduct.InvoiceDetails.First().InvoiceItem.No}",
                Description = d.InvoiceProduct.Brief,
                Quantity = d.Piece,
                Unit = d.PieceUnit,
                Unit_Price = d.UnitCost,
                Amount = d.CostAmount,
                Item_Tax_Type = d.TaxType,
                Remark = d.Remark,
            });

            DataSet ds = new DataSet();

            DataTable table = dataItems.ToDataTable();
            table.TableName = "Invoice";
            ds.Tables.Add(table);

            table = detailItems.ToDataTable();
            table.TableName = "Details";
            ds.Tables.Add(table);
            return ds;

        }
        public static DataSet GetInvoiceDataForCBE(this IQueryable<InvoiceItem> items, GenericManager<EIVOEntityDataContext> models)
        {
            var productItems = items.Join(models.GetTable<InvoiceDetail>(), i => i.InvoiceID, d => d.InvoiceID, (i, d) => d)
                                    .SelectMany(d => d.InvoiceProduct.InvoiceProductItem);

            var dataItems = items.ToArray().Select(i => new
            {
                Data_ID = i.InvoicePurchaseOrder?.OrderNo,
                Data_Date = i.InvoicePurchaseOrder?.PurchaseDate,
                Seller_ID = i.InvoiceSeller.ReceiptNo,
                Customer_ID = i.InvoiceBuyer.CustomerID,
                Sales_Amount = i.InvoiceAmountType.SalesAmount,
                Tax_Amount = i.InvoiceAmountType.TaxAmount,
                Total_Amount = i.InvoiceAmountType.TotalAmount,
                Currency = i.InvoiceAmountType.CurrencyType?.AbbrevName,
                Carrier_Id1 = i.InvoiceCarrier?.CarrierNo,
                Main_Remark = i.Remark,
            });

            var detailItems = productItems.ToArray().Select(d => new
            {
                Data_ID = d.InvoiceProduct.InvoiceDetails.First().InvoiceItem.InvoicePurchaseOrder?.OrderNo,
                Description = d.InvoiceProduct.Brief,
                Quantity = d.Piece,
                Unit = d.PieceUnit,
                Unit_Price = d.UnitCost,
                Amount = d.CostAmount,
                Remark = d.Remark,
            });

            DataSet ds = new DataSet();

            DataTable table = dataItems.ToDataTable();
            table.TableName = "Invoice";
            ds.Tables.Add(table);

            table = detailItems.ToDataTable();
            table.TableName = "Details";
            ds.Tables.Add(table);
            return ds;

        }

        public static DataSet GetVoidInvoiceData(this IQueryable<InvoiceCancellation> items, GenericManager<EIVOEntityDataContext> models)
        {
            var dataItems = items.ToArray().Select(i => new
            {
                Void_Invoice_No = $"{i.InvoiceItem.TrackCode}{i.InvoiceItem.No}",
                //Buyer_ID = i.InvoiceItem.InvoiceBuyer?.ReceiptNo,
                Seller_ID = i.InvoiceItem.InvoiceSeller?.ReceiptNo,
                Invoice_Date = i.InvoiceItem.InvoiceDate,
                Void_Date = i.CancelDate,
                Reason = i.CancelReason,
                Return_Tax_Document_No = i.ReturnTaxDocumentNo,
                Remark = i.Remark,
            });

            DataSet ds = new DataSet();

            DataTable table = dataItems.ToDataTable();
            table.TableName = "Void_Invoice";
            ds.Tables.Add(table);

            return ds;

        }

        public static DataSet GetInvoiceDataForFullAllowance(this IQueryable<InvoiceItem> items, GenericManager<EIVOEntityDataContext> models)
        {
            var dataItems = items.ToArray().Select(i => new
            {
                Data_No = $"{i.TrackCode}{i.No}",
                Seller_ID = i.InvoiceSeller.ReceiptNo,
                Allowance_No = "",
                //Process_Type = ((Naming.InvoiceProcessType?)i.CDS_Document.ProcessType).ToString(),
            });

            DataSet ds = new DataSet();

            DataTable table = dataItems.ToDataTable();
            table.TableName = "Allowance";
            ds.Tables.Add(table);

            return ds;

        }

        public static DataSet GetVoidAllowanceData(this IQueryable<InvoiceAllowanceCancellation> items, GenericManager<EIVOEntityDataContext> models)
        {
            var dataItems = items.ToArray().Select(i => new
            {
                Void_AllowanceNo = i.InvoiceAllowance.AllowanceNumber,
                //Buyer_ID = i.InvoiceAllowance.InvoiceAllowanceBuyer?.ReceiptNo,
                Seller_ID = i.InvoiceAllowance.InvoiceAllowanceSeller?.ReceiptNo,
                Allowance_Date = i.InvoiceAllowance.AllowanceDate,
                Reason = i.CancelReason,
                Void_Date = i.CancelDate,
                Remark = i.Remark,
                //Process_Type = ((Naming.InvoiceProcessType?)i.CDS_Document.ProcessType).ToString(),
            });

            DataSet ds = new DataSet();

            DataTable table = dataItems.ToDataTable();
            table.TableName = "Void_Allowance";
            ds.Tables.Add(table);

            return ds;

        }

        public static InvoiceAllowanceCancellation PrepareVoidItem(this InvoiceAllowance allowance, GenericManager<EIVOEntityDataContext> models, ref DerivedDocument doc)
        {
            InvoiceAllowanceCancellation voidItem = new InvoiceAllowanceCancellation
            {
                InvoiceAllowance = allowance,
                AllowanceID = allowance.AllowanceID,
            };

            doc = new DerivedDocument
            {
                CDS_Document = new CDS_Document
                {
                    DocDate = DateTime.Now,
                    DocType = (int)Naming.DocumentTypeDefinition.E_AllowanceCancellation,
                },
                SourceID = allowance.AllowanceID,
            };

            if (allowance.CDS_Document.DocumentOwner != null)
            {
                doc.CDS_Document.DocumentOwner = new DocumentOwner
                {
                    OwnerID = allowance.CDS_Document.DocumentOwner.OwnerID
                };
            }

            models.GetTable<InvoiceAllowanceCancellation>().InsertOnSubmit(voidItem);
            models.GetTable<DerivedDocument>().InsertOnSubmit(doc);

            if (allowance.CDS_Document.ProcessType == (int)Naming.InvoiceProcessType.B0401)
            {
                B0501Handler.PushStepQueueOnSubmit(models, doc.CDS_Document, Naming.InvoiceStepDefinition.已開立);
                B0501Handler.PushStepQueueOnSubmit(models, doc.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
            }
            else
            {
                D0501Handler.PushStepQueueOnSubmit(models, doc.CDS_Document, Naming.InvoiceStepDefinition.已開立);
                D0501Handler.PushStepQueueOnSubmit(models, doc.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
            }

            return voidItem;
        }

        public static InvoiceCancellation PrepareVoidItem(this InvoiceItem invoice, GenericManager<EIVOEntityDataContext> models, ref DerivedDocument doc)
        {
            InvoiceCancellation voidItem = new InvoiceCancellation
            {
                InvoiceItem = invoice,
                InvoiceID = invoice.InvoiceID,
                CancellationNo = $"{invoice.TrackCode}{invoice.No}",
            };

            doc = new DerivedDocument
            {
                CDS_Document = new CDS_Document
                {
                    DocType = (int)Naming.DocumentTypeDefinition.E_InvoiceCancellation,
                    DocDate = DateTime.Now,
                },
                SourceID = invoice.InvoiceID
            };

            if (invoice.CDS_Document.DocumentOwner != null)
            {
                doc.CDS_Document.DocumentOwner = new DocumentOwner
                {
                    OwnerID = invoice.CDS_Document.DocumentOwner.OwnerID
                };
            }

            models.GetTable<InvoiceCancellation>().InsertOnSubmit(voidItem);
            models.GetTable<DerivedDocument>().InsertOnSubmit(doc);

            if (invoice.CDS_Document.ProcessType == (int)Naming.InvoiceProcessType.A0401)
            {
                A0501Handler.PushStepQueueOnSubmit(models, doc.CDS_Document, Naming.InvoiceStepDefinition.已開立);
                A0501Handler.PushStepQueueOnSubmit(models, doc.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
            }
            else
            {
                C0501Handler.PushStepQueueOnSubmit(models, doc.CDS_Document, Naming.InvoiceStepDefinition.已開立);
                C0501Handler.PushStepQueueOnSubmit(models, doc.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
            }

            return voidItem;
        }

        public static IQueryable<InvoiceProductItem> GetInvoiceProductItem(this InvoiceItem item, GenericManager<EIVOEntityDataContext> models)
        {
            return models.GetTable<InvoiceItem>().Where(v => v.InvoiceID == item.InvoiceID)
                                    .Join(models.GetTable<InvoiceDetail>(), v => v.InvoiceID, d => d.InvoiceID, (v, d) => d)
                                    .Join(models.GetTable<InvoiceProduct>(), d => d.ProductID, p => p.ProductID, (d, p) => p)
                                    .Join(models.GetTable<InvoiceProductItem>(), p => p.ProductID, t => t.ProductID, (p, t) => t);
        }

        public static IQueryable<InvoiceItem> PromptWinningInvoiceForNotification(this GenericManager<EIVOEntityDataContext> models, int year, int period)
        {
            var items = models.GetTable<InvoiceItem>()
                .Where(i => i.InvoiceCancellation == null)
                .Where(i => i.InvoiceDonation == null)
                .Where(i => i.PrintMark == "N")
                .Join(models.GetTable<Organization>()
                    .Join(models.GetTable<OrganizationStatus>().Where(s => (s.InvoiceNoticeSetting & (int)Naming.InvoiceNoticeStatus.Winning) > 0),
                        o => o.CompanyID, s => s.CompanyID, (o, s) => o),
                    i => i.SellerID, o => o.CompanyID, (i, o) => i)
                .Join(models.GetTable<InvoiceWinningNumber>()
                    .Join(models.GetTable<UniformInvoiceWinningNumber>()
                        .Where(u => u.Year == year && u.Period == period),
                        w => w.WinningID, u => u.WinningID, (w, u) => w),
                    i => i.InvoiceID, w => w.InvoiceID, (i, w) => i);

            return items;
        }

        public static InvoicePurchaseOrderAudit CreateInvoicePurchaseOrderAudit(this GenericManager<EIVOEntityDataContext> models, int sellerID, String orderNo)
        {
            lock (typeof(InvoiceExtensionMethods))
            {
                var table = models.GetTable<InvoicePurchaseOrderAudit>();
                if (table.Where(a => a.SellerID == sellerID)
                    .Where(a => a.OrderNo == orderNo).Any())
                {
                    return null;
                }

                InvoicePurchaseOrderAudit audit = new InvoicePurchaseOrderAudit
                {
                    OrderNo = orderNo,
                    SellerID = sellerID,
                };

                table.InsertOnSubmit(audit);
                models.SubmitChanges();

                return audit;
            }
        }

        public static int? TurnkeyLogFeedback(this GenericManager<EIVOEntityDataContext> models, string msgType, string code, string no)
        {
            try
            {
                switch (msgType)
                {
                    case "A0401":
                    case "C0401":
                    case "F0401":
                        if (no?.Length == 10)
                        {
                            var invoice = models.GetTable<InvoiceItem>()
                                    .Where(i => i.TrackCode == no.Substring(0, 2))
                                    .Where(i => i.No == no.Substring(2))
                                    .OrderByDescending(i => i.InvoiceID)
                                    .FirstOrDefault();

                            if (invoice != null)
                            {
                                if (code == "C")
                                {
                                    invoice.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_C, Naming.DataProcessStatus.Done);
                                    models.SubmitChanges();
                                    //Console.WriteLine($"Invoice:({invoice.InvoiceID},{invoice.TrackCode}{invoice.No}) => C");
                                    return invoice.InvoiceID;
                                }
                                else if (code == "E")
                                {
                                    invoice.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_E, Naming.DataProcessStatus.Done);
                                    models.SubmitChanges();
                                    //Console.WriteLine($"Invoice:({invoice.InvoiceID},{invoice.TrackCode}{invoice.No}) => E");
                                    return invoice.InvoiceID;
                                }
                            }
                        }
                        break;

                    case "C0701":
                    case "F0701":
                        if (no?.Length == 10)
                        {
                            var invoice = models.GetTable<InvoiceItem>()
                                    .Where(i => i.TrackCode == no.Substring(0, 2))
                                    .Where(i => i.No == no.Substring(2))
                                    .OrderByDescending(i => i.InvoiceID)
                                    .FirstOrDefault();

                            if (invoice != null)
                            {
                                var request = invoice.CDS_Document.VoidInvoiceRequest;
                                if (request != null)
                                {
                                    request.CommitDate = DateTime.Now;
                                    models.SubmitChanges();

                                    if (code == "C")
                                    {
                                        models.CommitVoidInvoiceRequest(request);
                                    }
                                }
                                return invoice.InvoiceID;
                            }
                        }
                        break;

                    case "A0501":
                    case "C0501":
                    case "F0501":
                        var cancelItem = models.GetTable<InvoiceCancellation>()
                                .Where(i => i.CancellationNo == no)
                                .OrderByDescending(i => i.InvoiceID)
                                .FirstOrDefault();

                        var doc = cancelItem?.InvoiceItem.CDS_Document.ChildDocument.FirstOrDefault()?.CDS_Document;
                        if (doc != null)
                        {
                            if (code == "C")
                            {
                                doc.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_C, Naming.DataProcessStatus.Done);
                                models.SubmitChanges();
                                //Console.WriteLine($"InvoiceCancellation:({cancelItem.InvoiceID},{cancelItem.CancellationNo}) => C");
                                return doc.DocID;
                            }
                            else if (code == "E")
                            {
                                doc.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_E, Naming.DataProcessStatus.Done);
                                models.SubmitChanges();
                                //Console.WriteLine($"InvoiceCancellation:({cancelItem.InvoiceID},{cancelItem.CancellationNo}) => E");
                                return doc.DocID;
                            }
                        }
                        break;

                    case "B0401":
                    case "D0401":
                    case "G0401":
                        var allowance = models.GetTable<InvoiceAllowance>()
                                .Where(i => i.AllowanceNumber == no)
                                .OrderByDescending(i => i.AllowanceID)
                                .FirstOrDefault();

                        if (allowance != null)
                        {
                            if (code == "C")
                            {
                                allowance.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_C, Naming.DataProcessStatus.Done);
                                models.SubmitChanges();
                                //Console.WriteLine($"Allowance:({allowance.AllowanceID},{allowance.AllowanceNumber}) => C");
                                return allowance.AllowanceID;
                            }
                            else if (code == "E")
                            {
                                allowance.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_E, Naming.DataProcessStatus.Done);
                                models.SubmitChanges();
                                //Console.WriteLine($"Allowance:({allowance.AllowanceID},{allowance.AllowanceNumber}) => E");
                                return allowance.AllowanceID;
                            }
                        }
                        break;
                    case "B0501":
                    case "D0501":
                    case "G0501":
                        var cancelAllowance = models.GetTable<InvoiceAllowance>()
                                .Where(i => i.AllowanceNumber == no)
                                .OrderByDescending(i => i.AllowanceID)
                                .Select(a => a.InvoiceAllowanceCancellation)
                                .FirstOrDefault();

                        doc = cancelAllowance?.InvoiceAllowance.CDS_Document.ChildDocument.FirstOrDefault()?.CDS_Document;
                        if (doc != null)
                        {
                            if (code == "C")
                            {
                                doc.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_C, Naming.DataProcessStatus.Done);
                                models.SubmitChanges();
                                //Console.WriteLine($"AllowanceCancellation:({cancelAllowance.AllowanceID},{cancelAllowance.InvoiceAllowance.AllowanceNumber}) => C");
                                return doc.DocID;
                            }
                            else if (code == "E")
                            {
                                doc.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_E, Naming.DataProcessStatus.Done);
                                models.SubmitChanges();
                                //Console.WriteLine($"AllowanceCancellation:({cancelAllowance.AllowanceID},{cancelAllowance.InvoiceAllowance.AllowanceNumber}) => E");
                                return doc.DocID;
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }

        private static void CommitVoidInvoiceRequest(this GenericManager<EIVOEntityDataContext> models, VoidInvoiceRequest request)
        {
            var item = request.CDS_Document.InvoiceItem;
            request.InvoiceContent = item.GetJsonString();

            var c0401 = item.CreateF0401().ConvertToXml();
            models.GetTable<ExceptionLog>().InsertOnSubmit(new ExceptionLog
            {
                DataContent = c0401.OuterXml,
                CompanyID = item.SellerID,
                LogTime = DateTime.Now,
                TypeID = (int)Naming.DocumentTypeDefinition.E_InvoiceVoid,
                Message = $"發票註銷({item.TrackCode}{item.No})"
            });

            models.SubmitChanges();

            Naming.VoidActionMode? mode = (Naming.VoidActionMode?)request.RequestType;
            if (mode == Naming.VoidActionMode.註銷作廢
                || mode == Naming.VoidActionMode.索取紙本)
            {
                if (mode == Naming.VoidActionMode.索取紙本
                    && item.InvoiceCancellation == null)
                {
                    item.PrintMark = "Y";
                    models.DeleteAnyOnSubmit<InvoiceCarrier>(c => c.InvoiceID == item.InvoiceID);
                    models.SubmitChanges();
                }

                C0401Handler.PushStepQueueOnSubmit(models, request.CDS_Document, Naming.InvoiceStepDefinition.已開立);

                models.SubmitChanges();

                if (mode == Naming.VoidActionMode.註銷作廢)
                {
                    models.ExecuteCommand(@"DELETE FROM CDS_Document
                        FROM    DerivedDocument INNER JOIN
                                CDS_Document ON DerivedDocument.DocID = CDS_Document.DocID
                        WHERE   (DerivedDocument.SourceID = {0})", item.InvoiceID);
                    models.DeleteAny<InvoiceCancellation>(d => d.InvoiceID == item.InvoiceID);
                }
            }
            else if (mode == Naming.VoidActionMode.註銷重開)
            {
                String storedPath = Path.Combine(Logger.LogPath, "Archive").CheckStoredPath();
                c0401.Save(Path.Combine(storedPath, $"INV0401_{item.TrackCode}{item.No}_{DateTime.Now.Ticks}.xml"));

                request.CDS_Document.DocType = (int)Naming.DocumentTypeDefinition.E_InvoiceVoid;
                models.SubmitChanges();

                models.ExecuteCommand(@"DELETE FROM CDS_Document
                        FROM    DerivedDocument INNER JOIN
                                CDS_Document ON DerivedDocument.DocID = CDS_Document.DocID
                        WHERE   (DerivedDocument.SourceID = {0})", item.InvoiceID);
                models.ExecuteCommand("delete InvoiceItem where InvoiceID={0}", item.InvoiceID);
            }
        }
    }
}
