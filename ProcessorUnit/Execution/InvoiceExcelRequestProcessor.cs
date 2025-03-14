using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Helper;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using CommonLib.Utility;
using ModelCore.InvoiceManagement;
using ClosedXML.Excel;
using CommonLib.Core.Utility;
using CommonLib.Utility.Properties;


namespace ProcessorUnit.Execution
{
    public class InvoiceExcelRequestProcessor : ProcessRequestExecutorForever
    {
        public InvoiceExcelRequestProcessor()
        {
            appliedProcessType = Naming.InvoiceProcessType.C0401_Xlsx;
            processDataSet = (ds, requestItem) =>
            {
                using (InvoiceDataSetManager manager = new InvoiceDataSetManager(models))
                {
                    return manager.SaveUploadInvoiceAutoTrackNo(ds, requestItem);
                }
            };
        }

        protected Func<DataSet, ProcessRequest, DataTable> processDataSet;

        static String[] __AcceptedTableName =
            {
                "Invoice",
                "Details",
                "Void_Invoice",
                "Allowance",
                "Void_Allowance",
            };
        protected override void ProcessRequestItem()
        {
            ProcessRequest requestItem = queueItem.ProcessRequest;
            String requestFile = requestItem.RequestPath.StoreTargetPath();
            if (File.Exists(requestFile))
            {
                Organization agent = requestItem.Organization;
                requestItem.ProcessStart = DateTime.Now;
                models.SubmitChanges();

                using (DataSet ds = requestFile.ImportExcelXLS())
                {
                    int idx = 1;
                    var tables = ds.Tables.Cast<DataTable>().ToList();
                    var expectedNames = __AcceptedTableName.ToList();
                    foreach (var t in tables.ToList())
                    {
                        t.TableName = t.TableName.Replace("$", "");
                        var item = expectedNames.Where(n => n == t.TableName).FirstOrDefault();
                        if (item != null)
                        {
                            tables.Remove(t);
                            expectedNames.Remove(item);
                        }
                    }

                    foreach (var t in tables)
                    {
                        var item = expectedNames.Where(n => t.TableName.Contains(n)).FirstOrDefault();
                        if (item != null)
                        {
                            t.TableName = item;
                            expectedNames.Remove(item);
                        }
                        else
                        {
                            t.TableName = $"undetermined_{idx++}";
                        }
                    }
                    var result = processDataSet(ds, requestItem);
                    result.TableName = "Process Result";
                    ds.Tables.Add(result);

                    using (XLWorkbook xls = new XLWorkbook())
                    {
                        xls.Worksheets.Add(ds);

                        String responseName = $"{Path.GetFileNameWithoutExtension(requestFile)}_Response.xlsx";
                        String responsePath = Path.Combine(Path.GetDirectoryName(requestFile), responseName);

                        if (ProcessorUnit.Properties.AppSettings.Default.ResponsePath != null)
                        {
                            responsePath = responsePath.Replace(AppSettingsBase.AppRoot, ProcessorUnit.Properties.AppSettings.Default.ResponsePath);
                        }

                        xls.SaveAs(responsePath);
                        requestItem.ProcessComplete = DateTime.Now;
                        requestItem.ResponsePath = responsePath;
                        if (requestItem.ProcessCompletionNotification == null)
                            requestItem.ProcessCompletionNotification = new ProcessCompletionNotification { };
                        models.DeleteAnyOnSubmit<ProcessRequestQueue>(d => d.TaskID == queueItem.TaskID);
                        models.SubmitChanges();

                    }
                }
            }
        }

    }
}
