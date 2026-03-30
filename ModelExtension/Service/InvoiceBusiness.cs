using CommonLib.DataAccess;
using ModelCore.DataEntity;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Security.MembershipManagement;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Core.Utility;
using CommonLib.Utility;

namespace ModelCore.Service
{
    public static class InvoiceBusiness
    {
        public static void MarkPrintedLog(this GenericManager<EIVOEntityDataContext> models, InvoiceItem item, int uid)
        {
            if (!item.CDS_Document.DocumentPrintLog.Any(l => l.TypeID == (int)Naming.DocumentTypeDefinition.E_Invoice))
            {
                item.CDS_Document.DocumentPrintLog.Add(new DocumentPrintLog
                {
                    PrintDate = DateTime.Now,
                    UID = uid,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_Invoice
                });
            }

            models.DeleteAnyOnSubmit<DocumentPrintQueue>(d => d.DocID == item.InvoiceID);
            models.DeleteAnyOnSubmit<DocumentAuthorization>(d => d.DocID == item.InvoiceID);
            models.SubmitChanges();
        }

        public static void MarkPrintedLog(this GenericManager<EIVOEntityDataContext> models, InvoiceAllowance item, int uid)
        {
            if (!item.CDS_Document.DocumentPrintLog.Any(l => l.TypeID == (int)Naming.DocumentTypeDefinition.E_Allowance))
            {
                item.CDS_Document.DocumentPrintLog.Add(new DocumentPrintLog
                {
                    PrintDate = DateTime.Now,
                    UID = uid,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_Allowance
                });
            }

            models.DeleteAnyOnSubmit<DocumentPrintQueue>(d => d.DocID == item.AllowanceID);
            models.SubmitChanges();
        }

        public static void ProcessWinningNoExcel(this int taskID)
        {
            try
            {

                using (ModelSource<InvoiceItem> db = new ModelSource<InvoiceItem>())
                {
                    var taskItem = db.GetTable<ProcessRequest>()
                                        .Where(t => t.TaskID == taskID).FirstOrDefault();

                    if(taskItem == null || !File.Exists(taskItem.RequestPath))
                    {
                        // Handle missing process request or data path
                        return;
                    }

                    String excelPath = taskItem.RequestPath;
                    String resultFile = taskItem.ResponsePath;
                    if(String.IsNullOrEmpty(resultFile))
                    {
                        resultFile = Path.Combine(Logger.LogDailyPath, Guid.NewGuid().ToString() + ".xlsx");
                        taskItem.ResponsePath = resultFile;
                        db.SubmitChanges();
                    }

                    using (DataSet ds = excelPath.ImportExcelXLS())
                    {
                        Exception? exception = null;
                        if (ds.Tables.Count > 0)
                        {
                            List<int> winningID = new List<int>();
                            DataTable table = ds.Tables[0];
                            table.Columns.Add(new DataColumn("處理狀態", typeof(String)));
                            var statusIdx = table.Columns.Count - 1;

                            IEnumerable<DataRow> rows = table.Rows.Cast<DataRow>();
                            IEnumerable<DataRow> assumedRows = rows
                                    .Where(r => !r.IsNull(0))
                                    .Where(r => !r.IsNull(1))
                                    .Where(r => !r.IsNull(2))
                                    .Where(r => !r.IsNull(3))
                                    .Where(r => !r.IsNull(4));

                            foreach (var row in assumedRows)
                            {
                                try
                                {
                                    String periodNo = row.GetString(0).GetEfficientString();
                                    var trackCode = row.GetString(1).GetEfficientString();
                                    var no = row.GetString(2).GetEfficientString();
                                    var items = db.GetTable<InvoiceItem>()
                                            .Where(i => i.TrackCode == trackCode)
                                            .Where(i => i.No == no)
                                            .ToList();

                                    var invoice = items.Where(i => $"{i.InvoiceDate.Value.Year - 1911:000}{(i.InvoiceDate.Value.Month + 1) / 2 * 2:00}" == periodNo)
                                            .FirstOrDefault();

                                    if (invoice == null)
                                    {
                                        row[statusIdx] = "發票號碼不存在";
                                        continue;
                                    }

                                    var winningInvoice = invoice.InvoiceWinningNumber;
                                    if (winningInvoice == null)
                                    {
                                        winningInvoice = new InvoiceWinningNumber
                                        {
                                            InvoiceID = invoice.InvoiceID,
                                        };

                                        db.GetTable<InvoiceWinningNumber>().InsertOnSubmit(winningInvoice);
                                    }

                                    winningInvoice.PrizeType = row.GetString(3).GetEfficientString();
                                    winningInvoice.Bonus = row.GetData<int>(4);

                                    db.SubmitChanges();
                                    winningID.Add(invoice.InvoiceID);

                                }
                                catch (Exception ex)
                                {
                                    exception = exception ?? ex;
                                    Logger.Error(ex);
                                    row[statusIdx] = ex.Message;
                                }
                            }

                            if (winningID.Count > 0)
                            {
                                winningID.NotifyWinningInvoice(false);
                            }
                        }


                        using (var xls = ds.ConvertToExcel())
                        {
                            xls.SaveAs(resultFile);
                        }

                        if (exception != null)
                        {
                            taskItem.ExceptionLog = new ExceptionLog
                            {
                                DataContent = exception.Message
                            };
                        }
                        taskItem.ProcessComplete = DateTime.Now;
                        db.SubmitChanges();

                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

        }

    }
}
