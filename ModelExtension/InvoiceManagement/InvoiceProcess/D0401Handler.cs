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
using CommonLib.DataAccess;
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement.InvoiceProcess
{
    public class D0401Handler
    {
        static D0401Handler()
        {
            AppSettings.Default.G0401Outbound.CheckStoredPath();
        }

        private GenericManager<EIVOEntityDataContext> models;
        private Table<D0401DispatchQueue> _table;

        public D0401Handler(GenericManager<EIVOEntityDataContext> models)
        {
            this.models = models;
            _table = models.GetTable<D0401DispatchQueue>();
        }

        public void WriteToTurnkey()
        {

            int docID = 0;
            IQueryable<D0401DispatchQueue> queryItems =
                _table
                    .Where(q => q.DocID > docID && q.StepID == (int)Naming.InvoiceStepDefinition.已開立)
                    .OrderBy(d => d.DocID);

            var buffer = queryItems.Take(4096).ToList();
            while (buffer.Count > 0)
            {
                foreach (var item in buffer)
                {
                    docID = item.DocID;
                    var allowance = item.CDS_Document.InvoiceAllowance;
                    try
                    {
                        var xmlMIG = allowance.CreateG0401().ConvertToXml();
                        item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);
                        models.SubmitChanges();

                        var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.G0401Outbound, $"ALN0401-{allowance.AllowanceID}-{allowance.InvoiceAllowanceSeller.ReceiptNo}.xml");
                        File.WriteAllText(fileName, xmlMIG.OuterXml.Replace(".00000", ""));
                        //xmlMIG.Save(fileName);

                        if (allowance.InvoiceAllowanceSeller.Organization.OrganizationStatus.DownloadDispatch == true)
                        {
                            PushStepQueueOnSubmit(models, item.CDS_Document, Naming.InvoiceStepDefinition.回傳MIG);
                            models.SubmitChanges();
                        }

                        models.ExecuteCommand("delete [proc].D0401DispatchQueue where DocID={0} and StepID={1}",
                            item.DocID, item.StepID);

                        //models.ExecuteCommand("update [proc].D0401DispatchQueue set StepID = 1323 where DocID={0} and StepID={1}",
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

            D0401DispatchQueue item;
            int docID = 0;
            IQueryable<D0401DispatchQueue> queryItems =
                _table
                    .Where(q => q.DocID > docID && q.StepID == (int)Naming.InvoiceStepDefinition.已接收資料待通知);

            while ((item = queryItems.FirstOrDefault()) != null)
            {
                docID = item.DocID;
                var allowance = item.CDS_Document.InvoiceAllowance;

                try
                {

                    EIVOTurnkeyFactory.NotifyIssuedAllowance(item.DocID);

                    models.ExecuteCommand("delete [proc].D0401DispatchQueue where DocID={0} and StepID={1}",
                        item.DocID, item.StepID);

                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

        }

        private void prepareStep(D0401DispatchQueue item, Naming.InvoiceStepDefinition targetStep)
        {
            item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done);
            PushStepQueueOnSubmit(models, item.CDS_Document, targetStep);
        }

        public static void PushStepQueueOnSubmit(GenericManager<EIVOEntityDataContext> models, CDS_Document docItem, Naming.InvoiceStepDefinition stepID)
        {
            models.GetTable<D0401DispatchQueue>().InsertOnSubmit(
                new D0401DispatchQueue
                {
                    CDS_Document = docItem,
                    DispatchDate = DateTime.Now,
                    StepID = (int)stepID
                });

            docItem.PushLogOnSubmit(models, stepID, Naming.DataProcessStatus.Ready);
        }
    }

}
