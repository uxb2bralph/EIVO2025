using ModelCore.DataEntity;
using ModelCore.InvoiceManagement.enUS;
using ModelCore.InvoiceManagement.ErrorHandle;
using ModelCore.InvoiceManagement.InvoiceProcess;
using ModelCore.InvoiceManagement.Validator;
using ModelCore.Locale;
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
    public class AllowanceDataSetManager : InvoiceManagerV3
    {
        public AllowanceDataSetManager() : base() { }
        public AllowanceDataSetManager(GenericManager<EIVOEntityDataContext> mgr) : base(mgr) { }

        public enum ResultField
        {
            AllowanceNo = 0,
            StatusCode,
            Description,
            InvoiceNo,
            DataNo,
        }
        public virtual DataTable InitializeAllowanceResponseTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("Allowance No", typeof(String)));
            table.Columns.Add(new DataColumn("Status Code", typeof(int)));
            table.Columns.Add(new DataColumn("Description", typeof(String)));
            table.Columns.Add(new DataColumn("Invoice No", typeof(String)));
            table.TableName = "Process Result";
            return table;
        }

        public virtual DataTable SaveUploadAllowance(DataSet item, ProcessRequest request)
        {
            Organization owner = request.Organization;

            AllowanceDataSetValidator validator = new AllowanceDataSetValidator(this, owner);
            DataTable result = InitializeAllowanceResponseTable();

            IEnumerable<DataRow> allowanceItems = item.Tables["Allowance"].Rows.Cast<DataRow>();
            IEnumerable<DataRow> details = item.Tables["Details"].Rows.Cast<DataRow>();
            String dataID = "";
            foreach (DataRow row in details)
            {
                if (row.IsNull(validator.DetailsField.Allowance_No) || String.IsNullOrEmpty(row.GetString(validator.DetailsField.Allowance_No)))
                {
                    row[validator.DetailsField.Allowance_No] = dataID;
                }
                else
                {
                    dataID = row.GetString(validator.DetailsField.Allowance_No);
                }
            }

            if (allowanceItems.Count()>0)
            {
                this.EventItems_Allowance = null;
                List<InvoiceAllowance> eventItems = new List<InvoiceAllowance>();

                var table = this.GetTable<InvoiceAllowance>();

                for (int idx = 0; idx < allowanceItems.Count(); idx++)
                {
                    try
                    {
                        var allowanceItem = allowanceItems.ElementAt(idx);
                        dataID = allowanceItem.GetString(validator.AllowanceField.Allowance_No);
                        IEnumerable<DataRow> invoiceDetails = details.Where(d => d.GetString(validator.DetailsField.Allowance_No) == dataID);

                        Exception ex;
                        if ((ex = validator.Validate(allowanceItem,invoiceDetails)) != null)
                        {
                            ReportError(result, allowanceItem, ex, validator);
                            continue;
                        }

                        InvoiceAllowance? newItem = validator.Allowance;

                        table.InsertOnSubmit(newItem!);
                        if (newItem!.CDS_Document.ProcessType == (int)Naming.InvoiceProcessType.G0401)
                        {
                            newItem.CDS_Document.PushStepQueueOnSubmit(this, validator.Seller!.StepReadyToAllowanceMIG(), Naming.InvoiceProcessType.G0401);
                            newItem.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.已接收資料待通知, Naming.InvoiceProcessType.G0401);
                        }
                        else
                        {
                            newItem.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.B0101);
                        }

                        this.SubmitChanges();

                        eventItems.Add(newItem);
                        ReportSuccess(result, newItem);

                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        ReportError(result, null, ex, null);
                    }
                }

                if (eventItems.Count > 0)
                {
                    HasItem = true;
                }

                EventItems_Allowance = eventItems;

                if (this.HasError == true)
                {
                    this.PushProcessExceptionNotification(request, validator.ExpectedSeller ?? owner);
                }
            }

            return result;
        }


        protected virtual void ReportError(DataTable result, DataRow source, Exception ex, AllowanceDataSetValidator validator)
        {
            DataRow row = result.NewRow();
            row[(int)ResultField.Description] = ex.Message;
            row[(int)ResultField.StatusCode] = 0;
            if (source != null)
            {
                row[(int)ResultField.AllowanceNo] = source[validator.AllowanceField.Allowance_No];
            }
            result.Rows.Add(row);
            HasError = true;
        }

        protected void ReportSuccess(DataTable result, InvoiceAllowance target)
        {
            DataRow row = result.NewRow();
            row[(int)ResultField.AllowanceNo] = target.AllowanceNumber;
            row[(int)ResultField.InvoiceNo] = String.Join(",", target.InvoiceAllowanceDetails.Select(d => d.InvoiceAllowanceItem.InvoiceNo));
            row[(int)ResultField.StatusCode] = 1;
            result.Rows.Add(row);
        }
                

    }
}
