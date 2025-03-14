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
    public class VoidAllowanceDataSetManager : InvoiceManagerV3
    {
        public VoidAllowanceDataSetManager() : base() { }
        public VoidAllowanceDataSetManager(GenericManager<EIVOEntityDataContext> mgr) : base(mgr) { }
        
        public enum ResultField
        {
            AllowanceNo = 0,
            SellerID,
            StatusCode,
            Description,
            InvoiceNo,
        }
        public DataTable InitializeVoidAllowanceResponseTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("Allowance No", typeof(String)));
            table.Columns.Add(new DataColumn("Seller ID", typeof(String)));
            table.Columns.Add(new DataColumn("Status Code", typeof(int)));
            table.Columns.Add(new DataColumn("Description", typeof(String)));
            table.Columns.Add(new DataColumn("Invoice No", typeof(String)));
            table.TableName = "Process Result";
            return table;
        }
        private void ReportError(DataTable result, Exception ex)
        {
            DataRow row = result.NewRow();
            row[(int)ResultField.Description] = ex.Message;
            row[(int)ResultField.StatusCode] = 0;

            result.Rows.Add(row);
            HasError = true;
        }

        private void ReportSuccess(DataTable result, InvoiceAllowanceCancellation target)
        {
            InvoiceAllowance allowance = target.InvoiceAllowance;
            DataRow row = result.NewRow();
            row[(int)ResultField.SellerID] = allowance.InvoiceAllowanceSeller.ReceiptNo;
            row[(int)ResultField.AllowanceNo] = allowance.AllowanceNumber;
            row[(int)ResultField.InvoiceNo] = String.Join(",", allowance.InvoiceAllowanceDetails.Select(d => d.InvoiceAllowanceItem.InvoiceNo));
            row[(int)ResultField.StatusCode] = 1;
            result.Rows.Add(row);
        }

        public DataTable SaveUploadAllowanceCancellation(DataSet item, ProcessRequest request)
        {
            Organization owner = request.Organization;

            DataTable result = InitializeVoidAllowanceResponseTable();
            IEnumerable<DataRow> items = item.Tables[0].Rows.Cast<DataRow>();

            if (items.Count()>0)
            {
                EventItems = null;
                EventItems_Allowance = null;
                Organization expectedSeller = null;
                List<InvoiceAllowance> eventItems = new List<InvoiceAllowance>();
                int invSeq = 0;
                for (int idx = 0; idx < items.Count(); idx++, invSeq++)
                {
                    var row = items.ElementAt(idx);
                    try
                    {
                        Exception ex;
                        InvoiceAllowanceCancellation voidItem = null;
                        DerivedDocument p = null;

                        if ((ex = row.VoidAllowance(this, owner, ref voidItem, ref p, ref expectedSeller)) != null)
                        {
                            ReportError(result, ex);
                            continue;
                        }

                        eventItems.Add(voidItem.InvoiceAllowance);
                        ReportSuccess(result, voidItem);

                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        ReportError(result, ex);
                    }
                }

                if (eventItems.Count > 0)
                {
                    HasItem = true;
                }

                EventItems_Allowance = eventItems;

                if (this.HasError == true)
                {
                    this.PushProcessExceptionNotification(request, expectedSeller ?? owner);
                }
            }
            return result;
        }

    }
}
