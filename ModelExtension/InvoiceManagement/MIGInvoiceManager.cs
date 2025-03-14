using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using CommonLib.DataAccess;

using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement.zhTW;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using CommonLib.Utility;
using System.Globalization;
using ModelCore.InvoiceManagement.Validator;
using ModelCore.InvoiceManagement.InvoiceProcess;
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement
{
    public class MIGInvoiceManager : InvoiceManagerV2
    {
        public MIGInvoiceManager() : base() { }
        public MIGInvoiceManager(GenericManager<EIVOEntityDataContext> mgr) : base(mgr) { }

        public virtual Exception SaveUploadInvoice(ModelCore.Schema.TurnKey.C0401.Invoice invItem, OrganizationToken owner)
        {

            if (invItem != null)
            {
                List<InvoiceItem> eventItem = new List<InvoiceItem>();
                C0401Validator validator = new C0401Validator(this, owner.Organization);

                try
                {

                    Exception ex;
                    if ((ex = validator.Validate(invItem)) != null)
                    {
                        return ex;
                    }

                    if (_checkUploadInvoice != null)
                    {
                        ex = _checkUploadInvoice();
                        if (ex != null)
                        {
                            return ex;
                        }
                    }

                    InvoiceItem newItem = validator.InvoiceItem;
                    this.EntityList.InsertOnSubmit(newItem);

                    C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
                    C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已開立);

                    this.SubmitChanges();

                    eventItem.Add(newItem);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                if (eventItem.Count > 0)
                {
                    HasItem = true;
                }

            }
            return null;
        }

        public virtual Exception SaveUploadAllowance(ModelCore.Schema.TurnKey.D0401.Allowance allowanceItem, OrganizationToken owner)
        {
            if (allowanceItem != null)
            {
                this.EventItems_Allowance = null;
                List<InvoiceAllowance> eventItems = new List<InvoiceAllowance>();

                D0401Validator validator = new D0401Validator(this, owner.Organization);

                try
                {

                    Exception ex;
                    if ((ex = validator.Validate(allowanceItem)) != null)
                    {
                        return ex;
                    }

                    InvoiceAllowance newItem = validator.Allowance;
                    this.GetTable<InvoiceAllowance>().InsertOnSubmit(newItem);

                    D0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, validator.Seller.StepReadyToAllowanceMIG());
                    D0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);

                    this.SubmitChanges();

                    eventItems.Add(newItem);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                if (eventItems.Count > 0)
                {
                    HasItem = true;
                }

                EventItems_Allowance = eventItems;

            }
            return null;
        }

        public virtual Exception SaveUploadInvoiceCancellation(ModelCore.Schema.TurnKey.C0501.CancelInvoice item, OrganizationToken owner)
        {
            EventItems = null;
            if (item != null)
            {
                List<InvoiceItem> eventItems = new List<InvoiceItem>();
                var invItem = item;
                try
                {
                    Exception ex;
                    if ((ex = invItem.CheckMandatoryFields(this, owner, out InvoiceItem invoice, out DateTime cancelDate)) != null)
                    {
                        return ex;
                    }

                    InvoiceCancellation cancelItem = new InvoiceCancellation
                    {
                        InvoiceItem = invoice,
                        CancellationNo = invItem.CancelInvoiceNumber,
                        Remark = invItem.Remark,
                        ReturnTaxDocumentNo = invItem.ReturnTaxDocumentNumber,
                        CancelDate = cancelDate,
                        CancelReason = invItem.CancelReason
                    };

                    var doc = new DerivedDocument
                    {
                        CDS_Document = new CDS_Document
                        {
                            DocType = (int)Naming.DocumentTypeDefinition.E_InvoiceCancellation,
                            DocDate = DateTime.Now,
                            DocumentOwner = new DocumentOwner
                            {
                                OwnerID = owner.CompanyID
                            }
                        },
                        SourceID = invoice.InvoiceID
                    };

                    this.GetTable<DerivedDocument>().InsertOnSubmit(doc);
                    C0501Handler.PushStepQueueOnSubmit(this, doc.CDS_Document, Naming.InvoiceStepDefinition.已開立);
                    C0501Handler.PushStepQueueOnSubmit(this, doc.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
                    this.SubmitChanges();

                    eventItems.Add(cancelItem.InvoiceItem);

                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                if (eventItems.Count > 0)
                {
                    HasItem = true;
                }

                EventItems = eventItems;

            }

            return null;
        }

        public virtual Exception SaveUploadAllowanceCancellation(ModelCore.Schema.TurnKey.D0501.CancelAllowance item, OrganizationToken owner)
        {
            this.EventItems_Allowance = null;
            if (item != null)
            {
                List<InvoiceAllowance> eventItems = new List<InvoiceAllowance>();
                try
                {
                    Exception ex;
                    if ((ex = item.CheckMandatoryFields(this, owner, out InvoiceAllowance allowance, out DateTime cancelDate)) != null)
                    {
                        return ex;
                    }

                    InvoiceAllowanceCancellation cancelItem = new InvoiceAllowanceCancellation
                    {
                        InvoiceAllowance         = allowance,
                        Remark = item.Remark,
                        CancelDate = cancelDate,
                        CancelReason = item.CancelReason
                    };

                    var doc = new DerivedDocument
                    {
                        CDS_Document = new CDS_Document
                        {
                            DocType = (int)Naming.DocumentTypeDefinition.E_AllowanceCancellation,
                            DocDate = DateTime.Now,
                            DocumentOwner = new DocumentOwner
                            {
                                OwnerID = owner.CompanyID
                            }
                        },
                        SourceID = allowance.AllowanceID
                    };

                    this.GetTable<DerivedDocument>().InsertOnSubmit(doc);
                    D0501Handler.PushStepQueueOnSubmit(this, doc.CDS_Document, Naming.InvoiceStepDefinition.已開立);
                    D0501Handler.PushStepQueueOnSubmit(this, doc.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
                    this.SubmitChanges();

                    eventItems.Add(cancelItem.InvoiceAllowance);

                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                if (eventItems.Count > 0)
                {
                    HasItem = true;
                }

                EventItems_Allowance = eventItems;

            }

            return null;
        }

    }
}
