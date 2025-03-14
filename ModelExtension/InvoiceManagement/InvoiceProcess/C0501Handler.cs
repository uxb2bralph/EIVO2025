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
    public class C0501Handler
    {
        static C0501Handler()
        {
            ModelExtension.Properties.AppSettings.Default.C0501Outbound.CheckStoredPath();
        }

        private GenericManager<EIVOEntityDataContext> models;
        private Table<C0501DispatchQueue> _table;

        public C0501Handler(GenericManager<EIVOEntityDataContext> models)
        {
            this.models = models;
            _table = models.GetTable<C0501DispatchQueue>();
        }

        public void WriteToTurnkey()
        {

            int docID = 0;
            IQueryable<C0501DispatchQueue> queryItems =
                _table
                    .Where(q => q.DocID > docID && q.StepID == (int)Naming.InvoiceStepDefinition.已開立)
                    .OrderBy(d => d.DocID);

            var buffer = queryItems.Take(4096).ToList();
            while (buffer.Count > 0)
            {
                foreach (var item in buffer)
                {
                    docID = item.DocID;
                    var invoiceItem = item.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem;
                    if (invoiceItem == null)
                    {
                        invoiceItem = item.CDS_Document.InvoiceItem;
                    }

                    if (invoiceItem == null)
                        continue;

                    try
                    {
                        var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.C0501Outbound, $"INV0501-{invoiceItem.InvoiceID}-{invoiceItem.TrackCode}{invoiceItem.No}.xml");
                        var xmlMIG = invoiceItem.CreateInvoiceCancellationMIG().ConvertToXml();
                        item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);
                        models.SubmitChanges();
                        xmlMIG.Save(fileName);

                        if (invoiceItem.Organization.OrganizationStatus.DownloadDispatch == true)
                        {
                            PushStepQueueOnSubmit(models, item.CDS_Document, Naming.InvoiceStepDefinition.回傳MIG);
                            models.SubmitChanges();
                        }

                        models.ExecuteCommand("delete [proc].C0501DispatchQueue where DocID={0} and StepID={1}",
                            item.DocID, item.StepID);

                        //models.ExecuteCommand("update [proc].C0501DispatchQueue set StepID = 1323 where DocID={0} and StepID={1}",
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

        public void NotifyIssued()
        {


            C0501DispatchQueue item;
            int docID = 0;
            IQueryable<C0501DispatchQueue> queryItems =
                _table
                    .Where(q => q.DocID > docID && q.StepID == (int)Naming.InvoiceStepDefinition.已接收資料待通知);

            while ((item = queryItems.FirstOrDefault()) != null)
            {
                docID = item.DocID;
                var invoiceItem = item.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem;
                if (invoiceItem == null)
                {
                    invoiceItem = item.CDS_Document.InvoiceItem;
                }

                try
                {

                    EIVOTurnkeyFactory.NotifyIssuedInvoiceCancellation(new NotifyToProcessID
                    {
                        DocID = item.DocID,
                    });

                    models.ExecuteCommand("delete [proc].C0501DispatchQueue where DocID={0} and StepID={1}",
                        item.DocID, item.StepID);

                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

        }

        private void prepareStep(C0501DispatchQueue item, Naming.InvoiceStepDefinition targetStep)
        {
            item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done);
            PushStepQueueOnSubmit(models, item.CDS_Document, targetStep);
        }

        public static void PushStepQueueOnSubmit(GenericManager<EIVOEntityDataContext> models, CDS_Document docItem, Naming.InvoiceStepDefinition stepID)
        {
            models.GetTable<C0501DispatchQueue>().InsertOnSubmit(
                new C0501DispatchQueue
                {
                    CDS_Document = docItem,
                    DispatchDate = DateTime.Now,
                    StepID = (int)stepID
                });

            docItem.PushLogOnSubmit(models, stepID, Naming.DataProcessStatus.Ready);
        }
    }

}
