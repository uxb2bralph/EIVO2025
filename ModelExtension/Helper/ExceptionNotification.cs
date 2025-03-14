using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModelCore.InvoiceManagement;
using ModelCore.Schema.EIVO;
using ModelCore.DataEntity;
using CommonLib.Utility;
using ModelCore.Locale;
using ModelCore.UploadManagement;
using ModelCore.Schema.EIVO.B2B;
using CommonLib.Core.Utility;

namespace ModelCore.Helper
{
    public class ExceptionInfo
    {
        public OrganizationToken Token { get; set; }
        public Dictionary<int, Exception> ExceptionItems { get; set; }
        public InvoiceRoot InvoiceData { get; set; }
        public SellerInvoiceRoot B2BInvoiceData { get; set; }
        public ModelCore.Schema.TurnKey.C0401.Invoice InvoiceData_C0401 { get; set; }
        public ModelCore.Schema.TurnKey.C0501.CancelInvoice CancelInvoiceData_C0501 { get; set; }
        public ModelCore.Schema.TurnKey.D0401.Allowance AllowanceData_D0401 { get; set; }
        public ModelCore.Schema.TurnKey.D0501.CancelAllowance CancelAllowanceData_D0501 { get; set; }
        public CancelInvoiceRoot CancelInvoiceData { get; set; }
        public AllowanceRoot AllowanceData { get; set; }
        public CancelAllowanceRoot CancelAllowanceData { get; set; }
        public IEnumerable<ItemUpload<InvoiceItem>> InvoiceError { get; set; }
        public IEnumerable<ItemUpload<InvoiceItem>> InvoiceCancellationError { get; set; }
        public IEnumerable<ItemUpload<OrganizationBranch>> BusinessCounterpartError { get; set; }
        public IEnumerable<ItemUpload<InvoiceAllowance>> AllowanceError { get; set; }
        public IEnumerable<ItemUpload<InvoiceAllowance>> AllowanceCancellationError { get; set; }
        public Schema.TurnKey.E0402.BranchTrackBlank BranchTrackBlankError { get; set; }
        public BuyerInvoiceRoot BuyerInvoiceData { get; set; }
        public ReceiptRoot ReceiptData { get; set; }
        public CancelReceiptRoot CancelReceiptData { get; set; }
        public IEnumerable<ItemUpload<Organization>> CounterpartBusinessError { get; set; }
        public ReturnInvoiceRoot ReturnInvoiceData { get; set; }
        public ReturnCancelInvoiceRoot ReturnCancelInvoiceData { get; set; }
        public ReturnAllowanceRoot ReturnAllowanceData { get; set; }
        public ReturnCancelAllowanceRoot ReturnCancelAllowanceData { get; set; }
        public DeleteInvoiceRoot DeleteInvoiceData { get; set; }
        public DeleteCancelInvoiceRoot DeleteCancelInvoiceData { get; set; }
        public DeleteAllowanceRoot DeleteAllowanceData { get; set; }
        public DeleteCancelAllowanceRoot DeleteCancelAllowanceData { get; set; }
    }

    public partial class ExceptionEventArgs : EventArgs
    {
        public int? CompanyID { get; set; }
        public String EMail {get;set;}
        public int MaxLogID { get; set; }
        public EnterpriseGroup Enterprise { get; set; }
    }

    public static class ExceptionNotification
    {
        public static EventHandler<ExceptionEventArgs>? SendExceptionNotification;
        public static Action<Exception>? SendExceptionNotificationToSysAdmin;

