using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml;


using ModelCore.DataEntity;
using ModelCore.DocumentManagement;
using ModelCore.Helper;
using ModelCore.Locale;
using ModelExtension.Properties;
using CommonLib.Utility;
using System.Security.Cryptography.Pkcs;

using System.Data.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using CommonLib.DataAccess;
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement.InvoiceProcess
{
    public class C0401Handler
    {
        static C0401Handler()
        {
            ModelExtension.Properties.AppSettings.Default.C0401Outbound.CheckStoredPath();
        }

        private GenericManager<EIVOEntityDataContext> models;
        private Table<C0401DispatchQueue> _table;

        public C0401Handler(GenericManager<EIVOEntityDataContext> models)
        {
            this.models = models;
            _table = models.GetTable<C0401DispatchQueue>();
        }

        static C0401DispatchQueue GetReadyItem(C0401Handler handler)
        {
            lock (typeof(C0401Handler))
            {
                C0401DispatchQueue item = handler._table
                    .Where(q => q.StepID == (int)Naming.InvoiceStepDefinition.已開立)
                    .Where(q => q.DispatchDate < DateTime.Now)
                    .Where(q => !q.BookingTime.HasValue)
                    .FirstOrDefault();

                if (item == null)
                {
                    DateTime available = DateTime.Now.AddMinutes(-5);
                    item = handler._table
                        .Where(q => q.StepID == (int)Naming.InvoiceStepDefinition.已開立)
                        .Where(q => q.DispatchDate < DateTime.Now)
                        .Where(q => q.BookingTime < available)
                        .FirstOrDefault();
                }

                if (item != null)
                {
                    handler.models.ExecuteCommand("update [proc].C0401DispatchQueue set BookingTime = GetDate() where DocID={0} and StepID={1}",
                        item.DocID, item.StepID);
                }

                return item;

            }
        }

        public void WriteToTurnkey(int? procIdx = null, int? totalProc = null)
        {
            C0401DispatchQueue item;
            while ((item = GetReadyItem(this)) != null)
            {
                WriteToTurnkey(item);
            }
        }

        //static List<C0401DispatchQueue> GetReadyItem(C0401Handler handler, int maxCount = 4096)
        //{
        //    lock (typeof(C0401Handler))
        //    {
        //        DateTime available = DateTime.Now.AddMinutes(-5);
        //        var items = handler._table
        //            .Where(q => q.StepID == (int)Naming.InvoiceStepDefinition.已開立)
        //            .Where(q => !q.BookingTime.HasValue || q.BookingTime < available)
        //            .Take(maxCount)
        //            .ToList();

        //        foreach (var item in items)
        //        {
        //            handler.models.ExecuteCommand("update [proc].C0401DispatchQueue set BookingTime = GetDate() where DocID={0} and StepID={1}",
        //                item.DocID, item.StepID);
        //        }

        //        return items;

        //    }
        //}

        //public void WriteToTurnkey(int? procIdx = null, int? totalProc = null)
        //{
        //    foreach (var item in GetReadyItem(this))
        //    {
        //        WriteToTurnkey(item);
        //    }
        //}

        private void WriteToTurnkey(C0401DispatchQueue item)
        {
            var invoiceItem = item.CDS_Document.InvoiceItem;
            try
            {
                var xmlMIG = invoiceItem.CreateInvoiceMIG().ConvertToXml();
                item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);
                models.SubmitChanges();

                var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.C0401Outbound, $"INV0401-{invoiceItem.InvoiceID}-{invoiceItem.TrackCode}{invoiceItem.No}.xml");
                xmlMIG.Save(fileName);

                if (invoiceItem.Organization.OrganizationStatus.SubscribeB2BInvoicePDF == true
                    && (!invoiceItem.InvoiceBuyer.IsB2C() || invoiceItem.Organization.OrganizationStatus.PrintAll == true)
                    && item.CDS_Document.DocumentSubscriptionQueue == null)
                {
                    models.ExecuteCommand(
                        @"INSERT INTO DocumentSubscriptionQueue
                                (DocID)
                            SELECT  {0}
                            WHERE   (NOT EXISTS
                                    (SELECT NULL
                                        FROM DocumentSubscriptionQueue
                                        WHERE (DocID = {0})))", item.DocID);
                }


                if (invoiceItem.Organization.OrganizationStatus.DownloadDataNumber == true)
                {
                    models.ExecuteCommand(@"INSERT INTO DocumentMappingQueue
                                            (DocID)
                                        SELECT  {0}
                                        WHERE   (NOT EXISTS
                                                (SELECT NULL
                                                    FROM DocumentMappingQueue
                                                    WHERE (DocID = {0})))", invoiceItem.InvoiceID);
                }

                if (invoiceItem.Organization.OrganizationStatus.DownloadDispatch == true)
                {
                    PushStepQueueOnSubmit(models, item.CDS_Document, Naming.InvoiceStepDefinition.回傳MIG);
                    models.SubmitChanges();
                }

                models.ExecuteCommand("delete [proc].C0401DispatchQueue where DocID={0} and StepID={1}",
                    item.DocID, item.StepID);

                //models.ExecuteCommand("update [proc].C0401DispatchQueue set StepID = 1323 where DocID={0} and StepID={1}",
                //    item.DocID, item.StepID);


            }
            catch (Exception ex)
            {
                Logger.Warn($"fail to write to turnkey: {item.DocID}");
                Logger.Error(ex);
            }
        }

        public void NotifyIssued()
        {

            C0401DispatchQueue item;
            int docID = 0;
            IQueryable<C0401DispatchQueue> queryItems =
                _table
                    .Where(q => q.DocID > docID && q.StepID == (int)Naming.InvoiceStepDefinition.已接收資料待通知);

            while ((item = queryItems.FirstOrDefault()) != null)
            {
                docID = item.DocID;
                var invoiceItem = item.CDS_Document.InvoiceItem;

                try
                {

                    EIVOTurnkeyFactory.NotifyIssuedInvoice(new NotifyToProcessID
                    {
                        DocID = item.DocID,
                        AppendAttachment = true,
                    });

                    models.ExecuteCommand("delete [proc].C0401DispatchQueue where DocID={0} and StepID={1}",
                        item.DocID, item.StepID);

                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

        }

        private void prepareStep(C0401DispatchQueue item, Naming.InvoiceStepDefinition targetStep)
        {
            item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done);
            PushStepQueueOnSubmit(models, item.CDS_Document, targetStep);
        }

        public static void PushStepQueueOnSubmit(GenericManager<EIVOEntityDataContext> models, CDS_Document docItem, Naming.InvoiceStepDefinition stepID)
        {
            models.GetTable<C0401DispatchQueue>().InsertOnSubmit(
                new C0401DispatchQueue
                {
                    CDS_Document = docItem,
                    DispatchDate = DateTime.Now,
                    StepID = (int)stepID
                });

            docItem.PushLogOnSubmit(models, stepID, Naming.DataProcessStatus.Ready);
        }

        //TODO:yuki加一筆到ProcessRequestDocument
        //public static void PushProcessRequestDocumentOnSubmit(GenericManager<EIVOEntityDataContext> models, CDS_Document docItem, int? taskID)
        //{            
        //    models.GetTable<ProcessRequestDocument>().InsertOnSubmit(
        //                    new ProcessRequestDocument
        //                    {
        //                        CDS_Document = docItem,
        //                        TaskID = taskID,
        //                        CreateDate = DateTime.Now
        //                    });            
        //}

        private static Task __Turnkey;
        public static void WriteToTurnkeyInBatches()
        {
            //System.Diagnostics.Debugger.Launch();
            if (__Turnkey == null)
            {
                var t = Task.Run(() =>
                {
                    Parallel.For(0, ModelExtension.Properties.AppSettings.Default.CommonParallelProcessCount, idx =>
                    {
                        using (InvoiceManager models = new InvoiceManager())
                        {
                            C0401Handler c0401 = new C0401Handler(models);
                            c0401.WriteToTurnkey(idx, AppSettings.Default.CommonParallelProcessCount);
                        }
                    });
                });

                t = t.ContinueWith(ts =>
                {
                    Task.Delay(ModelExtension.Properties.AppSettings.Default.TaskDelayInMilliseconds).ContinueWith(ts1 =>
                    {
                        __Turnkey = null;
                        WriteToTurnkeyInBatches();
                    });
                });
                Interlocked.Exchange<Task>(ref __Turnkey, t);
            }
            //else
            //{
            //    Logger.Debug($"C0401 WriteToTurnkey is running...");
            //}
        }
    }

}
