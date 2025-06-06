﻿using System;
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
    public class B0201Handler
    {
        static B0201Handler()
        {
            ModelExtension.Properties.AppSettings.Default.G0501Outbound.CheckStoredPath();
        }

        private GenericManager<EIVOEntityDataContext> models;
        private Table<B0501DispatchQueue> _table;

        public B0201Handler(GenericManager<EIVOEntityDataContext> models)
        {
            this.models = models;
            _table = models.GetTable<B0501DispatchQueue>();
        }

        public void WriteToTurnkey()
        {

            int docID = 0;
            IQueryable<B0501DispatchQueue> queryItems =
                _table
                    .Where(q => q.DocID > docID && q.StepID == (int)Naming.InvoiceStepDefinition.待傳送)
                    .OrderBy(d => d.DocID);

            var buffer = queryItems.Take(4096).ToList();
            while (buffer.Count > 0)
            {
                foreach (var item in buffer)
                {
                    docID = item.DocID;
                    var allowance = item.CDS_Document.DerivedDocument.ParentDocument.InvoiceAllowance;
                    try
                    {
                        var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.G0501Outbound, $"B0501-{allowance.AllowanceID}-{allowance.AllowanceNumber}.xml");
                        var xmlMIG = allowance.CreateG0501()?.ConvertToXml();
                        if (xmlMIG != null)
                        {
                            item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);
                            models.SubmitChanges();
                            xmlMIG.Save(fileName);

                            if (allowance.InvoiceAllowanceSeller.Organization.OrganizationStatus.DownloadDispatch == true)
                            {
                                PushStepQueueOnSubmit(models, item.CDS_Document, Naming.InvoiceStepDefinition.回傳MIG);
                                models.SubmitChanges();
                            }
                        }

                        models.ExecuteCommand("delete [proc].B0501DispatchQueue where DocID={0} and StepID={1}",
                            item.DocID, item.StepID);

                        //models.ExecuteCommand("update [proc].B0501DispatchQueue set StepID = 1323 where DocID={0} and StepID={1}",
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
            B0501DispatchQueue item;
            int docID = 0;
            IQueryable<B0501DispatchQueue> queryItems =
                _table
                    .Where(q => q.DocID > docID && q.StepID == (int)Naming.InvoiceStepDefinition.已接收資料待通知);

            while ((item = queryItems.FirstOrDefault()) != null)
            {
                docID = item.DocID;

                try
                {

                    EIVOTurnkeyFactory.NotifyIssuedAllowanceCancellation(item.DocID);

                    models.ExecuteCommand("delete [proc].B0501DispatchQueue where DocID={0} and StepID={1}",
                        item.DocID, item.StepID);

                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

        }

        private void prepareStep(B0501DispatchQueue item, Naming.InvoiceStepDefinition targetStep)
        {
            item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done);
            PushStepQueueOnSubmit(models, item.CDS_Document, targetStep);
        }

        public static void PushStepQueueOnSubmit(GenericManager<EIVOEntityDataContext> models, CDS_Document docItem, Naming.InvoiceStepDefinition stepID)
        {
            models.GetTable<B0501DispatchQueue>().InsertOnSubmit(
                new B0501DispatchQueue
                {
                    CDS_Document = docItem,
                    DispatchDate = DateTime.Now,
                    StepID = (int)stepID
                });

            docItem.PushLogOnSubmit(models, stepID, Naming.DataProcessStatus.Ready);
        }
    }

}
