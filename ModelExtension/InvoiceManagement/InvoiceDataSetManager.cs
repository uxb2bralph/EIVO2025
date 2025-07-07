using ModelCore.DataEntity;
using ModelCore.Helper;
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
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement
{
    public class InvoiceDataSetManager : InvoiceManagerV3
    {
        public InvoiceDataSetManager() : base() { }
        public InvoiceDataSetManager(GenericManager<EIVOEntityDataContext> mgr) : base(mgr) { }

        public enum ResultField
        {
            InvoiceNo = 0,
            DataID,
            InvoiceDate,
            StatusCode,
            Description,
            SellerID,
            EncData,
        }
        public DataTable InitializeInvoiceResponseTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("Invoice No", typeof(String)));
            table.Columns.Add(new DataColumn("Data ID", typeof(String)));
            table.Columns.Add(new DataColumn("Invoice Date", typeof(DateTime)));
            table.Columns.Add(new DataColumn("Status Code", typeof(int)));
            table.Columns.Add(new DataColumn("Description", typeof(String)));
            table.Columns.Add(new DataColumn("Seller ID", typeof(String)));
            table.Columns.Add(new DataColumn("EncData", typeof(String)));
            table.TableName = "Process Result";
            return table;
        }

        protected void ReportError(DataTable result, DataRow source, Exception ex, InvoiceDataSetValidator validator)
        {
            DataRow row = result.NewRow();
            row[(int)ResultField.Description] = ex.Message;
            row[(int)ResultField.StatusCode] = 0;
            if (source != null)
            {
                row[(int)ResultField.DataID] = source[validator.InvoiceField.Data_ID];
                row[(int)ResultField.SellerID] = source[validator.InvoiceField.Seller_ID];
            }
            result.Rows.Add(row);
            this.HasError = true;
        }

        protected void ReportSuccess(DataTable result, InvoiceItem target)
        {
            DataRow row = result.NewRow();
            row[(int)ResultField.SellerID] = target.InvoiceSeller.ReceiptNo;
            row[(int)ResultField.InvoiceNo] = target.TrackCode + target.No;
            row[(int)ResultField.DataID] = target.InvoicePurchaseOrder?.OrderNo;
            row[(int)ResultField.InvoiceDate] = target.InvoiceDate;
            row[(int)ResultField.StatusCode] = 1;
            row[(int)ResultField.EncData] = target.BuildEncryptedData();
            result.Rows.Add(row);
        }

        public DataTable SaveUploadInvoiceAutoTrackNoForCBE(DataSet item, ProcessRequest request)
        {
            Organization owner = request.Organization;
            this.ApplyInvoiceDate = request.ProcessRequestCondition.Any(c => c.ConditionID == (int)ProcessRequestCondition.ConditionType.UseLastPeriodTrackCodeNo)
                        ? new DateTime(DateTime.Today.Year, (DateTime.Today.Month + 1) / 2 * 2 - 1, 1).AddDays(-1)
                        : DateTime.Today;

            InvoiceDataSetValidator validator = new InvoiceDataSetValidator(this, owner, Naming.InvoiceProcessType.C0401_Xlsx_CBE)
            {
                UseDefaultCrossBorderMerchantCarrier = true,
            };

            DataTable result = InitializeInvoiceResponseTable();

            IEnumerable<DataRow> invoiceItems = (item.Tables["Invoice"] ?? item.Tables[0])?.Rows.Cast<DataRow>();
            IEnumerable<DataRow> details = (item.Tables["Details"] ?? item.Tables[1])?.Rows.Cast<DataRow>();

            if (invoiceItems == null || details == null)
            {
                throw new Exception("Bad Invoice Data Sheets");
            }            //String dataID = "";
            //foreach (DataRow row in details)
            //{
            //    if (row.IsNull(validator.DetailsField.Data_ID) || String.IsNullOrEmpty((String)row[validator.DetailsField.Data_ID]))
            //    {
            //        row[validator.DetailsField.Data_ID] = dataID;
            //    }
            //    else
            //    {
            //        dataID = (String)row[validator.DetailsField.Data_ID];
            //    }
            //}

            if (invoiceItems.Count() > 0)
            {
                EventItems = null;
                List<InvoiceItem> eventItems = new List<InvoiceItem>();

                int invSeq = 0;
                processAutoTrackInvoiceNo(request, invoiceItems, details, Naming.InvoiceTypeDefinition.一般稅額計算之電子發票, validator, ref invSeq, eventItems, result);

                if (eventItems.Count > 0)
                {
                    HasItem = true;
                }
                EventItems = eventItems;

                if (this.HasError == true)
                {
                    this.PushProcessExceptionNotification(request, validator.ExpectedSeller ?? owner);
                }
            }

            return result;
        }

        public DataTable SaveUploadInvoiceAutoTrackNoForVAC(DataSet item, ProcessRequest request)
        {
            return SaveUploadInvoiceAutoTrackNo(item, request, Naming.InvoiceProcessType.C0401_Xlsx_Allocation_ByVAC);
        }

        void processAutoTrackInvoiceNo(ProcessRequest request, IEnumerable<DataRow> items, IEnumerable<DataRow> itemDetails, Naming.InvoiceTypeDefinition indication, InvoiceDataSetValidator validator,ref int invSeq, List<InvoiceItem> eventItems, DataTable result)
        {
            List<DataRow> details = itemDetails.ToList();
            bool deferredNotice = request.ProcessRequestCondition.Any(c => c.ConditionID == (int)ProcessRequestCondition.ConditionType.DeferredIssueNotice);
            validator.InvoiceTypeIndication = indication;
            validator.StartAutoTrackNo(ApplyInvoiceDate);
            for (int idx = 0; idx < items.Count(); idx++, invSeq++)
            {
                Console.WriteLine($"start {invSeq} => {DateTime.Now}");

                try
                {
                    var invItem = items.ElementAt(idx);
                    String dataID = invItem.GetString(validator.InvoiceField.Data_ID).GetEfficientString();
                    IEnumerable<DataRow> productDetails;
                    if (dataID != null)
                    {
                        productDetails = details.Where(d => d.GetString(validator.DetailsField.Data_ID) == dataID)
                            .ToList();
                    }
                    else
                    {
                        dataID = invItem.GetString(validator.InvoiceField.Invoice_No).GetEfficientString();
                        productDetails = details.Where(d => d.GetString(validator.DetailsField.Invoice_No) == dataID)
                            .ToList();
                    }

                    foreach(var r in productDetails)
                    {
                        details.Remove(r);
                    }

                    Exception ex;
                    if ((ex = validator.Validate(invItem, productDetails)) != null)
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

                        ReportError(result, invItem, ex, validator);
                        continue;
                    }

                    InvoiceItem newItem = validator.InvoiceItem;
                    if (!newItem.InvoiceBuyer.IsB2C() && validator.ExpectedSeller.HybridB2B())
                    {
                        newItem.CDS_Document.ProcessType = (int)(int)Naming.InvoiceProcessType.A0401;
                        A0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已開立);
                        A0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document,
                            deferredNotice ? Naming.InvoiceStepDefinition.文件準備中 : Naming.InvoiceStepDefinition.已接收資料待通知);
                    }
                    else
                    {
                        newItem.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.已開立, Naming.InvoiceProcessType.F0401);
                        newItem.CDS_Document.PushStepQueueOnSubmit(this, 
                            deferredNotice ? Naming.InvoiceStepDefinition.文件準備中 : Naming.InvoiceStepDefinition.已接收資料待通知, Naming.InvoiceProcessType.F0401);
                    }

                    this.EntityList.InsertOnSubmit(newItem);
                    this.SubmitChanges();

                    eventItems.Add(newItem);
                    ReportSuccess(result, newItem);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    ReportError(result, null, ex, validator);
                }

                Console.WriteLine($"end {invSeq} => {DateTime.Now}");

            }
            validator.EndAutoTrackNo();
        }

        public DataTable SaveUploadInvoiceAutoTrackNo(DataSet item, ProcessRequest request, Naming.InvoiceProcessType processType = Naming.InvoiceProcessType.C0401_Xlsx)
        {
            Organization owner = request.Organization;
            this.ApplyInvoiceDate = request.ProcessRequestCondition.Any(c => c.ConditionID == (int)ProcessRequestCondition.ConditionType.UseLastPeriodTrackCodeNo)
                        ? new DateTime(DateTime.Today.Year, (DateTime.Today.Month + 1) / 2 * 2 - 1, 1).AddDays(-1)
                        : DateTime.Today;

            InvoiceDataSetValidator validator = new InvoiceDataSetValidator(this, owner, processType);

            DataTable result = InitializeInvoiceResponseTable();

            IEnumerable<DataRow> invoiceItems = (item.Tables["Invoice"] ?? item.Tables[0])?.Rows.Cast<DataRow>();
            IEnumerable<DataRow> details = (item.Tables["Details"] ?? item.Tables[1])?.Rows.Cast<DataRow>();

            if (invoiceItems == null || details == null)
            {
                throw new Exception("Bad Invoice Data Sheets");
            }
            //String dataID = "";
            //foreach (DataRow row in details)
            //{
            //    if (row.IsNull(validator.DetailsField.Data_ID) || String.IsNullOrEmpty((String)row[validator.DetailsField.Data_ID]))
            //    {
            //        row[validator.DetailsField.Data_ID] = dataID;
            //    }
            //    else
            //    {
            //        dataID = (String)row[validator.DetailsField.Data_ID];
            //    }
            //}

            if (invoiceItems.Count() > 0)
            {
                Console.WriteLine($"start total {invoiceItems.Count()} => {DateTime.Now}");

                EventItems = null;
                List<InvoiceItem> eventItems = new List<InvoiceItem>();

                int invSeq = 0;

                IEnumerable<DataRow> invoiceData = invoiceItems.Where(t => t.GetData<int>(validator.InvoiceField.Invoice_Type) == (int)Naming.InvoiceTypeDefinition.特種稅額計算之電子發票);
                if (invoiceData != null && invoiceData.Count() > 0)
                {

                    processAutoTrackInvoiceNo(request, invoiceData, details, Naming.InvoiceTypeDefinition.特種稅額計算之電子發票, validator, ref invSeq, eventItems, result);
                    var tmp = invoiceData;
                    invoiceData = invoiceItems.Except(tmp);
                }
                else
                {
                    invoiceData = invoiceItems;
                }

                processAutoTrackInvoiceNo(request, invoiceData, details, Naming.InvoiceTypeDefinition.一般稅額計算之電子發票, validator, ref invSeq, eventItems, result);

                if (eventItems.Count > 0)
                {
                    HasItem = true;
                }
                EventItems = eventItems;

                Console.WriteLine($"end total {invoiceItems.Count()} => {DateTime.Now}");

                if (this.HasError == true)
                {
                    this.PushProcessExceptionNotification(request, validator.ExpectedSeller ?? owner);
                }
            }

            return result;
        }

        //void proc(IEnumerable<DataRow> items, IEnumerable<DataRow> details, Naming.InvoiceTypeDefinition indication, InvoiceDataSetValidator validator, ref int invSeq, List<InvoiceItem> eventItems, DataTable result, bool useA0401)
        //{
        //    validator.InvoiceTypeIndication = indication;
        //    for (int idx = 0; idx < items.Count(); idx++, invSeq++)
        //    {
        //        Console.WriteLine($"start {invSeq} => {DateTime.Now}");

        //        try
        //        {
        //            var invItem = items.ElementAt(idx);
        //            String invoiceNo = invItem.GetString(validator.InvoiceField.Invoice_No);
        //            IEnumerable<DataRow> productDetails = details.Where(d => d.GetString(validator.DetailsField.Invoice_No) == invoiceNo);

        //            Exception ex;
        //            if ((ex = validator.Validate(invItem, productDetails)) != null)
        //            {
        //                if (IgnoreDuplicateDataNumberException && (ex is DuplicateDataNumberException))
        //                {
        //                    var testItem = ((DuplicateDataNumberException)ex).CurrentPO.InvoiceItem;
        //                    if (testItem != null)
        //                    {
        //                        eventItems.Add(testItem);
        //                        continue;
        //                    }
        //                }

        //                ReportError(result, invItem, ex, validator);
        //                continue;
        //            }

        //            InvoiceItem newItem = validator.InvoiceItem;

        //            if (useA0401)
        //            {
        //                A0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已開立);
        //                A0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
        //            }
        //            else
        //            {
        //                C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已開立);
        //                C0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已接收資料待通知);
        //            }

        //            this.EntityList.InsertOnSubmit(newItem);
        //            this.SubmitChanges();

        //            eventItems.Add(newItem);
        //            ReportSuccess(result, newItem);

        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.Error(ex);
        //            ReportError(result, null, ex, validator);
        //        }

        //        Console.WriteLine($"end {invSeq} => {DateTime.Now}");
        //    }
        //}

        public DataTable SaveUploadInvoice(DataSet item, ProcessRequest request,bool useA0401 = false)
        {
            Organization owner = request.Organization;

            InvoiceDataSetValidator validator = new InvoiceDataSetValidator(this, owner, useA0401 ? Naming.InvoiceProcessType.A0401_Xlsx_Allocation_ByIssuer : Naming.InvoiceProcessType.C0401_Xlsx_Allocation_ByIssuer);

            DataTable result = InitializeInvoiceResponseTable();

            IEnumerable<DataRow> invoiceItems = (item.Tables["Invoice"] ?? item.Tables[0])?.Rows.Cast<DataRow>();
            IEnumerable<DataRow> details = (item.Tables["Details"] ?? item.Tables[1])?.Rows.Cast<DataRow>();

            if (invoiceItems == null || details == null)
            {
                throw new Exception("Bad Invoice Data Sheets");
            }

            String invoiceNo = "";
            //foreach (DataRow row in details)
            //{
            //    if (row.IsNull(validator.DetailsField.Invoice_No) || String.IsNullOrEmpty((String)row[validator.DetailsField.Invoice_No]))
            //    {
            //        row[validator.DetailsField.Invoice_No] = invoiceNo;
            //    }
            //    else
            //    {
            //        invoiceNo = (String)row[validator.DetailsField.Invoice_No];
            //    }
            //}

            if (invoiceItems.Count() > 0)
            {
                Console.WriteLine($"start total {invoiceItems.Count()} => {DateTime.Now}");

                EventItems = null;
                List<InvoiceItem> eventItems = new List<InvoiceItem>();

                int invSeq = 0;
                bool deferredNotice = request.ProcessRequestCondition.Any(c => c.ConditionID == (int)ProcessRequestCondition.ConditionType.DeferredIssueNotice);
                void proc(IEnumerable<DataRow> items, Naming.InvoiceTypeDefinition indication)
                {
                    validator.InvoiceTypeIndication = indication;
                    for (int idx = 0; idx < items.Count(); idx++, invSeq++)
                    {
                        Console.WriteLine($"start {invSeq} => {DateTime.Now}");

                        try
                        {
                            var invItem = items.ElementAt(idx);
                            invoiceNo = invItem.GetString(validator.InvoiceField.Invoice_No);
                            IEnumerable<DataRow> productDetails = details.Where(d => d.GetString(validator.DetailsField.Invoice_No) == invoiceNo);

                            Exception ex;
                            if ((ex = validator.Validate(invItem, productDetails)) != null)
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

                                ReportError(result, invItem, ex, validator);
                                continue;
                            }

                            InvoiceItem newItem = validator.InvoiceItem;

                            if (useA0401)
                            {
                                A0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已開立);
                                A0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document,
                                    deferredNotice ? Naming.InvoiceStepDefinition.文件準備中 : Naming.InvoiceStepDefinition.已接收資料待通知);
                            }
                            else
                            {
                                if(!newItem.InvoiceBuyer.IsB2C() && validator.ExpectedSeller.HybridB2B())
                                {
                                    newItem.CDS_Document.ProcessType = (int)(int)Naming.InvoiceProcessType.A0401;
                                    A0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document, Naming.InvoiceStepDefinition.已開立);
                                    A0401Handler.PushStepQueueOnSubmit(this, newItem.CDS_Document,
                                        deferredNotice ? Naming.InvoiceStepDefinition.文件準備中 : Naming.InvoiceStepDefinition.已接收資料待通知);
                                }
                                else
                                {
                                    newItem.CDS_Document.PushStepQueueOnSubmit(this, Naming.InvoiceStepDefinition.已開立, Naming.InvoiceProcessType.F0401);
                                    newItem.CDS_Document.PushStepQueueOnSubmit(this, 
                                        deferredNotice ? Naming.InvoiceStepDefinition.文件準備中 : Naming.InvoiceStepDefinition.已接收資料待通知, Naming.InvoiceProcessType.F0401);
                                }
                            }

                            this.EntityList.InsertOnSubmit(newItem);
                            this.SubmitChanges();

                            eventItems.Add(newItem);
                            ReportSuccess(result, newItem);

                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            ReportError(result, null, ex, validator);
                        }

                        Console.WriteLine($"end {invSeq} => {DateTime.Now}");
                    }
                }

                IEnumerable<DataRow> invoiceData = invoiceItems.Where(t => t.GetData<int>(validator.InvoiceField.Invoice_Type) == (int)Naming.InvoiceTypeDefinition.特種稅額計算之電子發票);
                if (invoiceData != null && invoiceData.Count() > 0)
                {

                    proc(invoiceData, Naming.InvoiceTypeDefinition.特種稅額計算之電子發票);
                    //proc(invoiceData, details, Naming.InvoiceTypeDefinition.特種稅額計算之電子發票, validator, ref invSeq, eventItems, result, useA0401);
                    var tmp = invoiceData;
                    invoiceData = invoiceItems.Except(tmp);
                }
                else
                {
                    invoiceData = invoiceItems;
                }

                proc(invoiceData, Naming.InvoiceTypeDefinition.一般稅額計算之電子發票);
                //proc(invoiceData, details, Naming.InvoiceTypeDefinition.一般稅額計算之電子發票, validator, ref invSeq, eventItems, result, useA0401);

                if (eventItems.Count > 0)
                {
                    HasItem = true;
                }
                EventItems = eventItems;

                Console.WriteLine($"end total {invoiceItems.Count()} => {DateTime.Now}");

                if (this.HasError == true)
                {
                    this.PushProcessExceptionNotification(request, validator.ExpectedSeller ?? owner);
                }
            }

            return result;
        }

    }
}
