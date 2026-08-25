using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement.ErrorHandle;
using ModelCore.Locale;
using ModelCore.Resource;
using ModelCore.Schema.EIVO;
using ModelCore.Models.ViewModel;
using CommonLib.Utility;
using ModelCore.InvoiceManagement.InvoiceProcess;
using CommonLib.Core.DataWork;

namespace ModelCore.InvoiceManagement.Validator
{
    public partial class InvoiceRootInvoiceValidatorForRevision : InvoiceRootInvoiceValidator
    {
        public InvoiceRootInvoiceValidatorForRevision(GenericDbContext<ApplicationDbContext> models, Organization? owner) 
            :base(models, owner)
        {

        }

        public InvoiceItem? Original { get; private set; }

        public override Exception? Validate(InvoiceRootInvoice dataItem)
        {
            _invItem = dataItem;

            Exception? ex;

            //_seller = null;
            _newItem = null;
            Original = null;
            _container = new InvoiceItem { };

            if ((ex = checkBusiness()) != null)
            {
                return ex;
            }

            appendDataNumber();

            if ((ex = checkAmount()) != null)
            {
                return ex;
            }

            if ((ex = checkInvoiceDelivery()) != null)
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

        protected override Exception? checkInvoice()
        {
            DuplicateProcess = false;
            _container.CDS_Document = new CDS_Document
            {
                DocDate = DateTime.Now,
                DocType = (int)Naming.DocumentTypeDefinition.E_Invoice,
                DocumentOwner = new DocumentOwner
                {
                    OwnerID = _owner?.CompanyID ?? _seller!.CompanyID
                },
                ProcessType = (int)(processType ?? Naming.InvoiceProcessType.C0401),
            };
            _container.DonateMark = _donation == null ? "0" : "1";
            _container.SellerID = _seller!.CompanyID;
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
                ZeroTaxRateReason = _invItem.ZeroTaxRateReason,
            };
            _container.InvoiceCarrier = _carrier;
            _container.InvoiceDonation = _donation;
            _container.PrintMark = _invItem.PrintMark;
            _container.Remark = _invItem.MainRemark;
            if (_invItem.CustomerDefined != null)
            {
                if (_container.CDS_Document.CustomerDefined == null)
                {
                    _container.CDS_Document.CustomerDefined = new CustomerDefined { };
                }
                _container.CDS_Document.CustomerDefined.DataContent = _invItem.CustomerDefined.ConvertToXml().OuterXml;
            }

            if (_order != null)
            {
                _container.InvoicePurchaseOrder = _order;
            }

            if (_orderAudit != null)
            {
                _orderAudit.Invoice = _container;
            }

            _container.Product!.AddRange(_productItems.Select(p => p.Product));

            DateTime invoiceDate = DateTime.Now;

            if (String.IsNullOrEmpty(_invItem.InvoiceDate))
            {
                return new Exception(MessageResources.AlertInvoiceDate);
            }

            _invItem.InvoiceTime = _invItem.InvoiceTime.GetEfficientString();
            if (_invItem.InvoiceTime == null)
            {
                _invItem.InvoiceTime = "12:00:00";
            }

            if (!DateTime.TryParseExact($"{_invItem.InvoiceDate} {_invItem.InvoiceTime}", __InvoiceDateTimeFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out invoiceDate)
                    || invoiceDate > DateTime.Today.AddDays(3))
            {
                return new Exception(String.Format(MessageResources.AlertInvoiceDateTime, _invItem.InvoiceDate, _invItem.InvoiceTime));
            }

            _container.InvoiceDate = invoiceDate;

            var ex = CheckInvoiceNo(null!, invoiceDate, _invItem.InvoiceNumber);
            if (ex != null)
            {
                return ex;
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

        protected override Exception? CheckInvoiceNo(InvoiceItem item, DateTime invoiceDate, String? invoiceNo)
        {
            if (invoiceNo == null || !Regex.IsMatch(invoiceNo, "^[a-zA-Z]{2}[0-9]{8}$"))
            {
                return new Exception(String.Format(MessageResources.AlertInvoiceNumber, invoiceNo));
            }

            var trackCode = invoiceNo[..2];
            var no = invoiceNo[2..];
            DateTime periodStart = new DateTime(invoiceDate.Year, (invoiceDate.Month - 1) / 2 * 2 + 1, 1);

            Original = _models.GetTable<InvoiceItem>()
                .Where(i => i.TrackCode == trackCode && i.No == no
                        && i.InvoiceDate >= periodStart && i.InvoiceDate < periodStart.AddMonths(2))
                .Where(i => i.SellerID == _seller!.CompanyID)
                .FirstOrDefault();

            if (Original == null)
            {
                return new Exception(String.Format("發票號碼錯誤:{0}，TAG:< InvoicNumber />", _invItem.InvoiceNumber));
            }

            return null;
        }
    }

}
