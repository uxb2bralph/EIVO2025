using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonLib.DataAccess;
using ModelCore.DataEntity;
using ModelCore.InvoiceManagement.InvoiceProcess;
using ModelCore.Locale;
using ModelCore.Resource;
using ModelCore.Schema.EIVO;

namespace ModelCore.InvoiceManagement.Validator
{
    public class GoogleInvoiceRootInvoiceValidator : InvoiceRootInvoiceValidator
    {
        public GoogleInvoiceRootInvoiceValidator(GenericManager<EIVOEntityDataContext> mgr, Organization owner)
            : base(mgr, owner)
        {

        }

        protected override Exception checkBusiness()
        {
            Exception ex = base.checkBusiness();
            if (ex != null)
            {
                return ex;
            }

            if (String.IsNullOrEmpty(_invItem.GoogleId))
            {
                return new Exception(MessageResources.AlertGoogleId);
            }
            else if (_invItem.GoogleId.Length > 64)
            {
                return new Exception(String.Format("GoogleId can not be over 64 characters，TAG:< GoogleId />"));
            }

            return null;
        }

        public InvoiceItem SaveRootInvoice(InvoiceRootInvoice invItem, bool forTerms, String invoiceClientID, int? channelID, out Exception exception)
        {
            if ((exception = this.Validate(invItem)) != null)
            {
                return null;
            }

            InvoiceItem newItem = this.InvoiceItem;
            newItem.CDS_Document.DocumentOwner.ClientID = invoiceClientID;
            newItem.CDS_Document.ChannelID = channelID;
            if (forTerms)
            {
                newItem.CDS_Document.CustomerDefined = new CustomerDefined { };
            }
            //else
            //{
            //    C0401Handler.PushStepQueueOnSubmit(_mgr, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
            //}

            //yuki 儲存發票
            _models.GetTable<InvoiceItem>().InsertOnSubmit(newItem);

            //yuki 加一筆到Queue、DataProcessLog
            C0401Handler.PushStepQueueOnSubmit(_models, newItem.CDS_Document, Naming.InvoiceStepDefinition.已開立);

            //yuki 加一筆到ProcessRequestDocument
            //C0401Handler.PushProcessRequestDocumentOnSubmit(_mgr, newItem.CDS_Document, taskID);

            _models.SubmitChanges();

            if (forTerms)
            {

            }
            else
            {
                if (_isCrossBorderMerchant)
                {
                    newItem.CDS_Document.DocumentSubscriptionQueue = new DocumentSubscriptionQueue { };
                }
                else
                {
                    C0401Handler.PushStepQueueOnSubmit(_models, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
                }
                _models.SubmitChanges();
            }

            return newItem;
        }
    }
}
