
using ModelCore.DataEntity;
using ModelCore.InvoiceManagement.enUS;
using ModelCore.InvoiceManagement.ErrorHandle;
using ModelCore.InvoiceManagement.InvoiceProcess;
using ModelCore.InvoiceManagement.Validator;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using ModelCore.Schema.EIVO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Utility;
using CommonLib.DataAccess;
using ModelCore.Helper;
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement
{
    public class InvoiceManagerForCBE : InvoiceManagerV3
    {
        public InvoiceManagerForCBE() : base() { }
        public InvoiceManagerForCBE(GenericManager<EIVOEntityDataContext> mgr) : base(mgr) { }

        public override Dictionary<int, Exception> SaveUploadInvoice(InvoiceRoot item, OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();

            if (item != null && item.Invoice != null && item.Invoice.Length > 0)
            {
                List<InvoiceItem> eventItems = new List<InvoiceItem>();
                InvoiceRootInvoiceValidatorForCBE validator = new InvoiceRootInvoiceValidatorForCBE(this, owner.Organization);

                //Organization donatory = owner.Organization.InvoiceWelfareAgencies.Select(w => w.WelfareAgency.Organization).FirstOrDefault();

                for (int idx = 0; idx < item.Invoice.Length; idx++)
                {
                    try
                    {
                        var invItem = item.Invoice[idx];

                        InvoiceItem newItem = validator.SaveRootInvoice(invItem, out Exception ex);

                        if (!validator.DuplicateProcess)
                        {
                            this.EntityList.InsertOnSubmit(newItem);
                            C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
                            C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已開立);

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

                bool countInfo = item.Invoice.Length > 1000;
                if (countInfo)
                {
                    Console.WriteLine($"Large file process:{item.Invoice.Length}");
                }

                int count = 0, dbCheckCount = 180;
                InvoiceManager workingMgr = new InvoiceManager();
                InvoiceRootInvoiceValidatorForCBE validator = new InvoiceRootInvoiceValidatorForCBE(workingMgr, owner.Organization);
                validator.StartAutoTrackNo(ApplyInvoiceDate);
                for (int idx = 0; idx < item.Invoice.Length; idx++, count++)
                {
                    if (count == dbCheckCount)
                    {
                        count = 0;
                        validator.EndAutoTrackNo();
                        workingMgr.Dispose();
                        workingMgr = new InvoiceManager();
                        validator = new InvoiceRootInvoiceValidatorForCBE(workingMgr, owner.Organization);
                        validator.StartAutoTrackNo(ApplyInvoiceDate);
                    }

                    try
                    {
                        var invItem = item.Invoice[idx];

                        InvoiceItem newItem = validator.SaveRootInvoice(invItem, out Exception ex);
                        if (countInfo)
                        {
                            Console.Write("+");
                        }

                        if (ex != null)
                        {
                            if (IgnoreDuplicateDataNumberException && (ex is DuplicateDataNumberException))
                            {
                                newItem = ((DuplicateDataNumberException)ex).CurrentPO.InvoiceItem;
                                if (newItem == null)
                                {
                                    result.Add(idx, ex);
                                    continue;
                                }
                            }
                            else
                            {
                                result.Add(idx, ex);
                                continue;
                            }
                        }
                        var p = newItem.InvoicePurchaseOrder;
                        var c = newItem.InvoiceCarrier;
                        var s = newItem.InvoiceSeller;
                        eventItems.Add(newItem);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        result.Add(idx, ex);
                    }
                }
                validator.EndAutoTrackNo();
                workingMgr.Dispose();

                if (eventItems.Count > 0)
                {
                    HasItem = true;
                }

                EventItems = eventItems;
            }

            return result;

        }

        protected ProcessRequest _fromRequest;
        public ProcessRequest FromRequest 
        { 
            get => _fromRequest; 
            set
            {
                _fromRequest = value;
                if (value?.ViewModel != null)
                {
                    InvoiceRequestViewModel viewModel = JsonConvert.DeserializeObject<InvoiceRequestViewModel>(value.ViewModel);
                    ApplyInvoiceDate = viewModel.ApplyInvoiceDate;
                }
            }
        }
    }
}
