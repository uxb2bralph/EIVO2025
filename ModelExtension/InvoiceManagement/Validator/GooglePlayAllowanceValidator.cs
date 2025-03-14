using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using ModelCore.DataEntity;
using ModelCore.Schema.EIVO;
using System.Text.RegularExpressions;
using ModelCore.Resource;
using System.Globalization;
using ModelCore.Locale;
using CommonLib.Utility;
using ModelCore.InvoiceManagement.ErrorHandle;
using CommonLib.DataAccess;

namespace ModelCore.InvoiceManagement.Validator
{
    public partial class GooglePlayAllowanceValidator : AllowanceRootAllowanceValidator
    {

        public GooglePlayAllowanceValidator(GenericManager<EIVOEntityDataContext> mgr, Organization owner) : base(mgr,owner)
        {

        }

        public override Exception Validate(AllowanceRootAllowance dataItem)
        {
            _allowanceItem = dataItem;

            Exception ex;

            _seller = null;
            _newItem = null;
            
            if((ex = CheckBusiness()) != null)
            {
                return ex;
            }

            if((ex = CheckMandatoryFields()) != null)
            {
                return ex;
            }

            if((ex = CheckAllowanceItem()) != null)
            {
                return ex;
            }

            return null;
        }

        protected override Exception CheckBusiness()
        {
            _seller = models.GetTable<Organization>().Where(o => o.ReceiptNo == _allowanceItem.SellerId).FirstOrDefault();

            if (_seller == null)
            {
                return new Exception(String.Format(MessageResources.AlertInvalidSeller, _allowanceItem.SellerId));
            }

            if (_seller.CompanyID != _owner.CompanyID && !models.GetTable<InvoiceIssuerAgent>().Any(a => a.AgentID == _owner.CompanyID && a.IssuerID == _seller.CompanyID))
            {
                return new Exception(String.Format(MessageResources.AlertSellerSignature, _allowanceItem.SellerId));
            }

            //if (_allowanceItem.BuyerId == "0000000000")
            //{

            //}
            //else if (_allowanceItem.BuyerId == null || !Regex.IsMatch(_allowanceItem.BuyerId, "^[0-9]{8}$"))
            //{
            //    return new Exception(String.Format(MessageResources.InvalidBuyerId, _allowanceItem.BuyerId));
            //}
            //else if (_allowanceItem.BuyerName!=null && _allowanceItem.BuyerName.Length > 60)
            //{
            //    return new Exception(String.Format(MessageResources.AlertBuyerNameLength, _allowanceItem.BuyerName));
            //}

            return null;
        }

