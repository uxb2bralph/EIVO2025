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

namespace ModelCore.Models.ViewModel
{
    public partial class AllowanceViewModelValidator<TEntity>
        where TEntity : class, new()
    {
        protected ModelSource<TEntity> _mgr;
        protected Organization _owner;

        protected AllowanceViewModel _allowanceItem;

        protected InvoiceAllowance _newItem;
        protected Organization _seller;
        protected Organization? _buyer;
        protected List<InvoiceAllowanceItem> _productItems;
        protected DateTime _allowanceDate;


        public AllowanceViewModelValidator(ModelSource<TEntity> mgr, Organization owner)
        {
            _mgr = mgr;
            _owner = owner;
        }

        public Organization Seller
        {
            get
            {
                return _seller;
            }
        }

        public InvoiceAllowance Allowance
        {
            get
            {
                return _newItem;
            }
        }

        public virtual Exception Validate(AllowanceViewModel dataItem)
        {
            _allowanceItem = dataItem;

            Exception ex;

            _seller = null;
            _newItem = null;

            //bool IsProxy = _owner.Organization.OrganizationCategory.Any(c => c.CategoryID == (int)CategoryDefinition.CategoryEnum.開立發票營業人代理);

            if ((ex = CheckBusiness()) != null/* && !IsProxy*/)
            {
                return ex;
            }

            //if((ex = CheckBusiness_Proxy()) != null && IsProxy)
            //{
            //    return ex;
            //}

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
            _seller = _mgr.GetTable<Organization>().Where(o => o.CompanyID == _allowanceItem.SellerID).FirstOrDefault();

            if (_seller == null)
            {
                return new Exception(String.Format(MessageResources.AlertInvalidSeller, $"CompanyID:{_allowanceItem.SellerID}"));
            }
            else if (_owner == null)
            {
                _owner = _seller;
            }

            if (_allowanceItem.BuyerReceiptNo == "0000000000")
            {
                //if (_allowanceItem.BuyerName == null || Encoding.GetEncoding(950).GetBytes(_allowanceItem.BuyerName).Length != 4)
                //{
                //    return new Exception(String.Format(MessageResources.InvalidBuyerName, _allowanceItem.BuyerName));
                //}
            }
            else if (_allowanceItem.BuyerReceiptNo == null || !Regex.IsMatch(_allowanceItem.BuyerReceiptNo, "^[0-9]{8}$"))
            {
                return new Exception(String.Format(MessageResources.InvalidBuyerId, _allowanceItem.BuyerReceiptNo));
            }
            else
            {
                _buyer = _mgr.GetTable<Organization>().Where(o => o.ReceiptNo == _allowanceItem.BuyerReceiptNo).FirstOrDefault();
                if (_allowanceItem.BuyerName != null && _allowanceItem.BuyerName.Length > 60)
                {
                    if (_buyer == null)
                        return new Exception(String.Format(MessageResources.AlertBuyerNameLength, _allowanceItem.BuyerName));
                    else
                        _allowanceItem.BuyerName = _buyer.CompanyName;
                }
            }

            return null;
        }

        protected virtual Exception CheckMandatoryFields()
        {
            //if (String.IsNullOrEmpty(_allowanceItem.AllowanceNumber))
            //{
            //    var count = _mgr.GetTable<InvoiceAllowanceSeller>().Where(s => s.SellerID == _seller.CompanyID).Count() + 1;
            //    _allowanceItem.AllowanceNumber = $"{_seller.ReceiptNo}-{count:0000000}";
            //}

            //折讓證明單號碼
            _allowanceItem.AllowanceNumber = _allowanceItem.AllowanceNumber.GetEfficientString();
            if (_allowanceItem.AllowanceNumber?.Length > 16)
            {
                return new Exception(String.Format(MessageResources.AlertAllowanceNoLength, _allowanceItem.AllowanceNumber));
            }

            var table = _mgr.GetTable<InvoiceAllowance>();
            if (table.Any(i => i.AllowanceNumber == _allowanceItem.AllowanceNumber))
            {
                return new Exception(String.Format(MessageResources.AlertAllowanceDuplicated, _allowanceItem.AllowanceNumber));
            }

            //折讓證明單日期
            if (!_allowanceItem.AllowanceDate.HasValue)
            {
                _allowanceItem.AllowanceDate = DateTime.Now;
            }

            _allowanceDate = _allowanceItem.AllowanceDate.Value;

            return null;
        }

        protected virtual Exception CheckAllowanceItem()
        {
            _productItems = new List<InvoiceAllowanceItem>();
            var invTable = _mgr.GetTable<InvoiceItem>();

            InvoiceItem originalInvoice = null;

            if (!(_allowanceItem.OriginalInvoiceNo?.Length > 0))
            {
                return new Exception(String.Format(MessageResources.InvalidAllowance_NoInvoiceData, "N/A"));
            }

            for (int i = 0; i < _allowanceItem.OriginalInvoiceNo?.Length; i++)
            {

                if (!String.IsNullOrEmpty(_allowanceItem.OriginalInvoiceNo[i]) && _allowanceItem.OriginalInvoiceNo[i].Length == 10)
                {
                    String invNo, trackCode;
                    trackCode = _allowanceItem.OriginalInvoiceNo[i].Substring(0, 2);
                    invNo = _allowanceItem.OriginalInvoiceNo[i].Substring(2);
                    originalInvoice = invTable.Where(n => n.TrackCode == trackCode && n.No == invNo).FirstOrDefault();
                }

                if (originalInvoice == null)
                {
                    return new Exception(String.Format(MessageResources.InvalidAllowance_NoInvoiceData, _allowanceItem.OriginalInvoiceNo[i]));
                }

                if (originalInvoice.InvoiceCancellation != null)
                {
                    return new Exception(MessageResources.InvalidAllowance_InvoiceHasBeenCanceled);
                }

                _allowanceItem.OriginalInvoiceDate[i] = originalInvoice.InvoiceDate.Value;

                if (originalInvoice.SellerID != _allowanceItem.SellerID)
                {
                    return new Exception(String.Format(MessageResources.AlertAllowance_InvoiceSellerIsDifferent, _allowanceItem.OriginalInvoiceNo[i]));
                }

                //原明細排列序號
                if (_allowanceItem.OriginalSequenceNo[i] > 1000 || _allowanceItem.OriginalSequenceNo[i] < 0)
                {
                    return new Exception(String.Format(MessageResources.AlertAllowance_OriginalSequenceNumber, _allowanceItem.OriginalSequenceNo[i]));
                }

                //原品名
                _allowanceItem.OriginalDescription[i] = _allowanceItem.OriginalDescription[i].GetEfficientString();
                if (_allowanceItem.OriginalDescription[i] == null || _allowanceItem.OriginalDescription[i].Length > 256)
                {
                    return new Exception(String.Format(MessageResources.AlertAllowance_OriginalDescription, _allowanceItem.OriginalDescription[i]));
                }

                //單位
                _allowanceItem.PieceUnit[i] = _allowanceItem.PieceUnit[i].GetEfficientString();
                if (_allowanceItem.PieceUnit[i] != null && _allowanceItem.PieceUnit[i].Length > 6)
                {
                    return new Exception(String.Format(MessageResources.AlertAllowance_Unit, _allowanceItem.PieceUnit[i]));
                }

                if (_allowanceDate.AddDays(1) < originalInvoice.InvoiceDate)
                {
                    _allowanceDate = originalInvoice.InvoiceDate.Value.AddDays(1);
                }

                if(_allowanceItem.AllowanceNumber == null)
                {
                    _allowanceItem.AllowanceNumber = $"{_allowanceItem.OriginalInvoiceNo[i]}{_mgr.GetTable<InvoiceAllowanceItem>().Where(a => a.InvoiceNo == _allowanceItem.OriginalInvoiceNo[i]).Count():000000}";
                }

                var allowanceItem = new InvoiceAllowanceItem
                {
                    Amount = _allowanceItem.Amount[i],
                    InvoiceNo = _allowanceItem.OriginalInvoiceNo[i],
                    InvoiceDate = _allowanceItem.OriginalInvoiceDate[i],
                    //ItemNo = i.Item,
                    OriginalSequenceNo = _allowanceItem.OriginalSequenceNo[i],
                    Piece = _allowanceItem.Piece[i],
                    PieceUnit = _allowanceItem.PieceUnit[i],
                    OriginalDescription = _allowanceItem.OriginalDescription[i],
                    TaxType = (byte)_allowanceItem.TaxType[i],
                    No = (short)(i + 1),
                    UnitCost = _allowanceItem.UnitCost[i],
                    Tax = _allowanceItem.Tax[i],
                };

                _productItems.Add(allowanceItem);
            }

            _newItem = new InvoiceAllowance()
            {
                CDS_Document = new CDS_Document
                {
                    DocDate = DateTime.Now,
                    DocType = (int)Naming.DocumentTypeDefinition.E_Allowance,
                    ProcessType = originalInvoice.CDS_Document.ProcessType == (int)Naming.InvoiceProcessType.A0401
                                    ? (int)Naming.InvoiceProcessType.B0401
                                    : (int)Naming.InvoiceProcessType.D0401,
                },
                AllowanceDate = _allowanceDate,
                IssueDate = _allowanceDate,
                AllowanceNumber = _allowanceItem.AllowanceNumber,
                AllowanceType = (byte)_allowanceItem.AllowanceType,
                BuyerId = _allowanceItem.BuyerReceiptNo,
                SellerId = _seller.ReceiptNo,
                TaxAmount = _allowanceItem.TaxAmount,
                TotalAmount = _allowanceItem.TotalAmount,
                CurrencyID = originalInvoice.InvoiceAmountType.CurrencyID,
                InvoiceAllowanceBuyer = new InvoiceAllowanceBuyer
                {
                    BuyerID = _buyer?.CompanyID,
                    Name = _allowanceItem.BuyerName,
                    ReceiptNo = _allowanceItem.BuyerReceiptNo,
                    ContactName = _allowanceItem.BuyerName,
                    CustomerName = _allowanceItem.BuyerName
                },
                InvoiceAllowanceSeller = new InvoiceAllowanceSeller
                {
                    Name = _seller.CompanyName,
                    ReceiptNo = _seller.ReceiptNo,
                    Address = _seller.Addr,
                    ContactName = _seller.ContactName,
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

            _newItem.CDS_Document.DocumentOwner = new DocumentOwner
            {
                OwnerID = _owner.CompanyID,
            };

            return null;
        }
    }
}
