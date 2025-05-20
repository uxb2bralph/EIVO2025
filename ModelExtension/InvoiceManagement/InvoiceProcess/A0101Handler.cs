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
using CommonLib.DataAccess;
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement.InvoiceProcess
{
    public class A0101Handler
    {
        private static String __A0101ReceiptDone;
        private static String __A0101ReceiptFailed;
        static A0101Handler()
        {
            AppSettings.Default.A0101Outbound.CheckStoredPath();
            AppSettings.Default.A0101Receipt.CheckStoredPath();
            __A0101ReceiptDone = Path.Combine(ModelExtension.Properties.AppSettings.Default.A0101Receipt, "OK");
            __A0101ReceiptDone.CheckStoredPath();
            __A0101ReceiptFailed = Path.Combine(ModelExtension.Properties.AppSettings.Default.A0101Receipt, "Failed");
            __A0101ReceiptFailed.CheckStoredPath();

        }

        private GenericManager<EIVOEntityDataContext> models;
        private Table<A0101DispatchQueue> _table;

        public A0101Handler(GenericManager<EIVOEntityDataContext> models)
        {
            this.models = models;
            _table = models.GetTable<A0101DispatchQueue>();
        }

        public void WriteToTurnkey()
        {
            int docID = 0;
            IQueryable<A0101DispatchQueue> queryItems =
                _table
                    .Where(q => q.DocID > docID && q.StepID == (int)Naming.InvoiceStepDefinition.待傳送)
                    .OrderBy(d => d.DocID);

            var buffer = queryItems.Take(4096).ToList();
            while (buffer.Count > 0)
            {
                foreach (var item in buffer)
                {
                    docID = item.DocID;
                    var invoiceItem = item.CDS_Document.InvoiceItem;
                    try
                    {
                        var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.A0101Outbound, $"A0101-{DateTime.Now:yyyyMMddHHmmssf}-{invoiceItem.TrackCode}{invoiceItem.No}.xml");
                        var xmlMIG = invoiceItem.CreateA0101();
                        item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);
                        item.CDS_Document.CurrentStep = (int)Naming.InvoiceStepDefinition.待接收;

                        models.SubmitChanges();
                        xmlMIG.Save(fileName);

                        models.ExecuteCommand("delete [proc].A0101DispatchQueue where DocID={0} and StepID={1}",
                            item.DocID, item.StepID);

                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }

                }
                buffer = queryItems.Take(4096).ToList();
            }

        }

        //public void NotifyToReceive()
        //{

        //    A0101DispatchQueue item;
        //    int docID = 0;
        //    IQueryable<A0101DispatchQueue> queryItems =
        //        _table
        //            .Where(q => q.DocID > docID && q.StepID == (int)Naming.InvoiceStepDefinition.未接收資料待通知);

        //    while ((item = queryItems.FirstOrDefault()) != null)
        //    {
        //        docID = item.DocID;

        //        try
        //        {

        //            EIVOPlatformFactory.NotifyToReceiveA0101(item.DocID);
        //            prepareStep(item, Naming.InvoiceStepDefinition.待接收);
        //            models.SubmitChanges();

        //            models.ExecuteCommand("delete [proc].A0101DispatchQueue where DocID={0} and StepID={1}",
        //                item.DocID, item.StepID);

        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.Error(ex);
        //        }
        //    }
        //}

        //public void NotifyIssued()
        //{


        //    A0101DispatchQueue item;
        //    int docID = 0;
        //    IQueryable<A0101DispatchQueue> queryItems =
        //        _table
        //            .Where(q => q.DocID > docID && q.StepID == (int)Naming.InvoiceStepDefinition.已接收資料待通知);

        //    while ((item = queryItems.FirstOrDefault()) != null)
        //    {
        //        docID = item.DocID;

        //        try
        //        {

        //            EIVOPlatformFactory.NotifyIssuedA0101(item.DocID);

        //            models.ExecuteCommand("delete [proc].A0101DispatchQueue where DocID={0} and StepID={1}",
        //                item.DocID, item.StepID);

        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.Error(ex);
        //        }
        //    }

        //}

        //public void ProcessToIssue()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    bool bSigned = false;

        //    A0101DispatchQueue item;
        //    int docID = 0;
        //    IQueryable<A0101DispatchQueue> queryItems =
        //        _table
        //            .Where(q => q.DocID > docID && q.StepID == (int)Naming.InvoiceStepDefinition.待開立);

        //    while ((item = queryItems.FirstOrDefault()) != null)
        //    {
        //        docID = item.DocID;

        //        try
        //        {
        //            //models.ExecuteCommand("Update [proc].A0101DispatchQueue set StepID = {2} where DocID={0} and StepID={1}",
        //            //    item.DocID, item.StepID, (int)Naming.B2BInvoiceStepDefinition.待開立處理中);

        //            if (item.CDS_Document.InvoiceItem.InvoiceSeller.Organization.OrganizationStatus.Entrusting == true)
        //            {
        //                sb.Clear();
        //                bSigned = false;
        //                if (item.CDS_Document.InvoiceItem.InvoiceSeller.Organization.IsEnterpriseGroupMember())
        //                {
        //                    var cert = item.CDS_Document.InvoiceItem.InvoiceSeller.Organization.PrepareSignerCertificate();
        //                    if (cert != null)
        //                    {
        //                        bSigned = item.CDS_Document.InvoiceItem.SignAndCheckToIssueInvoiceItem(cert, sb);
        //                    }
        //                }
        //                else
        //                {
        //                    bSigned = item.CDS_Document.InvoiceItem.SignAndCheckToIssueInvoiceItem(null, sb);
        //                }

        //                if (bSigned)
        //                {
        //                    _table.InsertOnSubmit(new A0101DispatchQueue
        //                    {
        //                        DocID = item.DocID,
        //                        StepID = (int)Naming.InvoiceStepDefinition.已接收資料待通知,
        //                        DispatchDate = DateTime.Now
        //                    });

        //                    prepareStep(item, Naming.InvoiceStepDefinition.已開立);
        //                    models.SubmitChanges();
        //                }
        //            }
        //            else
        //            {
        //                prepareStep(item, Naming.InvoiceStepDefinition.未接收資料待通知);
        //                models.SubmitChanges();
        //            }

        //            models.ExecuteCommand("delete [proc].A0101DispatchQueue where DocID={0} and StepID={1}",
        //                item.DocID, item.StepID);
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.Error(ex);
        //        }

        //    }
        //}

        //private void prepareStep(A0101DispatchQueue item, Naming.InvoiceStepDefinition targetStep)
        //{
        //    item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done);
        //    PushStepQueueOnSubmit(models, item.CDS_Document, targetStep);
        //}

        public static void PushStepQueueOnSubmit(GenericManager<EIVOEntityDataContext> models, CDS_Document docItem, Naming.InvoiceStepDefinition stepID)
        {
            models.GetTable<A0101DispatchQueue>().InsertOnSubmit(
                new A0101DispatchQueue
                {
                    CDS_Document = docItem,
                    DispatchDate = DateTime.Now,
                    StepID = (int)stepID
                });

            docItem.PushLogOnSubmit(models, stepID, Naming.DataProcessStatus.Ready);
        }

        private static Task? __Turnkey;
        public static void ReceiveFiles()
        {
            lock (typeof(A0101Handler))
            {
                if (__Turnkey == null)
                {
                    Console.WriteLine($"Create A0101 ReceiveFiles...");

                    var t = Task.Run(() =>
                    {
                        try
                        {
                            String[] files;
                            do
                            {
                                Thread.Sleep(1000);
                                files = Directory.GetFiles(ModelExtension.Properties.AppSettings.Default.A0101Receipt);
                                if (files != null && files.Count() > 0)
                                {
                                    Parallel.ForEach(files, fullPath =>
                                    {
                                        Logger.Info($"Receive A0101:{fullPath}");
                                        try
                                        {
                                            XmlDocument doc = new XmlDocument();
                                            doc.Load(fullPath);
                                            using (B2BInvoiceManager models = new B2BInvoiceManager { ProcessType = Naming.InvoiceProcessType.A0101 })
                                            {
                                                models.ReceiveA0101(doc);
                                            }
                                            File.Move(fullPath, Path.Combine(__A0101ReceiptDone, Path.GetFileName(fullPath)));
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.Error(ex);
                                            File.Move(fullPath, Path.Combine(__A0101ReceiptFailed, Path.GetFileName(fullPath)));
                                        }
                                    });
                                }
                            } while (files != null && files.Count() > 0);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }

                    });

                    __Turnkey = t.ContinueWith(ts =>
                    {
                        __Turnkey = null;
                    });
                }
            }
        }

    }

}
