using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement.ErrorHandle;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using ModelCore.Resource;
using ModelCore.Schema.EIVO;
using CommonLib.Utility;
using CommonLib.DataAccess;

namespace ModelCore.InvoiceManagement.Validator
{
    public partial class InvoiceRootFormatValidator : InvoiceRootInvoiceValidator
    {
        readonly List<Exception> exceptions = new List<Exception>();

        public InvoiceRootFormatValidator(GenericManager<EIVOEntityDataContext> mgr, Organization owner) : base(mgr, owner)
        {

        }

        public bool IsAutoTrackNo
        {
            get => _isAutoTrackNo;
            protected set => _isAutoTrackNo = value;
        }

        public virtual List<Exception> ValidateAll(InvoiceRootInvoice dataItem)
        {
            Validate(dataItem);
            return exceptions;
        }

        public override Exception Validate(InvoiceRootInvoice dataItem)
        {
            exceptions.Clear();
            _invItem = dataItem;

            _newItem = null;
            _container = new InvoiceItem { };

            checkBusiness();

            if (String.IsNullOrEmpty(_invItem.InvoiceNumber))
            {
                IsAutoTrackNo = true;

                checkDataNumber();
            }
            else
            {
                IsAutoTrackNo = false;
            }

            checkAmount();
            checkInvoiceDelivery();

            checkMandatoryFields();
            checkInvoiceProductItems();

            checkInvoice();

            return exceptions.FirstOrDefault();
        }


        protected override Exception checkInvoice()
        {
            _container.CDS_Document = new CDS_Document
            {
                DocDate = DateTime.Now,
                DocType = (int)Naming.DocumentTypeDefinition.E_Invoice,
                DocumentOwner = new DocumentOwner
                {
                    OwnerID = _owner.CompanyID
                },
                ProcessType = (int)(processType ?? Naming.InvoiceProcessType.C0401),
            };
            _container.DonateMark = _donation == null ? "0" : "1";
            _container.SellerID = _seller.CompanyID;
            _container.CustomsClearanceMark = _invItem.CustomsClearanceMark;
            _container.InvoiceSeller = new InvoiceSeller
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
            };
            _container.InvoiceBuyer = _buyer;
            _container.RandomNo = _invItem.RandomNumber;
            _container.InvoiceAmountType = new InvoiceAmountType
            {
                DiscountAmount = _invItem.DiscountAmount,
                SalesAmount = _invItem.SalesAmount,
                FreeTaxSalesAmount = _invItem.FreeTaxSalesAmount,
                ZeroTaxSalesAmount = _invItem.ZeroTaxSalesAmount,
                TaxAmount = _invItem.TaxAmount,
                TaxRate = _invItem.TaxRate,
                TaxType = _invItem.TaxType,
                TotalAmount = _invItem.TotalAmount,
                TotalAmountInChinese = ValidityAgent.MoneyShow(_invItem.TotalAmount),
                CurrencyID = _currency?.CurrencyID,
                BondedAreaConfirm = _invItem.BondedAreaConfirm,
            };
            _container.InvoiceCarrier = _carrier;
            _container.InvoiceDonation = _donation;
            _container.PrintMark = _invItem.PrintMark;
            _container.Remark = _invItem.MainRemark;


            if (_order != null)
            {
                _container.InvoicePurchaseOrder = _order;
            }

            _container.InvoiceDetails.AddRange(_productItems.Select(p => new InvoiceDetail
            {
                InvoiceProduct = p.InvoiceProduct,
            }));

