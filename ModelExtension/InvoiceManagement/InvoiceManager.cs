using CommonLib.Core.Utility;
using CommonLib.DataAccess;
using CommonLib.Utility;
using DocumentFormat.OpenXml.Spreadsheet;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement.ErrorHandle;
using ModelCore.InvoiceManagement.InvoiceProcess;
using ModelCore.InvoiceManagement.Validator;
using ModelCore.InvoiceManagement.zhTW;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.TurnKey.Allowance;
using ModelCore.Schema.TurnKey.Invoice;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace ModelCore.InvoiceManagement
{
    public class InvoiceManager: EIVOEntityManager<InvoiceItem>
    {

        protected Func<Exception> _checkUploadInvoice;

        public InvoiceManager() : base() { }
        public InvoiceManager(GenericManager<EIVOEntityDataContext> mgr) : base(mgr) { }

        public bool HasItem { get; set; }

        public List<InvoiceAllowance>? EventItems_Allowance
        {
            get;
            protected set;
        }

        public String? InvoiceClientID
        { get; set; }

        public int? ChannelID { get; set; }

        public Naming.InvoiceProcessType? ProcessType { get; set; }

        public virtual Dictionary<int, Exception> SaveUploadInvoice(InvoiceRoot item, OrganizationToken? owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();

            if (item != null && item.Invoice != null && item.Invoice.Length > 0)
            {
                List<InvoiceItem> eventItems = new List<InvoiceItem>();
                InvoiceRootInvoiceValidator validator = new InvoiceRootInvoiceValidator(this, owner?.Organization)
                {
                    ProcessType = item.ProcessType != null ? Enum.Parse<Naming.InvoiceProcessType>(item.ProcessType) : this.ProcessType,
                };

                //Organization donatory = owner.Organization.InvoiceWelfareAgencies.Select(w => w.WelfareAgency.Organization).FirstOrDefault();

                for (int idx = 0; idx < item.Invoice.Length; idx++)
                {
                    try
                    {
                        var invItem = item.Invoice[idx];

                        Exception? ex;
                        if ((ex = validator.Validate(invItem)) != null)
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

                        InvoiceItem newItem = validator.InvoiceItem;

                        if (!validator.DuplicateProcess)
                        {
                            this.EntityList.InsertOnSubmit(newItem);

                            if (validator.ProcessType == Naming.InvoiceProcessType.A0101)
                            {
                                newItem.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.A0101);
                            }
                            else if( validator.ProcessType == Naming.InvoiceProcessType.ReceivedA0101)
                            {
                                newItem.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.待接收, Naming.InvoiceProcessType.A0101);
                            }
                            else
                            {
                                newItem.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.已開立, Naming.InvoiceProcessType.F0401);
                                switch ((Naming.NotificationIndication?)item.Notification)
                                {
                                    case Naming.NotificationIndication.None:
                                        break;
                                    case Naming.NotificationIndication.Deferred:
                                        newItem.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.文件準備中, Naming.InvoiceProcessType.F0401);
                                        break;
                                    default:
                                        newItem.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.已接收資料待通知, Naming.InvoiceProcessType.F0401);
                                        break;
                                }
                            }

                            this.SubmitChanges();
                        }
                        else
                        {
                            if (validator.ProcessType == Naming.InvoiceProcessType.ReceivedA0101)
                            {
                                newItem.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.待接收, Naming.InvoiceProcessType.A0101);
                                this.SubmitChanges();
                            }
                        }

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
                        CarrierType = ModelExtension.Properties.AppSettings.Default.DefaultUserCarrierType,
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

        public virtual Dictionary<int, Exception> SaveUploadInvoiceCancellation(CancelInvoiceRoot item, OrganizationToken? owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();
            if (item != null && item.CancelInvoice != null && item.CancelInvoice.Length > 0)
            {
                Naming.InvoiceProcessType processType = item.ProcessType != null ? Enum.Parse<Naming.InvoiceProcessType>(item.ProcessType) : Naming.InvoiceProcessType.F0501;
                EventItems = null;
                List<InvoiceItem> eventItems = new List<InvoiceItem>();

                for (int idx = 0; idx < item.CancelInvoice.Length; idx++)
                {
                    var invItem = item.CancelInvoice[idx];
                    try
                    {
                        Exception? ex;
                        InvoiceItem? invoice;
                        DateTime cancelDate;
                        if ((ex = invItem.CheckMandatoryFields(this, owner, out invoice, out cancelDate, out bool duplicateProcess, processType)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        if (!duplicateProcess)
                        {
                            DerivedDocument? doc = null;
                            InvoiceCancellation cancelItem = invoice!.PrepareVoidItem(this, ref doc, processType);
                            cancelItem.Remark = invItem.Remark;
                            cancelItem.CancelDate = cancelDate;
                            cancelItem.CancelReason = invItem.CancelReason;
                            cancelItem.ReturnTaxDocumentNo = invItem.ReturnTaxDocumentNumber;

                            this.SubmitChanges();
                            eventItems.Add(cancelItem.InvoiceItem);
                        }
                        else
                        {
                            if (processType == Naming.InvoiceProcessType.ReceivedA0201)
                            {
                                invoice!.CDS_Document.ChildDocument.FirstOrDefault()?.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.待接收, Naming.InvoiceProcessType.A0201);
                                this.SubmitChanges();
                            }
                            eventItems.Add(invoice!);
                        }
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
        public IEnumerable<WelfareReplication> GetUpdatedWelfareAgenciesForSeller(String receiptNo)
        {
            return this.GetTable<WelfareReplication>().Where(w => w.InvoiceWelfareAgency.Organization.ReceiptNo == receiptNo);
        }

        public IEnumerable<InvoiceWelfareAgency> GetWelfareAgenciesForSeller(String receiptNo)
        {
            return this.GetTable<InvoiceWelfareAgency>().Where(w => w.Organization.ReceiptNo == receiptNo);
        }

        public virtual Dictionary<int, Exception> SaveUploadAllowance(AllowanceRoot item, OrganizationToken? owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();

            if (item != null && item.Allowance != null && item.Allowance.Length > 0)
            {
                this.EventItems_Allowance = null;
                List<InvoiceAllowance> eventItems = new List<InvoiceAllowance>();

                AllowanceRootAllowanceValidator validator = new AllowanceRootAllowanceValidator(this, owner?.Organization)
                {
                    ProcessType = item.ProcessType != null ? Enum.Parse<Naming.InvoiceProcessType>(item.ProcessType) : this.ProcessType,
                };
                var table = this.GetTable<InvoiceAllowance>();

                for (int idx = 0; idx < item.Allowance.Length; idx++)
                {
                    try
                    {
                        var allowanceItem = item.Allowance[idx];

                        Exception? ex;
                        InvoiceAllowance? newItem;
                        if ((ex = validator.Validate(allowanceItem)) != null)
                        {
                            if (ex is DuplicateAllowanceNumberException)
                            {
                                newItem = ((DuplicateAllowanceNumberException)ex).CurrentAllowance;
                                eventItems.Add(newItem);
                                if (validator.ProcessType != Naming.InvoiceProcessType.ReceivedB0101)
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                result.Add(idx, ex);
                                continue;
                            }
                        }
                        else
                        {
                            newItem = validator.Allowance!;
                            table.InsertOnSubmit(newItem);
                        }

                        if (ProcessType == Naming.InvoiceProcessType.B0101)
                        {
                            newItem.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.B0101);
                        }
                        else if (validator.ProcessType == Naming.InvoiceProcessType.ReceivedB0101)
                        {
                            newItem.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.待接收, Naming.InvoiceProcessType.B0101);
                        }
                        else
                        {
                            newItem.CDS_Document.PushStepQueueOnSubmit(this, validator.Seller!.StepReadyToAllowanceMIG(), Naming.InvoiceProcessType.G0401);
                            newItem.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.已接收資料待通知, Naming.InvoiceProcessType.G0401);
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
            doc.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.已開立, Naming.InvoiceProcessType.F0501);
            doc.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.已接收資料待通知, Naming.InvoiceProcessType.F0501);

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
                                ZeroTaxRateReason = invItem.ZeroTaxRateReason,
                            },
                            DonationID = donatory?.CompanyID,
                            CustomsClearanceMark = invItem.CustomsClearanceMark,
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

                        newItem.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.已開立, Naming.InvoiceProcessType.F0401);
                        newItem.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.已接收資料待通知, Naming.InvoiceProcessType.F0401);

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

        public void VoidAllowance(IEnumerable<int>? allowanceID)
        {
            if (allowanceID != null && allowanceID.Any())
            {
                DateTime cancelDate = DateTime.Now;
                this.EventItems_Allowance = new List<InvoiceAllowance>();
                foreach (var id in allowanceID)
                {
                    var item = this.GetTable<InvoiceAllowance>().Where(i => i.AllowanceID == id).FirstOrDefault();
                    if (item != null && item.InvoiceAllowanceCancellation == null && VoidAllowance(item, ref cancelDate))
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
                DerivedDocument? doc = null;
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
                DerivedDocument? doc = null;
                InvoiceAllowanceCancellation cancelItem = item.PrepareVoidItem(this, ref doc);
                cancelItem.Remark = "作廢折讓";
                cancelItem.CancelDate = cancelDate;
                cancelItem.CancelReason = "作廢折讓";

                this.SubmitChanges();

                return true;
            }

            return false;
        }

        public Dictionary<int, Exception> SaveInvoiceMIG(XmlDocument docInv,Naming.InvoiceProcessType processType)
        {
            InvoiceRoot root = new InvoiceRoot
            {
                ProcessType = $"{processType}",
            };

            if (docInv.DocumentElement != null)
            {
                if (docInv.DocumentElement.Attributes["xmlns"] != null)
                {
                    docInv.DocumentElement.SetAttribute("xmlns", "");
                    docInv.LoadXml(docInv.OuterXml);
                }

                var migItem = docInv.ConvertTo<Invoice>();
                _ = this.CheckBusinessRoleFromMIG(migItem.Main!.Seller!);
                _ = this.CheckBusinessRoleFromMIG(migItem.Main!.Buyer!);

                root.Invoice =
                [
                    migItem.FromMIG()
                ];

            }

            return SaveUploadInvoice(root, null);
        }

        public Dictionary<int, Exception> SaveAllowanceMIG(XmlDocument docInv, Naming.InvoiceProcessType processType)
        {
            AllowanceRoot root = ConvertToAllowanceRoot(docInv, processType);

            return SaveUploadAllowance(root, null);
        }

        public AllowanceRoot ConvertToAllowanceRoot(XmlDocument docInv, Naming.InvoiceProcessType processType)
        {
            AllowanceRoot root = new AllowanceRoot
            {
                ProcessType = $"{processType}",
            };

            if (docInv.DocumentElement != null)
            {
                if (docInv.DocumentElement.Attributes["xmlns"] != null)
                {
                    docInv.DocumentElement.SetAttribute("xmlns", "");
                    docInv.LoadXml(docInv.OuterXml);
                }

                var migItem = docInv.ConvertTo<Allowance>();
                _ = this.CheckBusinessRoleFromMIG(migItem.Main!.Seller!);
                _ = this.CheckBusinessRoleFromMIG(migItem.Main!.Buyer!);

                root.Allowance =
                [
                    migItem.FromMIG()
                ];

            }

            return root;
        }

        public Dictionary<int, Exception> SaveInvoiceCancellationMIG(XmlDocument docInv, Naming.InvoiceProcessType processType)
        {
            CancelInvoiceRoot root = new CancelInvoiceRoot
            {
                ProcessType = $"{processType}",
            };

            if (docInv.DocumentElement != null)
            {
                if (docInv.DocumentElement.Attributes["xmlns"] != null)
                {
                    docInv.DocumentElement.SetAttribute("xmlns", "");
                    docInv.LoadXml(docInv.OuterXml);
                }

                var migItem = docInv.ConvertTo<CancelInvoice>();

                root.CancelInvoice =
                [
                    migItem.FromMIG()
                ];

            }

            return SaveUploadInvoiceCancellation(root, null);
        }

        public Dictionary<int, Exception> SaveAllowanceCancellationMIG(XmlDocument docInv, Naming.InvoiceProcessType processType)
        {
            CancelAllowanceRoot root = new CancelAllowanceRoot
            {
                ProcessType = $"{processType}",
            };

            if (docInv.DocumentElement != null)
            {
                if (docInv.DocumentElement.Attributes["xmlns"] != null)
                {
                    docInv.DocumentElement.SetAttribute("xmlns", "");
                    docInv.LoadXml(docInv.OuterXml);
                }

                var migItem = docInv.ConvertTo<CancelAllowance>();

                root.CancelAllowance =
                [
                    migItem.FromMIG()
                ];

            }

            return SaveUploadAllowanceCancellation(root, null);
        }

        public virtual Dictionary<int, Exception> SaveUploadAllowanceCancellation(CancelAllowanceRoot root, OrganizationToken? owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();
            if (root != null && root.CancelAllowance != null && root.CancelAllowance.Length > 0)
            {
                var tblAllowance = this.GetTable<InvoiceAllowance>();
                var tblCancel = this.GetTable<InvoiceAllowanceCancellation>();

                Naming.InvoiceProcessType? processType = root.ProcessType != null ? Enum.Parse<Naming.InvoiceProcessType>(root.ProcessType) : Naming.InvoiceProcessType.G0501;

                EventItems = null;
                EventItems_Allowance = null;
                List<InvoiceAllowance> eventItems = new List<InvoiceAllowance>();

                for (int idx = 0; idx < root.CancelAllowance.Length; idx++)
                {
                    var item = root.CancelAllowance[idx];
                    try
                    {
                        Exception? ex;
                        InvoiceAllowanceCancellation? voidItem = null;
                        DerivedDocument? derivedDoc = null;

                        if ((ex = item.VoidAllowance(this, owner?.Organization, ref voidItem, ref derivedDoc, out bool duplicateProcess, processType)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        if (duplicateProcess)
                        {
                            if (processType == Naming.InvoiceProcessType.ReceivedB0201)
                            {
                                derivedDoc?.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.待接收, Naming.InvoiceProcessType.B0201);
                                this.SubmitChanges();
                            }
                        }

                        eventItems.Add(voidItem!.InvoiceAllowance);
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

    }
}