        protected override Exception CheckMandatoryFields()
        {
            _allowanceDate = default(DateTime);

            _allowanceItem.AllowanceNumber = _allowanceItem.AllowanceNumber.GetEfficientString();
            if (_allowanceItem.AllowanceNumber==null)
            {
                return new Exception(MessageResources.InvalidAllowanceNo);
            }

            //折讓證明單號碼
            //if (_allowanceItem.AllowanceNumber.Length > 16)
            //{
            //    return new Exception(String.Format(MessageResources.AlertAllowanceNoLength, _allowanceItem.AllowanceNumber));
            //}

            var currentItem = models.GetTable<InvoiceAllowance>().Where(a => a.AllowanceNumber == _allowanceItem.AllowanceNumber)
                        .Join(models.GetTable<InvoiceAllowanceSeller>().Where(s => s.SellerID == _seller.CompanyID),
                            a => a.AllowanceID, s => s.AllowanceID, (a, s) => a).FirstOrDefault();
            if (currentItem != null)
            {
                return new DuplicateAllowanceNumberException(String.Format(MessageResources.AlertAllowanceDuplicated, _allowanceItem.AllowanceNumber)) { CurrentAllowance = currentItem };
            }

            //折讓證明單日期
            if (String.IsNullOrEmpty(_allowanceItem.AllowanceDate))
            {
                return new Exception(MessageResources.InvalidAllowanceDate);
            }

            if (!DateTime.TryParseExact(_allowanceItem.AllowanceDate, "yyyy/MM/dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out _allowanceDate))
            {
                return new Exception(String.Format(MessageResources.AlertAllowanceDateFormat, _allowanceItem.AllowanceDate));
            }

            //折讓種類
            if (_allowanceItem.AllowanceType != 1 && _allowanceItem.AllowanceType != 2)
            {
                return new Exception(String.Format(MessageResources.AlertAllowanceType, _allowanceItem.AllowanceType));
            }

            _allowanceItem.BuyerId = _allowanceItem.BuyerId.GetEfficientString() ?? "0000000000";

            _currency = null;
            if (!String.IsNullOrEmpty(_allowanceItem.Currency))
            {
                _currency = models.GetTable<CurrencyType>().Where(c => c.AbbrevName == _allowanceItem.Currency).FirstOrDefault();
                if (_currency == null)
                {
                    return new Exception($"Invalid currency code：{_allowanceItem.Currency}，TAG：<Currency/>");
                }
            }

            return null;
        }

        protected override Exception CheckAllowanceItem()
        {
            _productItems = new List<InvoiceAllowanceItem>();
            InvoiceItem originalInvoice = null;
            var productItems = models.GetTable<InvoiceItem>().Where(i => i.InvoiceCancellation == null)
                    .Where(i => i.SellerID == _seller.CompanyID)
                    .Join(models.GetTable<InvoiceBuyer>().Where(b => b.CustomerID == _allowanceItem.GoogleId), i => i.InvoiceID, b => b.InvoiceID, (i, b) => i)
                    .Join(models.GetTable<InvoiceDetail>(), i => i.InvoiceID, d => d.InvoiceID, (i, d) => d)
                    .Join(models.GetTable<InvoiceProduct>(), d => d.ProductID, p => p.ProductID, (d, p) => p);

            _allowanceItem.TaxAmount = 0;
            _allowanceItem.TotalAmount = 0;
            foreach (var i in _allowanceItem.AllowanceItem)
            {
                //課稅別
                if (!Enum.IsDefined(typeof(Naming.TaxTypeDefinition), (int)i.TaxType))
                {
                    return new Exception(String.Format(MessageResources.AlertAllowance_TaxType, i.TaxType));
                }

                i.OriginalDescription = i.OriginalDescription.GetEfficientString();
                InvoiceProduct item = null;
                InvoiceProductItem prodItem = null;
                //foreach (var p in productItems.Where(p => p.Brief == i.OriginalDescription).OrderByDescending(d => d.ProductID))
                foreach (var p in productItems.OrderByDescending(d => d.ProductID))
                {
                    prodItem = p.InvoiceProductItem.FirstOrDefault();
                    if (prodItem != null)
                    {
                        var totalCost = (prodItem.CostAmount ?? 0)
                                - ((prodItem.InvoiceAllowanceItem.Sum(a => a.Amount)) ?? 0);
                        if (totalCost >= i.Amount)
                        {
                            item = p;
                            break;
                        }
                    }
                }

                if (item == null)
                {
                    return new InvoiceNotFoundException(String.Format(MessageResources.InvalidAllowance_NoInvoiceData, i.OriginalDescription));
                }

                originalInvoice = item.InvoiceDetails.First().InvoiceItem;
                if (_allowanceItem.BuyerId != originalInvoice.InvoiceBuyer.ReceiptNo)
                {
                    return new Exception(String.Format(MessageResources.InvalidBuyerId, _allowanceItem.BuyerId));
                }

                if (_allowanceDate.AddDays(1) < originalInvoice.InvoiceDate)
                {
                    _allowanceDate = originalInvoice.InvoiceDate.Value.AddDays(1);
                }

                _allowanceItem.TotalAmount += i.Amount;
                _allowanceItem.TaxAmount += i.Tax;

                var allowanceItem = new InvoiceAllowanceItem
                {
                    Amount = i.Amount,
                    InvoiceNo = $"{originalInvoice.TrackCode}{originalInvoice.No}",
                    InvoiceDate = originalInvoice.InvoiceDate,
                    Piece = i.Quantity,
                    PieceUnit = i.Unit,
                    OriginalDescription = i.OriginalDescription,
                    OriginalSequenceNo = prodItem.No,
                    TaxType = i.TaxType,
                    No = (short)(_productItems.Count + 1),
                    UnitCost = i.UnitPrice,
                    Tax = i.Tax,
                    ItemID = prodItem.ItemID,
                };

                _productItems.Add(allowanceItem);
            }

            _newItem = new InvoiceAllowance() 
            {
                CDS_Document = new CDS_Document
                {
                    DocDate = DateTime.Now,
                    DocType = (int)Naming.DocumentTypeDefinition.E_Allowance
                },
                AllowanceDate = _allowanceDate,
                IssueDate = _allowanceDate,
                AllowanceNumber = _allowanceItem.AllowanceNumber,
                AllowanceType = _allowanceItem.AllowanceType,
                BuyerId = _allowanceItem.BuyerId,
                SellerId = _allowanceItem.SellerId,
                TaxAmount = _allowanceItem.TaxAmount,
                TotalAmount = _allowanceItem.TotalAmount,
                CurrencyID = _currency?.CurrencyID,
                InvoiceAllowanceBuyer = new InvoiceAllowanceBuyer
                {
                    Name = _allowanceItem.BuyerName,
                    ReceiptNo = _allowanceItem.BuyerId,
                    CustomerID = _allowanceItem.GoogleId,
                    ContactName = _allowanceItem.BuyerName,
                    CustomerName = _allowanceItem.BuyerName
                },
                InvoiceAllowanceSeller = new InvoiceAllowanceSeller
                {
                    Name = _seller.CompanyName,
                    ReceiptNo = _seller.ReceiptNo,
                    Address = _seller.Addr,
                    ContactName = _seller.ContactName,
                    CustomerID = null,
                    CustomerName = _seller.CompanyName,
                    EMail = _seller.ContactEmail,
                    Fax = _seller.Fax,
                    Phone = _seller.Phone,
                    PersonInCharge = _seller.UndertakerName,
                    SellerID = _seller.CompanyID,
                }
            };

            _newItem.InvoiceAllowanceDetails.AddRange(_productItems.Select(p => new InvoiceAllowanceDetail 
            {
                InvoiceAllowanceItem = p,
            }));

            if (_owner != null)
            {
                _newItem.CDS_Document.DocumentOwner = new DocumentOwner
                {
                    OwnerID = _owner.CompanyID,
                };
            }

            return null;
        }
    }
}
