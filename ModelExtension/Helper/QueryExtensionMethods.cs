using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using CommonLib.DataAccess;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Security.MembershipManagement;
using System.Threading.Tasks;
using CommonLib.Utility;
using ModelCore.Models.ViewModel;
using ModelCore.Models;

namespace ModelCore.Helper
{
    public static class QueryExtensionMethods
    {
        public static ModelSourceInquiry<InvoiceItem> CreateInvoiceInquiry(this InquireInvoiceViewModel? viewModel, UserProfile? profile)
        {
            var inquireConsumption = new InquireInvoiceConsumption { QueryViewModel = viewModel };
            //inquireConsumption.Append(new InquireInvoiceConsumptionExtensionToPrint { });

            return (ModelSourceInquiry<InvoiceItem>)(new InquireEffectiveInvoice { QueryViewModel = viewModel })
                .Append(new InquireInvoiceByRole(profile) { QueryViewModel = viewModel })
                .Append(inquireConsumption)
                .Append(new InquireInvoiceSeller { QueryViewModel = viewModel })
                .Append(new InquireInvoiceBuyer { QueryViewModel = viewModel })
                .Append(new InquireInvoiceBuyerByName { QueryViewModel = viewModel })
                .Append(new InquireCustomerID { QueryViewModel = viewModel })
                .Append(new InquireInvoiceDate { QueryViewModel = viewModel })
                .Append(new InquireInvoiceAttachment { QueryViewModel = viewModel })
                .Append(new InquireInvoiceNo { QueryViewModel = viewModel })
                .Append(new InquireInvoiceAgent { QueryViewModel = viewModel })
                .Append(new InquireWinningInvoice { QueryViewModel = viewModel });
        }

        public static void BuildInvoiceQuery(this ModelSource<InvoiceItem> modelSource, InquireInvoiceViewModel viewModel, UserProfile? profile, String? resultAction)
        {
            void checkQueryExtension(String? resultAction, InquireInvoiceViewModel viewModel)
            {
                switch (resultAction)
                {
                    case "Allow":
                        modelSource.Items = modelSource.Items.Where(i => i.AuthorizeToVoid != null);
                        break;

                    case "Print":
                        modelSource.Items = modelSource.Items.Where(i => i.InvoiceBuyer.ReceiptNo != "0000000000"
                            || (i.InvoiceBuyer.ReceiptNo == "0000000000"
                                && i.InvoiceDonation == null
                                && (i.InvoiceCarrier == null
                                    || (i.InvoiceCarrier.CarrierType == ModelExtension.Properties.AppSettings.Default.DefaultUserCarrierType && i.InvoiceWinningNumber != null))));

                        break;

                    case "Notify":
                        if (viewModel.IsNoticed == false)
                        {
                            modelSource.Items = modelSource.Items.Where(i => !modelSource.GetTable<IssuingNotice>().Any(n => n.DocID == i.InvoiceID && n.IssueDate.HasValue));
                        }
                        break;

                    default:
                        break;
                }

            }

            modelSource.Inquiry = viewModel.CreateInvoiceInquiry(profile);
            modelSource.BuildQuery();
            checkQueryExtension(resultAction, viewModel);

            if (!String.IsNullOrEmpty(viewModel.PrintMark))
            {
                modelSource.Items = modelSource.Items.Where(i => i.PrintMark == viewModel.PrintMark);
            }

            IQueryable<InvoiceCarrier>? carrierItems = null;
            if (!String.IsNullOrEmpty(viewModel.CarrierType))
            {
                carrierItems = modelSource.GetTable<InvoiceCarrier>().Where(c => c.CarrierType == viewModel.CarrierType);
            }

            viewModel.CarrierNo = viewModel.CarrierNo.GetEfficientString();
            if (viewModel.CarrierNo != null)
            {
                if (carrierItems == null)
                    carrierItems = modelSource.GetTable<InvoiceCarrier>();
                carrierItems = carrierItems.Where(c => c.CarrierNo == viewModel.CarrierNo || c.CarrierNo2 == viewModel.CarrierNo);
            }

            if (carrierItems != null)
            {
                modelSource.Items = modelSource.Items.Join(carrierItems, i => i.InvoiceID, c => c.InvoiceID, (i, c) => i);
            }

            if (!String.IsNullOrEmpty(viewModel.PrintMark))
            {
                modelSource.Items = modelSource.Items.Where(i => i.PrintMark == viewModel.PrintMark);
            }

            if (viewModel.Printed.HasValue)
            {
                var logs = modelSource.GetTable<DocumentPrintLog>();
                if (viewModel.Printed == true)
                {
                    modelSource.Items = modelSource.Items.Where(i => logs.Any(l => l.DocID == i.InvoiceID));
                }
                else
                {
                    modelSource.Items = modelSource.Items.Where(i => !logs.Any(l => l.DocID == i.InvoiceID));
                }
            }

            if (viewModel.HasAddr == true)
            {
                modelSource.Items = modelSource.Items.Where(i => i.InvoiceBuyer.Address != null);
            }
        }

