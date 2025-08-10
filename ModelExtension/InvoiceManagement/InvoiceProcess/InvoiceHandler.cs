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
using ModelCore.Models.ViewModel;
using Newtonsoft.Json;

namespace ModelCore.InvoiceManagement.InvoiceProcess
{
    public class InvoiceHandler
    {
        static InvoiceHandler()
        {
            AppSettings.Default.A0101Outbound.CheckStoredPath();
            AppSettings.Default.A0102Outbound.CheckStoredPath();
            AppSettings.Default.A0301Outbound.CheckStoredPath();
            AppSettings.Default.A0302Outbound.CheckStoredPath();
            AppSettings.Default.A0201Outbound.CheckStoredPath();
            AppSettings.Default.A0202Outbound.CheckStoredPath();
            AppSettings.Default.B0101Outbound.CheckStoredPath();
            AppSettings.Default.B0102Outbound.CheckStoredPath();
            AppSettings.Default.B0201Outbound.CheckStoredPath();
            AppSettings.Default.B0202Outbound.CheckStoredPath();
            AppSettings.Default.F0401Outbound.CheckStoredPath();
            AppSettings.Default.F0501Outbound.CheckStoredPath();
            AppSettings.Default.G0401Outbound.CheckStoredPath();
            AppSettings.Default.G0501Outbound.CheckStoredPath();
        }

        private GenericManager<EIVOEntityDataContext> models;
        private Table<DataProcessQueue> _table;

        public InvoiceHandler(GenericManager<EIVOEntityDataContext> models)
        {
            this.models = models;
            _table = models.GetTable<DataProcessQueue>();
        }

