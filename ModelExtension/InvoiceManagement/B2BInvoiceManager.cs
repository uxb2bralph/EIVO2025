using CommonLib.Core.Utility;
using CommonLib.DataAccess;
using CommonLib.Utility;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement.InvoiceProcess;
using ModelCore.InvoiceManagement.Validator;
using ModelCore.Locale;
using ModelCore.Properties;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TurnKey.Invoice;
using ModelCore.Schema.TurnKey.Allowance;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

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

        protected virtual void ApplyProcessFlow(CDS_Document doc, Naming.InvoiceCenterBusinessType? businessType = null)
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
                        doc.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.待接收, Naming.InvoiceProcessType.A0101);
                    }
                    else
                    {
                        doc.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.A0101);
                    }
                    break;

            }
        }

        //public override Dictionary<int, Exception> SaveUploadInvoice(InvoiceRoot item, OrganizationToken? owner)
        //{
        //    Dictionary<int, Exception> result = new Dictionary<int, Exception>();

        //    if (item != null && item.Invoice != null && item.Invoice.Length > 0)
        //    {
        //        List<InvoiceItem> eventItems = new List<InvoiceItem>();
        //        InvoiceRootInvoiceValidator validator = new InvoiceRootInvoiceValidator(this, owner?.Organization);

        //        for (int idx = 0; idx < item.Invoice.Length; idx++)
        //        {
        //            try
        //            {
        //                var invItem = item.Invoice[idx];

        //                Exception? ex;
        //                if ((ex = validator.Validate(invItem)) != null)
        //                {
        //                    result.Add(idx, ex);
        //                    continue;
        //                }

        //                InvoiceItem newItem = validator.InvoiceItem;

        //                if (!validator.DuplicateProcess)
        //                {
        //                    this.EntityList.InsertOnSubmit(newItem);
        //                    ApplyProcessFlow(newItem.CDS_Document);

        //                    this.SubmitChanges();
        //                }

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
        //    return result;
        //}


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

        //public void SaveB1101(Schema.TurnKey.B1101.Allowance allowance, OrganizationToken owner)
        //{
        //    InvoiceAllowance newItem = ConvertToInvoiceAllowance(owner, allowance);
        //    this.GetTable<InvoiceAllowance>().InsertOnSubmit(newItem);
        //    this.SubmitChanges();
        //}

        public Dictionary<int, Exception> ReceiveB0101(XmlDocument docInv)
        {
            return SaveAllowanceMIG(docInv, Naming.InvoiceProcessType.ReceivedB0101);
        }


        //public void SaveB1401(Schema.TurnKey.B1401.Allowance allowance, OrganizationToken owner)
        //{
        //    InvoiceAllowance newItem = ConvertToInvoiceAllowance(owner, allowance);
        //    applyProcessFlow(newItem.CDS_Document, Naming.B2BInvoiceDocumentTypeDefinition.發票折讓);
        //    this.GetTable<InvoiceAllowance>().InsertOnSubmit(newItem);
        //    this.SubmitChanges();
        //}

        public void SaveA0201(XmlDocument docInv)
        {
            SaveInvoiceCancellationMIG(docInv, Naming.InvoiceProcessType.A0201);
        }

        public void SaveB0201(XmlDocument docInv)
        {
            SaveAllowanceCancellationMIG(docInv, Naming.InvoiceProcessType.B0201);
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

        public Exception? CheckBranchTrackBlank(Schema.TurnKey.E0402.BranchTrackBlank item)
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
            int year = 1911 + Convert.ToInt16(item.Main.YearMonth) / 100;
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

        public Dictionary<int, Exception> ReceiveA0101(XmlDocument docInv)
        {
            return SaveInvoiceMIG(docInv, Naming.InvoiceProcessType.ReceivedA0101);
        }

        public void ReceiveA0102(XmlDocument docInv)
        {
            if (docInv.DocumentElement != null)
            {
                if (docInv.DocumentElement.Attributes["xmlns"] != null)
                {
                    docInv.DocumentElement.SetAttribute("xmlns", "");
                    docInv.LoadXml(docInv.OuterXml);
                }

                var migItem = docInv.ConvertTo<InvoiceConfirm>();
                if (migItem != null)
                {
                    var match = migItem.InvoiceNumber.ParseInvoiceNo();
                    if (match.Success)
                    {
                        var item = this.GetTable<InvoiceItem>()
                            .Where(i => i.TrackCode == match.Groups[1].Value && i.No == match.Groups[2].Value)
                            .Where(i => i.InvoiceBuyer.ReceiptNo == migItem.BuyerId.GetEfficientString())
                            .FirstOrDefault();

                        if (item != null)
                        {
                            item.CDS_Document.PushLogOnSubmit(this, Naming.InvoiceStepDefinition.已接收, Naming.DataProcessStatus.Done);
                            this.SubmitChanges();
                        }
                    }
                }
            }
        }
        public void ReceiveA0301(XmlDocument docInv)
        {
            if (docInv.DocumentElement != null)
            {
                if (docInv.DocumentElement.Attributes["xmlns"] != null)
                {
                    docInv.DocumentElement.SetAttribute("xmlns", "");
                    docInv.LoadXml(docInv.OuterXml);
                }

                var migItem = docInv.ConvertTo<RejectInvoice>();
                if (migItem != null)
                {
                    var match = migItem.RejectInvoiceNumber.ParseInvoiceNo();
                    if (match.Success)
                    {
                        var item = this.GetTable<InvoiceItem>()
                            .Where(i => i.TrackCode == match.Groups[1].Value && i.No == match.Groups[2].Value)
                            .Where(i => i.InvoiceBuyer.ReceiptNo == migItem.BuyerId.GetEfficientString())
                            .FirstOrDefault();

                        if (item != null)
                        {
                            item.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.待接收, Naming.InvoiceProcessType.A0301);
                            this.SubmitChanges();
                        }
                    }
                }
            }
        }
        public void ReceiveA0302(XmlDocument docInv)
        {
            if (docInv.DocumentElement != null)
            {
                if (docInv.DocumentElement.Attributes["xmlns"] != null)
                {
                    docInv.DocumentElement.SetAttribute("xmlns", "");
                    docInv.LoadXml(docInv.OuterXml);
                }

                var migItem = docInv.ConvertTo<RejectInvoiceConfirm>();
                if (migItem != null)
                {
                    var match = migItem.RejectInvoiceNumber.ParseInvoiceNo();
                    if (match.Success)
                    {
                        var item = this.GetTable<InvoiceItem>()
                            .Where(i => i.TrackCode == match.Groups[1].Value && i.No == match.Groups[2].Value)
                            .Where(i => i.InvoiceBuyer.ReceiptNo == migItem.BuyerId.GetEfficientString())
                            .FirstOrDefault();

                        if (item != null)
                        {
                            item.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.待接收, Naming.InvoiceProcessType.A0302);
                            this.SubmitChanges();
                        }
                    }
                }
            }
        }

        public Dictionary<int, Exception> ReceiveA0201(XmlDocument docInv)
        {
            return SaveInvoiceCancellationMIG(docInv, Naming.InvoiceProcessType.ReceivedA0201);
        }

        public Dictionary<int, Exception> ReceiveB0201(XmlDocument docInv)
        {
            return SaveAllowanceCancellationMIG(docInv, Naming.InvoiceProcessType.ReceivedB0201);
        }

        public void ReceiveA0202(XmlDocument docInv)
        {
            if (docInv.DocumentElement != null)
            {
                if (docInv.DocumentElement.Attributes["xmlns"] != null)
                {
                    docInv.DocumentElement.SetAttribute("xmlns", "");
                    docInv.LoadXml(docInv.OuterXml);
                }

                var migItem = docInv.ConvertTo<CancelInvoiceConfirm>();
                if (migItem != null)
                {
                    var item = this.GetTable<InvoiceCancellation>()
                        .Where(i => i.CancellationNo == migItem.CancelInvoiceNumber)
                        .Where(i => i.CancelDate.HasValue && i.CancelDate.Value.Date == migItem.CancelDateTime.Date)
                        .FirstOrDefault();

                    if (item != null)
                    {
                        item.InvoiceItem.CDS_Document.ChildDocument.FirstOrDefault()?.CDS_Document.PushLogOnSubmit(this, Naming.InvoiceStepDefinition.已接收, Naming.DataProcessStatus.Done);
                        this.SubmitChanges();
                    }
                }
            }
        }

        public void ReceiveB0102(XmlDocument docInv)
        {
            if (docInv.DocumentElement != null)
            {
                if (docInv.DocumentElement.Attributes["xmlns"] != null)
                {
                    docInv.DocumentElement.SetAttribute("xmlns", "");
                    docInv.LoadXml(docInv.OuterXml);
                }

                var migItem = docInv.ConvertTo<AllowanceConfirm>();
                if (migItem != null)
                {
                    // Assume AllowanceNumber and BuyerId are properties of AllowanceConfirm
                    var allowance = this.GetTable<InvoiceAllowance>()
                        .Where(a => a.AllowanceNumber == migItem.AllowanceNumber)
                        .Where(a => a.InvoiceAllowanceBuyer.ReceiptNo == migItem.BuyerId.GetEfficientString())
                        .FirstOrDefault();

                    if (allowance != null)
                    {
                        allowance.CDS_Document.PushLogOnSubmit(this, Naming.InvoiceStepDefinition.已接收, Naming.DataProcessStatus.Done);
                        this.SubmitChanges();
                    }
                }
            }
        }

        public void ReceiveB0202(XmlDocument docInv)
        {
            if (docInv.DocumentElement != null)
            {
                if (docInv.DocumentElement.Attributes["xmlns"] != null)
                {
                    docInv.DocumentElement.SetAttribute("xmlns", "");
                    docInv.LoadXml(docInv.OuterXml);
                }

                var migItem = docInv.ConvertTo<ModelCore.Schema.TurnKey.Allowance.CancelAllowanceConfirm>();
                if (migItem != null)
                {
                    var item = this.GetTable<InvoiceAllowanceCancellation>()
                        .Where(i => i.InvoiceAllowance.AllowanceNumber == migItem.CancelAllowanceNumber)
                        .Where(i => i.CancelDate.HasValue && i.CancelDate.Value.Date == migItem.CancelDateTime.Date)
                        .FirstOrDefault();

                    if (item != null)
                    {
                        // 已存在，更新或記錄已接收
                        // 可根據需求進行額外處理
                        item.InvoiceAllowance.CDS_Document.ChildDocument.FirstOrDefault()?.CDS_Document.PushLogOnSubmit(this, Naming.InvoiceStepDefinition.已接收, Naming.DataProcessStatus.Done);
                        this.SubmitChanges();
                    }
                }
            }
        }
    }
}
