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
using System.Data;
using CommonLib.Utility;
using ModelCore.Helper;
using CommonLib.DataAccess;

namespace ModelCore.InvoiceManagement.Validator
{
    public partial class FullAllowanceDataSetValidator : AllowanceDataSetValidator
    {
        public FullAllowanceDataSetValidator(GenericManager<EIVOEntityDataContext> mgr, Organization owner) : base(mgr, owner)
        {
            AllowanceField.DataNo = 0;
            AllowanceField.Seller_ID = 1;
            AllowanceField.Allowance_No = 2;
        }

        public override Exception Validate(DataRow dataItem, IEnumerable<DataRow> details = null)
        {
            _allowanceItem = dataItem;

            Exception ex;

            _seller = null;
            _newItem = null;

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

        String _dataNo;
        protected override Exception CheckMandatoryFields()
        {
            _allowanceDate = DateTime.Today;

            _dataNo = _allowanceItem.GetString(AllowanceField.DataNo).GetEfficientString();
            if (_dataNo == null)
            {
                return new Exception("Empty data number.");
            }

            _allowanceNo = _allowanceItem.GetString(AllowanceField.Allowance_No).GetEfficientString();
            if (_allowanceNo == null)
            {
                _allowanceNo = _dataNo.Length > 64 ? _dataNo.Substring(0, 64) : _dataNo;
            }
            else if (_allowanceNo.Length > 64)
            {
                return new Exception(String.Format(MessageResources.AlertAllowanceNoLength, _allowanceNo));
            }

            var currentItem = models.GetTable<InvoiceAllowance>().Where(a => a.AllowanceNumber == _allowanceNo)
                        .Join(models.GetTable<InvoiceAllowanceSeller>().Where(s => s.SellerID == _seller.CompanyID),
                            a => a.AllowanceID, s => s.AllowanceID, (a, s) => a).FirstOrDefault();
            if (currentItem != null)
            {
                return new DuplicateAllowanceNumberException(String.Format(MessageResources.AlertAllowanceDuplicated, _allowanceNo)) { CurrentAllowance = currentItem };
            }

            return null;
        }

        protected override Exception CheckAllowanceItem()
        {
            InvoiceAllowanceBuyer allowanceBuyer = null;

            byte allowanceType = (byte)Naming.AllowanceTypeDefinition.賣方開立;

            _productItems = new List<InvoiceAllowanceItem>();
            var invTable = models.GetTable<InvoiceItem>().Where(i => i.SellerID == _seller.CompanyID);

            InvoiceItem originalInvoice = null;
            String invoiceNo = _dataNo;
            if (invoiceNo != null && invoiceNo.Length == 10)
            {
                String invNo, trackCode;
                trackCode = invoiceNo.Substring(0, 2);
                invNo = invoiceNo.Substring(2);
                originalInvoice = invTable
                    .Where(n => n.SellerID == _seller.CompanyID)
                    .Where(n => n.TrackCode == trackCode)
                    .Where(n => n.No == invNo)
                    .FirstOrDefault();
            }

            if (originalInvoice == null)
            {
                originalInvoice = invTable.Join(models.GetTable<InvoicePurchaseOrder>().Where(p => p.OrderNo == invoiceNo),
                        v => v.InvoiceID, p => p.InvoiceID, (v, p) => v)
                    .FirstOrDefault();
            }

            if (originalInvoice == null)
            {
                return new Exception(String.Format(MessageResources.InvalidAllowance_NoInvoiceData, _dataNo));
            }

            if (originalInvoice.InvoiceCancellation != null)
            {
                return new Exception(MessageResources.InvalidAllowance_InvoiceHasBeenCanceled);
            }

            if (originalInvoice.InvoiceSeller.ReceiptNo != _seller.ReceiptNo)
            {
                return new Exception(String.Format(MessageResources.AlertAllowance_InvoiceSellerIsDifferent, _dataNo));
            }

            var productItems = originalInvoice.GetInvoiceProductItem(models);

            InvoiceAllowanceItem allowanceItem = new InvoiceAllowanceItem
            {
                InvoiceNo = $"{originalInvoice.TrackCode}{originalInvoice.No}",
                InvoiceDate = originalInvoice.InvoiceDate,
                Piece = 1,
                OriginalDescription = "原發票銷貨全數折退",
                TaxType = originalInvoice.InvoiceAmountType.TaxType,
                No = 1,
                UnitCost = originalInvoice.InvoiceAmountType.SalesAmount,

            };

            if (originalInvoice.InvoiceAmountType.TaxAmount > 0)
            {
                allowanceItem.Amount = originalInvoice.InvoiceAmountType.SalesAmount;
                allowanceItem.Tax = originalInvoice.InvoiceAmountType.TaxAmount;
            }
            else
            {
                allowanceItem.Amount = ((originalInvoice.InvoiceAmountType.TotalAmount ?? 0) / 1.05M).ToFix(originalInvoice.InvoiceAmountType.CurrencyType?.Decimals ?? 0);
                allowanceItem.Tax = originalInvoice.InvoiceAmountType.TotalAmount - allowanceItem.Amount;
            }

            if (productItems.Count() == 1)
            {
                var productItem = productItems.First();

                allowanceItem.OriginalSequenceNo = 1;
                allowanceItem.Piece = productItem.Piece;
                allowanceItem.OriginalDescription = productItem.InvoiceProduct.Brief;
                allowanceItem.TaxType = productItem.TaxType ?? originalInvoice.InvoiceAmountType.TaxType;
                allowanceItem.UnitCost = productItem.UnitCost;
            }

            allowanceBuyer = new InvoiceAllowanceBuyer
            {
                Name = originalInvoice.InvoiceBuyer.Name,
                ReceiptNo = originalInvoice.InvoiceBuyer.ReceiptNo,
                CustomerID = originalInvoice.InvoiceBuyer.CustomerID,
                ContactName = originalInvoice.InvoiceBuyer.ContactName,
                CustomerName = originalInvoice.InvoiceBuyer.CustomerName,
                EMail = originalInvoice.InvoiceBuyer.EMail,
                Address = originalInvoice.InvoiceBuyer.Address,
                Phone = originalInvoice.InvoiceBuyer.Phone,
            };

            _productItems.Add(allowanceItem);

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
                AllowanceNumber = _allowanceNo,
                AllowanceType = allowanceType,
                BuyerId = allowanceBuyer.ReceiptNo,
                SellerId = _seller.ReceiptNo,
                TaxAmount = allowanceItem.Tax,
                TotalAmount = allowanceItem.Amount,
                CurrencyID = originalInvoice.InvoiceAmountType.CurrencyID,
                //InvoiceID =  invTable.Where(i=>i.TrackCode + i.No == item.AllowanceItem.Select(a=>a.GetString(DetailsField.Original_Invoice_No)).FirstOrDefault()).Select(i=>i.InvoiceID).FirstOrDefault(),
                InvoiceAllowanceBuyer = allowanceBuyer,
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
