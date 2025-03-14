using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;


using ModelCore.DataEntity;
using ModelCore.Schema.EIVO;
using ModelCore.Resource;
using ModelCore.Locale;
using ModelCore.InvoiceManagement.ErrorHandle;
using ModelCore.Schema.TurnKey.D0401;
using CommonLib.DataAccess;

namespace ModelCore.InvoiceManagement.Validator
{
    public partial class D0401Validator : AllowanceRootAllowanceValidator
    {
        protected new ModelCore.Schema.TurnKey.D0401.Allowance _allowanceItem;

        public D0401Validator(GenericManager<EIVOEntityDataContext> mgr, Organization owner) : base(mgr,owner)
        {

        }

        public virtual Exception Validate(ModelCore.Schema.TurnKey.D0401.Allowance dataItem)
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
            String receiptNo = _allowanceItem.Main?.Seller?.Identifier;
            _seller = models.GetTable<Organization>().Where(o => o.ReceiptNo == receiptNo).FirstOrDefault();

            if (_seller == null)
            {
                return new Exception(String.Format(MessageResources.AlertInvalidSeller, receiptNo));
            }

            if (_seller.CompanyID != _owner.CompanyID && !models.GetTable<InvoiceIssuerAgent>().Any(a => a.AgentID == _owner.CompanyID && a.IssuerID == _seller.CompanyID))
            {
                return new Exception(String.Format(MessageResources.AlertSellerSignature, receiptNo));
            }

            //if (_allowanceItem.SellerName == null || _allowanceItem.SellerName.Length > 60)
            //{
            //    return new Exception(String.Format(MessageResources.AlertSellerNameLength, _allowanceItem.SellerName));
            //}

            receiptNo = _allowanceItem.Main.Buyer.Identifier;
            if (receiptNo == "0000000000")
            {
                //if (_allowanceItem.BuyerName == null || Encoding.GetEncoding(950).GetBytes(_allowanceItem.BuyerName).Length != 4)
                //{
                //    return new Exception(String.Format(MessageResources.InvalidBuyerName, _allowanceItem.BuyerName));
                //}
            }
            else if (receiptNo == null || !Regex.IsMatch(receiptNo, "^[0-9]{8}$"))
            {
                return new Exception(String.Format(MessageResources.InvalidBuyerId, receiptNo));
            }
            else if (_allowanceItem.Main?.Buyer?.Name!=null && _allowanceItem.Main?.Buyer?.Name.Length > 60)
            {
                return new Exception(String.Format(MessageResources.AlertBuyerNameLength, _allowanceItem.Main?.Buyer?.Name));
            }

            return null;
        }

        protected override Exception CheckMandatoryFields()
        {
            _allowanceDate = default;

            if (String.IsNullOrEmpty(_allowanceItem.Main?.AllowanceNumber))
            {
                return new Exception(MessageResources.InvalidAllowanceNo);
            }

            //折讓證明單號碼
            if (_allowanceItem.Main?.AllowanceNumber.Length > 16)
            {
                return new Exception(String.Format(MessageResources.AlertAllowanceNoLength, _allowanceItem.Main?.AllowanceNumber));
            }

            String allownaceNo = _allowanceItem.Main?.AllowanceNumber;
            var currentItem = models.GetTable<InvoiceAllowance>().Where(a => a.AllowanceNumber == allownaceNo)
                        .Join(models.GetTable<InvoiceAllowanceSeller>().Where(s => s.SellerID == _seller.CompanyID),
                            a => a.AllowanceID, s => s.AllowanceID, (a, s) => a).FirstOrDefault();
            if (currentItem != null)
            {
                return new DuplicateAllowanceNumberException(String.Format(MessageResources.AlertAllowanceDuplicated, _allowanceItem.Main?.AllowanceNumber)) { CurrentAllowance = currentItem };
            }

            //折讓證明單日期
            if (String.IsNullOrEmpty(_allowanceItem.Main?.AllowanceDate))
            {
                return new Exception(MessageResources.InvalidAllowanceDate);
            }

            if (!DateTime.TryParseExact(_allowanceItem.Main?.AllowanceDate, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out _allowanceDate))
            {
                return new Exception(String.Format(MessageResources.AlertAllowanceDateFormat, _allowanceItem.Main?.AllowanceDate));
            }

            //折讓種類
            if (_allowanceItem.Main?.AllowanceType != AllowanceTypeEnum.Item1 && _allowanceItem.Main?.AllowanceType != AllowanceTypeEnum.Item2)
            {
                return new Exception(String.Format(MessageResources.AlertAllowanceType, _allowanceItem.Main?.AllowanceType));
            }

            _currency = null;

            return null;
        }

        protected override Exception CheckAllowanceItem()
        {
            _productItems = new List<InvoiceAllowanceItem>();
            var invTable = models.GetTable<InvoiceItem>();

            InvoiceItem originalInvoice = null;
            foreach (var i in _allowanceItem.Details)
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
                        .FirstOrDefault();
                }

                if (originalInvoice == null)
                {
                    return new Exception(String.Format(MessageResources.InvalidAllowance_NoInvoiceData, i.OriginalInvoiceNumber));
                }

                if (originalInvoice.InvoiceCancellation != null)
                {
                    return new Exception(MessageResources.InvalidAllowance_InvoiceHasBeenCanceled);
                }

                if (i.OriginalInvoiceDate != String.Format("{0:yyyyMMdd}", originalInvoice.InvoiceDate))
                {
                    return new Exception(String.Format(MessageResources.AlertAllowance_InvoiceDate, $"{originalInvoice.InvoiceDate:yyyyMMdd}", i.OriginalInvoiceDate));
                }


