using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;



using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement.ErrorHandle;
using ModelCore.InvoiceManagement.InvoiceProcess;
using ModelCore.InvoiceManagement.Validator;
using ModelCore.InvoiceManagement.zhTW;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using CommonLib.Utility;
using CommonLib.DataAccess;
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement
{
    public class InvoiceManagerV3 : InvoiceManagerV2
    {
        public InvoiceManagerV3() : base() { }
        public InvoiceManagerV3(GenericManager<EIVOEntityDataContext> mgr) : base(mgr) { }
        public bool HasError { get; set; }
        public override Dictionary<int, Exception> SaveUploadInvoice(InvoiceRoot item, OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();

            if (item != null && item.Invoice != null && item.Invoice.Length > 0)
            {
                List<InvoiceItem> eventItems = new List<InvoiceItem>();
                InvoiceRootFormatValidator formatValidator = new InvoiceRootFormatValidator(this, owner.Organization);
                InvoiceRootInvoiceValidator validator = new InvoiceRootInvoiceValidator(this, owner.Organization)
                {
                    ProcessType = this.ProcessType,
                };

                //Organization donatory = owner.Organization.InvoiceWelfareAgencies.Select(w => w.WelfareAgency.Organization).FirstOrDefault();

                for (int idx = 0; idx < item.Invoice.Length; idx++)
                {
                    try
                    {
                        var invItem = item.Invoice[idx];

                        Exception ex;
                        if ((ex = validator.Validate(invItem)) != null)
                        {

                            var errors = formatValidator.ValidateAll(invItem);
                            if (errors.Count > 0)
                            {
                                result.Add(idx, new Exception(ex.Message + ";\r\n" + String.Join(";\r\n", errors.Where(x => x.Message != ex.Message)
                                    .Select(x => x.Message))));
                            }
                            else
                            {
                                result.Add(idx, ex);
                            }
                            continue;

                        }

                        if (_checkUploadInvoice != null)
                        {
                            ex = _checkUploadInvoice();
                            if (ex != null)
                            {
                                result.Add(idx, ex);
                                continue;
                            }
                        }

                        InvoiceItem newItem = validator.InvoiceItem;

                        if (!validator.DuplicateProcess)
                        {
                            this.EntityList.InsertOnSubmit(newItem);

                            if (this.ProcessType == Naming.InvoiceProcessType.A0401)
                            {
                                A0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已開立);
                                switch ((Naming.NotificationIndication?)item.Notification)
                                {
                                    case Naming.NotificationIndication.None:
                                        break;
                                    case Naming.NotificationIndication.Deferred:
                                        A0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.文件準備中);
                                        break;
                                    default:
                                        A0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
                                        break;
                                }
                            }
                            else
                            {
                                C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已開立);
                                switch ((Naming.NotificationIndication?)item.Notification)
                                {
                                    case Naming.NotificationIndication.None:
                                        break;
                                    case Naming.NotificationIndication.Deferred:
                                        C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.文件準備中);
                                        break;
                                    default:
                                        C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
                                        break;
                                }
                            }

                            this.SubmitChanges();
                        }

                        eventItems.Add(newItem);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        result.Add(idx, ex);
                    }
                }

                if (eventItems.Count > 0)
                {
                    HasItem = true;
                }

                EventItems = eventItems;

            }
            return result;
        }

        public override Dictionary<int, Exception> SaveUploadInvoiceAutoTrackNo(InvoiceRoot item, OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();

            if (item != null && item.Invoice != null && item.Invoice.Length > 0)
            {
                //Organization donatory = owner.Organization.InvoiceWelfareAgencies.Select(w => w.WelfareAgency.Organization).FirstOrDefault();
                EventItems = null;
                List<InvoiceItem> eventItems = new List<InvoiceItem>();

                InvoiceRootFormatValidator formatValidator = new InvoiceRootFormatValidator(this, owner.Organization);
                InvoiceRootInvoiceValidator validator = new InvoiceRootInvoiceValidator(this, owner.Organization)
                {
                    ProcessType = this.ProcessType,
                };
                int invSeq = 0;
                void proc(InvoiceRootInvoice[] items, Naming.InvoiceTypeDefinition indication)
                {
                    validator.InvoiceTypeIndication = indication;
                    validator.StartAutoTrackNo(ApplyInvoiceDate);
                    for (int idx = 0; idx < items.Length; idx++, invSeq++)
                    {
                        try
                        {
                            var invItem = items[idx];

                            Exception ex;
                            if ((ex = validator.Validate(invItem)) != null)
                            {
                                if (IgnoreDuplicateDataNumberException && (ex is DuplicateDataNumberException))
                                {
                                    var testItem = ((DuplicateDataNumberException)ex).CurrentPO.InvoiceItem;
                                    if (testItem != null)
                                    {
                                        eventItems.Add(testItem);
                                        continue;
                                    }
                                }

                                var errors = formatValidator.ValidateAll(invItem);
                                if (errors.Count > 0)
                                {
                                    result.Add(invSeq, new Exception(ex.Message + ";\r\n" + String.Join(";\r\n", errors.Where(x => x.Message != ex.Message)
                                        .Select(x => x.Message))));
                                }
                                else
                                {
                                    result.Add(invSeq, ex);
                                }
                                continue;
                            }

                            InvoiceItem newItem = validator.InvoiceItem;

                            if (this.ProcessType == Naming.InvoiceProcessType.A0401)
                            {
                                A0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已開立);
                                switch ((Naming.NotificationIndication?)item.Notification)
                                {
                                    case Naming.NotificationIndication.None:
                                        break;
                                    case Naming.NotificationIndication.Deferred:
                                        A0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.文件準備中);
                                        break;
                                    default:
                                        A0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
                                        break;
                                }
                            }
                            else
                            {
                                C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已開立);
                                switch ((Naming.NotificationIndication?)item.Notification)
                                {
                                    case Naming.NotificationIndication.None:
                                        break;
                                    case Naming.NotificationIndication.Deferred:
                                        C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.文件準備中);
                                        break;
                                    default:
                                        C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
                                        break;
                                }
                            }

                            this.EntityList.InsertOnSubmit(newItem);
                            this.SubmitChanges();

                            eventItems.Add(newItem);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            result.Add(invSeq, ex);
                        }
                    }
                    validator.EndAutoTrackNo();
                }

                var invoiceData = item.Invoice.Where(t => t.InvoiceType != null && t.InvoiceType.Contains("8")).ToArray();
                if (invoiceData != null && invoiceData.Length > 0)
                {
                    proc(invoiceData, Naming.InvoiceTypeDefinition.特種稅額計算之電子發票);
                    var tmp = invoiceData;
                    invoiceData = item.Invoice.Except(tmp).ToArray();
                }
                else
                {
                    invoiceData = item.Invoice;
                }

                proc(invoiceData, Naming.InvoiceTypeDefinition.一般稅額計算之電子發票);

                if (eventItems.Count > 0)
                {
                    HasItem = true;
                }
                EventItems = eventItems;
            }

            return result;
        }

        public override Dictionary<int, Exception> SaveUploadAllowance(AllowanceRoot item, OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();

            if (item != null && item.Allowance != null && item.Allowance.Length > 0)
            {
                this.EventItems_Allowance = null;
                List<InvoiceAllowance> eventItems = new List<InvoiceAllowance>();

                AllowanceRootAllowanceValidator validator = new AllowanceRootAllowanceValidator(this, owner.Organization);
                var table = this.GetTable<InvoiceAllowance>();

                for (int idx = 0; idx < item.Allowance.Length; idx++)
                {
                    try
                    {
                        var allowanceItem = item.Allowance[idx];

                        Exception ex;
                        if ((ex = validator.Validate(allowanceItem)) != null)
                        {
                            if (ex is DuplicateAllowanceNumberException)
                            {
                                eventItems.Add(((DuplicateAllowanceNumberException)ex).CurrentAllowance);
                                continue;
                            }
                            else
                            {
                                result.Add(idx, ex);
                                continue;
                            }
                        }

                        InvoiceAllowance newItem = validator.Allowance;

                        table.InsertOnSubmit(newItem);
                        if (newItem.CDS_Document.ProcessType == (int)Naming.InvoiceProcessType.B0401)
                        {
                            B0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, validator.Seller.StepReadyToAllowanceMIG());
                            B0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
                        }
                        else
                        {
                            D0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, validator.Seller.StepReadyToAllowanceMIG());
                            D0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
                        }

                        this.SubmitChanges();

                        eventItems.Add(newItem);

                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        result.Add(idx, ex);
                    }
                }

                if (eventItems.Count > 0)
                {
                    HasItem = true;
                }

                EventItems_Allowance = eventItems;
            }

            return result;
        }

        protected void PushProcessExceptionNotification(ProcessRequest requestItem, Organization notified)
        {
            if (requestItem != null && notified != null)
            {
                if (!this.GetTable<ProcessExceptionNotification>().Any(n => n.TaskID == requestItem.TaskID && n.CompanyID == notified.CompanyID))
                {
                    this.GetTable<ProcessExceptionNotification>().InsertOnSubmit(
                        new ProcessExceptionNotification
                        {
                            TaskID = requestItem.TaskID,
                            CompanyID = notified.CompanyID,
                        }
                        );
                    this.SubmitChanges();
                }
            }
        }
    }
}