        public static void SendNotification(object stateInfo)
        {
            ExceptionInfo? info = stateInfo as ExceptionInfo;
            if (info == null)
                return;

            try
            {
                if (info.InvoiceData != null)
                {
                    notifyExceptionWhenUploadInvoice(info);
                }
                else if (info.B2BInvoiceData != null)
                {
                    notifyExceptionWhenUploadB2BInvoice(info);
                }
                else if (info.InvoiceData_C0401 != null)
                {
                    notifyExceptionWhenUploadInvoice_C0401(info);
                }
                else if (info.CancelInvoiceData != null)
                {
                    notifyExceptionWhenUploadCancellation(info);
                }
                else if (info.CancelInvoiceData_C0501 != null)
                {
                    notifyExceptionWhenUploadCancellation_C0501(info);
                }
                else if (info.AllowanceData != null)
                {
                    notifyExceptionWhenUploadAllowance(info);
                }
                else if (info.CancelAllowanceData != null)
                {
                    notifyExceptionWhenUploadAllowanceCancellation(info);
                }
                else if (info.InvoiceError != null)
                {
                    notifyExceptionWhenUploadCsvInvoice(info);
                }
                else if (info.InvoiceCancellationError != null)
                {
                    notifyExceptionWhenUploadCsvInvoiceCancellation(info);
                }
                else if (info.AllowanceError != null)
                {
                    notifyExceptionWhenUploadCsvAllowance(info);
                }
                else if (info.AllowanceCancellationError != null)
                {
                    notifyExceptionWhenUploadCsvAllowanceCancellation(info);
                }
                else if (info.BuyerInvoiceData != null)
                {
                    notifyExceptionWhenUploadBuyerInvoice(info);
                }
                else if (info.ReceiptData != null)
                {
                    notifyExceptionWhenUploadReceipt(info);
                }
                else if (info.CancelReceiptData != null)
                {
                    notifyExceptionWhenUploadReceiptCancellation(info);
                }
                else if (info.ReturnInvoiceData != null)
                {
                    notifyExceptionWhenUploadInvoiceReturn(info);
                }
                else if (info.ReturnCancelInvoiceData != null)
                {
                    notifyExceptionWhenUploadInvoiceCancellationReturn(info);
                }
                else if (info.ReturnAllowanceData != null)
                {
                    notifyExceptionWhenUploadAllowanceReturn(info);
                }
                else if (info.ReturnCancelAllowanceData != null)
                {
                    notifyExceptionWhenUploadAllowanceCancellationReturn(info);
                }
                else if (info.DeleteInvoiceData != null)
                {
                    notifyExceptionWhenUploadInvoiceDelete(info);
                }
                else if (info.DeleteCancelInvoiceData != null)
                {
                    notifyExceptionWhenUploadInvoiceCancellationDelete(info);
                }
                else if (info.DeleteAllowanceData != null)
                {
                    notifyExceptionWhenUploadAllowanceDelete(info);
                }
                else if (info.DeleteCancelAllowanceData != null)
                {
                    notifyExceptionWhenUploadAllowanceCancellationDelete(info);
                }
                else if(info.CounterpartBusinessError!=null)
                {
                    notifyExceptionWhenUploadCounterpartBusiness(info);
                }

                lock (typeof(ExceptionNotification))
                {
                    processNotification();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

        }

        public static void ProcessNotification()
        {
            try
            {
                lock (typeof(ExceptionNotification))
                {
                    processNotification();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static void processNotification()
        {
            if (SendExceptionNotification == null)
            {
                return;
            }
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var items = mgr.GetTable<ExceptionLog>().Where(e => e.ExceptionReplication != null);
                if (items.Count() > 0)
                {
                    int maxLogID = items.OrderByDescending(e => e.LogID).First().LogID;
                    foreach (var item in items.GroupBy(i => i.CompanyID))
                    {
                        if (item.Key.HasValue)
                        {
                            SendExceptionNotification(mgr, new ExceptionEventArgs
                            {
                                CompanyID = item.Key,
                                EMail = item.First().Organization.ContactEmail,
                                MaxLogID = maxLogID
                            });
                        }
                    }

                    SendExceptionNotification(mgr, new ExceptionEventArgs
                    {
                        MaxLogID = maxLogID
                        ///送給系統管理員接收全部異常資料
                        ///
                    });

                    mgr.GetTable<ExceptionReplication>().DeleteAllOnSubmit(items
                        .Where(i => i.LogID <= maxLogID)
                        .Select(i => i.ExceptionReplication));
                    mgr.SubmitChanges();
                }
            }
        }

        private static void notifyExceptionWhenUploadInvoice(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_Invoice,
                    Message = e.Value.Message,
                    DataContent = info.InvoiceData.Invoice[e.Key].GetXml()
                }));
                mgr.SubmitChanges();
            }
        }

