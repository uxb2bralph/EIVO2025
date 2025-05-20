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
using ModelCore.InvoiceManagement.ErrorHandle;
using CommonLib.Utility;
using ModelCore.InvoiceManagement.InvoiceProcess;
using CommonLib.DataAccess;

namespace ModelCore.InvoiceManagement.Validator
{
    public partial class AllowanceRootAllowanceValidator
    {
        protected GenericManager<EIVOEntityDataContext> models;
        protected Organization _owner;

        protected AllowanceRootAllowance _allowanceItem;

        protected InvoiceAllowance _newItem;
        protected Organization _seller;
        protected List<InvoiceAllowanceItem> _productItems;
        protected DateTime _allowanceDate;
        protected CurrencyType _currency;


        public AllowanceRootAllowanceValidator(GenericManager<EIVOEntityDataContext> mgr, Organization owner)
        {
            models = mgr;
            _owner = owner;
        }

        public InvoiceAllowance Allowance
        {
            get
            {
                return _newItem;
            }
        }

        public Naming.InvoiceProcessType? ProcessType
        {
            get;
            set;
        }

        public Organization Seller
        {
            get
            {
                return _seller;
            }
        }

        InvoiceItem _originalInvoice;
        public virtual Exception Validate(AllowanceRootAllowance dataItem)
        {
            _allowanceItem = dataItem;

            Exception ex;

            _seller = null;
            _newItem = null;
            _originalInvoice = null;

            if ((ex = CheckBusiness()) != null)
            {
                return ex;
            }

            if ((ex = CheckMandatoryFields()) != null)
            {
                return ex;
            }

            if ((ex = CheckAllowanceItem()) != null)
            {
                return ex;
            }

            return null;
        }

        protected virtual Exception CheckBusiness()
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

            //if (_allowanceItem.SellerName == null || _allowanceItem.SellerName.Length > 60)
            //{
            //    return new Exception(String.Format(MessageResources.AlertSellerNameLength, _allowanceItem.SellerName));
            //}
            _allowanceItem.DataNumber = _allowanceItem.DataNumber.GetEfficientString();
            if (_allowanceItem.DataNumber != null)
            {
                _originalInvoice = models.GetTable<InvoiceItem>().Where(v => v.SellerID == _seller.CompanyID)
                        .Join(models.GetTable<InvoicePurchaseOrder>().Where(p => p.OrderNo == _allowanceItem.DataNumber),
                            v => v.InvoiceID, p => p.InvoiceID, (v, p) => v).FirstOrDefault();
            }

            String dataNo = null;
            if (_originalInvoice == null)
            {
                dataNo = _allowanceItem.AllowanceItem?.Select(a => a.OriginalDataNumber.GetEfficientString())
                                    .Where(d => d != null)
                                    .FirstOrDefault();
                if (dataNo != null)
                {
                    _originalInvoice = models.GetTable<InvoiceItem>().Where(v => v.SellerID == _seller.CompanyID)
                            .Join(models.GetTable<InvoicePurchaseOrder>().Where(p => p.OrderNo == dataNo),
                                v => v.InvoiceID, p => p.InvoiceID, (v, p) => v).FirstOrDefault();
                }
            }

            if (_originalInvoice != null)
            {
                if (_originalInvoice.InvoiceCancellation != null)
                {
                    return new Exception(MessageResources.InvalidAllowance_InvoiceHasBeenCanceled);
                }
            }

            if (_allowanceItem.BuyerId == "0000000000")
            {
                //if (_allowanceItem.BuyerName == null || Encoding.GetEncoding(950).GetBytes(_allowanceItem.BuyerName).Length != 4)
                //{
                //    return new Exception(String.Format(MessageResources.InvalidBuyerName, _allowanceItem.BuyerName));
                //}
            }
            else if (_allowanceItem.BuyerId == null)
            {
                _allowanceItem.BuyerId = "0000000000";
                if (_originalInvoice == null)
                {
                    //return new Exception(String.Format(MessageResources.InvalidAllowance_NoInvoiceData, _allowanceItem.DataNumber ?? dataNo));
                }
                else
                {
                    _allowanceItem.BuyerId = _originalInvoice.InvoiceBuyer.ReceiptNo;
                }
            }
            else if (!Regex.IsMatch(_allowanceItem.BuyerId, "^[0-9]{8}$"))
            {
                return new Exception(String.Format(MessageResources.InvalidBuyerId, _allowanceItem.BuyerId));
            }
            else if (_allowanceItem.BuyerName != null && _allowanceItem.BuyerName.Length > 60)
            {
                return new Exception(String.Format(MessageResources.AlertBuyerNameLength, _allowanceItem.BuyerName));
            }

            return null;
        }