            if (IsAutoTrackNo)
            {
                _container.InvoiceDate = DateTime.Now;

            }
            else
            {

                if (String.IsNullOrEmpty(_invItem.InvoiceDate))
                {
                    exceptions.Add(new Exception(MessageResources.AlertInvoiceDate));
                }
                else
                {
                    DateTime invoiceDate = DateTime.Now;

                    _invItem.InvoiceTime = _invItem.InvoiceTime.GetEfficientString();
                    if (_invItem.InvoiceTime == null)
                    {
                        _invItem.InvoiceTime = "12:00:00";
                    }

                    if (!DateTime.TryParseExact(String.Format("{0} {1}", _invItem.InvoiceDate, _invItem.InvoiceTime), InvoiceRootInvoiceValidator.__InvoiceDateTimeFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out invoiceDate)
                            || invoiceDate > DateTime.Today.AddDays(3))
                    {
                        exceptions.Add(new Exception(String.Format(MessageResources.AlertInvoiceDateTime, _invItem.InvoiceDate, _invItem.InvoiceTime)));
                    }
                    else
                    {
                        _container.InvoiceDate = invoiceDate;
                    }
                }

                if (_container.InvoiceDate.HasValue)
                {
                    DateTime invoiceDate = _container.InvoiceDate.Value;
                    DateTime periodStart = new DateTime(invoiceDate.Year, (invoiceDate.Month - 1) / 2 * 2 + 1, 1);

                    if (_invItem.InvoiceNumber == null || !Regex.IsMatch(_invItem.InvoiceNumber, "^[a-zA-Z]{2}[0-9]{8}$"))
                    {
                        exceptions.Add(new Exception(String.Format(MessageResources.AlertInvoiceNumber, _invItem.InvoiceNumber)));
                    }

                    _container.TrackCode = _invItem.InvoiceNumber.Substring(0, 2);
                    _container.No = _invItem.InvoiceNumber.Substring(2);
                    int periodNo = (invoiceDate.Month + 1) / 2;

                    if (_seller.OrganizationStatus?.EnableTrackCodeInvoiceNoValidation == true)
                    {
                        if (!_models.GetTable<InvoiceTrackCode>().Any(t => t.Year == invoiceDate.Year && t.PeriodNo == periodNo && t.TrackCode == _container.TrackCode))
                        {
                            exceptions.Add(new Exception(String.Format(MessageResources.InvalidTrackCode, _invItem.InvoiceNumber)));
                        }
                    }

                    var currentItem = _models.GetTable<InvoiceItem>().Where(i => i.TrackCode == _container.TrackCode && i.No == _container.No
                                && i.InvoiceDate >= periodStart && i.InvoiceDate < periodStart.AddMonths(2)).FirstOrDefault();

                    if (currentItem != null)
                    {
                        if (currentItem.SellerID == _seller.CompanyID && currentItem.RandomNo == _invItem.RandomNumber)
                        {
                            _newItem = currentItem;
                        }
                        else
                        {
                            exceptions.Add(new Exception(MessageResources.AlertInvoiceDuplicated));
                        }
                    }

                    if (_seller.OrganizationStatus?.EnableTrackCodeInvoiceNoValidation == true)
                    {
                        TrackNoManager trackMgr = new TrackNoManager(_models, _seller.CompanyID);
                        var item = trackMgr.GetAppliedInterval(invoiceDate, _container.TrackCode, int.Parse(_container.No));

                        if (item == null)
                        {
                            exceptions.Add(new Exception(String.Format("發票號碼錯誤:{0}，TAG:< InvoicNumber />", _invItem.InvoiceNumber)));
                        }
                    }
                }
            }

            if (_invItem.CustomerDefined != null)
            {
                _container.InvoiceItemExtension = new InvoiceItemExtension
                {
                    ProjectNo = _invItem.CustomerDefined.ProjectNo,
                    PurchaseNo = _invItem.CustomerDefined.PurchaseNo
                };

                if (_invItem.CustomerDefined.StampDutyFlagSpecified)
                {
                    _container.InvoiceItemExtension.StampDutyFlag = (byte?)_invItem.CustomerDefined.StampDutyFlag;
                }
            }

            _newItem = _container;

            return null;
        }