        private static void notifyExceptionWhenUploadInvoice_C0401(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_Invoice,
                    Message = e.Value.Message,
                    DataContent = info.InvoiceData_C0401.GetXml()
                }));
                mgr.SubmitChanges();
            }
        }

        private static void notifyExceptionWhenUploadCsvInvoice(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.InvoiceError.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_Invoice,
                    Message = e.Status,
                    DataContent = e.DataContent,
                    IsCSV = true
                }));
                mgr.SubmitChanges();
            }
        }

        private static void notifyExceptionWhenUploadCsvInvoiceCancellation(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.InvoiceCancellationError.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_InvoiceCancellation,
                    Message = e.Status,
                    DataContent = e.DataContent,
                    IsCSV = true
                }));
                mgr.SubmitChanges();
            }
        }

        private static void notifyExceptionWhenUploadCsvAllowance(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.AllowanceError.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_Allowance,
                    Message = e.Status,
                    DataContent = e.DataContent,
                    IsCSV = true
                }));
                mgr.SubmitChanges();
            }
        }

        private static void notifyExceptionWhenUploadCsvAllowanceCancellation(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.AllowanceCancellationError.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_AllowanceCancellation,
                    Message = e.Status,
                    DataContent = e.DataContent,
                    IsCSV = true
                }));
                mgr.SubmitChanges();
            }
        }


        private static void notifyExceptionWhenUploadAllowance(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_Allowance,
                    Message = e.Value.Message,
                    DataContent = info.AllowanceData.Allowance[e.Key].GetXml()
                }));
                mgr.SubmitChanges();
            }
        }


        private static void notifyExceptionWhenUploadCancellation(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_InvoiceCancellation,
                    Message = e.Value.Message,
                    DataContent = info.CancelInvoiceData.CancelInvoice[e.Key].GetXml()
                }));
                mgr.SubmitChanges();
            }
        }

        private static void notifyExceptionWhenUploadAllowanceCancellation(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_AllowanceCancellation,
                    Message = e.Value.Message,
                    DataContent = info.CancelAllowanceData.CancelAllowance[e.Key].GetXml()
                }));
                mgr.SubmitChanges();
            }
        }
        private static void notifyExceptionWhenUploadBranchTrackBlank(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.BranchTrackBlank,
                    Message = e.Value.Message,
                    DataContent = info.BranchTrackBlankError.GetXml()
                }));
                mgr.SubmitChanges();
            }
        }
        private static void notifyExceptionWhenUploadCancellation_C0501(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_InvoiceCancellation,
                    Message = e.Value.Message,
                    DataContent = info.CancelInvoiceData_C0501.GetXml()
                }));
                mgr.SubmitChanges();
            }
        }

        private static void notifyExceptionWhenUploadB2BInvoice(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_Invoice,
                    Message = e.Value.Message,
                    DataContent = info.B2BInvoiceData.Invoice[e.Key].GetXml()
                }));
                mgr.SubmitChanges();
            }
        }

        private static void notifyExceptionWhenUploadBuyerInvoice(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_Invoice,
                    Message = e.Value.Message,
                    DataContent = info.BuyerInvoiceData.Invoice[e.Key].GetXml()
                }));
                mgr.SubmitChanges();
            }
        }

        private static void notifyExceptionWhenUploadReceipt(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.B2BInvoiceDocumentTypeDefinition.收據,
                    Message = e.Value.Message,
                    DataContent = info.ReceiptData.Receipt[e.Key].GetXml()
                }));
                mgr.SubmitChanges();
            }
        }

        private static void notifyExceptionWhenUploadReceiptCancellation(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.B2BInvoiceDocumentTypeDefinition.作廢收據,
                    Message = e.Value.Message,
                    DataContent = info.CancelReceiptData.CancelReceipt[e.Key].GetXml()
                }));
                mgr.SubmitChanges();
            }
        }

        private static void notifyExceptionWhenUploadCounterpartBusiness(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.CounterpartBusinessError.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_InvoiceCancellation,
                    Message = e.Status,
                    DataContent = e.DataContent
                }));
                mgr.SubmitChanges();
            }
        }

        private static void notifyExceptionWhenUploadInvoiceReturn(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_Invoice,
                    Message = e.Value.Message,
                    DataContent = info.ReturnInvoiceData.ReturnInvoice[e.Key].GetXml()
                }));
                mgr.SubmitChanges();
            }
        }

        private static void notifyExceptionWhenUploadInvoiceDelete(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_Invoice,
                    Message = e.Value.Message,
                    DataContent = info.DeleteInvoiceData.DeleteInvoice[e.Key].GetXml()
                }));
                mgr.SubmitChanges();
            }
        }

        private static void notifyExceptionWhenUploadInvoiceCancellationReturn(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_InvoiceCancellation,
                    Message = e.Value.Message,
                    DataContent = info.ReturnCancelInvoiceData.ReturnCancelInvoice[e.Key].GetXml()
                }));
                mgr.SubmitChanges();
            }
        }

        private static void notifyExceptionWhenUploadInvoiceCancellationDelete(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_InvoiceCancellation,
                    Message = e.Value.Message,
                    DataContent = info.DeleteCancelInvoiceData.DeleteCancelInvoice[e.Key].GetXml()
                }));
                mgr.SubmitChanges();
            }
        }


        private static void notifyExceptionWhenUploadAllowanceReturn(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_Allowance,
                    Message = e.Value.Message,
                    DataContent = info.ReturnAllowanceData.ReturnAllowance[e.Key].GetXml()
                }));
                mgr.SubmitChanges();
            }
        }

        private static void notifyExceptionWhenUploadAllowanceDelete(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_Allowance,
                    Message = e.Value.Message,
                    DataContent = info.DeleteAllowanceData.DeleteAllowance[e.Key].GetXml()
                }));
                mgr.SubmitChanges();
            }
        }


        private static void notifyExceptionWhenUploadAllowanceCancellationReturn(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_AllowanceCancellation,
                    Message = e.Value.Message,
                    DataContent = info.ReturnCancelAllowanceData.ReturnCancelAllowance[e.Key].GetXml()
                }));
                mgr.SubmitChanges();
            }
        }

        private static void notifyExceptionWhenUploadAllowanceCancellationDelete(ExceptionInfo info)
        {
            int? companyID = info.Token != null ? info.Token.CompanyID : (int?)null;
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var table = mgr.GetTable<ExceptionLog>();

                table.InsertAllOnSubmit(info.ExceptionItems.Select(e => new ExceptionLog
                {
                    CompanyID = companyID,
                    LogTime = DateTime.Now,
                    TypeID = (int)Naming.DocumentTypeDefinition.E_AllowanceCancellation,
                    Message = e.Value.Message,
                    DataContent = info.DeleteCancelAllowanceData.DeleteCancelAllowance[e.Key].GetXml()
                }));
                mgr.SubmitChanges();
            }
        }



    }


}
