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
    public class FullAllowanceDataSetManager : AllowanceDataSetManager
    {
        public FullAllowanceDataSetManager() : base() { }
        public FullAllowanceDataSetManager(GenericManager<EIVOEntityDataContext> mgr) : base(mgr) { }


        public override DataTable SaveUploadAllowance(DataSet item, ProcessRequest request)
        {
            Organization owner = request.Organization;

            FullAllowanceDataSetValidator validator = new FullAllowanceDataSetValidator(this, owner);
            DataTable result = InitializeAllowanceResponseTable();

            IEnumerable<DataRow> allowanceItems = item.Tables["Allowance"].Rows.Cast<DataRow>();

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

                        Exception ex;
                        if ((ex = validator.Validate(allowanceItem)) != null)
                        {
                            ReportError(result, allowanceItem, ex, validator);
                            continue;
                        }

                        InvoiceAllowance newItem = validator.Allowance;

                        table.InsertOnSubmit(newItem);
                        if (newItem.CDS_Document.ProcessType == (int)Naming.InvoiceProcessType.D0401)
                        {
                            G0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, validator.Seller.StepReadyToAllowanceMIG());
                            G0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
                        }
                        else
                        {
                            B0101Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, validator.Seller.StepReadyToAllowanceMIG());
                            B0101Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
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

        public override DataTable InitializeAllowanceResponseTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("Allowance No", typeof(String)));
            table.Columns.Add(new DataColumn("Status Code", typeof(int)));
            table.Columns.Add(new DataColumn("Description", typeof(String)));
            table.Columns.Add(new DataColumn("Invoice No", typeof(String)));
            table.Columns.Add(new DataColumn("Data No", typeof(String)));
            table.TableName = "Process Result";
            return table;
        }

        protected override void ReportError(DataTable result, DataRow source, Exception ex, AllowanceDataSetValidator validator)
        {
            DataRow row = result.NewRow();
            row[(int)ResultField.Description] = ex.Message;
            row[(int)ResultField.StatusCode] = 0;
            if (source != null)
            {
                row[(int)ResultField.DataNo] = source[validator.AllowanceField.DataNo];
            }
            result.Rows.Add(row);
            HasError = true;
        }

    }
}
