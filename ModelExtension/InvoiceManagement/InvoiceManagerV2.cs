using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;



using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement.InvoiceProcess;
using ModelCore.InvoiceManagement.zhTW;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using CommonLib.Utility;
using CommonLib.DataAccess;
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement
{
    public class InvoiceManagerV2 : InvoiceManager
    {
        public InvoiceManagerV2() : base() { }
        public InvoiceManagerV2(GenericManager<EIVOEntityDataContext> mgr) : base(mgr) { }

        protected Func<Exception> _checkUploadInvoice;
        protected Func<Exception> _checkUploadAllowance;
        public DateTime? ApplyInvoiceDate { get; set; }

        public bool IgnoreDuplicateDataNumberException { get; set; } = false;

        public override Dictionary<int, Exception> SaveUploadInvoice(InvoiceRoot item, OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();

            if (item != null && item.Invoice != null && item.Invoice.Length > 0)
            {
                EventItems = null;
                List<InvoiceItem> eventItem = new List<InvoiceItem>();

                //Organization donatory = owner.Organization.InvoiceWelfareAgencies.Select(w => w.WelfareAgency.Organization).FirstOrDefault();

                for (int idx = 0; idx < item.Invoice.Length; idx++)
                {
                    try
                    {
                        var invItem = item.Invoice[idx];

                        Exception ex;
                        Organization seller;
                        //InvoicePurchaseOrder order;
                        String trackCode, invNo;
                        DateTime invoiceDate;

                        if ((ex = invItem.CheckInvoice(this, out trackCode, out invNo, out invoiceDate)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }


                        if ((ex = invItem.CheckBusiness(this, owner, out seller)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        if (seller.OrganizationStatus.EnableTrackCodeInvoiceNoValidation == true
                            && (ex = invItem.CheckAppliedInvoiceNo(this, seller, invoiceDate, trackCode, invNo)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        //if ((ex = invItem.CheckDataNumber(this, out order)) != null)
                        //{
                        //    result.Add(idx, ex);
                        //    continue;
                        //}

                        if ((ex = invItem.CheckAmount()) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        #region 列印、捐贈、載具

                        bool all_printed = seller.OrganizationStatus.PrintAll == true;
                        bool print_mark = invItem.PrintMark == "y" || invItem.PrintMark == "Y";

                        InvoiceDonation donation;
                        if ((ex = invItem.CheckInvoiceDonation(seller, all_printed, print_mark, out donation)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        InvoiceCarrier carrier;

                        if ((ex = invItem.CheckInvoiceCarrier(seller, donation, all_printed, print_mark, out carrier)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        if ((ex = invItem.CheckMandatoryFields(seller, all_printed, print_mark)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        #endregion

                        IEnumerable<InvoiceProductItem> productItems;
                        if ((ex = invItem.CheckInvoiceProductItems(out productItems)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }


                        if (_checkUploadInvoice != null)
                        {
                            ex = _checkUploadInvoice();
                            if (ex != null)
                            {
                                result.Add(idx, ex);
                                continue;
                            }
                        }

                        InvoiceItem newItem = createInvoiceItem(owner, invItem, seller, null, print_mark, all_printed, carrier, donation, productItems);
                        newItem.No = invNo;
                        newItem.TrackCode = trackCode;
                        newItem.InvoiceDate = invoiceDate;

                        this.EntityList.InsertOnSubmit(newItem);
                        this.SubmitChanges();

                        eventItem.Add(newItem);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        result.Add(idx, ex);
                    }
                }

                if (eventItem.Count > 0)
                {
                    HasItem = true;
                }

                EventItems = eventItem;

            }
            return result;
        }

        public override Dictionary<int, Exception> SaveUploadInvoiceCancellation(CancelInvoiceRoot item, OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();
            if (item != null && item.CancelInvoice != null && item.CancelInvoice.Length > 0)
            {
                EventItems = null;
                List<InvoiceItem> eventItems = new List<InvoiceItem>();

                for (int idx = 0; idx < item.CancelInvoice.Length; idx++)
                {
                    var invItem = item.CancelInvoice[idx];
                    try
                    {
                        Exception ex;
                        InvoiceItem invoice;
                        DateTime cancelDate;
                        if ((ex = invItem.CheckMandatoryFields(this, owner, out invoice, out cancelDate)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        DerivedDocument doc = null;
                        InvoiceCancellation cancelItem = invoice.PrepareVoidItem(this, ref doc);
                        cancelItem.Remark = invItem.Remark;
                        cancelItem.CancelDate = cancelDate;
                        cancelItem.CancelReason = invItem.CancelReason;
                        cancelItem.ReturnTaxDocumentNo = invItem.ReturnTaxDocumentNumber;

                        this.SubmitChanges();

                        eventItems.Add(cancelItem.InvoiceItem);


                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        result.Add(idx, ex);
                    }
                }

                if (eventItems.Count > 0)
                {
                    HasItem = true;
                }

                EventItems = eventItems;
            }
            return result;
        }

        public override Dictionary<int, Exception> SaveUploadAllowance(AllowanceRoot root, OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();
            if (root != null && root.Allowance != null && root.Allowance.Length > 0)
            {
                var table = this.GetTable<InvoiceAllowance>();
                var tblOrg = this.GetTable<Organization>();
                var invTable = this.GetTable<InvoiceItem>();

                this.EventItems_Allowance = null;
                List<InvoiceAllowance> eventItems = new List<InvoiceAllowance>();

                for (int idx = 0; idx < root.Allowance.Length; idx++)
                {
                    var allowanceItem = root.Allowance[idx];
                    try
                    {

                        Exception ex;
                        Organization seller;
                        if ((ex = allowanceItem.CheckBusiness(this, owner, out seller)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        DateTime allowanceDate;
                        if ((ex = allowanceItem.CheckMandatoryFields(this, out allowanceDate)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        List<InvoiceAllowanceItem> productItems;
                        InvoiceItem originalInvoice;
                        if ((ex = allowanceItem.CheckAllowanceItem(this, out productItems,out originalInvoice)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        if (allowanceDate.AddDays(1) < originalInvoice.InvoiceDate)
                        {
                            allowanceDate = originalInvoice.InvoiceDate.Value.AddDays(1);
                        }

                        InvoiceAllowance newItem = new InvoiceAllowance
                        {
                            CDS_Document = new CDS_Document
                            {
                                DocDate = DateTime.Now,
                                DocType = (int)Naming.DocumentTypeDefinition.E_Allowance,
                                ProcessType = originalInvoice.CDS_Document.ProcessType == (int)Naming.InvoiceProcessType.A0401
                                    ? (int)Naming.InvoiceProcessType.B0401
                                    : (int)Naming.InvoiceProcessType.D0401,
                            },
                            AllowanceDate = allowanceDate,
                            IssueDate = allowanceDate,
                            AllowanceNumber = allowanceItem.AllowanceNumber,
                            AllowanceType = allowanceItem.AllowanceType,
                            BuyerId = allowanceItem.BuyerId,
                            SellerId = allowanceItem.SellerId,
                            TaxAmount = allowanceItem.TaxAmount,
                            TotalAmount = allowanceItem.TotalAmount,
                            //InvoiceID =  invTable.Where(i=>i.TrackCode + i.No == item.AllowanceItem.Select(a=>a.OriginalInvoiceNumber).FirstOrDefault()).Select(i=>i.InvoiceID).FirstOrDefault(),

                            InvoiceAllowanceBuyer = new InvoiceAllowanceBuyer
                            {
                                Name = allowanceItem.BuyerName,
                                ReceiptNo = allowanceItem.BuyerId,
                                CustomerID = String.IsNullOrEmpty(allowanceItem.GoogleId) ? "" : allowanceItem.GoogleId,
                                ContactName = allowanceItem.BuyerName,
                                CustomerName = allowanceItem.BuyerName
                            },
                            InvoiceAllowanceSeller = new InvoiceAllowanceSeller
                            {
                                Name = seller.CompanyName,
                                ReceiptNo = seller.ReceiptNo,
                                Address = seller.Addr,
                                ContactName = seller.ContactName,
                                CustomerID = String.IsNullOrEmpty(allowanceItem.GoogleId) ? "" : allowanceItem.GoogleId,
                                CustomerName = seller.CompanyName,
                                EMail = seller.ContactEmail,
                                Fax = seller.Fax,
                                Phone = seller.Phone,
                                PersonInCharge = seller.UndertakerName,
                                SellerID = seller.CompanyID,
                            },
                        };

                        newItem.InvoiceAllowanceDetails.AddRange(productItems.Select(p => new InvoiceAllowanceDetail
                        {
                            InvoiceAllowanceItem = p
                        }));

                        if (owner != null)
                        {
                            newItem.CDS_Document.DocumentOwner = new DocumentOwner
                            {
                                OwnerID = owner.CompanyID,
                            };
                        }

                        table.InsertOnSubmit(newItem);
                        if (newItem.CDS_Document.ProcessType == (int)Naming.InvoiceProcessType.D0401)
                        {
                            D0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, seller.StepReadyToAllowanceMIG());
                            D0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
                        }
                        else
                        {
                            B0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, seller.StepReadyToAllowanceMIG());
                            B0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
                        }

                        this.SubmitChanges();

                        eventItems.Add(newItem);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        result.Add(idx, ex);
                    }
                }

                if (eventItems.Count > 0)
                {
                    HasItem = true;
                }

                EventItems_Allowance = eventItems;
            }
            return result;
        }

        public override Dictionary<int, Exception> SaveUploadAllowanceCancellation(CancelAllowanceRoot root, OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();
            if (root != null && root.CancelAllowance != null && root.CancelAllowance.Length > 0)
            {
                var tblAllowance = this.GetTable<InvoiceAllowance>();
                var tblCancel = this.GetTable<InvoiceAllowanceCancellation>();

                EventItems = null;
                EventItems_Allowance = null;
                List<InvoiceAllowance> eventItems = new List<InvoiceAllowance>();

                for (int idx = 0; idx < root.CancelAllowance.Length; idx++)
                {
                    var item = root.CancelAllowance[idx];
                    try
                    {
                        Exception ex;
                        InvoiceAllowanceCancellation voidItem = null;
                        DerivedDocument p = null;

                        if ((ex = item.VoidAllowance(this, owner.Organization, ref voidItem,ref p)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        eventItems.Add(voidItem.InvoiceAllowance);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        result.Add(idx, ex);
                    }
                }

                if (eventItems.Count > 0)
                {
                    HasItem = true;
                }

                EventItems_Allowance = eventItems;
            }
            return result;
        }

        //public override Dictionary<int, Exception> SaveUploadInvoiceAutoTrackNo(InvoiceRoot item, OrganizationToken owner)
        //{
        //    Dictionary<int, Exception> result = new Dictionary<int, Exception>();
        //    TrackNoManager trackNoMgr = new TrackNoManager(this, owner.CompanyID);

        //    if (item != null && item.Invoice != null && item.Invoice.Length > 0)
        //    {
        //        //Organization donatory = owner.Organization.InvoiceWelfareAgencies.Select(w => w.WelfareAgency.Organization).FirstOrDefault();
        //        EventItems = null;
        //        List<InvoiceItem> eventItems = new List<InvoiceItem>();

        //        for (int idx = 0; idx < item.Invoice.Length; idx++)
        //        {
        //            try
        //            {
        //                var invItem = item.Invoice[idx];

        //                Exception ex;
        //                Organization seller;
        //                InvoicePurchaseOrder order;
        //                if ((ex = invItem.CheckBusiness(this, owner, out seller)) != null)
        //                {
        //                    result.Add(idx, ex);
        //                    continue;
        //                }

        //                if ((ex = invItem.CheckDataNumber(seller, this, out order)) != null)
        //                {
        //                    result.Add(idx, ex);
        //                    continue;
        //                }

        //                if ((ex = invItem.CheckAmount()) != null)
        //                {
        //                    result.Add(idx, ex);
        //                    continue;
        //                }

        //                #region 列印、捐贈、載具

        //                bool all_printed = seller.OrganizationStatus.PrintAll == true;
        //                bool print_mark = invItem.PrintMark == "y" || invItem.PrintMark == "Y";

        //                InvoiceDonation donation;
        //                if ((ex = invItem.CheckInvoiceDonation(seller, all_printed, print_mark, out donation)) != null)
        //                {
        //                    result.Add(idx, ex);
        //                    continue;
        //                }

        //                InvoiceCarrier carrier;

        //                if ((ex = invItem.CheckInvoiceCarrier(seller, donation, all_printed, print_mark, out carrier)) != null)
        //                {
        //                    result.Add(idx, ex);
        //                    continue;
        //                }

        //                if ((ex = invItem.CheckMandatoryFields(seller, all_printed, print_mark)) != null)
        //                {
        //                    result.Add(idx, ex);
        //                    continue;
        //                }

        //                #endregion

        //                IEnumerable<InvoiceProductItem> productItems;
        //                if ((ex = invItem.CheckInvoiceProductItems(out productItems)) != null)
        //                {
        //                    result.Add(idx, ex);
        //                    continue;
        //                }

        //                InvoiceItem newItem = createInvoiceItem(owner, invItem, seller, order, print_mark, all_printed, carrier, donation, productItems);

        //                if (!trackNoMgr.CheckInvoiceNo(newItem))
        //                {
        //                    result.Add(idx, new Exception("未設定發票字軌或發票號碼已用完"));
        //                    continue;
        //                }
        //                else
        //                {
        //                    newItem.InvoiceDate = DateTime.Now;
        //                }

        //                this.EntityList.InsertOnSubmit(newItem);
        //                this.SubmitChanges();

        //                eventItems.Add(newItem);
        //            }
        //            catch (Exception ex)
        //            {
        //                Logger.Error(ex);
        //                result.Add(idx, ex);
        //            }
        //        }

        //        if (eventItems.Count > 0)
        //        {
        //            HasItem = true;
        //        }
        //        EventItems = eventItems;
        //    }

        //    trackNoMgr.Dispose();
        //    return result;
        //}

        protected InvoiceItem createInvoiceItem(OrganizationToken owner, InvoiceRootInvoice invItem, Organization seller, InvoicePurchaseOrder order, bool Final_printed, bool all_printed, InvoiceCarrier carrier, InvoiceDonation donation, IEnumerable<InvoiceProductItem> productItems)
        {
            InvoiceItem newItem = new InvoiceItem
            {
                CDS_Document = new CDS_Document
                {
                    DocDate = DateTime.Now,
                    DocType = (int)Naming.DocumentTypeDefinition.E_Invoice,
                    ProcessType = (int)Naming.InvoiceProcessType.C0401,
                },
                DonateMark = donation == null ? "0" : "1",
                InvoiceType = byte.Parse(invItem.InvoiceType),
                //No = invNo,
                //TrackCode = trackCode,
                SellerID = seller.CompanyID,
                CustomsClearanceMark = invItem.CustomsClearanceMark,
                InvoiceSeller = new InvoiceSeller
                {
                    Name = seller.CompanyName,
                    ReceiptNo = seller.ReceiptNo,
                    Address = seller.Addr,
                    ContactName = seller.ContactName,
                    //CustomerID = String.IsNullOrEmpty(invItem.GoogleId) ? "" : invItem.GoogleId,
                    CustomerName = seller.CompanyName,
                    EMail = seller.ContactEmail,
                    Fax = seller.Fax,
                    Phone = seller.Phone,
                    PersonInCharge = seller.UndertakerName,
                    SellerID = seller.CompanyID,
                },
                InvoiceBuyer = new InvoiceBuyer
                {
                    BuyerMark = invItem.BuyerMark,
                    Name = invItem.BuyerId == "0000000000" ? invItem.BuyerName.CheckB2CMIGName() : invItem.BuyerName,
                    ReceiptNo = invItem.BuyerId,
                    CustomerID = String.IsNullOrEmpty(invItem.GoogleId) ? "" : invItem.GoogleId,
                    ContactName = invItem.Contact != null ? (String.IsNullOrEmpty(invItem.Contact.Name) ? "" : invItem.Contact.Name) : "",
                    Address = invItem.Contact != null ? (String.IsNullOrEmpty(invItem.Contact.Address) ? "" : invItem.Contact.Address) : "",
                    Phone = invItem.Contact != null ? (String.IsNullOrEmpty(invItem.Contact.TEL) ? "" : invItem.Contact.TEL) : "",
                    EMail = invItem.Contact != null ? (String.IsNullOrEmpty(invItem.Contact.Email) ? "" : invItem.Contact.Email.Replace(',', ';').Replace(' ', ';').Replace('、', ';')) : "",
                    CustomerName = invItem.BuyerName,
                },
                //InvoiceByHousehold = carrier != null ? new InvoiceByHousehold { InvoiceUserCarrier = carrier } : null,
                //InvoicePrintAssertion = bPrinted ? new InvoicePrintAssertion { PrintDate = DateTime.Now } : null,
                RandomNo = String.IsNullOrEmpty(invItem.RandomNumber) ? ValidityAgent.GenerateRandomCode(4) : invItem.RandomNumber,
                InvoiceAmountType = new InvoiceAmountType
                {
                    DiscountAmount = invItem.DiscountAmount,
                    SalesAmount = invItem.SalesAmount,
                    FreeTaxSalesAmount = invItem.FreeTaxSalesAmount,
                    ZeroTaxSalesAmount = invItem.ZeroTaxSalesAmount,
                    TaxAmount = invItem.TaxAmount,
                    TaxRate = invItem.TaxRate,
                    TaxType = invItem.TaxType,
                    TotalAmount = invItem.TotalAmount,
                    TotalAmountInChinese = ValidityAgent.MoneyShow(invItem.TotalAmount),
                    BondedAreaConfirm = invItem.BondedAreaConfirm,
                },
                //DonationID = donatory != null ? donatory.CompanyID : (int?)null,
                InvoiceCarrier = carrier,
                InvoiceDonation = donation,
                PrintMark = (invItem.BuyerId != "0000000000") == true ? "Y" : ((all_printed && !Final_printed) == true ? "N" : (Final_printed == true ? "Y" : "N")),
            };

            if (order != null)
            {
                newItem.InvoicePurchaseOrder = order;
            }

            newItem.InvoiceDetails.AddRange(productItems.Select(p => new InvoiceDetail
            {
                InvoiceProduct = p.InvoiceProduct,
            }));

            if (owner != null)
            {
                newItem.CDS_Document.DocumentOwner = new DocumentOwner
                {
                    OwnerID = owner.CompanyID,
                };
            }

            C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已開立);
            C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);

            return newItem;
        }


        //public Dictionary<int, Exception> SaveUploadInvoice_C0401(ModelCore.Schema.TurnKey.C0401.Invoice invoice, OrganizationToken owner)
        //{
        //    Dictionary<int, Exception> result = new Dictionary<int, Exception>();
        //    List<InvoiceItem> eventItem = new List<InvoiceItem>();

        //    int idx = 0;
        //    try
        //    {
        //        Exception ex;
        //        Organization seller;
        //        String trackCode, invNo;
        //        DateTime invoiceDate;

        //        if ((ex = InvoiceRootValidator_C0401.CheckDataLength(invoice)) != null)
        //        {
        //            result.Add(idx, ex);
        //            idx = idx + 1;
        //        }

        //        if ((ex = InvoiceRootValidator_C0401.CheckInvoice(invoice, this, out trackCode, out invNo, out invoiceDate)) != null)
        //        {
        //            result.Add(idx, ex);
        //            idx = idx + 1;
        //        }

        //        if ((ex = InvoiceRootValidator_C0401.CheckBusiness(invoice, this, owner, out seller)) != null)
        //        {
        //            result.Add(idx, ex);
        //            idx = idx + 1;
        //        }

        //        if ((ex = invoice.CheckAmount()) != null)
        //        {
        //            result.Add(idx, ex);
        //            idx = idx + 1;
        //        }


        //        #region 列印、捐贈、載具

        //        bool all_printed = seller.OrganizationStatus.PrintAll == true;
        //        bool print_mark = invoice.Main.PrintMark == "y" || invoice.Main.PrintMark == "Y";

        //        InvoiceDonation donation;
        //        if ((ex = Check_CarrierType_PrintMark_DonateMark.CheckInvoiceDonation_C0401(invoice, seller, all_printed, print_mark, out donation)) != null)
        //        {
        //            result.Add(idx, ex);
        //            idx = idx + 1;
        //        }

        //        InvoiceCarrier carrier;

        //        if ((ex = Check_CarrierType_PrintMark_DonateMark.CheckInvoiceCarrier_C0401(invoice, seller, donation, all_printed, print_mark, out carrier)) != null)
        //        {
        //            result.Add(idx, ex);
        //            idx = idx + 1;
        //        }

        //        if ((ex = Check_CarrierType_PrintMark_DonateMark.CheckMandatoryFields_C0401(invoice, seller, all_printed, print_mark)) != null)
        //        {
        //            result.Add(idx, ex);
        //            idx = idx + 1;
        //        }

        //        #endregion

        //        IEnumerable<InvoiceProductItem> productItems;
        //        if ((ex = InvoiceRootValidator_C0401.CheckInvoiceProductItems(invoice, out productItems)) != null)
        //        {
        //            result.Add(idx, ex);
        //            idx = idx + 1;
        //        }

        //        if (_checkUploadInvoice != null)
        //        {
        //            ex = _checkUploadInvoice();
        //            if (ex != null)
        //            {
        //                result.Add(idx, ex);
        //                idx = idx + 1;
        //            }
        //        }


        //        if (result.Count == 0)
        //        {
        //            InvoiceItem newItem = ConvertToInvoiceItem(invoice, owner, invoiceDate, carrier, seller, all_printed, print_mark);
        //            this.EntityList.InsertOnSubmit(newItem);
        //            this.SubmitChanges();
        //            eventItem.Add(newItem);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex);
        //        result.Add(idx, ex);
        //    }

        //    if (eventItem.Count > 0)
        //    {
        //        HasItem = true;
        //    }


        //    return result;
        //}

        //private InvoiceItem ConvertToInvoiceItem(Schema.TurnKey.C0401.Invoice invoice, OrganizationToken owner, DateTime invoiceDate, InvoiceCarrier carrier, Organization seller, bool all_printed, bool Final_printed)
        //{
        //    String invNo, trackCode;
        //    getInvoiceNo(invoice.Main.InvoiceNumber, out invNo, out trackCode);

        //    InvoiceItem newItem = new InvoiceItem
        //    {
        //        CDS_Document = new CDS_Document
        //        {
        //            DocDate = DateTime.Now,
        //            DocType = (int)Naming.DocumentTypeDefinition.E_Invoice,
        //            DocumentOwner = new DocumentOwner
        //            {
        //                OwnerID = owner.CompanyID
        //            },
        //            //CurrentStep = (int)Naming.B2BInvoiceStepDefinition.待傳送
        //        },
        //        InvoiceBuyer = new InvoiceBuyer
        //        {
        //            BuyerMark = invoice.Main.BuyerRemarkSpecified ? (int)invoice.Main.BuyerRemark : (int?)null,
        //            Name = invoice.Main.Buyer.Name,
        //            ReceiptNo = invoice.Main.Buyer.Identifier,
        //            ContactName = invoice.Main.Buyer.PersonInCharge,
        //            Address = invoice.Main.Buyer.Address,
        //            //CustomerID = invoice.Main.Buyer.CustomerNumber,
        //            CustomerName = invoice.Main.Buyer.Name,
        //            EMail = invoice.Main.Buyer.EmailAddress,
        //            Fax = invoice.Main.Buyer.FacsimileNumber,
        //            PersonInCharge = invoice.Main.Buyer.PersonInCharge,
        //            Phone = invoice.Main.Buyer.TelephoneNumber,
        //            RoleRemark = invoice.Main.Buyer.RoleRemark,
        //            CustomerNumber = invoice.Main.Buyer.CustomerNumber,
        //        },
        //        InvoiceSeller = new InvoiceSeller
        //        {
        //            Name = invoice.Main.Seller.Name,
        //            ReceiptNo = invoice.Main.Seller.Identifier,
        //            ContactName = invoice.Main.Seller.PersonInCharge,
        //            Address = invoice.Main.Seller.Address,
        //            CustomerID = invoice.Main.Seller.CustomerNumber,
        //            CustomerName = invoice.Main.Seller.Name,
        //            EMail = invoice.Main.Seller.EmailAddress,
        //            Fax = invoice.Main.Seller.FacsimileNumber,
        //            PersonInCharge = invoice.Main.Seller.PersonInCharge,
        //            Phone = invoice.Main.Seller.TelephoneNumber,
        //            RoleRemark = invoice.Main.Seller.RoleRemark,
        //            SellerID = seller.CompanyID,
        //        },
        //        InvoiceDate = invoiceDate,
        //        InvoiceType = (byte)((int)invoice.Main.InvoiceType),
        //        No = invNo,
        //        TrackCode = trackCode,
        //        Organization = seller,
        //        BuyerRemark = (byte?)invoice.Main.BuyerRemark,
        //        Category = invoice.Main.Category,
        //        CheckNo = invoice.Main.CheckNumber,
        //        DonateMark = ((int)invoice.Main.DonateMark).ToString(),
        //        InvoiceDonation = new InvoiceDonation
        //        {
        //            AgencyCode = invoice.Main.NPOBAN,
        //        },
        //        CustomsClearanceMark = (byte?)invoice.Main.CustomsClearanceMark,
        //        GroupMark = invoice.Main.GroupMark,
        //        //PermitDate = String.IsNullOrEmpty(invoice.Main.PermitDate) ? (DateTime?)null : DateTime.ParseExact(invoice.Main.PermitDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture),
        //        //PermitNumber = invoice.Main.PermitNumber,
        //        //PermitWord = invoice.Main.PermitWord,
        //        RelateNumber = invoice.Main.RelateNumber,
        //        Remark = invoice.Main.MainRemark,
        //        InvoiceCarrier = carrier,
        //        PrintMark = (invoice.Main.Buyer.Identifier != "0000000000") == true ? "Y" : ((all_printed && !Final_printed) == true ? "N" : (Final_printed == true ? "Y" : "N")),
        //        RandomNo = invoice.Main.RandomNumber,
        //        //TaxCenter = invoice.Main.TaxCenter,
        //        InvoiceAmountType = new InvoiceAmountType
        //        {
        //            DiscountAmount = invoice.Amount.DiscountAmountSpecified ? invoice.Amount.DiscountAmount : (decimal?)null,
        //            SalesAmount = (int)invoice.Amount.TaxType == 1 ? (decimal)invoice.Amount.SalesAmount :
        //                           (int)invoice.Amount.TaxType == 2 ? (decimal)invoice.Amount.ZeroTaxSalesAmount :
        //                           (int)invoice.Amount.TaxType == 3 ? (decimal)invoice.Amount.FreeTaxSalesAmount : invoice.Amount.SalesAmount,
        //            TaxAmount = invoice.Amount.TaxAmount,
        //            TaxType = (byte)((int)invoice.Amount.TaxType),
        //            TotalAmount = invoice.Amount.TotalAmount,
        //            TotalAmountInChinese = ValidityAgent.MoneyShow(invoice.Amount.TotalAmount),
        //            TaxRate = invoice.Amount.TaxRate,
        //            CurrencyID = invoice.Amount.CurrencySpecified ? (int)invoice.Amount.Currency : (int?)null,
        //            ExchangeRate = invoice.Amount.ExchangeRateSpecified ? invoice.Amount.ExchangeRate : (decimal?)null,
        //            OriginalCurrencyAmount = invoice.Amount.OriginalCurrencyAmountSpecified ? invoice.Amount.OriginalCurrencyAmount : (decimal?)null
        //        }
        //    };

        //    short seqNo = 1;

        //    var productItems = invoice.Details.Select(i => new InvoiceProductItem
        //    {
        //        InvoiceProduct = new InvoiceProduct { Brief = i.Description },
        //        CostAmount = i.Amount,
        //        ItemNo = i.SequenceNumber,
        //        Piece = (int?)i.Quantity,
        //        PieceUnit = i.Unit,
        //        UnitCost = i.UnitPrice,
        //        Remark = i.Remark,
        //        TaxType = newItem.InvoiceAmountType.TaxType,
        //        RelateNumber = i.RelateNumber,
        //        No = (seqNo++)
        //    }).ToList();

        //    newItem.InvoiceDetails.AddRange(productItems.Select(p => new InvoiceDetail
        //    {
        //        InvoiceProduct = p.InvoiceProduct
        //    }));
        //    return newItem;
        //}

    }
}