        protected virtual Exception CheckMandatoryFields()
        {

            if (String.IsNullOrEmpty(_allowanceItem.AllowanceNumber))
            {
                return new Exception(MessageResources.InvalidAllowanceNo);
            }

            //折讓證明單號碼
            if (_allowanceItem.AllowanceNumber.Length > 64)
            {
                return new Exception(String.Format(MessageResources.AlertAllowanceNoLength, _allowanceItem.AllowanceNumber));
            }

            var currentItem = models.GetTable<InvoiceAllowance>().Where(a => a.AllowanceNumber == _allowanceItem.AllowanceNumber)
                        .Join(models.GetTable<InvoiceAllowanceSeller>().Where(s => s.SellerID == _seller.CompanyID),
                            a => a.AllowanceID, s => s.AllowanceID, (a, s) => a).FirstOrDefault();
            if (currentItem != null)
            {
                if (currentItem.InvoiceAllowanceSeller.SellerID == _seller.CompanyID && _seller.IgnoreDuplicatedNo())
                {
                    return new DuplicateAllowanceNumberException(String.Format(MessageResources.AlertAllowanceDuplicated, _allowanceItem.AllowanceNumber)) { CurrentAllowance = currentItem };
                }
                else
                {
                    return new Exception(String.Format(MessageResources.AlertAllowanceDuplicated, _allowanceItem.AllowanceNumber));
                }
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
            if (_allowanceItem.AllowanceType == default(byte))
            {
                _allowanceItem.AllowanceType = 2;
            }
            else if (_allowanceItem.AllowanceType != 1 && _allowanceItem.AllowanceType != 2)
            {
                return new Exception(String.Format(MessageResources.AlertAllowanceType, _allowanceItem.AllowanceType));
            }

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

        protected virtual Exception CheckAllowanceItem()
        {
            _productItems = new List<InvoiceAllowanceItem>();
            var invTable = models.GetTable<InvoiceItem>()
                            .Where(v => v.SellerID == _seller.CompanyID);

            InvoiceItem originalInvoice = null;
            foreach (var i in _allowanceItem.AllowanceItem)
            {

                if (i.OriginalInvoiceNumber != null && i.OriginalInvoiceNumber.Length == 10)
                {
                    String invNo, trackCode;
                    trackCode = i.OriginalInvoiceNumber.Substring(0, 2);
                    invNo = i.OriginalInvoiceNumber.Substring(2);
                    originalInvoice = invTable
                        .Where(n => n.SellerID == _seller.CompanyID)
                        .Where(n => n.TrackCode == trackCode)
                        .Where(n => n.No == invNo)
                        .OrderByDescending(n => n.InvoiceID)
                        .FirstOrDefault();
                }
                else
                {
                    originalInvoice = _originalInvoice;
                }

                if (originalInvoice == null)
                {
                    return new Exception(String.Format(MessageResources.InvalidAllowance_NoInvoiceData, i.OriginalInvoiceNumber));
                }

                if (originalInvoice.InvoiceCancellation != null)
                {
                    return new Exception(MessageResources.InvalidAllowance_InvoiceHasBeenCanceled);
                }


                i.OriginalInvoiceDate = i.OriginalInvoiceDate.GetEfficientString();
                if (i.OriginalInvoiceDate != null)
                {
                    var invDate = String.Format("{0:yyyy/MM/dd}", originalInvoice.InvoiceDate);

                    if (i.OriginalInvoiceDate != invDate)
                    {
                        return new Exception(String.Format(MessageResources.AlertAllowance_InvoiceDate, invDate, i.OriginalInvoiceDate));
                    }
                }

                //DateTime invoiceDate;
                //if (String.IsNullOrEmpty(i.OriginalInvoiceDate) || !DateTime.TryParseExact(String.Format("{0}", i.OriginalInvoiceDate), "yyyy/MM/dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out invoiceDate))
                //{
                //    return new Exception(String.Format(MessageResources.AlertAllowance_InvoiceDateFormat, i.OriginalInvoiceDate));
                //}


                if (originalInvoice.InvoiceBuyer.ReceiptNo != _allowanceItem.BuyerId && _allowanceItem.BuyerId != "0000000000")
                {
                    return new Exception(String.Format(MessageResources.AlertAllowance_InvoiceBuyerError, i.OriginalInvoiceNumber));
                }

                if (originalInvoice.InvoiceSeller.ReceiptNo != _allowanceItem.SellerId)
                {
                    return new Exception(String.Format(MessageResources.AlertAllowance_InvoiceSellerIsDifferent, i.OriginalInvoiceNumber));
                }

                //原明細排列序號
                if (i.OriginalSequenceNumber.HasValue && (i.OriginalSequenceNumber > 1000 || i.OriginalSequenceNumber < 0))
                {
                    return new Exception(String.Format(MessageResources.AlertAllowance_OriginalSequenceNumber, i.OriginalSequenceNumber));
                }

                //折讓證明單明細排列序號
                if (i.AllowanceSequenceNumber > 1000 || i.AllowanceSequenceNumber < 0)
                {
                    return new Exception(String.Format(MessageResources.AlertAllowance_AllowanceSequenceNumber, i.AllowanceSequenceNumber));
                }

                //原品名
                if (i.OriginalDescription == null || i.OriginalDescription.Length > 256)
                {
                    return new Exception(String.Format(MessageResources.AlertAllowance_OriginalDescription, i.OriginalDescription));
                }

                //單位
                if (i.Unit != null && i.Unit.Length > 6)
                {
                    return new Exception(String.Format(MessageResources.AlertAllowance_Unit, i.Unit));
                }

                //課稅別
                if (!Enum.IsDefined(typeof(Naming.TaxTypeDefinition), (int)i.TaxType))
                {
                    return new Exception(String.Format(MessageResources.AlertAllowance_TaxType, i.TaxType));
                }

                if (_allowanceDate.AddDays(1) < originalInvoice.InvoiceDate)
                {
                    _allowanceDate = originalInvoice.InvoiceDate.Value.AddDays(1);
                }

                var allowanceItem = new InvoiceAllowanceItem
                {
                    Amount = i.Amount,
                    InvoiceNo = $"{originalInvoice.TrackCode}{originalInvoice.No}",
                    InvoiceDate = originalInvoice.InvoiceDate,
                    ItemNo = i.Item,
                    OriginalSequenceNo = i.OriginalSequenceNumber,
                    Piece = i.Quantity > 0 ? i.Quantity : Math.Round(i.Amount / i.UnitPrice),
                    PieceUnit = i.Unit,
                    OriginalDescription = i.OriginalDescription,
                    TaxType = i.TaxType,
                    No = i.AllowanceSequenceNumber,
                    UnitCost = i.UnitPrice,
                    Tax = i.Tax,
                };

                var invProductItem = models.GetTable<InvoiceItem>().Where(v => v.InvoiceID == originalInvoice.InvoiceID)
                    .Join(models.GetTable<InvoiceDetail>(), v => v.InvoiceID, d => d.InvoiceID, (v, d) => d)
                    .Join(models.GetTable<InvoiceProduct>(), d => d.ProductID, p => p.ProductID, (d, p) => p)
                    .Join(models.GetTable<InvoiceProductItem>(), p => p.ProductID, t => t.ProductID, (p, t) => t)
                    .Where(t => t.No == i.OriginalSequenceNumber).FirstOrDefault();

                if (invProductItem != null)
                {
                    allowanceItem.ItemID = invProductItem.ItemID;
                }

                _productItems.Add(allowanceItem);

                if (_originalInvoice == null)
                {
                    _originalInvoice = originalInvoice;
                }
            }

            _newItem = new InvoiceAllowance()
            {
                CDS_Document = new CDS_Document
                {
                    DocDate = DateTime.Now,
                    DocType = (int)Naming.DocumentTypeDefinition.E_Allowance,
                    ProcessType = _originalInvoice.CDS_Document.ProcessType == (int)Naming.InvoiceProcessType.A0101
                                    ? (int)Naming.InvoiceProcessType.B0101
                                    : (int)Naming.InvoiceProcessType.G0401,
                },
                AllowanceDate = _allowanceDate,
                IssueDate = _allowanceDate,
                AllowanceNumber = _allowanceItem.AllowanceNumber,
                AllowanceType = _allowanceItem.AllowanceType,
                BuyerId = _allowanceItem.BuyerId,
                SellerId = _allowanceItem.SellerId,
                TaxAmount = _allowanceItem.TaxAmount,
                TotalAmount = _allowanceItem.TotalAmount,
                CurrencyID = _currency?.CurrencyID ?? originalInvoice.InvoiceAmountType.CurrencyID,
                //InvoiceID =  invTable.Where(i=>i.TrackCode + i.No == item.AllowanceItem.Select(a=>a.OriginalInvoiceNumber).FirstOrDefault()).Select(i=>i.InvoiceID).FirstOrDefault(),
                InvoiceAllowanceBuyer = new InvoiceAllowanceBuyer
                {
                    Name = _allowanceItem.BuyerName,
                    ReceiptNo = _allowanceItem.BuyerId,
                    CustomerID = String.IsNullOrEmpty(_allowanceItem.GoogleId) ? "" : _allowanceItem.GoogleId,
                    ContactName = _allowanceItem.BuyerName,
                    CustomerName = _allowanceItem.BuyerName
                },
                InvoiceAllowanceSeller = new InvoiceAllowanceSeller
                {
                    Name = _seller.CompanyName,
                    ReceiptNo = _seller.ReceiptNo,
                    Address = _seller.Addr,
                    ContactName = _seller.ContactName,
                    CustomerID = String.IsNullOrEmpty(_allowanceItem.GoogleId) ? "" : _allowanceItem.GoogleId,
                    CustomerName = _seller.CompanyName,
                    EMail = _seller.ContactEmail,
                    Fax = _seller.Fax,
                    Phone = _seller.Phone,
                    PersonInCharge = _seller.UndertakerName,
                    SellerID = _seller.CompanyID,
                }
            };

            if (_allowanceItem.Contact != null)
            {
                var contact = _allowanceItem.Contact;
                _newItem.InvoiceAllowanceBuyer.Address = contact.Address;
                _newItem.InvoiceAllowanceBuyer.EMail = contact.Email;
                _newItem.InvoiceAllowanceBuyer.ContactName = contact.Name;
                _newItem.InvoiceAllowanceBuyer.Phone = contact.TEL;
            }

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