        public static DataSet GetDataSetResult<TEntity>(this ModelSource<TEntity> models)
            where TEntity : class, new()
        {
            return models.GetDataSetResult(models.Items);
        }

        public static ClosedXML.Excel.XLWorkbook GetExcelResult<TEntity>(this ModelSource<TEntity> models)
            where TEntity : class, new()
        {
            return models.GetExcelResult(models.Items);
        }

        public static DataSet GetDataSetResult<TEntity>(this ModelSource<TEntity> models, IQueryable items)
            where TEntity : class, new()
        {
            using (SqlCommand sqlCmd = (SqlCommand)models.GetCommand(items))
            {
                sqlCmd.Connection = (SqlConnection)models.GetDataContext().Connection;
                using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCmd))
                {
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    return ds;
                }
            }
        }

        //public static DataSet GetDataSetResult<TEntity>(this ModelSource<TEntity> models, IQueryable items,DataTable table)
        //    where TEntity : class, new()
        //{
        //    using (SqlCommand sqlCmd = (SqlCommand)models.GetCommand(items))
        //    {
        //        return models.GetDataSetResult(sqlCmd, table);
        //    }
        //}

        public static DataSet GetDataSetResult(this GenericManager<EIVOEntityDataContext> models, IQueryable items, DataTable table)
        {
            using (SqlCommand sqlCmd = (SqlCommand)models.GetCommand(items))
            {
                return models.GetDataSetResult(sqlCmd, table);
            }
        }

        //public static DataSet GetDataSetResult<TEntity>(this ModelSource<TEntity> models, SqlCommand sqlCmd, DataTable table)
        //    where TEntity : class, new()
        //{
        //    sqlCmd.Connection = (SqlConnection)models.GetDataContext().Connection;
        //    using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCmd))
        //    {
        //        int colCount = table.Columns.Count;
        //        adapter.Fill(table);
        //        if (colCount > 0)
        //        {
        //            while (table.Columns.Count > colCount)
        //            {
        //                table.Columns.RemoveAt(table.Columns.Count - 1);
        //            }
        //        }
        //        return table.DataSet;
        //    }
        //}


        public static DataSet GetDataSetResult(this GenericManager<EIVOEntityDataContext> models, SqlCommand sqlCmd, DataTable table)
        {
            sqlCmd.Connection = (SqlConnection)models.DataContext.Connection;
            using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCmd))
            {
                int colCount = table.Columns.Count;
                adapter.Fill(table);
                if (colCount > 0)
                {
                    while (table.Columns.Count > colCount)
                    {
                        table.Columns.RemoveAt(table.Columns.Count - 1);
                    }
                }
                return table.DataSet;
            }
        }

        public static ClosedXML.Excel.XLWorkbook GetExcelResult<TEntity>(this ModelSource<TEntity> models,IQueryable items,String? tableName = null)
            where TEntity : class, new()
        {
            using (DataSet ds = models.GetDataSetResult(items))
            {
                if (tableName != null)
                    ds.Tables[0].TableName = ds.DataSetName = tableName;
                return ds.ConvertToExcel();
            }
        }

        public static void SaveAsExcel(this SqlCommand sqlCmd, DataTable table, String resultFile, int? taskID = null)
        {
            Task.Run(() =>
            {
                try
                {
                    using (DataSet ds = new DataSet())
                    {
                        ds.Tables.Add(table);
                        using (ModelSource<InvoiceItem> db = new ModelSource<InvoiceItem>())
                        {
                            Exception exception = null;

                            try
                            {
                                db.GetDataSetResult(sqlCmd, table);

                                using (var xls = ds.ConvertToExcel())
                                {
                                    xls.SaveAs(resultFile);
                                }
                            }
                            catch (Exception ex)
                            {
                                CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                                exception = ex;
                            }

                            ProcessRequest taskItem = db.GetTable<ProcessRequest>()
                                            .Where(t => t.TaskID == taskID).FirstOrDefault();

                            if (taskItem != null)
                            {
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
                }
                catch (Exception ex)
                {
                    CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                }
            });

        }

        public static void SaveAsExcel(this IQueryable<dynamic> items, GenericManager<EIVOEntityDataContext> models, String tableName, String resultFile)
        {
            DataTable table = new DataTable(tableName);
            items.BuildDataColumns(table);

            SqlCommand sqlCmd = (SqlCommand)models.GetCommand(items);

            using (DataSet ds = new DataSet())
            {
                ds.Tables.Add(table);
                models.GetDataSetResult(sqlCmd, table);

                using (var xls = ds.ConvertToExcel())
                {
                    xls.SaveAs(resultFile);
                }
            }
        }

    }
}