                if (originalInvoice.InvoiceBuyer.ReceiptNo != _allowanceItem.Main?.Buyer?.Identifier && _allowanceItem.Main?.Buyer?.Identifier != "0000000000")
                {
                    return new Exception(String.Format(MessageResources.AlertAllowance_InvoiceBuyerError, i.OriginalInvoiceNumber));
                }

                if (originalInvoice.InvoiceSeller.ReceiptNo != _allowanceItem.Main?.Seller?.Identifier)
                {
                    return new Exception(String.Format(MessageResources.AlertAllowance_InvoiceSellerIsDifferent, i.OriginalInvoiceNumber));
                }

                //原明細排列序號
                short originalSeqNo = -1;
                if (!String.IsNullOrEmpty(i.OriginalSequenceNumber))
                {
                    if (!short.TryParse(i.OriginalSequenceNumber, out originalSeqNo) || originalSeqNo > 1000 || originalSeqNo < 0)
                    {
                        return new Exception(String.Format(MessageResources.AlertAllowance_OriginalSequenceNumber, i.OriginalSequenceNumber));
                    }
                }

                //折讓證明單明細排列序號
                if (String.IsNullOrEmpty(i.AllowanceSequenceNumber) || !short.TryParse(i.AllowanceSequenceNumber, out short seqNo) || seqNo > 1000 || seqNo < 0)
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

                if (String.IsNullOrEmpty(i.OriginalInvoiceDate) || !DateTime.TryParseExact(String.Format("{0}", i.OriginalInvoiceDate), "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime invoiceDate))
                {
                    return new Exception(String.Format(MessageResources.AlertAllowance_InvoiceDateFormat, i.OriginalInvoiceDate));
                }

                var allowanceItem = new InvoiceAllowanceItem
                {
                    Amount = i.Amount,
                    InvoiceNo = i.OriginalInvoiceNumber,
                    InvoiceDate = invoiceDate,
                    OriginalSequenceNo = originalSeqNo >= 0 ? originalSeqNo : (short?)null,
                    Piece = i.Quantity,
                    PieceUnit = i.Unit,
                    OriginalDescription = i.OriginalDescription,
                    TaxType = (byte)i.TaxType,
                    No = seqNo,
                    UnitCost = i.UnitPrice,
                    Tax = i.Tax,
                };

                if (originalInvoice != null)
                {
                    var invProductItem = models.GetTable<InvoiceItem>().Where(v => v.InvoiceID == originalInvoice.InvoiceID)
                        .Join(models.GetTable<InvoiceDetail>(), v => v.InvoiceID, d => d.InvoiceID, (v, d) => d)
                        .Join(models.GetTable<InvoiceProduct>(), d => d.ProductID, p => p.ProductID, (d, p) => p)
                        .Join(models.GetTable<InvoiceProductItem>(), p => p.ProductID, t => t.ProductID, (p, t) => t)
                        .Where(t => t.No == originalSeqNo).FirstOrDefault();
                    //var invProductItem = originalInvoice.InvoiceDetails.Join(_mgr.GetTable<InvoiceProductItem>(), d => d.ProductID, p => p.ProductID, (d, p) => p)
                    //    .Where(p => p.No == i.OriginalSequenceNumber).FirstOrDefault();
                    if (invProductItem != null)
                    {
                        allowanceItem.ItemID = invProductItem.ItemID;
                    }
                }
                _productItems.Add(allowanceItem);
            }

            _newItem = new InvoiceAllowance()
            {
                CDS_Document = new CDS_Document
                {
                    DocDate = DateTime.Now,
                    DocType = (int)Naming.DocumentTypeDefinition.E_Allowance,
                    DocumentOwner = new DocumentOwner
                    {
                        OwnerID = _owner.CompanyID
                    },
                    ProcessType = (int)Naming.InvoiceProcessType.D0401,
                },
                AllowanceDate = _allowanceDate,
                AllowanceNumber = _allowanceItem.Main?.AllowanceNumber,
                AllowanceType = (byte?)_allowanceItem.Main?.AllowanceType,
                BuyerId = _allowanceItem.Main?.Buyer?.Identifier,
                SellerId = _allowanceItem.Main?.Seller?.Identifier,
                TaxAmount = _allowanceItem.Amount?.TaxAmount,
                TotalAmount = _allowanceItem.Amount?.TotalAmount,
                CurrencyID = _currency?.CurrencyID ?? originalInvoice.InvoiceAmountType.CurrencyID,
                //InvoiceID =  invTable.Where(i=>i.TrackCode + i.No == item.AllowanceItem.Select(a=>a.OriginalInvoiceNumber).FirstOrDefault()).Select(i=>i.InvoiceID).FirstOrDefault(),
                InvoiceAllowanceBuyer = new InvoiceAllowanceBuyer
                {
                    Name = _allowanceItem.Main?.Buyer?.Name,
                    ReceiptNo = _allowanceItem.Main?.Buyer?.Identifier,
                    ContactName = _allowanceItem.Main?.Buyer?.Name,
                    CustomerName = _allowanceItem.Main?.Buyer?.Name,
                    CustomerID = _allowanceItem.Main?.Buyer?.CustomerNumber
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

            _newItem.InvoiceAllowanceBuyer.Address = _allowanceItem.Main?.Buyer?.Address;
            _newItem.InvoiceAllowanceBuyer.EMail = _allowanceItem.Main?.Buyer?.EmailAddress;
            _newItem.InvoiceAllowanceBuyer.ContactName = _allowanceItem.Main?.Buyer?.Name;
            _newItem.InvoiceAllowanceBuyer.Phone = _allowanceItem.Main?.Buyer?.TelephoneNumber;

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
