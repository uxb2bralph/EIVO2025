using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Xml;

using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement.InvoiceProcess;
using ModelCore.Locale;
using ModelCore.Properties;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.EIVO.B2B;
using CommonLib.Utility;
using Newtonsoft.Json;
using ModelCore.InvoiceManagement.Validator;
using CommonLib.DataAccess;
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement
{
    public partial class B2BInvoiceManager : InvoiceManager
    {
        private X509Certificate2 _signerCert;
        private bool _useSigner;

        public B2BInvoiceManager() : base() 
        {
            this.ProcessType = Naming.InvoiceProcessType.A0401;
        }

        public B2BInvoiceManager(GenericManager<EIVOEntityDataContext> mgr) : base(mgr) 
        {
            this.ProcessType = Naming.InvoiceProcessType.A0401;
        }

        public X509Certificate2 PrepareSignerCertificate(Organization org)
        {
            _useSigner = org.OrganizationStatus.Entrusting == true;
            var signerToken = org.OrganizationStatus.UserToken;
            if (_useSigner && signerToken != null)
            {
                _signerCert = new X509Certificate2(Convert.FromBase64String(signerToken.PKCS12), signerToken.Token.ToString().Substring(0, 8));
            }
            return _signerCert;
        }

        protected virtual void applyProcessFlow(CDS_Document doc, Naming.InvoiceCenterBusinessType? businessType = null)
        {
            switch (ProcessType)
            {
                case Naming.InvoiceProcessType.A0401:
                    doc.ProcessType = (int)ProcessType;
                    A0401Handler.PushStepQueueOnSubmit(this, doc, Naming.InvoiceStepDefinition.已接收資料待通知);
                    A0401Handler.PushStepQueueOnSubmit(this, doc, Naming.InvoiceStepDefinition.已開立);
                    break;

                case Naming.InvoiceProcessType.A0101:
                    doc.ProcessType = (int)ProcessType;
                    if (businessType == Naming.InvoiceCenterBusinessType.進項)
                    {
                        A0101Handler.PushStepQueueOnSubmit(this, doc, Naming.InvoiceStepDefinition.待接收);
                    }
                    else
                    {
                        A0101Handler.PushStepQueueOnSubmit(this, doc, Naming.InvoiceStepDefinition.待傳送);
                    }
                    break;

            }
        }

        public virtual Dictionary<int, Exception> SaveUploadInvoice(SellerInvoiceRoot item, OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();
            if (item != null && item.Invoice != null && item.Invoice.Length > 0)
            {
                for (int idx = 0; idx < item.Invoice.Length;idx++)
                {
                    var invItem = item.Invoice[idx];
                    try
                    {
                        Exception ex;
                        InvoiceItem newItem = ConvertToInvoiceItem(owner, invItem,out ex);

                        if (newItem == null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        applyProcessFlow(newItem.CDS_Document);

                        this.EntityList.InsertOnSubmit(newItem);

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

        public override Dictionary<int, Exception> SaveUploadInvoice(InvoiceRoot item, OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();

            if (item != null && item.Invoice != null && item.Invoice.Length > 0)
            {
                List<InvoiceItem> eventItems = new List<InvoiceItem>();
                InvoiceRootInvoiceValidator validator = new InvoiceRootInvoiceValidator(this, owner.Organization);

                for (int idx = 0; idx < item.Invoice.Length; idx++)
                {
                    try
                    {
                        var invItem = item.Invoice[idx];

                        Exception ex;
                        if ((ex = validator.Validate(invItem)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        InvoiceItem newItem = validator.InvoiceItem;

                        if (!validator.DuplicateProcess)
                        {
                            this.EntityList.InsertOnSubmit(newItem);
                            applyProcessFlow(newItem.CDS_Document);

                            this.SubmitChanges();
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


        public InvoiceItem ConvertToInvoiceItem(OrganizationToken owner, SellerInvoiceRootInvoice invItem, out Exception ex)
        {
            ex = null;
            String invNo, trackCode;
            invItem.InvoiceNumber.GetInvoiceNo(out invNo, out trackCode);
            if (string.IsNullOrEmpty(invItem.SellerId))
            {
                ex = new Exception("發票開立人不得為空白");
                return null;
            }
            var seller = this.GetTable<Organization>().Where(o => o.ReceiptNo == invItem.SellerId).FirstOrDefault();
            if (seller == null )
            {
                ex = new Exception(String.Format("發票開立人為非註冊會員,統一編號:{0}", invItem.SellerId));
                return null;
            }

            //if (String.IsNullOrEmpty(seller.InvoiceSignature))
            //{
            //    ex = new Exception(String.Format("發票開立人尚未匯入統一發票專用章,統一編號:{0}", invItem.SellerId));
            //    return null;
            //}

            if (string.IsNullOrEmpty(invItem.BuyerId))
            {
                ex = new Exception("發票買受人不得為空白");
                return null;
            }
            var buyer = this.GetTable<Organization>().Where(o => o.ReceiptNo == invItem.BuyerId).FirstOrDefault();
            if (buyer == null)
            {
                ex = new Exception(String.Format("發票買受人為非註冊會員,統一編號:{0}", invItem.BuyerId));
                return null;
            }
            //發票買受人名稱
            invItem.BuyerName = invItem.BuyerName.GetEfficientString();
            //if (string.IsNullOrEmpty(invItem.BuyerName))
            //{
            //    ex = new Exception(String.Format("發票買受人名稱不得為空白"));
            //    return null;
            //}

            if (this.EntityList.Any(i => i.No == invNo && i.TrackCode == trackCode && i.SellerID==seller.CompanyID))
            {
                ex = new Exception(String.Format("發票號碼已存在:{0}", invItem.InvoiceNumber));
                return null;
            }

            var relation = this.GetTable<BusinessRelationship>().Where(b => b.MasterID == seller.CompanyID && b.RelativeID == buyer.CompanyID && b.BusinessID == (int)Naming.InvoiceCenterBusinessType.銷項).FirstOrDefault();
            if (relation == null)
            {
                ex = new Exception(String.Format("發票買受人非為開立人之銷項相對營業人:{0}", invItem.InvoiceNumber));
                return null;
            }
            else if (relation.CurrentLevel == (int)Naming.MemberStatusDefinition.Mark_To_Delete)
            {
                ex = new Exception(String.Format("已停用發票買受人為銷項相對營業人之關係:{0}", invItem.InvoiceNumber));
                return null;
            }


            InvoiceItem newItem = new InvoiceItem
            {
                CDS_Document = new CDS_Document
                {
                    DocDate = DateTime.Now,
                    DocType = (int)Naming.DocumentTypeDefinition.E_Invoice,
                    DocumentOwner = new DocumentOwner
                    {
                        OwnerID = owner.CompanyID
                    },
                    CurrentStep = (int)Naming.InvoiceStepDefinition.待接收,
                    ProcessType = (int)Naming.InvoiceProcessType.A0401,
                },
                InvoiceBuyer = new InvoiceBuyer
                {
                    BuyerMark = invItem.BuyerMark,
                    Name = invItem.BuyerName ?? relation.CompanyName ?? buyer.CompanyName,
                    ReceiptNo = invItem.BuyerId,
                    Address = relation.Addr ?? buyer.Addr,
                    ContactName = buyer.ContactName,
                    CustomerName = relation.CompanyName ?? buyer.CompanyName,
                    EMail = relation.ContactEmail ?? buyer.ContactEmail,
                    Fax = buyer.Fax,
                    Phone = relation.Phone ?? buyer.Phone,
                    PersonInCharge = buyer.UndertakerName,
                    BuyerID = buyer.CompanyID
                },
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
                    SellerID = seller.CompanyID
                },
                InvoiceDate = String.IsNullOrEmpty(invItem.InvoiceTime) ? DateTime.ParseExact(invItem.InvoiceDate, "yyyy/MM/dd", System.Globalization.CultureInfo.CurrentCulture) : DateTime.ParseExact(String.Format("{0} {1}", invItem.InvoiceDate, invItem.InvoiceTime), "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture),
                InvoiceType = byte.Parse(invItem.InvoiceType),
                No = invNo,
                TrackCode = trackCode,
                SellerID = seller.CompanyID,
                InvoiceAmountType = new InvoiceAmountType
                {
                    DiscountAmount = invItem.DiscountAmount,
                    SalesAmount = invItem.SalesAmount,
                    TaxAmount = invItem.TaxAmount,
                    TaxType = invItem.TaxType,
                    TotalAmount = invItem.TotalAmount,
                    TotalAmountInChinese = ValidityAgent.MoneyShow(invItem.TotalAmount)
                },
                PrintMark = "Y",
            };

            short seqNo = 1;

            var productItems = invItem.InvoiceItem.Select(i => new InvoiceProductItem
            {
                InvoiceProduct = new InvoiceProduct { Brief = i.Description },
                CostAmount = i.Amount,
                ItemNo = String.Format("{0}", i.SequenceNumber),
                Piece = i.Quantity,
                Piece2 = i.Quantity2Specified ? (int?)i.Quantity2 : (int?)null,
                PieceUnit = i.Unit,
                PieceUnit2 = i.Unit2,
                UnitCost = i.UnitPrice,
                UnitCost2 = i.UnitPrice2Specified ? i.UnitPrice2 : (decimal?)null,
                CostAmount2 = i.Amount2Specified ? i.Amount2 : (decimal?)null,
                Remark = i.Remark,
                TaxType = invItem.TaxType,
                No = (seqNo++)
            });
            //bool CheckProductItem = true;
            //for (int i = 0; i < productItems.ToList().Count(); i++)
            //{
            //    if (!string.IsNullOrEmpty(productItems.ToList()[i].Remark))
            //        if (Encoding.UTF8.GetBytes(productItems.ToList()[i].Remark).Length > 40)
            //        {
            //            ex = new Exception(String.Format("發票明細備註長度不得超過40Bytes:{0}", productItems.ToList()[i].Remark));
            //            CheckProductItem = false;
            //            break;
            //        }
            //}
            //if (!CheckProductItem)
            //    return null;
            newItem.InvoiceDetails.AddRange(productItems.Select(p => new InvoiceDetail
            {
                InvoiceProduct = p.InvoiceProduct
            }));

            if (_useSigner)
            {
                if (_signerCert != null)
                {
                    //if (!newItem.SignAndCheckToIssueInvoiceItem(_signerCert,null))
                    //{
                    //    ex = new Exception(String.Format("發票開立人已設定自動開立＼接收，簽章失敗:{0}", invItem.InvoiceNumber));
                    //    return null;
                    //}
                }
                else
                {
                    ex = new Exception(String.Format("發票開立人已設定自動開立＼接收，但尚未設定簽章憑證:{0}", invItem.InvoiceNumber));
                    return null;
                }
            }

            if(invItem.CustomerDefined!=null && !String.IsNullOrEmpty(invItem.CustomerDefined.IsolationFolder))
            {
                newItem.CDS_Document.CustomerDefined = new CustomerDefined
                {
                    IsolationFolder = invItem.CustomerDefined.IsolationFolder
                };
            }

            return newItem;
        }

        public override Dictionary<int, Exception> SaveUploadInvoiceCancellation(CancelInvoiceRoot item, OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();
            if (item != null && item.CancelInvoice != null && item.CancelInvoice.Length > 0)
            {
                for (int idx = 0; idx < item.CancelInvoice.Length;idx++ )
                {
                    var cancelInvoice = item.CancelInvoice[idx];
                    try
                    {
                        Exception ex;
                        InvoiceItem invoice;
                        InvoiceCancellation cancelItem = ConvertToInvoiceCancellation(owner, cancelInvoice, out ex, out invoice);

                        if (cancelItem == null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        var doc = new DerivedDocument
                        {
                            CDS_Document = new CDS_Document
                            {
                                DocType = (int)Naming.DocumentTypeDefinition.E_InvoiceCancellation,
                                DocDate = DateTime.Now,
                                CurrentStep = (int)Naming.InvoiceStepDefinition.待傳送,
                                DocumentOwner = new DocumentOwner
                                {
                                    OwnerID = owner.CompanyID
                                }
                            },
                            SourceID = cancelItem.InvoiceID
                        };


                        applyProcessFlow(doc.CDS_Document);


                        this.GetTable<InvoiceCancellation>().InsertOnSubmit(cancelItem);
                        this.GetTable<DerivedDocument>().InsertOnSubmit(doc);

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

        public InvoiceCancellation ConvertToInvoiceCancellation(OrganizationToken owner, CancelInvoiceRootCancelInvoice cancellation,out Exception ex,out InvoiceItem invoice)
        {
            ex = null;
            invoice = null;

            String invNo, trackCode;
            if (cancellation.CancelInvoiceNumber.Length >= 10)
            {
                trackCode = cancellation.CancelInvoiceNumber.Substring(0, 2);
                invNo = cancellation.CancelInvoiceNumber.Substring(2);
            }
            else
            {
                trackCode = null;
                invNo = cancellation.CancelInvoiceNumber;
            }
            cancellation.SellerId = cancellation.SellerId.GetEfficientString();
            invoice = this.EntityList.Where(i => i.No == invNo && i.TrackCode == trackCode
                && i.Organization.ReceiptNo == cancellation.SellerId).FirstOrDefault();

            if (invoice == null)
            {
                ex = new Exception(String.Format("發票號碼不存在:{0}", cancellation.CancelInvoiceNumber));
                return null;
            }

            if (invoice.InvoiceCancellation != null)
            {
                ex = new Exception(String.Format("作廢發票已存在,發票號碼:{0}", cancellation.CancelInvoiceNumber));
                return null;
            }

            var sellerID = invoice.SellerID;
            if (this.GetTable<InvoiceAllowanceItem>()
                .Where(a => a.InvoiceNo == cancellation.CancelInvoiceNumber)
                .Where(a => a.InvoiceAllowanceDetail.Any(d => d.InvoiceAllowance.InvoiceAllowanceSeller.SellerID == sellerID))
                .Any())
            {
                ex = new Exception(String.Format("欲作廢之發票已開立折讓,發票號碼:{0}", cancellation.CancelInvoiceNumber));
                return null;
            }

            if (invoice.CDS_Document.CurrentStep == (int)Naming.InvoiceStepDefinition.待接收 || invoice.CDS_Document.CurrentStep == (int)Naming.InvoiceStepDefinition.待開立)
            {
                ex = new Exception(String.Format("發票未被接收或開立,無法作廢發票,發票號碼:{0}", cancellation.CancelInvoiceNumber));
                return null;
            }

            //if (string.IsNullOrEmpty(cancellation.Remark))
            //{
            //    ex = new Exception(String.Format("作廢備註不得為空白"));
            //    return null;
            //}
            //else
            //    if (Encoding.UTF8.GetBytes(cancellation.Remark).Length > 20)
            //    {
            //        ex = new Exception(String.Format("作廢備註長度過長:{0}", cancellation.Remark));
            //        return null;
            //    }
            if (string.IsNullOrEmpty(cancellation.CancelReason))
            {
                ex = new Exception(String.Format("作廢原因不得為空白"));
                return null;
            }
            InvoiceCancellation cancelItem = new InvoiceCancellation
            {
                InvoiceID = invoice.InvoiceID,
                CancellationNo = cancellation.CancelInvoiceNumber,
                 CancelReason = cancellation.CancelReason,
                ReturnTaxDocumentNo = cancellation.ReturnTaxDocumentNumber,
                CancelDate = DateTime.ParseExact(String.Format("{0} {1}", cancellation.CancelDate, cancellation.CancelTime), "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture),
            Remark = cancellation.Remark,
                //Remark = String.Format("{0}{1}", cancellation.CancelReason, cancellation.Remark),
            };

            if (_useSigner && invoice.SellerID == owner.CompanyID)
            {
                if (_signerCert != null)
                {
                    //if (!cancelItem.SignAndCheckToIssueInvoiceCancellation(invoice, _signerCert, null))
                    //{
                    //    ex = new Exception(String.Format("作廢發票開立人已設定自動開立＼接收，簽章失敗:{0}", cancellation.CancelInvoiceNumber));
                    //    return null;
                    //}
                }
                else
                {
                    ex = new Exception(String.Format("作廢發票開立人已設定自動開立＼接收，但尚未設定簽章憑證:{0}", cancellation.CancelInvoiceNumber));
                    return null;
                }
            }

            return cancelItem;
        }

        public virtual Dictionary<int, Exception> SaveUploadInvoice(BuyerInvoiceRoot item, OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();
            if (item != null && item.Invoice != null && item.Invoice.Length > 0)
            {
                for (int idx = 0; idx < item.Invoice.Length; idx++)
                {
                    var invItem = item.Invoice[idx];
                    try
                    {

                        var seller = this.GetTable<Organization>().Where(o => o.ReceiptNo == invItem.SellerId).FirstOrDefault();
                        if (seller == null)
                        {
                            result.Add(idx, new Exception(String.Format("發票開立人為非註冊會員,統一編號:{0}", invItem.SellerId)));
                            continue;
                        }

                        var buyer = this.GetTable<Organization>().Where(o => o.ReceiptNo == invItem.BuyerId).FirstOrDefault();
                        if (buyer == null)
                        {
                            result.Add(idx, new Exception(String.Format("發票買受人為非註冊會員,統一編號:{0}", invItem.BuyerId)));
                            continue;
                        }

                        var relation = this.GetTable<BusinessRelationship>().Where(b => b.MasterID == buyer.CompanyID && b.RelativeID == seller.CompanyID && b.BusinessID == (int)Naming.InvoiceCenterBusinessType.進項).FirstOrDefault();
                        if (relation==null)
                        {
                            result.Add(idx, new Exception(String.Format("發票開立人非為買受人之進項相對營業人,單據編號:{0}", invItem.DataNumber)));
                            continue;
                        }
                        else if (relation.CurrentLevel == (int)Naming.MemberStatusDefinition.Mark_To_Delete)
                        {
                            result.Add(idx, new Exception(String.Format("已停用發票開立人為買受人之進項相對營業人關係,單據編號:{0}", invItem.DataNumber)));
                            continue;
                        }



                        InvoiceItem newItem = new InvoiceItem
                        {
                            CDS_Document = new CDS_Document
                            {
                                DocDate = DateTime.Now,
                                DocType = (int)Naming.DocumentTypeDefinition.E_Invoice,
                                DocumentOwner = new DocumentOwner
                                {
                                    OwnerID = owner.CompanyID
                                },
                                CurrentStep = (int)Naming.InvoiceStepDefinition.待開立,
                                ProcessType = (int)Naming.InvoiceProcessType.A0401,
                            },
                            InvoiceBuyer = new InvoiceBuyer
                            {
                                BuyerMark = invItem.BuyerMark,
                                Name = buyer.CompanyName,
                                ReceiptNo = buyer.ReceiptNo,
                                Address = buyer.Addr,
                                ContactName = buyer.ContactName,
                                CustomerName = buyer.CompanyName,
                                EMail = buyer.ContactEmail,
                                Fax = buyer.Fax,
                                Phone = buyer.Phone,
                                PersonInCharge = buyer.UndertakerName,
                                BuyerID = buyer.CompanyID
                            },
                            InvoiceSeller = new InvoiceSeller
                            {
                                Name = invItem.SellerName,
                                ReceiptNo = seller.ReceiptNo,
                                Address = seller.Addr,
                                ContactName = seller.ContactName,
                                CustomerName = seller.CompanyName,
                                EMail = seller.ContactEmail,
                                Fax = seller.Fax,
                                Phone = seller.Phone,
                                PersonInCharge = seller.UndertakerName,
                                SellerID = seller.CompanyID
                            },
                            InvoiceType = byte.Parse(invItem.InvoiceType),
                            SellerID = seller.CompanyID,
                            InvoiceAmountType = new InvoiceAmountType
                            {
                                DiscountAmount = invItem.DiscountAmount,
                                SalesAmount = invItem.SalesAmount,
                                TaxAmount = invItem.TaxAmount,
                                TaxType = invItem.TaxType,
                                TotalAmount = invItem.TotalAmount,
                                TotalAmountInChinese = ValidityAgent.MoneyShow(invItem.TotalAmount)
                            },
                            B2BBuyerInvoiceTag = new B2BBuyerInvoiceTag
                            {
                                DataNumber = invItem.DataNumber
                            },
                            PrintMark = "Y",
                        };

                        short seqNo = 1;

                        var productItems = invItem.InvoiceItem.Select(i => new InvoiceProductItem
                        {
                            InvoiceProduct = new InvoiceProduct { Brief = i.Description },
                            CostAmount = i.Amount,
                            CostAmount2 = i.Amount2Specified ? i.Amount2 : (decimal?)null,
                            ItemNo = String.Format("{0}", i.SequenceNumber),
                            Piece = i.Quantity,
                            Piece2 = i.Quantity2Specified ? (int?)i.Quantity2 : (int?)null,
                            PieceUnit = i.Unit,
                            PieceUnit2 = i.Unit2,
                            UnitCost = i.UnitPrice,
                            UnitCost2 = i.UnitPrice2Specified ? i.UnitPrice2 : (decimal?)null,
                            Remark = i.Remark,
                            TaxType = invItem.TaxType,
                            No = (seqNo++)
                        });
                        //bool CheckProductItem = true;
                        //for (int i = 0; i < productItems.ToList().Count(); i++)
                        //{
                        //    if (!string.IsNullOrEmpty(productItems.ToList()[i].Remark))
                        //        if (Encoding.UTF8.GetBytes(productItems.ToList()[i].Remark).Length > 40)
                        //        {
                        //            result.Add(idx, new Exception(String.Format("發票明細備註長度不得超過40Bytes:{0}", productItems.ToList()[i].Remark)));
                        //            CheckProductItem = false;
                        //            break;
                        //        }
                        //}
                        //if (!CheckProductItem)
                        //    continue;
                        newItem.InvoiceDetails.AddRange(productItems.Select(p => new InvoiceDetail
                        {
                            InvoiceProduct = p.InvoiceProduct
                        }));

                        using (var trkMgr = new TrackNoManager(this, seller.CompanyID))
                        {
                            if (trkMgr.CheckInvoiceNo(newItem))
                            {
                                applyProcessFlow(newItem.CDS_Document);

                                this.EntityList.InsertOnSubmit(newItem);
                                this.SubmitChanges();
                            }
                            else
                            {
                                result.Add(idx, new Exception(String.Format("發票字軌號碼已用完或未設定!!,統一編號:{0}", invItem.SellerId)));
                                continue;
                            }
                        }
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

        public InvoiceAllowance ConvertToInvoiceAllowance(OrganizationToken owner, AllowanceRootAllowance item, out Exception ex)
        {
            ex = null; DateTime dt;
          
            if (item.AllowanceNumber.Trim().Length == 0)
            {
                ex = new Exception(String.Format("折讓證明單號碼不得為空白"));
                return null;
            }

            if (this.GetTable<InvoiceAllowance>().Any(i => i.AllowanceNumber == item.AllowanceNumber.Trim()))
            {
                ex = new Exception(String.Format("折讓證明單號碼已存在:{0}", item.AllowanceNumber));
                return null;
            }

            //折讓證明單日期
            if (item.AllowanceDate.Trim().Length == 0)
            {
                ex = new Exception(String.Format("折讓證明單日期不得為空白"));
                return null;
            }
            else
            {
               if (!DateTime.TryParse(item.AllowanceDate, out dt))
                {
                    ex = new Exception(String.Format("折讓證明單日期格式有錯:{0}", item.AllowanceDate));
                    return null;
                }
            }
            //發票開立人
            if (string.IsNullOrEmpty(item.SellerId))
            {
                ex = new Exception(String.Format("發票開立人不得為空白"));
                return null;
            }
            var seller = this.GetTable<Organization>().Where(o => o.ReceiptNo == item.SellerId).FirstOrDefault();
            if (seller == null)
            {
                ex = new Exception(String.Format("發票開立人為非註冊會員,統一編號:{0}", item.SellerId));
                return null;
            }
            //發票開立人名稱
            if (string.IsNullOrEmpty( item.SellerName ))
            {
                ex = new Exception(String.Format("發票開立人名稱不得為空白"));
                return null;
            }
            //發票買受人
            //發票開立人
            if (string.IsNullOrEmpty(item.BuyerId))
            {
                ex = new Exception(String.Format("發票買受人不得為空白"));
                return null;
            }
            var buyer = this.GetTable<Organization>().Where(o => o.ReceiptNo == item.BuyerId).FirstOrDefault();
            if (buyer == null)
            {
                ex = new Exception(String.Format("發票買受人為非註冊會員,統一編號:{0}", item.BuyerId));
                return null;
            }
            //發票買受人名稱
            if (string.IsNullOrEmpty(item.BuyerName))
            {
                ex = new Exception(String.Format("發票買受人名稱不得為空白"));
                return null;
            }
            //折讓種類
            if ((item.AllowanceType != 1) && (item.AllowanceType != 2))
            {
                ex = new Exception(String.Format("折讓證明單種類錯誤,號碼:{0}", item.AllowanceNumber));
                return null;
            }

            //折讓單營業稅額合計
            if (item.TaxAmount  > 999999999999M || item.TaxAmount < 0 || Math.Floor(item.TaxAmount)!=item.TaxAmount )
            {
                ex = new Exception(String.Format("折讓單營業稅額合計資料錯誤:{0}", item.AllowanceNumber));
                return null;
            }
              //折讓單金額合計
            if (item.TotalAmount   > 999999999999M || item.TotalAmount < 0 ||  Math.Floor(item.TotalAmount)!=item.TotalAmount)
            {
                ex = new Exception(String.Format("折讓單金額合計資料錯誤:{0}", item.AllowanceNumber));
                return null;
            }

            //折讓單明細發票日期
            InvoiceAllowance newItem = new InvoiceAllowance
            {
                CDS_Document = new CDS_Document
                {
                    DocDate = DateTime.Now,
                    DocType = (int)Naming.DocumentTypeDefinition.E_Allowance,
                    DocumentOwner = new DocumentOwner
                    {
                        OwnerID = owner.CompanyID
                    },
                    CurrentStep = item.AllowanceType == 1 ? (int)Naming.InvoiceStepDefinition.待傳送 : (int)Naming.InvoiceStepDefinition.待開立,
                    ProcessType = (int)Naming.InvoiceProcessType.B0401,
                },
                AllowanceDate = DateTime.ParseExact(item.AllowanceDate, "yyyy/MM/dd", System.Globalization.CultureInfo.CurrentCulture),
                AllowanceNumber = item.AllowanceNumber,
                AllowanceType = item.AllowanceType,
                BuyerId = item.BuyerId,
                SellerId = item.SellerId,
                TaxAmount = item.TaxAmount,
                TotalAmount = item.TotalAmount,
                InvoiceAllowanceSeller = new InvoiceAllowanceSeller
                {
                    SellerID = seller.CompanyID,
                    Address  = seller.Addr  ,
                    ContactName = seller.ContactName ,
                    CustomerID = seller.ReceiptNo  ,
                    CustomerName = seller.CompanyName ,
                    EMail = seller.ContactEmail,
                    Fax  = seller.Fax ,
                    Name = seller.CompanyName,
                    PersonInCharge = seller.UndertakerName ,
                    Phone  = seller.Phone ,
                    PostCode = "",
                    ReceiptNo = seller.ReceiptNo,
                    RoleRemark =""
                },
                InvoiceAllowanceBuyer = new InvoiceAllowanceBuyer
                {
                    BuyerID = buyer.CompanyID,
                    CustomerName = item.BuyerName,
                    Address = buyer.Addr,
                    ContactName = buyer.ContactName,
                    CustomerID = buyer.ReceiptNo,
                    EMail = buyer.ContactEmail,
                    Fax = buyer.Fax,
                    Name = buyer.CompanyName,
                    PersonInCharge = buyer.UndertakerName,
                    Phone = buyer.Phone,
                    PostCode = "",
                    ReceiptNo = buyer.ReceiptNo,
                    RoleRemark = ""
                }
            };

            foreach (var i in item.AllowanceItem)
            {
                String invNo, trackCode;
                i.OriginalInvoiceNumber.GetInvoiceNo(out invNo, out trackCode);

                var originalInvoice = this.EntityList.Where(v => v.TrackCode == trackCode && v.No == invNo).FirstOrDefault();
                if (originalInvoice == null)
                {
                    ex = new Exception(String.Format("折讓證明單明細之原始發票號碼不存在,折讓證明單號碼:{0}", item.AllowanceNumber));
                    return null;
                }

                if (originalInvoice.InvoiceCancellation != null)
                {
                    ex = new Exception(String.Format("折讓證明單明細之原始發票已作廢,無法再接受折讓,折讓證明單號碼:{0}", item.AllowanceNumber));
                    return null;
                }

                if (originalInvoice.CDS_Document.CurrentStep == (int)Naming.B2BInvoiceQueryStepDefinition.待接收 || originalInvoice.CDS_Document.CurrentStep == (int)Naming.B2BInvoiceQueryStepDefinition.待開立)
                {
                    ex = new Exception(String.Format("折讓證明單明細之原始發票未被接收或開立,無法接受折讓,折讓證明單號碼:{0}", item.AllowanceNumber));
                    return null;
                }

                //折讓單明細發票號碼
                if (string.IsNullOrEmpty(i.OriginalInvoiceNumber))
                {
                    ex = new Exception(String.Format("折讓單明細發票號碼不得為空白:{0}", item.AllowanceNumber));
                    return null;
                }
                if (i.OriginalInvoiceNumber.Trim().Length != 10)
                {
                    ex = new Exception(String.Format("折讓單明細發票號碼長度有錯:{0}", item.AllowanceNumber));
                    return null;
                }
                //折讓單明細發票原品名
                if (string.IsNullOrEmpty(i.OriginalDescription))
                {
                    ex = new Exception(String.Format("折讓單明細原品名不得為空白:{0}", item.AllowanceNumber));
                    return null;
                }
                //折讓單明細發票數量
                if (i.Quantity > 999999999999.9999M || i.Quantity < -999999999999.9999M)
                {
                    ex = new Exception(String.Format("折讓單明細發票數量超過核可範圍:{0}", item.AllowanceNumber));
                    return null;
                }
                //折讓單明細發票數量2
                //if (i.Quantity2 > 999999999999.9999M || i.Quantity2 < -999999999999.9999M)
                //{
                //    ex = new Exception(String.Format("折讓單明細發票數量2超過核可範圍:{0}", item.AllowanceNumber));
                //    return null;
                //}
                //折讓單明細發票單價
                if (i.UnitPrice > 999999999999.9999M || i.UnitPrice < -999999999999.9999M)
                {
                    ex = new Exception(String.Format("折讓單明細發票單價超過核可範圍:{0}", item.AllowanceNumber));
                    return null;
                }
                //折讓單明細發票單價2
                //if (i.UnitPrice2 > 999999999999.9999M || i.UnitPrice2 < -999999999999.9999M)
                //{
                //    ex = new Exception(String.Format("折讓單明細發票單價2超過核可範圍:{0}", item.AllowanceNumber));
                //    return null;
                //}
                //折讓單明細發票金額
                if (i.Amount > 999999999999.9999M || i.Amount < -999999999999.9999M)
                {
                    ex = new Exception(String.Format("折讓單明細發票金額超過核可範圍:{0}", item.AllowanceNumber));
                    return null;
                }
                //折讓單明細發票金額2
                //if (i.Amount2 > 999999999999.9999M || i.Amount2 < -999999999999.9999M)
                //{
                //    ex = new Exception(String.Format("折讓單明細發票金額2超過核可範圍:{0}", item.AllowanceNumber));
                //    return null;
                //}
                //折讓單明細發票稅額
                if ( i.Tax > 999999999999M || i.Tax < 0 || Math.Floor(i.Tax) != i.Tax)
                {
                    ex = new Exception(String.Format("折讓單明細發票稅額資料錯誤:{0}", item.AllowanceNumber));
                    return null;
                }
                //折讓單明細發票課稅別
                if (i.TaxType != 1 && i.TaxType != 2 && i.TaxType != 3)
                {
                    ex = new Exception(String.Format("折讓單明細發票課稅別資料錯誤:{0}", item.AllowanceNumber));
                    return null;
                }

                if (string.IsNullOrEmpty(i.OriginalInvoiceDate))
                {
                    ex = new Exception(String.Format("折讓單明細發票日期不得為空白:{0}", item.AllowanceNumber));
                    return null;
                }
                else if (i.OriginalInvoiceDate.Trim().Length != 10)
                {
                    ex = new Exception(String.Format("折讓單明細發票日期長度有錯:{0}", item.AllowanceNumber));
                    return null;
                }

                if (!DateTime.TryParse(i.OriginalInvoiceDate, out dt))
                {
                    ex = new Exception(String.Format("折讓單明細發票日期格式有錯:{0}", item.AllowanceNumber));
                    return null;
                }
                else
                {
                    if (newItem.AllowanceDate < dt)
                    {
                        ex = new Exception(String.Format("折讓單證明日期早於原發票日期:{0}", item.AllowanceNumber));
                        return null;
                    }

                }
            }

            if (item.AllowanceItem.GroupBy(w => w.AllowanceSequenceNumber).Any(g => g.Count() > 1))
            {
                ex = new Exception(String.Format("折讓單證明單明細序號重覆:{0}", item.AllowanceNumber));
                return null;
            }

            List<InvoiceAllowanceItem> productItems = new List<InvoiceAllowanceItem>();
            foreach (var i in item.AllowanceItem)
            {
                var allowanceItem = new InvoiceAllowanceItem
                {
                    Amount = i.Amount,
                    //Amount2 = i.Amount2,
                    InvoiceNo = i.OriginalInvoiceNumber,
                    InvoiceDate = DateTime.ParseExact(i.OriginalInvoiceDate, "yyyy/MM/dd", System.Globalization.CultureInfo.CurrentCulture),
                    OriginalSequenceNo = (short)i.OriginalSequenceNumber,
                    Piece = i.Quantity,
                    //Piece2 = i.Quantity2,
                    PieceUnit = i.Unit,
                    //PieceUnit2 = i.Unit2,
                    OriginalDescription = i.OriginalDescription,
                    Tax = i.Tax ,
                    TaxType = i.TaxType,
                    No = (short)i.AllowanceSequenceNumber,
                    UnitCost = i.UnitPrice,
                    //UnitCost2 = i.UnitPrice2
                };

                productItems.Add(allowanceItem);
            }

            newItem.InvoiceAllowanceDetails.AddRange(productItems.Select(p => new InvoiceAllowanceDetail
            {
                InvoiceAllowanceItem = p
            }));

            return newItem;
        }

        //public InvoiceAllowance ConvertToInvoiceAllowance(OrganizationToken owner, Schema.TurnKey.B1101.Allowance allowance)
        //{
        //    Organization buyer = this.GetTable<Organization>().Where(o => o.ReceiptNo == allowance.Main.Buyer.Identifier).FirstOrDefault();
        //    if (buyer == null)
        //    {
        //        buyer = new Organization
        //        {
        //            Addr = allowance.Main.Buyer.Address,
        //            CompanyName = allowance.Main.Buyer.Name,
        //            UndertakerName = allowance.Main.Buyer.PersonInCharge,
        //            Phone = allowance.Main.Buyer.TelephoneNumber,
        //            Fax = allowance.Main.Buyer.FacsimileNumber,
        //            ContactEmail = allowance.Main.Buyer.EmailAddress,
        //            ReceiptNo = allowance.Main.Buyer.Identifier
        //        };
        //    }

        //    Organization seller = this.GetTable<Organization>().Where(o => o.ReceiptNo == allowance.Main.Seller.Identifier).FirstOrDefault();
        //    if (buyer == null)
        //    {
        //        seller = new Organization
        //        {
        //            Addr = allowance.Main.Seller.Address,
        //            CompanyName = allowance.Main.Seller.Name,
        //            UndertakerName = allowance.Main.Seller.PersonInCharge,
        //            Phone = allowance.Main.Seller.TelephoneNumber,
        //            Fax = allowance.Main.Seller.FacsimileNumber,
        //            ContactEmail = allowance.Main.Seller.EmailAddress,
        //            ReceiptNo = allowance.Main.Seller.Identifier
        //        };
        //    }

        //    InvoiceAllowance newItem = new InvoiceAllowance
        //    {
        //        CDS_Document = new CDS_Document
        //        {
        //            DocDate = DateTime.Now,
        //            DocType = (int)Naming.DocumentTypeDefinition.E_Allowance,
        //            DocumentOwner = new DocumentOwner
        //            {
        //                OwnerID = owner.CompanyID
        //            },
        //            CurrentStep = (int)Naming.B2BInvoiceStepDefinition.待傳送
        //        },
        //        AllowanceDate = DateTime.ParseExact(allowance.Main.AllowanceDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture),
        //        AllowanceNumber = allowance.Main.AllowanceNumber,
        //        AllowanceType = (byte)((int)allowance.Main.AllowanceType),
        //        BuyerId = allowance.Main.Buyer.Identifier,
        //        SellerId = allowance.Main.Seller.Identifier,
        //        TaxAmount = allowance.Amount.TaxAmount,
        //        TotalAmount = allowance.Amount.TotalAmount,
        //        InvoiceAllowanceSeller = new InvoiceAllowanceSeller
        //        {
        //            Name = allowance.Main.Seller.Name,
        //            ReceiptNo = allowance.Main.Seller.Identifier,
        //            ContactName = allowance.Main.Seller.PersonInCharge,
        //            Address = allowance.Main.Seller.Address,
        //            CustomerID = allowance.Main.Seller.CustomerNumber,
        //            CustomerName = allowance.Main.Seller.Name,
        //            EMail = allowance.Main.Seller.EmailAddress,
        //            Fax = allowance.Main.Seller.FacsimileNumber,
        //            PersonInCharge = allowance.Main.Seller.PersonInCharge,
        //            Phone = allowance.Main.Seller.TelephoneNumber,
        //            RoleRemark = allowance.Main.Seller.RoleRemark,
        //            Organization = seller
        //        },
        //        InvoiceAllowanceBuyer = new InvoiceAllowanceBuyer
        //        {
        //            Name = allowance.Main.Buyer.Name,
        //            ReceiptNo = allowance.Main.Buyer.Identifier,
        //            ContactName = allowance.Main.Buyer.PersonInCharge,
        //            Address = allowance.Main.Buyer.Address,
        //            CustomerID = allowance.Main.Buyer.CustomerNumber,
        //            CustomerName = allowance.Main.Buyer.Name,
        //            EMail = allowance.Main.Buyer.EmailAddress,
        //            Fax = allowance.Main.Buyer.FacsimileNumber,
        //            PersonInCharge = allowance.Main.Buyer.PersonInCharge,
        //            Phone = allowance.Main.Buyer.TelephoneNumber,
        //            RoleRemark = allowance.Main.Buyer.RoleRemark,
        //            Organization = buyer
        //        }
        //    };

        //    //bool invalid = false;
        //    //foreach (var i in allowance.Details)
        //    //{
        //    //    String invNo, trackCode;
        //    //    getInvoiceNo(i.OriginalInvoiceNumber, out invNo, out trackCode);

        //    //    var originalInvoice = this.EntityList.Where(v => v.TrackCode == trackCode && v.No == invNo).FirstOrDefault();
        //    //    if (originalInvoice == null)
        //    //    {
        //    //        invalid = true;
        //    //        break;
        //    //    }
        //    //}

        //    //if (invalid)
        //    //{
        //    //    throw new Exception(String.Format("折讓證明單明細之原始發票號碼不存在,折讓證明單號碼:{0}", allowance.Main.AllowanceNumber));
        //    //}


        //    List<InvoiceAllowanceItem> productItems = new List<InvoiceAllowanceItem>();
        //    foreach (var i in allowance.Details)
        //    {
        //        var allowanceItem = new InvoiceAllowanceItem
        //        {
        //            Amount = i.Amount,
        //            Amount2 = i.Amount2,
        //            InvoiceNo = i.OriginalInvoiceNumber,
        //            InvoiceDate = DateTime.ParseExact(i.OriginalInvoiceDate, "yyyy/MM/dd", System.Globalization.CultureInfo.CurrentCulture),
        //            OriginalSequenceNo = !String.IsNullOrEmpty(i.OriginalSequenceNumber) ? short.Parse(i.OriginalSequenceNumber) : (short?)null,
        //            Piece = i.Quantity,
        //            Piece2 = i.Quantity2,
        //            PieceUnit = i.Unit,
        //            PieceUnit2 = i.Unit2,
        //            OriginalDescription = i.OriginalDescription,
        //            TaxType = (byte)i.TaxType,
        //            No = !String.IsNullOrEmpty(i.AllowanceSequenceNumber) ? short.Parse(i.AllowanceSequenceNumber) : (short?)null,
        //            UnitCost = i.UnitPrice,
        //            UnitCost2 = i.UnitPrice2
        //        };

        //        productItems.Add(allowanceItem);
        //    }

        //    newItem.InvoiceAllowanceDetails.AddRange(productItems.Select(p => new InvoiceAllowanceDetail
        //    {
        //        InvoiceAllowanceItem = p
        //    }));

        //    return newItem;
        //}

        //public InvoiceAllowance ConvertToInvoiceAllowance(OrganizationToken owner, Schema.TurnKey.B1401.Allowance allowance)
        //{
        //    Organization buyer = this.GetTable<Organization>().Where(o => o.ReceiptNo == allowance.Main.Buyer.Identifier).FirstOrDefault();
        //    if (buyer == null)
        //    {
        //        buyer = new Organization
        //        {
        //            Addr = allowance.Main.Buyer.Address,
        //            CompanyName = allowance.Main.Buyer.Name,
        //            UndertakerName = allowance.Main.Buyer.PersonInCharge,
        //            Phone = allowance.Main.Buyer.TelephoneNumber,
        //            Fax = allowance.Main.Buyer.FacsimileNumber,
        //            ContactEmail = allowance.Main.Buyer.EmailAddress,
        //            ReceiptNo = allowance.Main.Buyer.Identifier
        //        };
        //    }

        //    Organization seller = this.GetTable<Organization>().Where(o => o.ReceiptNo == allowance.Main.Seller.Identifier).FirstOrDefault();
        //    if (buyer == null)
        //    {
        //        seller = new Organization
        //        {
        //            Addr = allowance.Main.Seller.Address,
        //            CompanyName = allowance.Main.Seller.Name,
        //            UndertakerName = allowance.Main.Seller.PersonInCharge,
        //            Phone = allowance.Main.Seller.TelephoneNumber,
        //            Fax = allowance.Main.Seller.FacsimileNumber,
        //            ContactEmail = allowance.Main.Seller.EmailAddress,
        //            ReceiptNo = allowance.Main.Seller.Identifier
        //        };
        //    }

        //    InvoiceAllowance newItem = new InvoiceAllowance
        //    {
        //        CDS_Document = new CDS_Document
        //        {
        //            DocDate = DateTime.Now,
        //            DocType = (int)Naming.DocumentTypeDefinition.E_Allowance,
        //            DocumentOwner = new DocumentOwner
        //            {
        //                OwnerID = owner.CompanyID
        //            },
        //            CurrentStep = (int)Naming.B2BInvoiceStepDefinition.待傳送
        //        },
        //        AllowanceDate = DateTime.ParseExact(allowance.Main.AllowanceDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture),
        //        AllowanceNumber = allowance.Main.AllowanceNumber,
        //        AllowanceType = (byte)((int)allowance.Main.AllowanceType),
        //        BuyerId = allowance.Main.Buyer.Identifier,
        //        SellerId = allowance.Main.Seller.Identifier,
        //        TaxAmount = allowance.Amount.TaxAmount,
        //        TotalAmount = allowance.Amount.TotalAmount,
        //        InvoiceAllowanceSeller = new InvoiceAllowanceSeller
        //        {
        //            Name = allowance.Main.Seller.Name,
        //            ReceiptNo = allowance.Main.Seller.Identifier,
        //            ContactName = allowance.Main.Seller.PersonInCharge,
        //            Address = allowance.Main.Seller.Address,
        //            CustomerID = allowance.Main.Seller.CustomerNumber,
        //            CustomerName = allowance.Main.Seller.Name,
        //            EMail = allowance.Main.Seller.EmailAddress,
        //            Fax = allowance.Main.Seller.FacsimileNumber,
        //            PersonInCharge = allowance.Main.Seller.PersonInCharge,
        //            Phone = allowance.Main.Seller.TelephoneNumber,
        //            RoleRemark = allowance.Main.Seller.RoleRemark,
        //            Organization = seller
        //        },
        //        InvoiceAllowanceBuyer = new InvoiceAllowanceBuyer
        //        {
        //            Name = allowance.Main.Buyer.Name,
        //            ReceiptNo = allowance.Main.Buyer.Identifier,
        //            ContactName = allowance.Main.Buyer.PersonInCharge,
        //            Address = allowance.Main.Buyer.Address,
        //            CustomerID = allowance.Main.Buyer.CustomerNumber,
        //            CustomerName = allowance.Main.Buyer.Name,
        //            EMail = allowance.Main.Buyer.EmailAddress,
        //            Fax = allowance.Main.Buyer.FacsimileNumber,
        //            PersonInCharge = allowance.Main.Buyer.PersonInCharge,
        //            Phone = allowance.Main.Buyer.TelephoneNumber,
        //            RoleRemark = allowance.Main.Buyer.RoleRemark,
        //            Organization = buyer
        //        }
        //    };

        //    //bool invalid = false;
        //    //foreach (var i in allowance.Details)
        //    //{
        //    //    String invNo, trackCode;
        //    //    getInvoiceNo(i.OriginalInvoiceNumber, out invNo, out trackCode);

        //    //    var originalInvoice = this.EntityList.Where(v => v.TrackCode == trackCode && v.No == invNo).FirstOrDefault();
        //    //    if (originalInvoice == null)
        //    //    {
        //    //        invalid = true;
        //    //        break;
        //    //    }
        //    //}

        //    //if (invalid)
        //    //{
        //    //    throw new Exception(String.Format("折讓證明單明細之原始發票號碼不存在,折讓證明單號碼:{0}", allowance.Main.AllowanceNumber));
        //    //}


        //    List<InvoiceAllowanceItem> productItems = new List<InvoiceAllowanceItem>();
        //    foreach (var i in allowance.Details)
        //    {
        //        var allowanceItem = new InvoiceAllowanceItem
        //        {
        //            Amount = i.Amount,
        //            Amount2 = i.Amount2,
        //            InvoiceNo = i.OriginalInvoiceNumber,
        //            InvoiceDate = DateTime.ParseExact(i.OriginalInvoiceDate, "yyyy/MM/dd", System.Globalization.CultureInfo.CurrentCulture),
        //            OriginalSequenceNo = !String.IsNullOrEmpty(i.OriginalSequenceNumber) ? short.Parse(i.OriginalSequenceNumber) : (short?)null,
        //            Piece = i.Quantity,
        //            Piece2 = i.Quantity2,
        //            PieceUnit = i.Unit,
        //            PieceUnit2 = i.Unit2,
        //            OriginalDescription = i.OriginalDescription,
        //            TaxType = (byte)i.TaxType,
        //            No = !String.IsNullOrEmpty(i.AllowanceSequenceNumber) ? short.Parse(i.AllowanceSequenceNumber) : (short?)null,
        //            UnitCost = i.UnitPrice,
        //            UnitCost2 = i.UnitPrice2
        //        };

        //        productItems.Add(allowanceItem);
        //    }

        //    newItem.InvoiceAllowanceDetails.AddRange(productItems.Select(p => new InvoiceAllowanceDetail
        //    {
        //        InvoiceAllowanceItem = p
        //    }));

        //    return newItem;
        //}

        public InvoiceAllowance ConvertToInvoiceAllowance(OrganizationToken owner, Schema.TurnKey.B0401.Allowance allowance)
        {
            Organization buyer = this.GetTable<Organization>().Where(o => o.ReceiptNo == allowance.Main.Buyer.Identifier).FirstOrDefault();
            if (buyer == null)
            {
                buyer = new Organization
                {
                    Addr = allowance.Main.Buyer.Address,
                    CompanyName = allowance.Main.Buyer.Name,
                    UndertakerName = allowance.Main.Buyer.PersonInCharge,
                    Phone = allowance.Main.Buyer.TelephoneNumber,
                    Fax = allowance.Main.Buyer.FacsimileNumber,
                    ContactEmail = allowance.Main.Buyer.EmailAddress,
                    ReceiptNo = allowance.Main.Buyer.Identifier
                };
            }

            Organization seller = this.GetTable<Organization>().Where(o => o.ReceiptNo == allowance.Main.Seller.Identifier).FirstOrDefault();
            if (buyer == null)
            {
                seller = new Organization
                {
                    Addr = allowance.Main.Seller.Address,
                    CompanyName = allowance.Main.Seller.Name,
                    UndertakerName = allowance.Main.Seller.PersonInCharge,
                    Phone = allowance.Main.Seller.TelephoneNumber,
                    Fax = allowance.Main.Seller.FacsimileNumber,
                    ContactEmail = allowance.Main.Seller.EmailAddress,
                    ReceiptNo = allowance.Main.Seller.Identifier
                };
            }

            InvoiceAllowance newItem = new InvoiceAllowance
            {
                CDS_Document = new CDS_Document
                {
                    DocDate = DateTime.Now,
                    DocType = (int)Naming.DocumentTypeDefinition.E_Allowance,
                    DocumentOwner = new DocumentOwner
                    {
                        OwnerID = owner.CompanyID
                    },
                    CurrentStep = (int)Naming.InvoiceStepDefinition.待傳送,
                    ProcessType = (int)Naming.InvoiceProcessType.B0401,
                },
                AllowanceDate = DateTime.ParseExact(allowance.Main.AllowanceDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture),
                AllowanceNumber = allowance.Main.AllowanceNumber,
                AllowanceType = (byte)((int)allowance.Main.AllowanceType),
                BuyerId = allowance.Main.Buyer.Identifier,
                SellerId = allowance.Main.Seller.Identifier,
                TaxAmount = allowance.Amount.TaxAmount,
                TotalAmount = allowance.Amount.TotalAmount,
                InvoiceAllowanceSeller = new InvoiceAllowanceSeller
                {
                    Name = allowance.Main.Seller.Name,
                    ReceiptNo = allowance.Main.Seller.Identifier,
                    ContactName = allowance.Main.Seller.PersonInCharge,
                    Address = allowance.Main.Seller.Address,
                    CustomerID = allowance.Main.Seller.CustomerNumber,
                    CustomerName = allowance.Main.Seller.Name,
                    EMail = allowance.Main.Seller.EmailAddress,
                    Fax = allowance.Main.Seller.FacsimileNumber,
                    PersonInCharge = allowance.Main.Seller.PersonInCharge,
                    Phone = allowance.Main.Seller.TelephoneNumber,
                    RoleRemark = allowance.Main.Seller.RoleRemark,
                    Organization = seller
                },
                InvoiceAllowanceBuyer = new InvoiceAllowanceBuyer
                {
                    Name = allowance.Main.Buyer.Name,
                    ReceiptNo = allowance.Main.Buyer.Identifier,
                    ContactName = allowance.Main.Buyer.PersonInCharge,
                    Address = allowance.Main.Buyer.Address,
                    CustomerID = allowance.Main.Buyer.CustomerNumber,
                    CustomerName = allowance.Main.Buyer.Name,
                    EMail = allowance.Main.Buyer.EmailAddress,
                    Fax = allowance.Main.Buyer.FacsimileNumber,
                    PersonInCharge = allowance.Main.Buyer.PersonInCharge,
                    Phone = allowance.Main.Buyer.TelephoneNumber,
                    RoleRemark = allowance.Main.Buyer.RoleRemark,
                    Organization = buyer
                }
            };

            //bool invalid = false;
            //foreach (var i in allowance.Details)
            //{
            //    String invNo, trackCode;
            //    getInvoiceNo(i.OriginalInvoiceNumber, out invNo, out trackCode);

            //    var originalInvoice = this.EntityList.Where(v => v.TrackCode == trackCode && v.No == invNo).FirstOrDefault();
            //    if (originalInvoice == null)
            //    {
            //        invalid = true;
            //        break;
            //    }
            //}

            //if (invalid)
            //{
            //    throw new Exception(String.Format("折讓證明單明細之原始發票號碼不存在,折讓證明單號碼:{0}", allowance.Main.AllowanceNumber));
            //}


            List<InvoiceAllowanceItem> productItems = new List<InvoiceAllowanceItem>();
            foreach (var i in allowance.Details)
            {
                var allowanceItem = new InvoiceAllowanceItem
                {
                    Amount = i.Amount,
                    InvoiceNo = i.OriginalInvoiceNumber,
                    InvoiceDate = DateTime.ParseExact(i.OriginalInvoiceDate, "yyyy/MM/dd", System.Globalization.CultureInfo.CurrentCulture),
                    OriginalSequenceNo = !String.IsNullOrEmpty(i.OriginalSequenceNumber) ? short.Parse(i.OriginalSequenceNumber) : (short?)null,
                    Piece = i.Quantity,
                    PieceUnit = i.Unit,
                    OriginalDescription = i.OriginalDescription,
                    TaxType = (byte)i.TaxType,
                    No = !String.IsNullOrEmpty(i.AllowanceSequenceNumber) ? short.Parse(i.AllowanceSequenceNumber) : (short?)null,
                    UnitCost = i.UnitPrice
                };

                productItems.Add(allowanceItem);
            }

            newItem.InvoiceAllowanceDetails.AddRange(productItems.Select(p => new InvoiceAllowanceDetail
            {
                InvoiceAllowanceItem = p
            }));

            return newItem;
        }


        public override Dictionary<int, Exception> SaveUploadAllowance(AllowanceRoot root, OrganizationToken owner)
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
                        Exception ex;
                        InvoiceAllowance newItem = ConvertToInvoiceAllowance(owner, item,out ex);

                        if (newItem == null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        applyProcessFlow(newItem.CDS_Document);

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


        public override Dictionary<int, Exception> SaveUploadAllowanceCancellation(CancelAllowanceRoot root, OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();
            if (root != null && root.CancelAllowance != null && root.CancelAllowance.Length > 0)
            {
                var tblCancel = this.GetTable<InvoiceAllowanceCancellation>();

                for (int idx=0; idx<root.CancelAllowance.Length;idx++)
                {
                    var item = root.CancelAllowance[idx];
                    try
                    {
                        Exception ex;
                        InvoiceAllowance allowance;
                        InvoiceAllowanceCancellation cancelItem = ConvertToAllowanceCancellation(owner, item, out ex,out allowance);

                        if (cancelItem == null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        var doc = new DerivedDocument
                        {
                            CDS_Document = new CDS_Document
                            {
                                DocType = (int)Naming.DocumentTypeDefinition.E_AllowanceCancellation,
                                DocDate = DateTime.Now,
                                CurrentStep = (int)Naming.InvoiceStepDefinition.待傳送,
                                DocumentOwner = new DocumentOwner
                                {
                                    OwnerID = owner.CompanyID
                                }
                            },
                            SourceID = cancelItem.AllowanceID
                        };

                        applyProcessFlow(doc.CDS_Document);

                        this.GetTable<InvoiceAllowanceCancellation>().InsertOnSubmit(cancelItem);
                        this.GetTable<DerivedDocument>().InsertOnSubmit(doc);

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

        public InvoiceAllowanceCancellation ConvertToAllowanceCancellation(OrganizationToken owner, CancelAllowanceRootCancelAllowance item,out Exception ex,out InvoiceAllowance allowance)
        {
            ex = null;
            DateTime allowanceDate,cancelDate;

            allowance = this.GetTable<InvoiceAllowance>().Where(i => i.AllowanceNumber == item.CancelAllowanceNumber ).FirstOrDefault();

            if (allowance == null)
            {
                ex =  new Exception(String.Format("折讓證明單號碼不存在:{0}", item.CancelAllowanceNumber));
                return null;
            }

            if (allowance.InvoiceAllowanceCancellation != null)
            {
                ex = new Exception(String.Format("作廢折讓單已存在,折讓單號碼:{0}", item.CancelAllowanceNumber));
                return null;
            }

            if (allowance.CDS_Document.CurrentStep == (int)Naming.InvoiceStepDefinition.待接收 || allowance.CDS_Document.CurrentStep == (int)Naming.InvoiceStepDefinition.待開立)
            {
                ex = new Exception(String.Format("折讓證明單未被接收或開立,無法作廢折讓證明單,折讓單號碼:{0}", item.CancelAllowanceNumber));
                return null;
            }


            if (string.IsNullOrEmpty (item.AllowanceDate))
            {
                ex = new Exception(String.Format("折讓證明單日期不得為空白"));
                return null;
            }
            if (!DateTime.TryParse  (item.AllowanceDate,out allowanceDate))
            {
                ex = new Exception(String.Format("折讓證明單日期格式有誤"));
                return null;
            }
            if (string.IsNullOrEmpty(item.CancelDate))
            {
                ex = new Exception(String.Format("作廢折讓單日期不得為空白"));
                return null;
            }

            if (string.IsNullOrEmpty(item.CancelTime))
            {
                ex = new Exception(String.Format("作廢折讓單時間不得為空白"));
                return null;
            }

            if (!DateTime.TryParseExact(String.Format("{0} {1}", item.CancelDate, item.CancelTime), "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture,System.Globalization.DateTimeStyles.AssumeLocal,out cancelDate))
            {
                ex = new Exception(String.Format("作廢折讓單日期格式有誤"));
                return null;
            }
            if (cancelDate < allowanceDate)
            {
                ex = new Exception(String.Format("作廢折讓單日期不可小於折讓日期"));
                return null;
            }

            //if (allowance.AllowanceDate.Value != DateTime.Parse(item.AllowanceDate))
            //{
            //    ex = new Exception(String.Format("折讓證明單日期不合,折讓單日期:{0}", item.AllowanceDate));
            //    return null;
            //}

            if (string.IsNullOrEmpty(item.CancelReason ))
            {
                ex = new Exception(String.Format("作廢折讓單原因不得為空白"));
                return null;
            }

            InvoiceAllowanceCancellation cancelItem = new InvoiceAllowanceCancellation
            {
                AllowanceID = allowance.AllowanceID,
                Remark =item.CancelReason +" "+ item.Remark,
                CancelDate = cancelDate
            };

            return cancelItem;
        }

        //public InvoiceItem ConvertToInvoiceItem(OrganizationToken owner, ModelCore.Schema.TurnKey.A1101.Invoice invoice)
        //{
        //    Organization buyer = this.GetTable<Organization>().Where(o => o.ReceiptNo == invoice.Main.Buyer.Identifier).FirstOrDefault();
        //    if (buyer == null)
        //    {
        //        buyer = new Organization
        //        {
        //            Addr = invoice.Main.Buyer.Address,
        //            CompanyName = invoice.Main.Buyer.Name,
        //            UndertakerName = invoice.Main.Buyer.PersonInCharge,
        //            Phone = invoice.Main.Buyer.TelephoneNumber,
        //            Fax = invoice.Main.Buyer.FacsimileNumber,
        //            ContactEmail = invoice.Main.Buyer.EmailAddress,
        //            ReceiptNo = invoice.Main.Buyer.Identifier
        //        };
        //    }

        //    Organization seller = this.GetTable<Organization>().Where(o => o.ReceiptNo == invoice.Main.Seller.Identifier).FirstOrDefault();
        //    if (buyer == null)
        //    {
        //        seller = new Organization
        //        {
        //            Addr = invoice.Main.Seller.Address,
        //            CompanyName = invoice.Main.Seller.Name,
        //            UndertakerName = invoice.Main.Seller.PersonInCharge,
        //            Phone = invoice.Main.Seller.TelephoneNumber,
        //            Fax = invoice.Main.Seller.FacsimileNumber,
        //            ContactEmail = invoice.Main.Seller.EmailAddress,
        //            ReceiptNo = invoice.Main.Seller.Identifier
        //        };
        //    }

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
        //            CurrentStep = (int)Naming.B2BInvoiceStepDefinition.待傳送
        //        },
        //        InvoiceBuyer = new InvoiceBuyer
        //        {
        //            BuyerMark = invoice.Main.BuyerRemarkSpecified ? (int)invoice.Main.BuyerRemark : (int?)null,
        //            Name = invoice.Main.Buyer.Name,
        //            ReceiptNo = invoice.Main.Buyer.Identifier,
        //            ContactName = invoice.Main.Buyer.PersonInCharge,
        //            Address = invoice.Main.Buyer.Address,
        //            CustomerID = invoice.Main.Buyer.CustomerNumber,
        //            CustomerName = invoice.Main.Buyer.Name,
        //            EMail = invoice.Main.Buyer.EmailAddress,
        //            Fax = invoice.Main.Buyer.FacsimileNumber,
        //            PersonInCharge = invoice.Main.Buyer.PersonInCharge,
        //            Phone = invoice.Main.Buyer.TelephoneNumber,
        //            RoleRemark = invoice.Main.Buyer.RoleRemark,
        //            Organization = buyer
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
        //            Organization = seller
        //        },
        //        InvoiceDate = DateTime.ParseExact(String.Format("{0}", invoice.Main.InvoiceDate), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture).Add(invoice.Main.InvoiceTimeSpecified ? invoice.Main.InvoiceTime.TimeOfDay : TimeSpan.Zero),
        //        InvoiceType = (byte)((int)invoice.Main.Invoice),
        //        No = invNo,
        //        TrackCode = trackCode,
        //        SellerID = seller.CompanyID,
        //        BuyerRemark = (byte?)invoice.Main.BuyerRemark,
        //        Category = invoice.Main.Category,
        //        CheckNo = invoice.Main.CheckNumber,
        //        DonateMark = ((int)invoice.Main.DonateMark).ToString(),
        //        CustomsClearanceMark = (byte?)invoice.Main.CustomsClearanceMark,
        //        GroupMark = invoice.Main.GroupMark,
        //        PermitDate = String.IsNullOrEmpty(invoice.Main.PermitDate) ? (DateTime?)null : DateTime.ParseExact(invoice.Main.PermitDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture),
        //        PermitNumber = invoice.Main.PermitNumber,
        //        PermitWord = invoice.Main.PermitWord,
        //        RelateNumber = invoice.Main.RelateNumber,
        //        Remark = invoice.Main.MainRemark,
        //        TaxCenter = invoice.Main.TaxCenter,
        //        InvoiceAmountType = new InvoiceAmountType
        //        {
        //            DiscountAmount = invoice.Amount.DiscountAmountSpecified ? invoice.Amount.DiscountAmount : (decimal?)null,
        //            SalesAmount = invoice.Amount.SalesAmount,
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
        //        CostAmount2 = i.Amount2,
        //        ItemNo = i.SequenceNumber,
        //        Piece = (int?)i.Quantity,
        //        Piece2 = (int?)i.Quantity2,
        //        PieceUnit = i.Unit,
        //        PieceUnit2 = i.Unit2,
        //        UnitCost = i.UnitPrice,
        //        UnitCost2 = i.UnitPrice2,
        //        Remark = i.Remark,
        //        TaxType = newItem.InvoiceAmountType.TaxType,
        //        RelateNumber = i.RelateNumber,
        //        No = (seqNo++)
        //    });

        //    newItem.InvoiceDetails.AddRange(productItems.Select(p => new InvoiceDetail
        //    {
        //        InvoiceProduct = p.InvoiceProduct
        //    }));
        //    return newItem;
        //}

        //public InvoiceItem ConvertToInvoiceItem(OrganizationToken owner, ModelCore.Schema.TurnKey.A1401.Invoice invoice)
        //{
        //    Organization buyer = this.GetTable<Organization>().Where(o => o.ReceiptNo == invoice.Main.Buyer.Identifier).FirstOrDefault();
        //    if (buyer == null)
        //    {
        //        buyer = new Organization
        //        {
        //            Addr = invoice.Main.Buyer.Address,
        //            CompanyName = invoice.Main.Buyer.Name,
        //            UndertakerName = invoice.Main.Buyer.PersonInCharge,
        //            Phone = invoice.Main.Buyer.TelephoneNumber,
        //            Fax = invoice.Main.Buyer.FacsimileNumber,
        //            ContactEmail = invoice.Main.Buyer.EmailAddress,
        //            ReceiptNo = invoice.Main.Buyer.Identifier
        //        };
        //    }

        //    Organization seller = this.GetTable<Organization>().Where(o => o.ReceiptNo == invoice.Main.Seller.Identifier).FirstOrDefault();
        //    if (seller == null)
        //    {
        //        seller = new Organization
        //        {
        //            Addr = invoice.Main.Seller.Address,
        //            CompanyName = invoice.Main.Seller.Name,
        //            UndertakerName = invoice.Main.Seller.PersonInCharge,
        //            Phone = invoice.Main.Seller.TelephoneNumber,
        //            Fax = invoice.Main.Seller.FacsimileNumber,
        //            ContactEmail = invoice.Main.Seller.EmailAddress,
        //            ReceiptNo = invoice.Main.Seller.Identifier,
        //            OrganizationStatus = new OrganizationStatus
        //            {
        //                IronSteelIndustry = true
        //            }
        //        };
        //    }
        //    else if (seller.OrganizationStatus == null)
        //    {
        //        seller.OrganizationStatus = new OrganizationStatus
        //            {
        //                IronSteelIndustry = true
        //            };
        //    }

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
        //            CurrentStep = (int)Naming.B2BInvoiceStepDefinition.待傳送
        //        },
        //        InvoiceBuyer = new InvoiceBuyer
        //        {
        //            BuyerMark = invoice.Main.BuyerRemarkSpecified ? (int)invoice.Main.BuyerRemark : (int?)null,
        //            Name = invoice.Main.Buyer.Name,
        //            ReceiptNo = invoice.Main.Buyer.Identifier,
        //            ContactName = invoice.Main.Buyer.PersonInCharge,
        //            Address = invoice.Main.Buyer.Address,
        //            CustomerID = invoice.Main.Buyer.CustomerNumber,
        //            CustomerName = invoice.Main.Buyer.Name,
        //            EMail = invoice.Main.Buyer.EmailAddress,
        //            Fax = invoice.Main.Buyer.FacsimileNumber,
        //            PersonInCharge = invoice.Main.Buyer.PersonInCharge,
        //            Phone = invoice.Main.Buyer.TelephoneNumber,
        //            RoleRemark = invoice.Main.Buyer.RoleRemark,
        //            Organization = buyer
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
        //            Organization = seller
        //        },
        //        InvoiceDate = DateTime.ParseExact(String.Format("{0}", invoice.Main.InvoiceDate), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture).Add(invoice.Main.InvoiceTimeSpecified ? invoice.Main.InvoiceTime.TimeOfDay : TimeSpan.Zero),
        //        InvoiceType = (byte)((int)invoice.Main.InvoiceType),
        //        No = invNo,
        //        TrackCode = trackCode,
        //        SellerID = seller.CompanyID,
        //        BuyerRemark = (byte?)invoice.Main.BuyerRemark,
        //        Category = invoice.Main.Category,
        //        CheckNo = invoice.Main.CheckNumber,
        //        DonateMark = ((int)invoice.Main.DonateMark).ToString(),
        //        CustomsClearanceMark = (byte?)invoice.Main.CustomsClearanceMark,
        //        GroupMark = invoice.Main.GroupMark,
        //        PermitDate = String.IsNullOrEmpty(invoice.Main.PermitDate) ? (DateTime?)null : DateTime.ParseExact(invoice.Main.PermitDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture),
        //        PermitNumber = invoice.Main.PermitNumber,
        //        PermitWord = invoice.Main.PermitWord,
        //        RelateNumber = invoice.Main.RelateNumber,
        //        Remark = invoice.Main.MainRemark,
        //        TaxCenter = invoice.Main.TaxCenter,
        //        InvoiceAmountType = new InvoiceAmountType
        //        {
        //            DiscountAmount = invoice.Amount.DiscountAmountSpecified ? invoice.Amount.DiscountAmount : (decimal?)null,
        //            SalesAmount = invoice.Amount.SalesAmount,
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
        //        CostAmount2 = i.Amount2,
        //        ItemNo = i.SequenceNumber,
        //        Piece = (int?)i.Quantity,
        //        Piece2 = (int?)i.Quantity2,
        //        PieceUnit = i.Unit,
        //        PieceUnit2 = i.Unit2,
        //        UnitCost = i.UnitPrice,
        //        UnitCost2 = i.UnitPrice2,
        //        Remark = i.Remark,
        //        TaxType = newItem.InvoiceAmountType.TaxType,
        //        RelateNumber = i.RelateNumber,
        //        No = (seqNo++)
        //    });

        //    newItem.InvoiceDetails.AddRange(productItems.Select(p => new InvoiceDetail
        //    {
        //        InvoiceProduct = p.InvoiceProduct
        //    }));
        //    return newItem;
        //}

        //public void SaveA1101(ModelCore.Schema.TurnKey.A1101.Invoice invoice, OrganizationToken owner)
        //{
        //    InvoiceItem newItem = ConvertToInvoiceItem(owner, invoice);
        //    this.EntityList.InsertOnSubmit(newItem);
        //    this.SubmitChanges();
        //}

        //public void SaveA1401(ModelCore.Schema.TurnKey.A1401.Invoice invoice, OrganizationToken owner)
        //{
        //    InvoiceItem newItem = ConvertToInvoiceItem(owner, invoice);
        //    applyProcessFlow(newItem.CDS_Document, Naming.B2BInvoiceDocumentTypeDefinition.電子發票); 
        //    this.EntityList.InsertOnSubmit(newItem);
        //    this.SubmitChanges();
        //}

        public void SaveA0401(ModelCore.Schema.TurnKey.A0401.Invoice invoice, OrganizationToken owner)
        {
            InvoiceItem newItem = this.ConvertToInvoiceItem(invoice, owner);
            applyProcessFlow(newItem.CDS_Document);
            this.EntityList.InsertOnSubmit(newItem);
            this.SubmitChanges();
        }


        //public void SaveB1101(Schema.TurnKey.B1101.Allowance allowance, OrganizationToken owner)
        //{
        //    InvoiceAllowance newItem = ConvertToInvoiceAllowance(owner, allowance);
        //    this.GetTable<InvoiceAllowance>().InsertOnSubmit(newItem);
        //    this.SubmitChanges();
        //}

        public void SaveB0401(Schema.TurnKey.B0401.Allowance allowance, OrganizationToken owner)
        {
            InvoiceAllowance newItem = ConvertToInvoiceAllowance(owner, allowance);
            applyProcessFlow(newItem.CDS_Document); 
            this.GetTable<InvoiceAllowance>().InsertOnSubmit(newItem);
            this.SubmitChanges();
        }


        //public void SaveB1401(Schema.TurnKey.B1401.Allowance allowance, OrganizationToken owner)
        //{
        //    InvoiceAllowance newItem = ConvertToInvoiceAllowance(owner, allowance);
        //    applyProcessFlow(newItem.CDS_Document, Naming.B2BInvoiceDocumentTypeDefinition.發票折讓);
        //    this.GetTable<InvoiceAllowance>().InsertOnSubmit(newItem);
        //    this.SubmitChanges();
        //}


        public InvoiceCancellation ConvertToInvoiceCancellation(OrganizationToken owner, ModelCore.Schema.TurnKey.A0201.CancelInvoice cancellation)
        {
            String invNo, trackCode;
            if (cancellation.CancelInvoiceNumber.Length >= 10)
            {
                trackCode = cancellation.CancelInvoiceNumber.Substring(0, 2);
                invNo = cancellation.CancelInvoiceNumber.Substring(2);
            }
            else
            {
                trackCode = null;
                invNo = cancellation.CancelInvoiceNumber;
            }
            var invoice = this.EntityList.Where(i => i.No == invNo && i.TrackCode == trackCode).FirstOrDefault();

            if (invoice == null)
            {
                throw new Exception(String.Format("發票號碼不存在:{0}", cancellation.CancelInvoiceNumber));
            }

            if (invoice.InvoiceCancellation != null)
            {
                throw new Exception(String.Format("作廢發票已存在,發票號碼:{0}", cancellation.CancelInvoiceNumber));
            }

            InvoiceCancellation cancelItem = new InvoiceCancellation
            {
                InvoiceID = invoice.InvoiceID,
                CancellationNo = cancellation.CancelInvoiceNumber,
                //Remark = String.Format("{0}{1}", cancellation.CancelReason, cancellation.Remark),
                CancelReason = cancellation.CancelReason,
                ReturnTaxDocumentNo = cancellation.ReturnTaxDocumentNumber,
                CancelDate = DateTime.ParseExact(String.Format("{0}", cancellation.CancelDate), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture).Add(cancellation.CancelTime.TimeOfDay),
                Remark =cancellation.Remark,
            };

            return cancelItem;
        }

        public InvoiceCancellation ConvertToInvoiceCancellation(OrganizationToken owner, ModelCore.Schema.TurnKey.A0501.CancelInvoice cancellation)
        {
            String invNo, trackCode;
            if (cancellation.CancelInvoiceNumber.Length >= 10)
            {
                trackCode = cancellation.CancelInvoiceNumber.Substring(0, 2);
                invNo = cancellation.CancelInvoiceNumber.Substring(2);
            }
            else
            {
                trackCode = null;
                invNo = cancellation.CancelInvoiceNumber;
            }

            cancellation.SellerId = cancellation.SellerId.GetEfficientString();
            var invoice = this.EntityList.Where(i => i.No == invNo && i.TrackCode == trackCode
                            && i.Organization.ReceiptNo == cancellation.SellerId).FirstOrDefault();

            if (invoice == null)
            {
                throw new Exception(String.Format("發票號碼不存在:{0}", cancellation.CancelInvoiceNumber));
            }

            if (invoice.InvoiceCancellation != null)
            {
                throw new Exception(String.Format("作廢發票已存在,發票號碼:{0}", cancellation.CancelInvoiceNumber));
            }

            InvoiceCancellation cancelItem = new InvoiceCancellation
            {
                InvoiceID = invoice.InvoiceID,
                CancellationNo = cancellation.CancelInvoiceNumber,
                //Remark = String.Format("{0}{1}", cancellation.CancelReason, cancellation.Remark),
                CancelReason = cancellation.CancelReason,
                ReturnTaxDocumentNo = cancellation.ReturnTaxDocumentNumber,
                CancelDate = DateTime.ParseExact(String.Format("{0}", cancellation.CancelDate), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture).Add(cancellation.CancelTime.TimeOfDay),
            Remark =  cancellation.Remark,
            };

            return cancelItem;
        }

        public void SaveA0201(Schema.TurnKey.A0201.CancelInvoice item, OrganizationToken owner)
        {
            InvoiceCancellation cancelItem = ConvertToInvoiceCancellation(owner, item);
            this.GetTable<InvoiceCancellation>().InsertOnSubmit(cancelItem);

            var doc = new DerivedDocument
            {
                CDS_Document = new CDS_Document
                {
                    DocType = (int)Naming.DocumentTypeDefinition.E_InvoiceCancellation,
                    DocDate = DateTime.Now,
                    CurrentStep = (int)Naming.InvoiceStepDefinition.待傳送,
                    DocumentOwner = new DocumentOwner
                    {
                        OwnerID = owner.CompanyID
                    }
                },
                SourceID = cancelItem.InvoiceID
            };

            this.GetTable<DerivedDocument>().InsertOnSubmit(doc);
            this.SubmitChanges();
        }

        public void SaveA0501(Schema.TurnKey.A0501.CancelInvoice item, OrganizationToken owner)
        {
            InvoiceCancellation cancelItem = ConvertToInvoiceCancellation(owner, item);
            this.GetTable<InvoiceCancellation>().InsertOnSubmit(cancelItem);

            var doc = new DerivedDocument
            {
                CDS_Document = new CDS_Document
                {
                    DocType = (int)Naming.DocumentTypeDefinition.E_InvoiceCancellation,
                    DocDate = DateTime.Now,
                    CurrentStep = (int)Naming.InvoiceStepDefinition.待傳送,
                    DocumentOwner = new DocumentOwner
                    {
                        OwnerID = owner.CompanyID
                    }
                },
                SourceID = cancelItem.InvoiceID
            };

            applyProcessFlow(doc.CDS_Document);

            this.GetTable<DerivedDocument>().InsertOnSubmit(doc);
            this.SubmitChanges();
        }

        public InvoiceAllowanceCancellation ConvertToAllowanceCancellation(OrganizationToken owner, ModelCore.Schema.TurnKey.B0201.CancelAllowance item)
        {
            var allowance = this.GetTable<InvoiceAllowance>().Where(i => i.AllowanceNumber == item.CancelAllowanceNumber).FirstOrDefault();

            if (allowance == null)
            {
                throw new Exception(String.Format("折讓證明單號碼不存在:{0}", item.CancelAllowanceNumber));
            }

            if (allowance.InvoiceAllowanceCancellation != null)
            {
                throw new Exception(String.Format("作廢折讓單已存在,折讓單號碼:{0}", item.CancelAllowanceNumber));
            }

            InvoiceAllowanceCancellation cancelItem = new InvoiceAllowanceCancellation
            {
                AllowanceID = allowance.AllowanceID,
                Remark = item.Remark,
                CancelDate = DateTime.ParseExact(String.Format("{0} {1}", item.CancelDate, item.CancelTime), "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture)
            };

            return cancelItem;
        }

        public InvoiceAllowanceCancellation ConvertToAllowanceCancellation(OrganizationToken owner, ModelCore.Schema.TurnKey.B0501.CancelAllowance item)
        {
            var allowance = this.GetTable<InvoiceAllowance>().Where(i => i.AllowanceNumber == item.CancelAllowanceNumber).FirstOrDefault();

            if (allowance == null)
            {
                throw new Exception(String.Format("折讓證明單號碼不存在:{0}", item.CancelAllowanceNumber));
            }

            if (allowance.InvoiceAllowanceCancellation != null)
            {
                throw new Exception(String.Format("作廢折讓單已存在,折讓單號碼:{0}", item.CancelAllowanceNumber));
            }

            InvoiceAllowanceCancellation cancelItem = new InvoiceAllowanceCancellation
            {
                AllowanceID = allowance.AllowanceID,
                Remark = item.Remark,
                CancelDate = DateTime.ParseExact(String.Format("{0} {1}", item.CancelDate, item.CancelTime), "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture)
            };

            return cancelItem;
        }

        public void SaveB0201(Schema.TurnKey.B0201.CancelAllowance item, OrganizationToken owner)
        {

            InvoiceAllowanceCancellation cancelItem = ConvertToAllowanceCancellation(owner, item);
            this.GetTable<InvoiceAllowanceCancellation>().InsertOnSubmit(cancelItem);

            var doc = new DerivedDocument
            {
                CDS_Document = new CDS_Document
                {
                    DocType = (int)Naming.DocumentTypeDefinition.E_AllowanceCancellation,
                    DocDate = DateTime.Now,
                    CurrentStep = (int)Naming.InvoiceStepDefinition.待傳送,
                    DocumentOwner = new DocumentOwner
                    {
                        OwnerID = owner.CompanyID
                    }
                },
                SourceID = cancelItem.AllowanceID
            };

            this.GetTable<DerivedDocument>().InsertOnSubmit(doc);

            this.SubmitChanges();
        }

        public void SaveB0501(Schema.TurnKey.B0501.CancelAllowance item, OrganizationToken owner)
        {

            InvoiceAllowanceCancellation cancelItem = ConvertToAllowanceCancellation(owner, item);
            this.GetTable<InvoiceAllowanceCancellation>().InsertOnSubmit(cancelItem);

            var doc = new DerivedDocument
            {
                CDS_Document = new CDS_Document
                {
                    DocType = (int)Naming.DocumentTypeDefinition.E_AllowanceCancellation,
                    DocDate = DateTime.Now,
                    CurrentStep = (int)Naming.InvoiceStepDefinition.待傳送,
                    DocumentOwner = new DocumentOwner
                    {
                        OwnerID = owner.CompanyID
                    }
                },
                SourceID = cancelItem.AllowanceID
            };

            applyProcessFlow(doc.CDS_Document);
            this.GetTable<DerivedDocument>().InsertOnSubmit(doc);

            this.SubmitChanges();
        }

        public void CheckInvoiceNo()
        {
            var items = this.EntityList.Where(i => i.InvoiceNoAssignment == null).GroupBy(i => i.SellerID);

            if (items.Count() > 0)
            {
                foreach (var item in items)
                {
                    using (TrackNoManager mgr = new TrackNoManager(this, item.Key.Value))
                    {
                        if (item.Select(i => mgr.CheckInvoiceNo(i)).Count(r => r) > 0)
                        {
                            mgr.SubmitChanges();
                        }
                    }
                }
            }
        }

        public Exception CheckBranchTrackBlank(Schema.TurnKey.E0402.BranchTrackBlank item)
        {
            
           if (string.IsNullOrEmpty(item.Main.HeadBan.ToString()))
            {
                return new Exception(String.Format("總公司統一編號不得為空"));
              
            }
             else
                if (item.Main.HeadBan.ToString().Length > 10)
                {
                    return new Exception(String.Format("總公司統一編號不得超過10碼:{0}", item.Main.HeadBan.ToString()));
                   
                }
             if (string.IsNullOrEmpty(item.Main.BranchBan.ToString()))
            {
                return new Exception(String.Format("分支機構統一編號不得為空"));
              
            }
             else
                if (item.Main.BranchBan.ToString().Length > 10)
                {
                    return new Exception(String.Format("分支機構統一編號不得超過10碼:{0}", item.Main.BranchBan.ToString()));
                   
                }
             int year = 1911 + Convert.ToInt16( item.Main.YearMonth) / 100;
             int month = Convert.ToInt16(item.Main.YearMonth) % 100;
             if (!(month >= 1 && month <= 12))
                 return new Exception(String.Format("發票期別格式錯誤:{0}", item.Main.YearMonth));
             else
                 if (month % 2 == 1)
                     return new Exception(String.Format("發票期別格式錯誤:{0}", item.Main.YearMonth));


            if (string.IsNullOrEmpty(item.Main.InvoiceType.ToString()))
            {
                return new Exception(String.Format("發票類別不得為空"));
              
            }
            //else
            //    if (item.Main.InvoiceType.ToString().Length > 2)
            //    {
            //       throw new Exception(String.Format("發票類別長度不得超過2碼:{0}", item.Main.InvoiceType.ToString()));
                   
            //    }
            if (string.IsNullOrEmpty(item.Main.InvoiceTrack.ToString()))
            {
                return new Exception(String.Format("空白字軌不得為空"));

            }
            else
                if (item.Main.InvoiceTrack.ToString().Length > 2)
                {
                    return new Exception(String.Format("空白字軌長度不得超過2碼:{0}", item.Main.InvoiceTrack.ToString()));

                }
            for (int i = 0; i < item.Details.Count(); i++)
            {
                if (item.Details[i].InvoiceBeginNo.ToString().Length != 8)
                {
                    return new Exception(String.Format("空白發票起號長度必為8碼:{0}", item.Details[i].InvoiceBeginNo.ToString()));

                }
                if (item.Details[i].InvoiceEndNo.ToString().Length != 8)
                {
                    return new Exception(String.Format("空白發票迄號長度必為8碼:{0}", item.Details[i].InvoiceEndNo.ToString()));

                }
            }
                return null;
        }

        public void ReceiveA0101(XmlDocument doc)
        {
            ModelCore.Schema.TurnKey.A0101.Invoice a0101 = doc.TrimAll().ConvertTo<ModelCore.Schema.TurnKey.A0101.Invoice>();
            var data = JsonConvert.SerializeObject(a0101);
            ModelCore.Schema.TurnKey.A0401.Invoice a0401 = JsonConvert.DeserializeObject<ModelCore.Schema.TurnKey.A0401.Invoice>(data);

            InvoiceItem newItem = this.ConvertToInvoiceItem(a0401, null);
            applyProcessFlow(newItem.CDS_Document, Naming.InvoiceCenterBusinessType.進項);
            this.EntityList.InsertOnSubmit(newItem);
            this.SubmitChanges();
        }

    }
}
