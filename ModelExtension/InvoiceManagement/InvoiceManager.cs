using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;


using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement.ErrorHandle;
using ModelCore.InvoiceManagement.InvoiceProcess;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using CommonLib.Utility;
using CommonLib.DataAccess;
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement
{
    public class InvoiceManager: EIVOEntityManager<InvoiceItem>
    {

        public InvoiceManager() : base() { }
        public InvoiceManager(GenericManager<EIVOEntityDataContext> mgr) : base(mgr) { }

        public bool HasItem { get; set; }

        public List<InvoiceAllowance> EventItems_Allowance
        {
            get;
            protected set;
        }

        public String? InvoiceClientID
        { get; set; }

        public int? ChannelID { get; set; }

        public Naming.InvoiceProcessType? ProcessType { get; set; }

        public virtual Dictionary<int, Exception> SaveUploadInvoice(InvoiceRoot item,OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();
            if (item != null && item.Invoice != null && item.Invoice.Length > 0)
            {
                EventItems = null;
                List<InvoiceItem> eventItems = new List<InvoiceItem>();

                Organization donatory = owner.Organization.InvoiceWelfareAgencies.Select(w => w.WelfareAgency.Organization).FirstOrDefault();
                for (int idx=0;idx< item.Invoice.Length;idx++)
                {
                    var invItem = item.Invoice[idx];
                    try
                    {
                        String invNo, trackCode;
                        invItem.InvoiceNumber.GetInvoiceNo(out invNo, out trackCode);

                        if (this.EntityList.Any(i => i.No == invNo && i.TrackCode == trackCode))
                        {
                            result.Add(idx, new Exception(String.Format("發票號碼已存在:{0}", invItem.InvoiceNumber)));
                            continue;
                        }

                        var seller = this.GetTable<Organization>().Where(o => o.ReceiptNo == invItem.SellerId).FirstOrDefault();
                        if (seller == null)
                        {
                            result.Add(idx, new Exception(String.Format("賣方為非註冊營業人,統一編號:{0}", invItem.SellerId)));
                            continue;
                        }

                        if (seller.OrganizationStatus.CurrentLevel == (int)Naming.MemberStatusDefinition.Mark_To_Delete)
                        {
                            result.Add(idx,new Exception(String.Format("開立人已註記停用,開立人統一編號:{0}，TAG:<SellerId />", invItem.SellerId)));
                            continue;
                        }

                        //Organization donatory = null;
                        //bool bPrinted = invItem.PrintMark == "Y";
                        //if (invItem.DonateMark == "1")
                        //{
                        //    if (bPrinted)
                        //    {
                        //        result.Add(invItem.InvoiceNumber, new Exception(String.Format("發票已列印後不能捐贈,發票號碼:{0}", invItem.InvoiceNumber)));
                        //        continue;
                        //    }
                        //    donatory = this.GetTable<Organization>().Where(o => o.ReceiptNo == invItem.NPOBAN).FirstOrDefault();
                        //    if (donatory == null || !this.GetTable<InvoiceWelfareAgency>().Any(w => w.AgencyID == donatory.CompanyID && w.SellerID == seller.CompanyID))
                        //    {
                        //        result.Add(invItem.InvoiceNumber, new Exception(String.Format("發票受捐社福單位不符,統一編號:{0}", invItem.NPOBAN)));
                        //        continue;
                        //    }
                        //}

                        ///EIVO03網購業者電子發票不需要載具歸戶
                        ///
                        //InvoiceUserCarrier carrier = null;
                        //if (bPrinted)
                        //{
                        //    if (!String.IsNullOrEmpty(invItem.CarrierType) || !String.IsNullOrEmpty(invItem.CarrierId1) || !String.IsNullOrEmpty(invItem.CarrierId2))
                        //    {
                        //        result.Add(invItem.InvoiceNumber, new Exception(String.Format("發票已列印後不能指定歸戶,發票號碼:{0}", invItem.InvoiceNumber)));
                        //        continue;
                        //    }
                        //}
                        //else
                        //{
                        //    carrier = checkInvoiceCarrier(invItem);
                        //    if (carrier == null)
                        //    {
                        //        result.Add(invItem.InvoiceNumber, new Exception(String.Format("發票歸戶載具或卡號不符,發票號碼:{0}", invItem.InvoiceNumber)));
                        //        continue;
                        //    }
                        //}

                        InvoiceItem newItem = new InvoiceItem
                        {
                            CDS_Document = new CDS_Document
                            {
                                DocDate = DateTime.Now,
                                DocType = (int)Naming.DocumentTypeDefinition.E_Invoice,
                                ProcessType = (int)Naming.InvoiceProcessType.C0401,
                            },
                            DonateMark = "0",
                            PrintMark = "Y",
                            InvoiceBuyer = new InvoiceBuyer
                            {
                                Name = String.IsNullOrEmpty(invItem.BuyerId) ? invItem.BuyerName.CheckB2CMIGName() : invItem.BuyerName,
                                ReceiptNo = String.IsNullOrEmpty(invItem.BuyerId) ? "0000000000" : invItem.BuyerId,
                                CustomerName = invItem.BuyerName,
                                BuyerMark = invItem.BuyerMark,
                            },
                            InvoiceDate = DateTime.ParseExact(String.Format("{0} {1}", invItem.InvoiceDate, invItem.InvoiceTime), "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture),
                            InvoiceType = byte.Parse(invItem.InvoiceType),
                            No = invNo,
                            TrackCode = trackCode,
                            SellerID = seller.CompanyID,
                            InvoiceSeller = new InvoiceSeller
                            {
                                Name = seller.CompanyName,
                                ReceiptNo = seller.ReceiptNo,
                                Address = seller.Addr,
                                ContactName = seller.ContactName,
                                CustomerName = seller.CompanyName,
                                EMail = seller.ContactEmail,
                                Fax = seller.Fax,
                                Phone = seller.Phone,
                                PersonInCharge = seller.UndertakerName,
                                SellerID = seller.CompanyID,
                            },
                            //InvoiceByHousehold = carrier != null ? new InvoiceByHousehold { InvoiceUserCarrier = carrier } : null,
                            //InvoicePrintAssertion = bPrinted ? new InvoicePrintAssertion { PrintDate = DateTime.Now } : null,
                            RandomNo = ValidityAgent.GenerateRandomCode(4),//"AAAA",
                            InvoiceAmountType = new InvoiceAmountType
                            {
                                DiscountAmount = invItem.DiscountAmount,
                                SalesAmount = (invItem.TaxType == 1) ? invItem.SalesAmount :
                                        (invItem.TaxType == 2) ? invItem.ZeroTaxSalesAmount :
                                        (invItem.TaxType == 3) ? invItem.FreeTaxSalesAmount : invItem.SalesAmount,
                                TaxAmount = invItem.TaxAmount,
                                TaxRate = invItem.TaxRate,
                                TaxType = invItem.TaxType,
                                TotalAmount = invItem.TotalAmount,
                                TotalAmountInChinese = ValidityAgent.MoneyShow(invItem.TotalAmount),
                                BondedAreaConfirm = invItem.BondedAreaConfirm,
                            },
                            DonationID = donatory != null ? donatory.CompanyID : (int?)null
                        };

                        checkPrintMarkAndCarrier(newItem, invItem, seller);

                        short seqNo = 1;

                        var productItems = invItem.InvoiceItem.Select(i => new InvoiceProductItem
                        {
                            InvoiceProduct = new InvoiceProduct { Brief = i.Description },
                            CostAmount = i.Amount,
                            ItemNo = i.Item,
                            Piece = i.Quantity,
                            PieceUnit = i.Unit,
                            UnitCost = i.UnitPrice,
                            Remark = i.Remark,
                            TaxType = i.TaxType,
                            No = (seqNo++)
                        }).ToList();

                        newItem.InvoiceDetails.AddRange(productItems.Select(p => new InvoiceDetail
                        {
                            InvoiceProduct = p.InvoiceProduct
                        }));

                        if (owner != null)
                        {
                            newItem.CDS_Document.DocumentOwner = new DocumentOwner
                            {
                                OwnerID = owner.CompanyID
                            };
                        }

                        this.EntityList.InsertOnSubmit(newItem);

                        C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已開立);
                        C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);

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

                EventItems = eventItems;
            }

            return result;
        }

        private void checkPrintMarkAndCarrier(InvoiceItem item, InvoiceRootInvoice invItem, Organization seller)
        {
            ///B2C的發票=>列印或歸戶
            ///
            if (invItem.BuyerId == "0000000000")
            {
                if (seller.OrganizationStatus.PrintAll == true)
                {
                    item.PrintMark = "Y";
                }
                else
                {
                    item.PrintMark = "N";
                    String carrierNo = Guid.NewGuid().ToString();
                    item.InvoiceCarrier = new InvoiceCarrier
                    {
                        CarrierType = EIVOTurnkeyFactory.DefaultUserCarrierType,
                        CarrierNo = carrierNo,
                        CarrierNo2 = carrierNo
                    };
                }
            }
            ///B2B的發票=>全列印
            ///
            else
            {
                item.PrintMark = "Y";
            }
        }

        private InvoiceUserCarrier checkInvoiceCarrier(InvoiceRootInvoice invItem)
        {
            Expression<Func<InvoiceUserCarrier, bool>> expr = null;
            if (!String.IsNullOrEmpty(invItem.CarrierId1))
            {
                expr = c => c.CarrierNo == invItem.CarrierId1;
            }
            if (!String.IsNullOrEmpty(invItem.CarrierId2))
            {
                if (expr == null)
                {
                    expr = c => c.CarrierNo2 == invItem.CarrierId2;
                }
                else
                {
                    expr = expr.And(c => c.CarrierNo2 == invItem.CarrierId2);
                }
            }

            if (expr == null)
                return null;

            var carrier = this.GetTable<InvoiceUserCarrier>().Where(expr).FirstOrDefault();

            if (carrier == null)
            {
                if (String.IsNullOrEmpty(invItem.CarrierType))
                    return null;
                var carrierType = this.GetTable<InvoiceUserCarrierType>().Where(t => t.CarrierType == invItem.CarrierType).FirstOrDefault();
                if (carrierType == null)
                {
                    //carrierType = new InvoiceUserCarrierType
                    //{
                    //    CarrierType = invItem.CarrierType
                    //};
                    return null;
                }
                carrier = new InvoiceUserCarrier
                    {
                        InvoiceUserCarrierType = carrierType,
                        CarrierNo = invItem.CarrierId1,
                        CarrierNo2 = invItem.CarrierId2
                    };
            }

            return carrier;
        }

        public virtual Dictionary<int, Exception> SaveUploadInvoiceCancellation(CancelInvoiceRoot item, OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();
            if (item != null && item.CancelInvoice != null && item.CancelInvoice.Length > 0)
            {
                EventItems = null;
                var eventItem = new List<InvoiceItem>();

                for (int idx=0;idx<item.CancelInvoice.Length;idx++)
                {
                    var invItem = item.CancelInvoice[idx];
                    try
                    {

                        String invNo, trackCode;
                        if (invItem.CancelInvoiceNumber.Length >= 10)
                        {
                            trackCode = invItem.CancelInvoiceNumber.Substring(0, 2);
                            invNo = invItem.CancelInvoiceNumber.Substring(2);
                        }
                        else
                        {
                            trackCode = null;
                            invNo = invItem.CancelInvoiceNumber;
                        }
                        invItem.SellerId = invItem.SellerId.GetEfficientString();
                        var invoice = this.EntityList.Where(i => i.No == invNo && i.TrackCode == trackCode)
                            .OrderByDescending(i => i.InvoiceID)
                            .FirstOrDefault();

                        if (invoice == null)
                        {
                            result.Add(idx, new MarkToRetryException(String.Format("發票號碼不存在:{0}", invItem.CancelInvoiceNumber)));
                            continue;
                        }
                        else if (invoice.Organization.ReceiptNo != invItem.SellerId)
                        {
                            result.Add(idx, new Exception(String.Format("開立人統編錯誤:{0}", invItem.SellerId)));
                            continue;
                        }

                        if (invoice.SellerID != owner.CompanyID)
                        {
                            result.Add(idx, new Exception(String.Format("作廢之發票非原發票開立人,發票號碼:{0}", invItem.CancelInvoiceNumber)));
                            continue;
                        }

                        if (invoice.InvoiceCancellation != null)
                        {
                            result.Add(idx, new Exception(String.Format("作廢發票已存在,發票號碼:{0}", invItem.CancelInvoiceNumber)));
                            continue;
                        }

                        if (this.GetTable<InvoiceAllowanceItem>()
                            .Where(a => a.InvoiceNo == invItem.CancelInvoiceNumber)
                            .Where(a => a.InvoiceAllowanceDetail.Any(d => d.InvoiceAllowance.InvoiceAllowanceSeller.SellerID == invoice.SellerID))
                            .Any())
                        {
                            result.Add(idx, new Exception(String.Format("欲作廢之發票已開立折讓,發票號碼:{0}", invItem.CancelInvoiceNumber)));
                            continue;
                        }

                        InvoiceCancellation cancelItem = new InvoiceCancellation
                        {
                            InvoiceItem = invoice,
                            CancellationNo = invItem.CancelInvoiceNumber,
                            //Remark = invItem.Remark,
                            ReturnTaxDocumentNo = invItem.ReturnTaxDocumentNumber,
                            CancelDate = DateTime.ParseExact(String.Format("{0} {1}", invItem.CancelDate, invItem.CancelTime), "yyyy/M/d HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture),
                            CancelReason = invItem.Remark,
                        };

                        var doc = new DerivedDocument
                        {
                            CDS_Document = new CDS_Document
                            {
                                DocType = (int)Naming.DocumentTypeDefinition.E_InvoiceCancellation,
                                DocDate = DateTime.Now,
                                DocumentOwner = new DocumentOwner
                                {
                                    OwnerID = owner.CompanyID
                                }
                            },
                            SourceID = invoice.InvoiceID
                        };
                        this.GetTable<DerivedDocument>().InsertOnSubmit(doc);
                        C0501Handler.PushStepQueueOnSubmit(this, doc.CDS_Document, Naming.InvoiceStepDefinition.已開立);
                        C0501Handler.PushStepQueueOnSubmit(this, doc.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
                        this.SubmitChanges();

                        eventItem.Add(invoice);

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

        public IEnumerable<WelfareReplication> GetUpdatedWelfareAgenciesForSeller(String receiptNo)
        {
            return this.GetTable<WelfareReplication>().Where(w => w.InvoiceWelfareAgency.Organization.ReceiptNo == receiptNo);
        }

        public IEnumerable<InvoiceWelfareAgency> GetWelfareAgenciesForSeller(String receiptNo)
        {
            return this.GetTable<InvoiceWelfareAgency>().Where(w => w.Organization.ReceiptNo == receiptNo);
        }

        public virtual Dictionary<int, Exception> SaveUploadAllowance(AllowanceRoot root,OrganizationToken owner)
        {

            Dictionary<int, Exception> result = new Dictionary<int, Exception>();
            if (root != null && root.Allowance != null && root.Allowance.Length > 0)
            {
                var table = this.GetTable<InvoiceAllowance>();
                var tblOrg = this.GetTable<Organization>();

                for (int idx=0;idx<root.Allowance.Length;idx++)
                {
                    var item = root.Allowance[idx];
                    try
                    {

                        if (table.Any(i => i.AllowanceNumber == item.AllowanceNumber))
                        {
                            result.Add(idx, new Exception(String.Format("折讓證明單號碼已存在:{0}", item.AllowanceNumber)));
                            continue;
                        }


                        InvoiceAllowance newItem = new InvoiceAllowance
                        {
                            CDS_Document = new CDS_Document
                            {
                                DocDate = DateTime.Now,
                                DocType = (int)Naming.DocumentTypeDefinition.E_Allowance
                            },
                            AllowanceDate =  DateTime.ParseExact(item.AllowanceDate, "yyyy/MM/dd", System.Globalization.CultureInfo.CurrentCulture),
                            AllowanceNumber = item.AllowanceNumber,
                            AllowanceType = item.AllowanceType,
                            BuyerId = item.BuyerId,
                            SellerId = item.SellerId,
                            TaxAmount = item.TaxAmount,
                            TotalAmount = item.TotalAmount,
                        };
                        var buyer = tblOrg.Where(o => o.ReceiptNo == item.BuyerId).FirstOrDefault();
                        if (buyer != null)
                        {
                            newItem.InvoiceAllowanceBuyer = new InvoiceAllowanceBuyer
                            {
                                BuyerID = buyer.CompanyID,
                                Name  = buyer.CompanyName,
                                CustomerName = buyer.CompanyName,
                                ReceiptNo = buyer.ReceiptNo
                            };
                        }
                        else
                        {
                            newItem.InvoiceAllowanceBuyer = new InvoiceAllowanceBuyer
                            {
                                Name = item.BuyerName,
                                CustomerName = item.BuyerName,
                                ReceiptNo = item.BuyerId
                            };
                        }
                        var seller = tblOrg.Where(o => o.ReceiptNo == item.SellerId).FirstOrDefault();
                        if (seller != null)
                        {
                            newItem.InvoiceAllowanceSeller = new InvoiceAllowanceSeller
                            {
                                SellerID = seller.CompanyID
                            };
                        }

                        List<InvoiceAllowanceItem> productItems = new List<InvoiceAllowanceItem>();
                        foreach (var i in item.AllowanceItem)
                        {
                            var allowanceItem = new InvoiceAllowanceItem
                                {
                                    Amount = i.Amount,
                                    InvoiceNo = i.OriginalInvoiceNumber,
                                    InvoiceDate = DateTime.ParseExact(i.OriginalInvoiceDate, "yyyy/MM/dd", System.Globalization.CultureInfo.CurrentCulture),
                                    ItemNo = i.Item,
                                    OriginalSequenceNo = (short)i.OriginalSequenceNumber,
                                    Piece = i.Quantity,
                                    PieceUnit = i.Unit,
                                    OriginalDescription = i.OriginalDescription,
                                    TaxType = i.TaxType,
                                    No = (short)i.AllowanceSequenceNumber,
                                    UnitCost = i.UnitPrice,
                                    Tax = i.Tax,
                                };

                            String invNo, trackCode;
                            i.OriginalInvoiceNumber.GetInvoiceNo(out invNo, out trackCode);

                            var originalInvoice = this.EntityList.Where(v => v.TrackCode == trackCode && v.No == invNo).FirstOrDefault();
                            if (originalInvoice != null)
                            {
                                var invProductItem =  originalInvoice.InvoiceDetails.Join(this.GetTable<InvoiceProductItem>(), d => d.ProductID, p => p.ProductID, (d, p) => p)
                                    .Where(p => p.No == i.OriginalSequenceNumber).FirstOrDefault();
                                if (invProductItem != null)
                                {
                                    allowanceItem.ItemID = invProductItem.ItemID;
                                }
                            }
                            productItems.Add(allowanceItem);
                        }

                        newItem.InvoiceAllowanceDetails.AddRange(productItems.Select(p => new InvoiceAllowanceDetail
                        {
                            InvoiceAllowanceItem = p
                        }));

                        if (owner != null)
                        {
                            newItem.CDS_Document.DocumentOwner = new DocumentOwner
                            {
                                OwnerID = owner.CompanyID
                            };
                        }

                        table.InsertOnSubmit(newItem);

                        this.SubmitChanges();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        result.Add(idx, ex);
                    }
                }
            }

            return result;
        }

        public virtual Dictionary<int, Exception> SaveUploadAllowanceCancellation(CancelAllowanceRoot root,OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();
            if (root != null && root.CancelAllowance != null && root.CancelAllowance.Length > 0)
            {
                var tblAllowance = this.GetTable<InvoiceAllowance>();
                var tblCancel = this.GetTable<InvoiceAllowanceCancellation>();

                for (int idx=0;idx<root.CancelAllowance.Length;idx++)
                {
                    var item = root.CancelAllowance[idx];
                    try
                    {

                        var allowance = tblAllowance.Where(i => i.AllowanceNumber == item.CancelAllowanceNumber).FirstOrDefault();

                        if (allowance == null)
                        {
                            result.Add(idx, new Exception(String.Format("折讓證明單號碼不存在:{0}", item.CancelAllowanceNumber)));
                            continue;
                        }

                        if (allowance.InvoiceAllowanceCancellation != null)
                        {
                            result.Add(idx, new Exception(String.Format("作廢折讓單已存在,折讓單號碼:{0}", item.CancelAllowanceNumber)));
                            continue;
                        }

                        InvoiceAllowanceCancellation cancelItem = new InvoiceAllowanceCancellation
                        {
                            AllowanceID = allowance.AllowanceID,
                            Remark = item.Remark,
                            CancelDate = DateTime.ParseExact(String.Format("{0} {1}", item.CancelDate, item.CancelTime), "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture)
                        };

                        this.SubmitChanges();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        result.Add(idx, ex);
                    }
                }
            }

            return result;
        }

        //public InvoiceItem CreateInvoiceFromShipment(SCMDataEntity.CDS_Document item,int sellerID)
        //{
        //    Organization seller = this.GetTable<Organization>().Where(o => o.CompanyID == sellerID).FirstOrDefault();
        //    if (seller == null)
        //    {
        //        throw new Exception("發票開立人資料不存在!!");
        //    }

        //    using (TrackNoManager trackNoMgr = new TrackNoManager(this, sellerID))
        //    {
        //        if (trackNoMgr.InvoiceNoInterval == null)
        //        {
        //            throw new Exception("發票字軌號碼已用完或未設定!!");
        //        }

        //        InvoiceItem newItem = new InvoiceItem
        //        {
        //            CDS_Document = new CDS_Document
        //            {
        //                DocDate = DateTime.Now,
        //                DocType = (int)Naming.DocumentTypeDefinition.E_Invoice,
        //                DocumentOwner = new DocumentOwner
        //                {
        //                    OwnerID = seller.CompanyID
        //                },
        //                ProcessType = (int)Naming.InvoiceProcessType.C0401,
        //            },
        //            DonateMark = "0",
        //            PrintMark = "Y",
        //            InvoiceBuyer = new InvoiceBuyer
        //            {
        //                Name = String.IsNullOrEmpty(item.BUYER_ORDERS.BUYER_DATA.BUYER_BAN) ? item.BUYER_ORDERS.BUYER_DATA.BUYER_NAME.CheckB2CMIGName() : item.BUYER_ORDERS.BUYER_DATA.BUYER_NAME,
        //                ReceiptNo = String.IsNullOrEmpty(item.BUYER_ORDERS.BUYER_DATA.BUYER_BAN) ? "0000000000" : item.BUYER_ORDERS.BUYER_DATA.BUYER_BAN,
        //                CustomerName = item.BUYER_ORDERS.BUYER_DATA.BUYER_NAME,
        //            },
        //            InvoiceDate = item.BUYER_SHIPMENT.SHIPMENT_DATETIME,
        //            InvoiceType = 5,
        //            //No = invNo,
        //            //TrackCode = trackCode,
        //            SellerID = seller.CompanyID,
        //            //InvoiceByHousehold = carrier != null ? new InvoiceByHousehold { InvoiceUserCarrier = carrier } : null,
        //            //InvoicePrintAssertion = bPrinted ? new InvoicePrintAssertion { PrintDate = DateTime.Now } : null,
        //            RandomNo = ValidityAgent.GenerateRandomCode(4),
        //            InvoiceAmountType = new InvoiceAmountType
        //            {
        //                DiscountAmount = item.BUYER_ORDERS.ORDERS_DISCOUNT_AMOUNT,
        //                SalesAmount = item.BUYER_ORDERS.ORDERS_AMOUNT,
        //                TaxAmount = item.BUYER_ORDERS.TAX_AMOUNT,
        //                TaxRate = 0.05m,
        //                TaxType = 1,
        //                TotalAmount = item.BUYER_ORDERS.TOTAL_AMOUNT,
        //                TotalAmountInChinese = ValidityAgent.MoneyShow(item.BUYER_ORDERS.TOTAL_AMOUNT)
        //            }
        //        };

        //        short seqNo = 1;

        //        var productItems = item.BUYER_ORDERS.BUYER_ORDERS_DETAILS.Select(o => new InvoiceProductItem
        //        {
        //            InvoiceProduct = new InvoiceProduct { Brief = o.PRODUCTS_DATA.PRODUCTS_NAME },
        //            CostAmount = o.BO_UNIT_PRICE,
        //            ItemNo = String.Format("{0:00}", seqNo + 1),
        //            Piece = o.BO_QUANTITY,
        //            UnitCost = o.BO_UNIT_PRICE,
        //            TaxType = 1,
        //            No = (seqNo++)
        //        }).ToList();

        //        newItem.InvoiceDetails.AddRange(productItems.Select(p => new InvoiceDetail
        //        {
        //            InvoiceProduct = p.InvoiceProduct
        //        }));

        //        if (trackNoMgr.CheckInvoiceNo(newItem))
        //        {
        //            this.EntityList.InsertOnSubmit(newItem);

        //            C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已開立);
        //            C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);

        //            this.SubmitChanges();
        //            return newItem;
        //        }
        //        else
        //        {
        //            throw new Exception("發票字軌號碼已用完或未設定!!");
        //        }
        //    }
        //}

        public InvoiceCancellation CreateInvoiceCancellationFromReturnedGoods(int invoiceID,String remark)
        {
            var invoice = this.EntityList.Where(i => i.InvoiceID == invoiceID).FirstOrDefault();

            if(invoice==null)
            {
                throw new Exception("原始出貨發票不存在!!");
            }

            if (invoice.InvoiceCancellation != null)
            {
                throw new Exception("發票已作廢!!");
            }

            InvoiceCancellation cancelItem = new InvoiceCancellation
            {
                InvoiceItem = invoice,
                CancellationNo = String.Format("{0}{1}",invoice.TrackCode,invoice.No),
                Remark = remark,
                CancelDate = DateTime.Now
            };

            var doc = new DerivedDocument
            {
                CDS_Document = new CDS_Document
                {
                    DocType = (int)Naming.DocumentTypeDefinition.E_InvoiceCancellation,
                    DocDate = DateTime.Now,
                    DocumentOwner = new DocumentOwner
                    {
                        OwnerID = invoice.SellerID.Value
                    }
                },
                SourceID = invoice.InvoiceID
            };
            this.GetTable<DerivedDocument>().InsertOnSubmit(doc);
            C0501Handler.PushStepQueueOnSubmit(this, doc.CDS_Document, Naming.InvoiceStepDefinition.已開立);
            C0501Handler.PushStepQueueOnSubmit(this, doc.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);

            this.SubmitChanges();
            return cancelItem;
        }

        public virtual Dictionary<int, Exception> SaveUploadInvoiceAutoTrackNo(InvoiceRoot item, OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();
            TrackNoManager trackNoMgr = new TrackNoManager(this, owner.CompanyID);

            if (item != null && item.Invoice != null && item.Invoice.Length > 0)
            {
                Organization donatory = owner.Organization.InvoiceWelfareAgencies.Select(w => w.WelfareAgency.Organization).FirstOrDefault();
                EventItems = null;
                List<InvoiceItem> eventItems = new List<InvoiceItem>();

                for (int idx = 0; idx < item.Invoice.Length; idx++)
                {
                    try
                    {
                        var invItem = item.Invoice[idx];

                        var seller = this.GetTable<Organization>().Where(o => o.ReceiptNo == invItem.SellerId).FirstOrDefault();
                        if (seller == null)
                        {
                            result.Add(idx, new Exception(String.Format("賣方為非註冊營業人,統一編號:{0}", invItem.SellerId)));
                            continue;
                        }

                        if (seller.OrganizationStatus.CurrentLevel == (int)Naming.MemberStatusDefinition.Mark_To_Delete)
                        {
                            result.Add(idx, new Exception(String.Format("開立人已註記停用,統一編號:{0}", invItem.SellerId)));
                            continue;
                        }

                        //Organization donatory = null;
                        //bool bPrinted = invItem.PrintMark == "Y";
                        //if (invItem.DonateMark == "1")
                        //{
                        //    if (bPrinted)
                        //    {
                        //        result.Add(invItem.InvoiceNumber, new Exception(String.Format("發票已列印後不能捐贈,發票號碼:{0}", invItem.InvoiceNumber)));
                        //        continue;
                        //    }
                        //    donatory = this.GetTable<Organization>().Where(o => o.ReceiptNo == invItem.NPOBAN).FirstOrDefault();
                        //    if (donatory == null || !this.GetTable<InvoiceWelfareAgency>().Any(w => w.AgencyID == donatory.CompanyID && w.SellerID == seller.CompanyID))
                        //    {
                        //        result.Add(invItem.InvoiceNumber, new Exception(String.Format("發票受捐社福單位不符,統一編號:{0}", invItem.NPOBAN)));
                        //        continue;
                        //    }
                        //}

                        ///EIVO03網購業者電子發票不需要載具歸戶
                        ///
                        //InvoiceUserCarrier carrier = null;
                        //if (bPrinted)
                        //{
                        //    if (!String.IsNullOrEmpty(invItem.CarrierType) || !String.IsNullOrEmpty(invItem.CarrierId1) || !String.IsNullOrEmpty(invItem.CarrierId2))
                        //    {
                        //        result.Add(invItem.InvoiceNumber, new Exception(String.Format("發票已列印後不能指定歸戶,發票號碼:{0}", invItem.InvoiceNumber)));
                        //        continue;
                        //    }
                        //}
                        //else
                        //{
                        //    carrier = checkInvoiceCarrier(invItem);
                        //    if (carrier == null)
                        //    {
                        //        result.Add(invItem.InvoiceNumber, new Exception(String.Format("發票歸戶載具或卡號不符,發票號碼:{0}", invItem.InvoiceNumber)));
                        //        continue;
                        //    }
                        //}

                        InvoiceItem newItem = new InvoiceItem
                        {
                            CDS_Document = new CDS_Document
                            {
                                DocDate = DateTime.Now,
                                DocType = (int)Naming.DocumentTypeDefinition.E_Invoice,
                                ProcessType = (int)Naming.InvoiceProcessType.C0401,
                            },
                            DonateMark = "0",
                            PrintMark = "Y",
                            InvoiceBuyer = new InvoiceBuyer
                            {
                                Name = String.IsNullOrEmpty(invItem.BuyerId) ? invItem.BuyerName.CheckB2CMIGName() : invItem.BuyerName,
                                ReceiptNo = String.IsNullOrEmpty(invItem.BuyerId) ? "0000000000" : invItem.BuyerId,
                                CustomerName = invItem.BuyerName,
                                BuyerMark = invItem.BuyerMark,
                            },
                            InvoiceDate = DateTime.ParseExact(invItem.InvoiceDate, "yyyy/M/d", System.Globalization.CultureInfo.CurrentCulture),
                            InvoiceType = byte.Parse(invItem.InvoiceType),
                            //No = invNo,
                            //TrackCode = trackCode,
                            SellerID = seller.CompanyID,
                            InvoiceSeller = new InvoiceSeller
                            {
                                Name = seller.CompanyName,
                                ReceiptNo = seller.ReceiptNo,
                                Address = seller.Addr,
                                ContactName = seller.ContactName,
                                CustomerName = seller.CompanyName,
                                EMail = seller.ContactEmail,
                                Fax = seller.Fax,
                                Phone = seller.Phone,
                                PersonInCharge = seller.UndertakerName,
                                SellerID = seller.CompanyID,
                            },
                            //InvoiceByHousehold = carrier != null ? new InvoiceByHousehold { InvoiceUserCarrier = carrier } : null,
                            //InvoicePrintAssertion = bPrinted ? new InvoicePrintAssertion { PrintDate = DateTime.Now } : null,
                            RandomNo = ValidityAgent.GenerateRandomCode(4),//"AAAA",
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
                            DonationID = donatory != null ? donatory.CompanyID : (int?)null
                        };

                        checkPrintMarkAndCarrier(newItem, invItem, seller);

                        short seqNo = 1;

                        var productItems = invItem.InvoiceItem.Select(i => new InvoiceProductItem
                        {
                            InvoiceProduct = new InvoiceProduct { Brief = i.Description },
                            CostAmount = i.Amount,
                            ItemNo = i.Item,
                            Piece = i.Quantity,
                            PieceUnit = i.Unit,
                            UnitCost = i.UnitPrice,
                            Remark = i.Remark,
                            TaxType = i.TaxType,
                            No = (seqNo++)
                        }).ToList();

                        newItem.InvoiceDetails.AddRange(productItems.Select(p => new InvoiceDetail
                        {
                            InvoiceProduct = p.InvoiceProduct
                        }));

                        if (owner != null)
                        {
                            newItem.CDS_Document.DocumentOwner = new DocumentOwner
                            {
                                OwnerID = owner.CompanyID
                            };
                        }


                        if (!trackNoMgr.ApplyInvoiceDate(newItem.InvoiceDate.Value) || !trackNoMgr.CheckInvoiceNo(newItem))
                        {
                            result.Add(idx, new Exception("未設定發票字軌或發票號碼已用完"));
                            continue;
                        }

                        this.EntityList.InsertOnSubmit(newItem);

                        C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已開立);
                        C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);

                        this.SubmitChanges();
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

            trackNoMgr.Dispose();
            return result;
        }

        public void VoidInvoice(IEnumerable<int> invoiceID)
        {
            if (invoiceID != null && invoiceID.Count() > 0)
            {
                DateTime cancelDate = DateTime.Now;
                this.EventItems = new List<InvoiceItem>();
                foreach(var invID  in invoiceID)
                {
                    var item = this.EntityList.Where(i => i.InvoiceID == invID).FirstOrDefault();
                    if(item!=null && VoidInvoice(item,ref cancelDate))
                    {
                        EventItems.Add(item);
                    }
                }
            }
        }

        public void VoidAllowance(IEnumerable<int?> allowanceID)
        {
            if (allowanceID != null && allowanceID.Count() > 0)
            {
                DateTime cancelDate = DateTime.Now;
                this.EventItems_Allowance = new List<InvoiceAllowance>();
                foreach (var id in allowanceID)
                {
                    var item = this.GetTable<InvoiceAllowance>().Where(i => i.AllowanceID == id).FirstOrDefault();
                    if (item != null && VoidAllowance(item, ref cancelDate))
                    {
                        EventItems_Allowance.Add(item);
                    }
                }
            }
        }

        public bool VoidInvoice(InvoiceItem item,ref DateTime cancelDate)
        {
            if (item.InvoiceCancellation == null)
            {
                DerivedDocument doc = null;
                InvoiceCancellation cancelItem = item.PrepareVoidItem(this, ref doc);
                cancelItem.Remark = "作廢發票";
                cancelItem.CancelDate = cancelDate;
                cancelItem.CancelReason = "作廢發票";
                cancelItem.ReturnTaxDocumentNo = "";

                this.SubmitChanges();

                return true;
            }

            return false;
        }

        public bool VoidAllowance(InvoiceAllowance item, ref DateTime cancelDate)
        {
            if (item.InvoiceAllowanceCancellation == null)
            {
                DerivedDocument doc = null;
                InvoiceAllowanceCancellation cancelItem = item.PrepareVoidItem(this, ref doc);
                cancelItem.Remark = "作廢折讓";
                cancelItem.CancelDate = cancelDate;
                cancelItem.CancelReason = "作廢折讓";

                this.SubmitChanges();

                return true;
            }

            return false;
        }

    }
}
