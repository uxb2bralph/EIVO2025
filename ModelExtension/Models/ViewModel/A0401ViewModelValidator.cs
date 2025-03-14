using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommonLib.DataAccess;
using ModelCore.DataEntity;

using ModelCore.Locale;
using ModelCore.Resource;
using ModelCore.Schema.EIVO;
using CommonLib.Utility;
using ModelCore.InvoiceManagement;

namespace ModelCore.Models.ViewModel
{
    public partial class A0401ViewModelValidator<TEntity> : InvoiceViewModelValidator<TEntity>
        where TEntity : class, new()
    {
        public A0401ViewModelValidator(ModelSource<TEntity> mgr, Organization owner) : base(mgr, owner)
        {

        }

        public override Exception Validate(InvoiceViewModel dataItem)
        {
            _invItem = dataItem;

            Exception ex;

            _seller = null;
            _newItem = null;

            if ((ex = checkBusiness()) != null)
            {
                return ex;
            }

            if ((ex = checkDataNumber()) != null)
            {
                return ex;
            }

            if ((ex = checkAmount()) != null)
            {
                return ex;
            }


            if ((ex = checkMandatoryFields()) != null)
            {
                return ex;
            }

            if ((ex = checkInvoiceProductItems()) != null)
            {
                return ex;
            }

            if ((ex = checkInvoice()) != null)
            {
                return ex;
            }


            return null;
        }

        protected override Exception checkBusiness()
        {
            _seller = _mgr.GetTable<Organization>().Where(o => o.CompanyID == _invItem.SellerID).FirstOrDefault();
            if (_seller == null)
            {
                _seller = _mgr.GetTable<Organization>().Where(o => o.ReceiptNo == _invItem.SellerReceiptNo).FirstOrDefault();
            }
            if (_seller == null)
            {
                return new Exception(String.Format(MessageResources.AlertInvalidSeller, _invItem.SellerReceiptNo));
            }

            if (_seller.CompanyID != _owner.CompanyID && !_mgr.GetTable<InvoiceIssuerAgent>().Any(a => a.AgentID == _owner.CompanyID && a.IssuerID == _seller.CompanyID))
            {
                return new Exception(String.Format(MessageResources.InvalidSellerOrAgent, _invItem.SellerReceiptNo, _owner.ReceiptNo));
            }

            if (_seller.OrganizationStatus.CurrentLevel == (int)Naming.MemberStatusDefinition.Mark_To_Delete)
            {
                return new Exception(String.Format("開立人已註記停用,開立人統一編號:{0}，TAG:<SellerId />", _invItem.SellerReceiptNo));
            }

            _invItem.BuyerReceiptNo = _invItem.BuyerReceiptNo.GetEfficientString();
            if (_invItem.BuyerReceiptNo==null)
            {
                return new Exception(String.Format(MessageResources.InvalidReceiptNo, _invItem.BuyerReceiptNo));
            }
            else
            {
                if (!Regex.IsMatch(_invItem.BuyerReceiptNo, "^[0-9]{8}$"))
                {
                    return new Exception(String.Format(MessageResources.InvalidBuyerId, _invItem.BuyerReceiptNo));
                }
                else if (!_invItem.BuyerReceiptNo.CheckRegno())
                {
                    return new Exception(String.Format(MessageResources.InvalidReceiptNo, _invItem.BuyerReceiptNo));
                }
            }

            _invItem.BuyerName = _invItem.BuyerName.GetEfficientString();
            if (_invItem.BuyerName == null)
            {
                return new Exception(String.Format(MessageResources.InvalidBuyerNameLengthLimit, _invItem.BuyerName));
            }

            return checkBusinessDetails();
        }


        protected override Exception checkInvoice()
        {
            _newItem = new InvoiceItem
            {
                CDS_Document = new CDS_Document
                {
                    DocDate = DateTime.Now,
                    DocType = (int)Naming.DocumentTypeDefinition.E_Invoice,
                    DocumentOwner = new DocumentOwner
                    {
                        OwnerID = _owner.CompanyID
                    },
                    ProcessType = (int)Naming.InvoiceProcessType.A0401,
                },
                InvoiceType = _invItem.InvoiceType,
                SellerID = _seller.CompanyID,
                CustomsClearanceMark = _invItem.CustomsClearanceMark,
                InvoiceSeller = new InvoiceSeller
                {
                    Name = _seller.CompanyName,
                    ReceiptNo = _seller.ReceiptNo,
                    Address = _seller.Addr,
                    ContactName = _seller.ContactName,
                    //CustomerID = String.IsNullOrEmpty(_invItem.GoogleId) ? "" : _invItem.GoogleId,
                    CustomerName = _seller.CompanyName,
                    EMail = _seller.ContactEmail,
                    Fax = _seller.Fax,
                    Phone = _seller.Phone,
                    PersonInCharge = _seller.UndertakerName,
                    SellerID = _seller.CompanyID,
                },
                InvoiceBuyer = _buyer,
                RandomNo = _invItem.RandomNo,
                InvoiceAmountType = new InvoiceAmountType
                {
                    DiscountAmount = _invItem.DiscountAmount,
                    SalesAmount = _invItem.SalesAmount,
                    TaxAmount = _invItem.TaxAmount,
                    TaxRate = _invItem.TaxRate,
                    TaxType = _invItem.TaxType,
                    TotalAmount = _invItem.TotalAmount,
                    TotalAmountInChinese = ValidityAgent.MoneyShow(_invItem.TotalAmount),
                },
                RelateNumber = _invItem.RelateNumber,
                Remark = _invItem.Remark,
                Category = _invItem.Category,
                CheckNo = _invItem.CheckNo,
                PrintMark = "Y",
            };

            if (_order != null)
            {
                _newItem.InvoicePurchaseOrder = _order;
            }

            _newItem.InvoiceDetails.AddRange(_productItems.Select(p => new InvoiceDetail
            {
                InvoiceProduct = p.InvoiceProduct,
            }));

            if (_invItem.TrackCode == null || _invItem.No == null)
            {
                try
                {
                    using (TrackNoManager trackNoMgr = new TrackNoManager(_mgr, _seller.CompanyID))
                    {
                        if (!trackNoMgr.ApplyInvoiceDate(_invItem.InvoiceDate.Value) || !trackNoMgr.CheckInvoiceNo(_newItem))
                        {
                            return new Exception(String.Format(MessageResources.AlertNullTrackNoInterval, _seller.ReceiptNo));
                        }
                        else
                        {
                            _newItem.InvoiceDate = _invItem.InvoiceDate;
                        }
                    }
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }
            else
            {
                _newItem.TrackCode = _invItem.TrackCode;
                _newItem.No = _invItem.No;
                _newItem.InvoiceDate = _invItem.InvoiceDate ?? DateTime.Now;
            }

            return null;
        }

        protected override Exception checkBusinessDetails()
        {
            _buyer = new InvoiceBuyer
            {
                BuyerMark = _invItem.BuyerMark,
                Name = _invItem.BuyerName,
                ReceiptNo = String.IsNullOrEmpty(_invItem.BuyerReceiptNo) ? "0000000000" : _invItem.BuyerReceiptNo,
                CustomerID = _invItem.CustomerID,
                CustomerName = _invItem.BuyerName,
                ContactName = _invItem.BuyerName,
                Phone = _invItem.Phone,
                Address = _invItem.Address,
                EMail = _invItem.EMail
            };

            Organization buyer = _mgr.GetTable<Organization>().Where(o => o.ReceiptNo == _buyer.ReceiptNo).FirstOrDefault();
            if (buyer == null)
            {
                buyer = new Organization
                {
                    OrganizationStatus = new OrganizationStatus
                    {
                        CurrentLevel = (int)Naming.MemberStatusDefinition.Wait_For_Check
                    },
                    Addr = _buyer.Address,
                    CompanyName = _buyer.Name,
                    UndertakerName = _buyer.PersonInCharge,
                    Phone = _buyer.Phone,
                    Fax = _buyer.Fax,
                    ContactEmail = _buyer.EMail,
                    ReceiptNo = _buyer.ReceiptNo,
                };

                buyer.OrganizationCategory.Add(new OrganizationCategory
                {
                    CategoryID = (int)CategoryDefinition.CategoryEnum.相對營業人
                });

                _buyer.Organization = buyer;
            }
            else
            {
                _buyer.BuyerID = buyer.CompanyID;
            }


            if (_invItem.Counterpart == true)
            {
                if (!_mgr.GetTable<BusinessRelationship>().Any(b => b.MasterID == _seller.CompanyID
                     && b.Counterpart.ReceiptNo == _buyer.ReceiptNo))
                {
                    _mgr.GetTable<BusinessRelationship>().InsertOnSubmit
                        (
                            new BusinessRelationship
                            {
                                MasterID = _seller.CompanyID,
                                Counterpart = buyer,
                                BusinessID = (int)Naming.InvoiceCenterBusinessType.銷項,
                                CompanyName = _buyer.Name,
                                Addr = _buyer.Address,
                                Phone = _buyer.Phone,
                                ContactEmail = _buyer.EMail,
                            }
                        );
                }
            }

            return null;
        }
        
        protected override Exception checkMandatoryFields()
        {

            if(!_invItem.InvoiceType.IsValidInvoiceType())
            {
                return new Exception(String.Format(MessageResources.InvalidInvoiceType, _invItem.InvoiceType));
            }

            return null;
        }

    }
}