        protected override Exception checkDataNumber()
        {
            _order = null;
            if (String.IsNullOrEmpty(_invItem.DataNumber))
            {
                exceptions.Add(new Exception(MessageResources.AlertDataNumber));
            }

            if (_invItem.DataNumber?.Length > 60)
            {
                exceptions.Add(new Exception(String.Format(MessageResources.AlertDataNumberLimitedLength, _invItem.DataNumber)));
            }

            var po = _models.GetTable<InvoicePurchaseOrder>().Where(d => d.OrderNo == _invItem.DataNumber
                    && d.InvoiceItem.SellerID == _seller.CompanyID).FirstOrDefault();
            if (po != null)
            {
                exceptions.Add(new DuplicateDataNumberException(String.Format(MessageResources.AlertDataNumberDuplicated, _invItem.DataNumber))
                {
                    CurrentPO = po
                });
            }

            if (String.IsNullOrEmpty(_invItem.DataDate))
            {
                exceptions.Add(new Exception(MessageResources.AlertDataDate));
            }

            DateTime dataDate;
            if (!DateTime.TryParseExact(_invItem.DataDate, "yyyy/MM/dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out dataDate))
            {
                exceptions.Add(new Exception(String.Format(MessageResources.AlertDataDateFormat, _invItem.DataDate)));
            }

            _order = new InvoicePurchaseOrder
            {
                OrderNo = _invItem.DataNumber,
                PurchaseDate = dataDate
            };

            return null;
        }

        protected override Exception checkBusiness()
        {
            if (_seller == null || _seller.ReceiptNo != _invItem.SellerId)
            {

                _seller = _models.GetTable<Organization>().Where(o => o.ReceiptNo == _invItem.SellerId).FirstOrDefault();
                if (_seller == null)
                {
                    exceptions.Add(new Exception(String.Format(MessageResources.AlertInvalidSeller, _invItem.SellerId)));
                    _seller = new Organization { };
                }

                if (_seller.CompanyID != _owner.CompanyID && !_models.GetTable<InvoiceIssuerAgent>().Any(a => a.AgentID == _owner.CompanyID && a.IssuerID == _seller.CompanyID))
                {
                    //return new Exception(String.Format(MessageResources.AlertSellerSignature, _invItem.SellerId));
                    exceptions.Add(new Exception(String.Format(MessageResources.InvalidSellerOrAgent, _invItem.SellerId, _owner.ReceiptNo)));
                }

                if (_seller?.OrganizationStatus?.CurrentLevel == (int)Naming.MemberStatusDefinition.Mark_To_Delete)
                {
                    exceptions.Add(new Exception(String.Format("開立人已註記停用,開立人統一編號:{0}，TAG:<SellerId />", _invItem.SellerId)));
                }

                _isCrossBorderMerchant = _models.GetTable<OrganizationCategory>().Any(c => c.CompanyID == _seller.CompanyID && c.CategoryID == (int)Naming.CategoryID.COMP_CROSS_BORDER_MURCHANT);

            }

            _invItem.BuyerId = _invItem.BuyerId.GetEfficientString();
            if (_invItem.BuyerId == "0000000000")
            {
                //if (_invItem.BuyerName == null || Encoding.GetEncoding(950).GetBytes(_invItem.BuyerName).Length != 4)
                //{
                //    return new Exception(String.Format(MessageResources.InvalidBuyerName, _invItem.BuyerName));
                //}
            }
            else if (_invItem.BuyerId == null)
            {
                _invItem.BuyerId = "0000000000";

                //if (_isCrossBorderMerchant)
                //{
                //    _invItem.BuyerId = "0000000000";
                //}
                //else
                //{
                //    exceptions.Add(new Exception(String.Format(MessageResources.InvalidBuyerId, _invItem.BuyerId)));
                //}
            }
            else if (!Regex.IsMatch(_invItem.BuyerId, "^[0-9]{8}$"))
            {
                exceptions.Add(new Exception(String.Format(MessageResources.InvalidBuyerId, _invItem.BuyerId)));
            }
            else if (_seller?.OrganizationStatus?.EnableBuyerIDValidation != false && !_invItem.BuyerId.CheckRegno())
            {
                exceptions.Add(new Exception(String.Format(MessageResources.InvalidReceiptNo, _invItem.BuyerId)));
            }
            else if (_invItem.PrintMark != "Y" && (String.IsNullOrEmpty(_invItem.BuyerName) || _invItem.BuyerName.Length > 60))
            {
                exceptions.Add(new Exception(String.Format(MessageResources.InvalidBuyerNameLengthLimit, _invItem.BuyerName)));
            }

            if (String.IsNullOrEmpty(_invItem.RandomNumber))
            {
                _invItem.RandomNumber = String.Format("{0:ffff}", DateTime.Now); //ValidityAgent.GenerateRandomCode(4)
            }
            else if (!Regex.IsMatch(_invItem.RandomNumber, "^[0-9]{4}$"))
            {
                exceptions.Add(new Exception(String.Format(MessageResources.InvalidRandomNumber, _invItem.RandomNumber)));
            }

            checkBusinessDetails();

            return null;

        }

        protected override Exception checkBusinessDetails()
        {
            _buyer = new InvoiceBuyer
            {
                BuyerMark = _invItem.BuyerMark,
                Name = _invItem.BuyerId == "0000000000" ? _invItem.BuyerName.CheckB2CMIGName() : _invItem.BuyerName,
                ReceiptNo = _invItem.BuyerId,
                CustomerID = _invItem.GoogleId.GetEfficientString() ?? _invItem.CustomerID.GetEfficientString(),
                CustomerName = _invItem.BuyerName,
            };

            if (_isCrossBorderMerchant)
            {
                _buyer.CustomerNumber = _buyer.ReceiptNo;
                _buyer.ReceiptNo = "0000000000";
                _buyer.EMail = _invItem.CarrierId1 ?? _invItem.CarrierId2;
            }

            if (_invItem.Contact != null)
            {

                if (!String.IsNullOrEmpty(_invItem.Contact.Name) && _invItem.Contact.Name.Length > 64)
                {
                    exceptions.Add(new Exception(String.Format(MessageResources.InvalidContactName, _invItem.Contact.Name)));
                }

                if (!String.IsNullOrEmpty(_invItem.Contact.Address) && _invItem.Contact.Address.Length > 128)
                {
                    exceptions.Add(new Exception(String.Format(MessageResources.InvalidContactAddress, _invItem.Contact.Address)));
                }

                if (!String.IsNullOrEmpty(_invItem.Contact.Email) && _invItem.Contact.Email.Length > 512)
                {
                    exceptions.Add(new Exception(String.Format(MessageResources.InvalidContactEMail, _invItem.Contact.Email)));
                }

                if (!String.IsNullOrEmpty(_invItem.Contact.TEL) && _invItem.Contact.TEL.Length > 64)
                {
                    exceptions.Add(new Exception(String.Format(MessageResources.InvalidContactPhone, _invItem.Contact.TEL)));
                }

                _buyer.ContactName = _invItem.Contact.Name;
                _buyer.Address = _invItem.Contact.Address;
                _buyer.Phone = _invItem.Contact.TEL;
                _buyer.EMail = _invItem.Contact.Email != null ? _invItem.Contact.Email.Replace(';', ',').Replace('、', ',').Replace(' ', ',') : null;
            }

            return null;
        }

        protected override Exception checkInvoiceDelivery()
        {

            var ex = base.checkInvoiceDelivery();

            if (ex != null)
            {
                exceptions.Add(ex);
            }

            return ex;

        }


        protected override Exception checkAmount()
        {
            //應稅銷售額
            if (_invItem.SalesAmount < 0 /*|| decimal.Floor(_invItem.SalesAmount) != _invItem.SalesAmount*/)
            {
                exceptions.Add(new Exception(String.Format(MessageResources.InvalidSellingPrice, _invItem.SalesAmount)));
            }

            if (_invItem.FreeTaxSalesAmount < 0)
            {
                exceptions.Add(new Exception(String.Format(MessageResources.InvalidFreeTaxAmount, _invItem.FreeTaxSalesAmount)));
            }

            if (_invItem.ZeroTaxSalesAmount < 0)
            {
                exceptions.Add(new Exception(String.Format(MessageResources.InvalidZeroTaxAmount, _invItem.ZeroTaxSalesAmount)));
            }


            if (_invItem.TaxAmount < 0 /*|| decimal.Floor(_invItem.TaxAmount) != _invItem.TaxAmount*/)
            {
                exceptions.Add(new Exception(String.Format(MessageResources.InvalidTaxAmount, _invItem.TaxAmount)));
            }

            if (_invItem.TotalAmount < 0 /*|| decimal.Floor(_invItem.TotalAmount) != _invItem.TotalAmount*/)
            {
                exceptions.Add(new Exception(String.Format(MessageResources.InvalidTotalAmount, _invItem.TotalAmount)));
            }

            //課稅別
            if (!Enum.IsDefined(typeof(Naming.TaxTypeDefinition), (int)_invItem.TaxType))
            {
                exceptions.Add(new Exception(String.Format(MessageResources.InvalidTaxType, _invItem.TaxType)));
            }

            if (_invItem.TaxRate < 0m)
            {
                exceptions.Add(new Exception(String.Format(MessageResources.InvalidTaxRate, _invItem.TaxRate)));
            }

            if (_invItem.TaxType == (byte)Naming.TaxTypeDefinition.零稅率)
            {
                if (!_invItem.CustomsClearanceMark.HasValue)
                {
                    exceptions.Add(new Exception(String.Format(MessageResources.AlertClearanceMarkZeroTax, _invItem.CustomsClearanceMark)));
                }
                else if (_invItem.CustomsClearanceMark != 1 && _invItem.CustomsClearanceMark != 2)
                {
                    exceptions.Add(new Exception(String.Format(MessageResources.AlertClearanceMarkExport, _invItem.CustomsClearanceMark)));
                }
            }
            else if (_invItem.CustomsClearanceMark.HasValue)
            {
                if (_invItem.CustomsClearanceMark != 1 && _invItem.CustomsClearanceMark != 2)
                {
                    exceptions.Add(new Exception(String.Format(MessageResources.AlertClearanceMarkExport, _invItem.CustomsClearanceMark)));
                }
            }

            _invItem.Currency = _invItem.Currency.GetEfficientString();
            _currency = null;
            if (!String.IsNullOrEmpty(_invItem.Currency))
            {
                _currency = _models.GetTable<CurrencyType>().Where(c => c.AbbrevName == _invItem.Currency).FirstOrDefault();
                if (_currency == null)
                {
                    exceptions.Add(new Exception($"Invalid currency code：{_invItem.Currency}，TAG：<Currency/>"));
                }
            }

            return null;
        }


        protected override Exception checkInvoiceProductItems()
        {
            if (_invItem.InvoiceItem == null || _invItem.InvoiceItem.Length == 0)
            {
                exceptions.Add(new Exception(MessageResources.InvalidInvoiceDetails));
                _productItems = new List<InvoiceProductItem>();
            }
            else
            {
                short seqNo = 1;
                _productItems = _invItem.InvoiceItem.Select(i => new InvoiceProductItem
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
            }


            foreach (var product in _productItems)
            {
                if (String.IsNullOrEmpty(product.InvoiceProduct.Brief) || product.InvoiceProduct.Brief.Length > 256)
                {
                    exceptions.Add(new Exception(String.Format(MessageResources.InvalidProductDescription, product.InvoiceProduct.Brief)));
                }


                if (!String.IsNullOrEmpty(product.PieceUnit) && product.PieceUnit.Length > 6)
                {
                    exceptions.Add(new Exception(String.Format(MessageResources.InvalidPieceUnit, product.PieceUnit)));
                }


                //if (!product.UnitCost.HasValue || product.UnitCost == 0)
                //{
                //    items.Add(new Exception(String.Format(MessageResources.InvalidUnitPrice, product.UnitCost)));
                //}

                //if (!product.CostAmount.HasValue || product.CostAmount == 0)
                //{
                //    items.Add(new Exception(String.Format(MessageResources.InvalidCostAmount, product.CostAmount)));
                //}

                //if (!product.Piece.HasValue || product.Piece == 0)
                //{
                //    items.Add(new Exception(String.Format(MessageResources.InvalidQuantity, product.Piece)));
                //}

            }
            return null;
        }

        protected override Exception checkMandatoryFields()
        {

            if (_invItem.BuyerId == "0000000000" && _invItem.DonateMark != "0" && _invItem.DonateMark != "1")
            {
                exceptions.Add(new Exception(String.Format(MessageResources.InvalidDonationMark, _invItem.DonateMark)));
            }

            if (String.IsNullOrEmpty(_invItem.PrintMark))
            {
                //return new Exception(MessageResources.InvalidPrintMark);
                _invItem.PrintMark = "N";
            }
            else
            {
                _invItem.PrintMark = _invItem.PrintMark.ToUpper();
                if (_invItem.PrintMark != "Y" && _invItem.PrintMark != "N")
                {
                    exceptions.Add(new Exception(MessageResources.InvalidPrintMark));
                }
            }

            if (!_invItem.InvoiceType.IsValidInvoiceType(out byte data))
            {
                exceptions.Add(new Exception(String.Format(MessageResources.InvalidInvoiceType, _invItem.InvoiceType)));
            }


            return null;
        }


    }

}