        public static DataProcessQueue? GetReadyItem(InvoiceHandler handler, Naming.InvoiceStepDefinition stepID,Naming.InvoiceProcessType processType)
        {
            lock (typeof(InvoiceHandler))
            {
                DateTime available = DateTime.Now.AddMinutes(-5);
                DataProcessQueue? item = handler._table!
                    .Where(q => q.StepID == (int)stepID)
                    .Where(q => q.ProcessType == (int)processType)
                    .Where(q => q.DispatchDate < DateTime.Now)
                    .Where(q => !q.BookingTime.HasValue || q.BookingTime < available)
                    .FirstOrDefault();

                if (item != null)
                {
                    if(handler.models.ExecuteCommand("update [proc].DataProcessQueue set BookingTime = GetDate() where DocID={0} and StepID={1} and ProcessType = {2}",
                        item.DocID, item.StepID, item.ProcessType) > 0)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        private RenderStyleViewModel? GetNoticeItem()
        {
            lock (typeof(InvoiceHandler))
            {

                var files = Directory.EnumerateFiles(AppSettings.Default.MailQueuePath, "*.json");

                if (files.Any())
                {
                    foreach(var file in files)
                    {
                        try
                        {
                            var readyFile = Path.Combine(AppSettings.Default.MailReadyPath, Path.GetFileName(file));
                            File.Move(file, readyFile);
                            var content = File.ReadAllText(readyFile);
                            var viewModel = JsonConvert.DeserializeObject<RenderStyleViewModel>(content);
                            if (viewModel != null)
                            {
                                viewModel.JsonPath = readyFile;
                                return viewModel;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                }
            }
            return null;
        }


        public void WriteA0101ToTurnkey()
        {
            DataProcessQueue? item;
            while ((item = GetReadyItem(this, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.A0101)) != null)
            {
                WriteA0101ToTurnkey(item);
            }
        }

        public void WriteF0401ToTurnkey()
        {
            DataProcessQueue? item;
            while ((item = GetReadyItem(this, Naming.InvoiceStepDefinition.已開立, Naming.InvoiceProcessType.F0401)) != null)
            {
                WriteF0401ToTurnkey(item);
            }
        }

        public void SendMailNotification()
        {
            RenderStyleViewModel? viewModel;
            while ((viewModel = GetNoticeItem()) != null)
            {
                try
                {
                    if (EIVONotificationFactory.SendNotification(viewModel))
                    {
                        var docItem = models.GetTable<CDS_Document>()
                            .Where(d => d.DocID == viewModel.DocID).FirstOrDefault();

                        if (docItem != null)
                        {
                            docItem.PushLogOnSubmit(models, viewModel.StepID, Naming.DataProcessStatus.Done, processType: viewModel.ProcessType);
                            models.SubmitChanges();
                        }

                        PopupNoticeItem(viewModel);
                    }

                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        private void PopupNoticeItem(RenderStyleViewModel viewModel)
        {
            if (viewModel?.JsonPath != null && File.Exists(viewModel.JsonPath))
            {
                try
                {
                    File.Delete(viewModel.JsonPath);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        public void WriteG0401ToTurnkey()
        {
            DataProcessQueue? item;
            while ((item = GetReadyItem(this, Naming.InvoiceStepDefinition.已開立, Naming.InvoiceProcessType.G0401)) != null)
            {
                WriteG0401ToTurnkey(item);
            }
        }

        public void WriteG0501ToTurnkey()
        {
            DataProcessQueue? item;
            while ((item = GetReadyItem(this, Naming.InvoiceStepDefinition.已開立, Naming.InvoiceProcessType.G0501)) != null)
            {
                WriteG0501ToTurnkey(item);
            }
        }

        public void WriteF0501ToTurnkey()
        {
            DataProcessQueue? item;
            while ((item = GetReadyItem(this, Naming.InvoiceStepDefinition.已開立, Naming.InvoiceProcessType.F0501)) != null)
            {
                WriteF0501ToTurnkey(item);
            }
        }

        public void WriteB0101ToTurnkey()
        {
            DataProcessQueue? item;
            while ((item = GetReadyItem(this, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.B0101)) != null)
            {
                WriteB0101ToTurnkey(item);
            }
        }

        public void WriteB0102ToTurnkey()
        {
            DataProcessQueue? item;
            while ((item = GetReadyItem(this, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.B0102)) != null)
            {
                WriteB0102ToTurnkey(item);
            }
        }

        public int WriteB0101ToTurnkey(DataProcessQueue item)
        {
            int docID = item.DocID;
            var allowance = item.CDS_Document.InvoiceAllowance;
            try
            {
                var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.B0101Outbound, $"{(Naming.InvoiceProcessType)item.ProcessType}-{DateTime.Now:yyyyMMddHHmmssf}-{allowance.AllowanceID}-{allowance.InvoiceAllowanceSeller.ReceiptNo}.xml");
                var xmlMIG = allowance.CreateB0101();
                if (xmlMIG == null)
                {
                    return -1; // No B0101 to write
                }

                item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);

                models.SubmitChanges();
                xmlMIG.Save(fileName);

                PopupQueueItem(item);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return docID;
        }

        private int WriteB0102ToTurnkey(DataProcessQueue item)
        {
            int docID = item.DocID;
            var allowance = item.CDS_Document.InvoiceAllowance;
            try
            {
                var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.B0102Outbound, $"{(Naming.InvoiceProcessType)item.ProcessType}-{DateTime.Now:yyyyMMddHHmmssf}-{allowance.AllowanceID}-{allowance.InvoiceAllowanceSeller.ReceiptNo}.xml");
                var xmlMIG = allowance.CreateB0102();
                if (xmlMIG == null)
                {
                    return -1; // No B0102 to write
                }

                item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);

                models.SubmitChanges();
                xmlMIG.Save(fileName);

                PopupQueueItem(item);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return docID;
        }

        public void WriteA0102ToTurnkey()
        {
            DataProcessQueue? item;
            while ((item = GetReadyItem(this, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.A0102)) != null)
            {
                WriteA0102ToTurnkey(item);
            }
        }

        public void WriteA0301ToTurnkey()
        {
            DataProcessQueue? item;
            while ((item = GetReadyItem(this, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.A0301)) != null)
            {
                WriteA0301ToTurnkey(item);
            }
        }

        public void WriteA0302ToTurnkey()
        {
            DataProcessQueue? item;
            while ((item = GetReadyItem(this, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.A0302)) != null)
            {
                WriteA0302ToTurnkey(item);
            }
        }

        public void WriteA0201ToTurnkey()
        {
            DataProcessQueue? item;
            while ((item = GetReadyItem(this, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.A0201)) != null)
            {
                WriteA0201ToTurnkey(item);
            }
        }

        public void WriteA0202ToTurnkey()
        {
            DataProcessQueue? item;
            while ((item = GetReadyItem(this, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.A0202)) != null)
            {
                WriteA0202ToTurnkey(item);
            }
        }

        public void CompleteReceivedA0302()
        {
            DataProcessQueue? item;
            while ((item = GetReadyItem(this, Naming.InvoiceStepDefinition.已接收, Naming.InvoiceProcessType.A0302)) != null)
            {
                var invoiceItem = item.CDS_Document.InvoiceItem;
                var requestItem = models.ProcessVoidInvoiceRequest(Naming.VoidActionMode.確認接收退回, null, invoiceItem);
                models.CommitToVoidInvoice(requestItem!);
                FinishStep(item);
            }
        }

        private int WriteA0101ToTurnkey(DataProcessQueue item)
        {
            int docID = item.DocID;
            var invoiceItem = item.CDS_Document.InvoiceItem;
            try
            {
                var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.A0101Outbound, $"A0101-{DateTime.Now:yyyyMMddHHmmssf}-{invoiceItem.TrackCode}{invoiceItem.No}.xml");
                var xmlMIG = invoiceItem.CreateA0101();
                item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);

                models.SubmitChanges();
                xmlMIG.Save(fileName);

                PopupQueueItem(item);

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return docID;
        }

        private int WriteF0401ToTurnkey(DataProcessQueue item)
        {
            int docID = item.DocID;
            var invoiceItem = item.CDS_Document.InvoiceItem;
            try
            {
                var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.F0401Outbound, $"{(Naming.InvoiceProcessType)item.ProcessType}-{DateTime.Now:yyyyMMddHHmmssf}-{invoiceItem.TrackCode}{invoiceItem.No}.xml");
                var xmlMIG = invoiceItem.CreateF0401();
                if (xmlMIG == null)
                {
                    return -1; // No F0401 to write
                }
                item.PushStepLogOnSubmit(models, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);
                models.SubmitChanges();
                xmlMIG.Save(fileName);

                if (invoiceItem.Organization.OrganizationStatus.DownloadDispatch == true)
                {
                    item.CDS_Document.PushStepQueueOnSubmit(models, Naming.InvoiceStepDefinition.回傳MIG, Naming.InvoiceProcessType.F0401);
                    models.SubmitChanges();
                }

                PopupQueueItem(item);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return docID;
        }

        private int WriteG0401ToTurnkey(DataProcessQueue item)
        {
            int docID = item.DocID;
            var allowance = item.CDS_Document.InvoiceAllowance;
            try
            {
                var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.G0401Outbound, $"{(Naming.InvoiceProcessType)item.ProcessType}-{DateTime.Now:yyyyMMddHHmmssf}-{allowance.AllowanceID}-{allowance.InvoiceAllowanceSeller.ReceiptNo}.xml");
                var xmlMIG = allowance.CreateG0401();
                item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);

                models.SubmitChanges();
                xmlMIG.Save(fileName);

                if (allowance.InvoiceAllowanceSeller.Organization.OrganizationStatus.DownloadDispatch == true)
                {
                    item.CDS_Document.PushStepQueueOnSubmit(models, Naming.InvoiceStepDefinition.回傳MIG, Naming.InvoiceProcessType.G0401);
                    models.SubmitChanges();
                }

                PopupQueueItem(item);

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return docID;
        }

        private int WriteG0501ToTurnkey(DataProcessQueue item)
        {
            int docID = item.DocID;
            var allowance = item.CDS_Document.DerivedDocument?.ParentDocument.InvoiceAllowance;
            if (allowance == null)
            {
                allowance = item.CDS_Document.InvoiceAllowance;
            }

            try
            {
                var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.G0501Outbound, $"{(Naming.InvoiceProcessType)item.ProcessType}-{DateTime.Now:yyyyMMddHHmmssf}-{allowance.AllowanceID}-{allowance.InvoiceAllowanceSeller.ReceiptNo}.xml");
                var xmlMIG = allowance.CreateG0501();
                if (xmlMIG == null)
                {
                    return -1; // No G0501 to write
                }

                item.PushStepLogOnSubmit(models, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);

                models.SubmitChanges();
                xmlMIG.Save(fileName);

                if (allowance.InvoiceAllowanceSeller.Organization.OrganizationStatus.DownloadDispatch == true)
                {
                    item.CDS_Document.PushStepQueueOnSubmit(models, Naming.InvoiceStepDefinition.回傳MIG, Naming.InvoiceProcessType.G0501);
                    models.SubmitChanges();
                }
                
                PopupQueueItem(item);

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return docID;
        }

        private int WriteF0501ToTurnkey(DataProcessQueue item)
        {
            int docID = item.DocID;
            var invoice = item.CDS_Document.DerivedDocument?.ParentDocument.InvoiceItem;
            if (invoice == null)
            {
                invoice = item.CDS_Document.InvoiceItem;
            }

            try
            {
                var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.F0501Outbound, $"{(Naming.InvoiceProcessType)item.ProcessType}-{DateTime.Now:yyyyMMddHHmmssf}-{invoice.TrackCode}{invoice.No}.xml");
                var xmlMIG = invoice.CreateF0501();
                if (xmlMIG == null)
                {
                    return -1; // No F0501 to write
                }

                item.PushStepLogOnSubmit(models, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);

                models.SubmitChanges();
                xmlMIG.Save(fileName);

                if (invoice.Organization.OrganizationStatus.DownloadDispatch == true)
                {
                    item.CDS_Document.PushStepQueueOnSubmit(models, Naming.InvoiceStepDefinition.回傳MIG, Naming.InvoiceProcessType.F0501);
                    models.SubmitChanges();
                }

                PopupQueueItem(item);

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return docID;
        }

        private int WriteA0102ToTurnkey(DataProcessQueue item)
        {
            int docID = item.DocID;
            var invoiceItem = item.CDS_Document.InvoiceItem;
            try
            {
                var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.A0102Outbound, $"A0102-{DateTime.Now:yyyyMMddHHmmssf}-{invoiceItem.TrackCode}{invoiceItem.No}.xml");
                var xmlMIG = invoiceItem.CreateA0102();
                if (xmlMIG == null)
                {
                    return -1; // No A0102 to write
                }

                item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);

                models.SubmitChanges();
                xmlMIG.Save(fileName);

                PopupQueueItem(item);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return docID;
        }

        private int WriteA0202ToTurnkey(DataProcessQueue item)
        {
            int docID = item.DocID;
            var invoiceItem = item.CDS_Document.DerivedDocument.ParentDocument.InvoiceItem;
            try
            {
                var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.A0202Outbound, $"{(Naming.InvoiceProcessType)item.ProcessType}-{DateTime.Now:yyyyMMddHHmmssf}-{invoiceItem.TrackCode}{invoiceItem.No}.xml");
                var xmlMIG = invoiceItem.CreateA0202();
                if (xmlMIG == null)
                {
                    return -1; // No A0102 to write
                }

                item.PushStepLogOnSubmit(models, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);

                models.SubmitChanges();
                xmlMIG.Save(fileName);

                PopupQueueItem(item);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return docID;
        }

        private int WriteA0301ToTurnkey(DataProcessQueue item)
        {
            int docID = item.DocID;
            var invoiceItem = item.CDS_Document.InvoiceItem;
            try
            {
                var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.A0301Outbound, $"A0301-{DateTime.Now:yyyyMMddHHmmssf}-{invoiceItem.TrackCode}{invoiceItem.No}.xml");
                var xmlMIG = invoiceItem.CreateA0301();
                if (xmlMIG == null)
                {
                    return -1; // No A0102 to write
                }

                item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);

                models.SubmitChanges();
                xmlMIG.Save(fileName);

                PopupQueueItem(item);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return docID;
        }

        private int WriteA0302ToTurnkey(DataProcessQueue item)
        {
            int docID = item.DocID;
            var invoiceItem = item.CDS_Document.InvoiceItem;
            try
            {
                var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.A0302Outbound, $"A0302-{DateTime.Now:yyyyMMddHHmmssf}-{invoiceItem.TrackCode}{invoiceItem.No}.xml");
                var xmlMIG = invoiceItem.CreateA0302();
                if (xmlMIG == null)
                {
                    return -1; // No A0102 to write
                }

                item.PushStepLogOnSubmit(models, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);

                models.SubmitChanges();
                xmlMIG.Save(fileName);

                PopupQueueItem(item);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return docID;
        }

        private int WriteA0201ToTurnkey(DataProcessQueue item)
        {
            int docID = item.DocID;
            var invoiceItem = item.CDS_Document.DerivedDocument.ParentDocument.InvoiceItem;
            try
            {
                var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.A0201Outbound, $"A0201-{DateTime.Now:yyyyMMddHHmmssf}-{invoiceItem.TrackCode}{invoiceItem.No}.xml");
                var xmlMIG = invoiceItem.CreateA0201();
                if (xmlMIG == null)
                {
                    return -1; // No A0102 to write
                }

                item.PushStepLogOnSubmit(models, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);

                models.SubmitChanges();
                xmlMIG.Save(fileName);

                PopupQueueItem(item);

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return docID;
        }

        public void WriteB0201ToTurnkey()
        {
            DataProcessQueue? item;
            while ((item = GetReadyItem(this, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.B0201)) != null)
            {
                WriteB0201ToTurnkey(item);
            }
        }

        private int WriteB0201ToTurnkey(DataProcessQueue item)
        {
            int docID = item.DocID;
            var allowance = item.CDS_Document.DerivedDocument.ParentDocument.InvoiceAllowance;
            try
            {
                var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.B0201Outbound, $"{(Naming.InvoiceProcessType)item.ProcessType}-{DateTime.Now:yyyyMMddHHmmssf}-{allowance.AllowanceID}.xml");
                var xmlMIG = allowance?.CreateB0201();
                if (xmlMIG == null)
                {
                    return -1; // No B0201 to write
                }

                item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);

                models.SubmitChanges();
                xmlMIG.Save(fileName);

                PopupQueueItem(item);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return docID;
        }

        public void WriteB0202ToTurnkey()
        {
            DataProcessQueue? item;
            while ((item = GetReadyItem(this, Naming.InvoiceStepDefinition.待傳送, Naming.InvoiceProcessType.B0202)) != null)
            {
                WriteB0202ToTurnkey(item);
            }
        }

        private int WriteB0202ToTurnkey(DataProcessQueue item)
        {
            int docID = item.DocID;
            var allowance = item.CDS_Document.DerivedDocument.ParentDocument.InvoiceAllowance;
            try
            {
                var fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.B0202Outbound, $"{(Naming.InvoiceProcessType)item.ProcessType}-{DateTime.Now:yyyyMMddHHmmssf}-{allowance.AllowanceID}.xml");
                var xmlMIG = allowance?.CreateB0202();
                if (xmlMIG == null)
                {
                    return -1; // No B0202 to write
                }

                item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done, xmlMIG.OuterXml);

                models.SubmitChanges();
                xmlMIG.Save(fileName);

                PopupQueueItem(item);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return docID;
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

        public void PrepareStep(DataProcessQueue item, Naming.InvoiceStepDefinition targetStep, Naming.InvoiceProcessType targetProcessType)
        {
            item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done);
            item.CDS_Document.PushStepQueueOnSubmit(models, targetStep, targetProcessType);
            models.SubmitChanges();

            PopupQueueItem(item);
        }

        public void FinishStep(DataProcessQueue item)
        {
            item.CDS_Document.PushLogOnSubmit(models, (Naming.InvoiceStepDefinition)item.StepID, Naming.DataProcessStatus.Done, processType: (Naming.InvoiceProcessType)item.ProcessType);
            models.SubmitChanges();

            PopupQueueItem(item);
        }

        private void PopupQueueItem(DataProcessQueue item)
        {
            models.ExecuteCommand("delete [proc].DataProcessQueue where DocID={0} and StepID={1} and ProcessType = {2}",
                item.DocID, item.StepID, item.ProcessType);
        }

        public static void PushA0101StepQueueOnSubmit(GenericManager<EIVOEntityDataContext> models, CDS_Document docItem, Naming.InvoiceStepDefinition stepID)
        {
            docItem.PushStepQueueOnSubmit(models, stepID, Naming.InvoiceProcessType.A0101);
        }

        public static DataProcessQueue? GetReadyItem(GenericManager<EIVOEntityDataContext> models, CDS_Document docItem, Naming.InvoiceStepDefinition stepID, Naming.InvoiceProcessType processType)
        {
            lock (typeof(InvoiceHandler))
            {
                DateTime available = DateTime.Now.AddMinutes(-5);
                DataProcessQueue? item = models.GetTable<DataProcessQueue>()
                    .Where(q => q.DocID == docItem.DocID)
                    .Where(q => q.StepID == (int)stepID)
                    .Where(q => q.ProcessType == (int)processType)
                    .Where(q => q.DispatchDate < DateTime.Now)
                    .Where(q => !q.BookingTime.HasValue || q.BookingTime < available)
                    .FirstOrDefault();

                if (item != null)
                {
                    if (models.ExecuteCommand("update [proc].DataProcessQueue set BookingTime = GetDate() where DocID={0} and StepID={1} and ProcessType = {2}",
                        item.DocID, item.StepID, item.ProcessType) > 0)
                    {
                        return item;
                    }
                }
            }
            return null;
        }


    }

}
