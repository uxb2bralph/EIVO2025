using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ModelCore.DataEntity;
using ModelCore.InvoiceManagement.InvoiceProcess;
using ModelCore.Locale;
using ModelCore.Resource;
using ModelCore.Schema.EIVO;
using CommonLib.Utility;
using CommonLib.DataAccess;

namespace ModelCore.InvoiceManagement.Validator
{
    public class InvoiceRootInvoiceValidatorForCBE : InvoiceRootInvoiceValidator
    {
        protected bool _pdfSubscription;
        public InvoiceRootInvoiceValidatorForCBE(GenericManager<EIVOEntityDataContext> mgr, Organization owner)
            : base(mgr, owner)
        {
            UseDefaultCrossBorderMerchantCarrier = true;
        }

        protected override Exception checkBusiness()
        {
            var result = base.checkBusiness();
            if(result!=null)
            {
                return result;
            }

            if (_isCrossBorderMerchant == true)
            {
                _pdfSubscription = _seller.OrganizationStatus.SubscribeB2BInvoicePDF == true;
            }

            return null;
        }

        protected override Exception checkMandatoryFields()
        {
            _invItem.DonateMark = "0";
            _invItem.PrintMark = "N";
            _invItem.InvoiceType = ((byte)Naming.InvoiceTypeDefinition.一般稅額計算之電子發票).ToString();

            return null;
        }

        protected override Exception checkAmount()
        {
            //應稅銷售額
            if (_invItem.SalesAmount < 0 /*|| decimal.Floor(_invItem.SalesAmount) != _invItem.SalesAmount*/)
            {
                return new Exception(String.Format(MessageResources.InvalidSellingPrice, _invItem.SalesAmount));
            }

            if (_invItem.FreeTaxSalesAmount < 0 /*|| decimal.Floor(_invItem.FreeTaxSalesAmount) != _invItem.FreeTaxSalesAmount*/)
            {
                return new Exception(String.Format(MessageResources.InvalidFreeTaxAmount, _invItem.FreeTaxSalesAmount));
            }

            if (_invItem.ZeroTaxSalesAmount < 0 /*|| decimal.Floor(_invItem.ZeroTaxSalesAmount) != _invItem.ZeroTaxSalesAmount*/)
            {
                return new Exception(String.Format(MessageResources.InvalidZeroTaxAmount, _invItem.ZeroTaxSalesAmount));
            }


            if (_invItem.TaxAmount < 0 /*|| decimal.Floor(_invItem.TaxAmount) != _invItem.TaxAmount*/)
            {
                return new Exception(String.Format(MessageResources.InvalidTaxAmount, _invItem.TaxAmount));
            }

            if (_invItem.TotalAmount < 0 /*|| decimal.Floor(_invItem.TotalAmount) != _invItem.TotalAmount*/)
            {
                return new Exception(String.Format(MessageResources.InvalidTotalAmount, _invItem.TotalAmount));
            }

            //課稅別
            _invItem.TaxType = (byte)Naming.TaxTypeDefinition.應稅;
            _invItem.TaxRate = 0.05m;

            _invItem.Currency = _invItem.Currency.GetEfficientString();
            _currency = null;
            if (_invItem.Currency!=null)
            {
                _currency = _models.GetTable<CurrencyType>().Where(c => c.AbbrevName == _invItem.Currency).FirstOrDefault();
                if (_currency == null)
                {
                    return new Exception($"Invalid currency code：{_invItem.Currency}，TAG：<Currency/>");
                }
            }

            return null;
        }



        public InvoiceItem SaveRootInvoice(InvoiceRootInvoice invItem, out Exception exception)
        {
            if ((exception = this.Validate(invItem)) != null)
            {
                return null;
            }

            InvoiceItem newItem = this.InvoiceItem;
            _models.GetTable<InvoiceItem>().InsertOnSubmit(newItem);

            F0401Handler.PushStepQueueOnSubmit(_models, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
            F0401Handler.PushStepQueueOnSubmit(_models, newItem.CDS_Document, Naming.InvoiceStepDefinition.已開立);

            _models.SubmitChanges();

            if(_pdfSubscription)
            {
                _models.PushDocumentSubscriptionQueue(newItem.CDS_Document.DocID);
            }

            return newItem;
        }
    }
}
