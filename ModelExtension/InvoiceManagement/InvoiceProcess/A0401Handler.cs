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
using ModelCore.Properties;
using CommonLib.Utility;
using System.Security.Cryptography.Pkcs;

using System.Data.Linq;
using CommonLib.DataAccess;
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement.InvoiceProcess
{
    public class A0401Handler
    {
        static A0401Handler()
        {
            ModelExtension.Properties.AppSettings.Default.F0401Outbound.CheckStoredPath();
        }

        private GenericManager<EIVOEntityDataContext> models;
        private Table<A0401DispatchQueue> _table;

        public A0401Handler(GenericManager<EIVOEntityDataContext> models)
        {
            this.models = models;
            _table = models.GetTable<A0401DispatchQueue>();
        }

        public void WriteToTurnkey()
        {
            int docID = 0;
            IQueryable<A0401DispatchQueue> queryItems =
                _table
                    .Where(q => q.DocID > docID && q.StepID == (int)Naming.InvoiceStepDefinition.已開立)
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
                        var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.F0401Outbound, $"F0401-{invoiceItem.InvoiceID}-{invoiceItem.TrackCode}{invoiceItem.No}.xml");
                        var xmlMIG = invoiceItem.CreateF0401();
                        xmlMIG.Save(fileName);
                        item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);
                        item.CDS_Document.CurrentStep = (int)Naming.InvoiceStepDefinition.已接收;


                        models.SubmitChanges();

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

                        models.ExecuteCommand("delete [proc].A0401DispatchQueue where DocID={0} and StepID={1}",
                            item.DocID, item.StepID);

                        //models.ExecuteCommand("update [proc].A0401DispatchQueue set StepID = 1323 where DocID={0} and StepID={1}",
                        //    item.DocID, item.StepID);

                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }

                }
                buffer = queryItems.Take(4096).ToList();
            }

        }

        public void NotifyToReceive()
        {

            A0401DispatchQueue item;
            int docID = 0;
            IQueryable<A0401DispatchQueue> queryItems =
                _table
                    .Where(q => q.DocID > docID && q.StepID == (int)Naming.InvoiceStepDefinition.未接收資料待通知);

            while ((item = queryItems.FirstOrDefault()) != null)
            {
                docID = item.DocID;

                try
                {

                    EIVOTurnkeyFactory.NotifyToReceiveA0401(item.DocID);
                    prepareStep(item, Naming.InvoiceStepDefinition.待接收);
                    models.SubmitChanges();

                    models.ExecuteCommand("delete [proc].A0401DispatchQueue where DocID={0} and StepID={1}",
                        item.DocID, item.StepID);

                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        public void NotifyIssued()
        {


            A0401DispatchQueue item;
            int docID = 0;
            IQueryable<A0401DispatchQueue> queryItems =
                _table
                    .Where(q => q.DocID > docID && q.StepID == (int)Naming.InvoiceStepDefinition.已接收資料待通知);

            while ((item = queryItems.FirstOrDefault()) != null)
            {
                docID = item.DocID;

                try
                {

                    EIVOTurnkeyFactory.NotifyIssuedA0401(new NotifyToProcessID
                    {
                        DocID = item.DocID,
                    });

                    models.ExecuteCommand("delete [proc].A0401DispatchQueue where DocID={0} and StepID={1}",
                        item.DocID, item.StepID);

                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

        }

        public void ProcessToIssue()
        {
            StringBuilder sb = new StringBuilder();
            bool bSigned = false;

            A0401DispatchQueue item;
            int docID = 0;
            IQueryable<A0401DispatchQueue> queryItems =
                _table
                    .Where(q => q.DocID > docID && q.StepID == (int)Naming.InvoiceStepDefinition.待開立);

            while ((item = queryItems.FirstOrDefault()) != null)
            {
                docID = item.DocID;

                try
                {
                    //models.ExecuteCommand("Update [proc].A0401DispatchQueue set StepID = {2} where DocID={0} and StepID={1}",
                    //    item.DocID, item.StepID, (int)Naming.B2BInvoiceStepDefinition.待開立處理中);

                    if (item.CDS_Document.InvoiceItem.InvoiceSeller.Organization.OrganizationStatus.Entrusting == true)
                    {
                        sb.Clear();
                        bSigned = false;
                        if (item.CDS_Document.InvoiceItem.InvoiceSeller.Organization.IsEnterpriseGroupMember())
                        {
                            var cert = item.CDS_Document.InvoiceItem.InvoiceSeller.Organization.PrepareSignerCertificate();
                            if (cert != null)
                            {
                                bSigned = item.CDS_Document.InvoiceItem.SignAndCheckToIssueInvoiceItem(cert, sb);
                            }
                        }
                        else
                        {
                            bSigned = item.CDS_Document.InvoiceItem.SignAndCheckToIssueInvoiceItem(null, sb);
                        }

                        if (bSigned)
                        {
                            _table.InsertOnSubmit(new A0401DispatchQueue
                            {
                                DocID = item.DocID,
                                StepID = (int)Naming.InvoiceStepDefinition.已接收資料待通知,
                                DispatchDate = DateTime.Now
                            });

                            prepareStep(item, Naming.InvoiceStepDefinition.已開立);
                            models.SubmitChanges();
                        }
                    }
                    else
                    {
                        prepareStep(item, Naming.InvoiceStepDefinition.未接收資料待通知);
                        models.SubmitChanges();
                    }

                    models.ExecuteCommand("delete [proc].A0401DispatchQueue where DocID={0} and StepID={1}",
                        item.DocID, item.StepID);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

            }
        }

        private void prepareStep(A0401DispatchQueue item, Naming.InvoiceStepDefinition targetStep)
        {
            item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done);
            PushStepQueueOnSubmit(models, item.CDS_Document, targetStep);
        }

        public void MatchDocumentAttachment()
        {
            ((EIVOEntityDataContext)_table.Context).MatchDocumentAttachment();
        }

        public static void PushStepQueueOnSubmit(GenericManager<EIVOEntityDataContext> models, CDS_Document docItem, Naming.InvoiceStepDefinition stepID)
        {
            models.GetTable<A0401DispatchQueue>().InsertOnSubmit(
                new A0401DispatchQueue
                {
                    CDS_Document = docItem,
                    DispatchDate = DateTime.Now,
                    StepID = (int)stepID
                });

            docItem.PushLogOnSubmit(models, stepID, Naming.DataProcessStatus.Ready);
        }
    }

}
