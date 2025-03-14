using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using ModelCore.DataEntity;
using ModelCore.Properties;
using CommonLib.Utility;

using ModelCore.Helper;
using ModelCore.Locale;

using ModelCore.InvoiceManagement.InvoiceProcess;
using ModelCore.Models.ViewModel;
using CommonLib.Helper;
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement
{
    public static partial class EIVOTurnkeyFactory
    {
        private static QueuedProcessHandler __Handler;
        private static List<Task> __Tasks = new List<Task>();

        public static Func<XmlDocument, bool> Sign
        {
            get;
            set;
        }

        public static String DefaultUserCarrierType
        {
            get;
            set;
        } = ModelExtension.Properties.AppSettings.Default.DefaultUserCarrierType;

        public static Func<String, SignedCms> SignCms
        {
            get;
            set;
        }

        public static EventHandler SendNotification
        {
            get;
            set;
        }


        public static Action<NotifyToProcessID> NotifyIssuedInvoice
        {
            get;
            set;
        }

        public static Action<NotifyToProcessID> NotifyWinningInvoice
        {
            get;
            set;
        }

        public static Action<OrganizationViewModel> NotifyLowerInvoiceNoStock
        {
            get;
            set;
        }

        public static Action<OrganizationViewModel> NotifyInvoiceNotUpload
        {
            get;
            set;
        }

        public static Action<int> NotifyIssuedAllowance
        {
            get;
            set;
        }


        public static Action<NotifyToProcessID> NotifyIssuedInvoiceCancellation
        {
            get;
            set;
        }

        public static Action<int> NotifyIssuedAllowanceCancellation
        {
            get;
            set;
        }


        public static Action<NotifyToProcessID> NotifyIssuedA0401
        {
            get;
            set;
        }

        public static Action<int> NotifyToReceiveA0401
        {
            get;
            set;
        }

        public static EventHandler<EventArgs<NotifyToProcessID>> NotifyCommissionedToReceive
        {
            get;
            set;
        }

        public static Action<NotifyToProcessID> NotifyCommissionedToReceiveA0401
        {
            get;
            set;
        }

        public static EventHandler<EventArgs<NotifyToProcessID>> NotifyCommissionedToReceiveInvoiceCancellation
        {
            get;
            set;
        }

        //public static QueuedProcessHandler CheckTurnkey { get; set; } = new QueuedProcessHandler
        //{
        //    Process = () =>
        //    {
        //        Task.Run(() =>
        //        {
        //            using (InvoiceManager models = new InvoiceManager())
        //            {
        //                using (TurnKey2DataContext turnkeyDB = new TurnKey2DataContext())
        //                {
        //                    var items = models.GetTable<C0401DispatchQueue>().Where(q => q.StepID == (int)Naming.InvoiceStepDefinition.MIG_P);
        //                    for (var item = items.FirstOrDefault(); item != null;)
        //                    {
        //                        var invoice = item.CDS_Document.InvoiceItem;
        //                        var log = turnkeyDB.GetTable<V_Invoice>()
        //                                .Where(i => i.TrackCode == invoice.TrackCode)
        //                                .Where(i => i.No == invoice.No)
        //                                .Where(i => i.DocType == "C0401" || i.DocType == "A0401")
        //                                .OrderByDescending(i => i.MESSAGE_DTS)
        //                                .FirstOrDefault();

        //                        if (log != null)
        //                        {
        //                            if (log.STATUS == "C")
        //                            {
        //                                invoice.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_C, Naming.DataProcessStatus.Done);
        //                                Console.WriteLine($"{invoice.TrackCode}{invoice.No}:C0401:C");
        //                            }
        //                            else
        //                            {
        //                                invoice.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_E, Naming.DataProcessStatus.Done);
        //                                Console.WriteLine($"{invoice.TrackCode}{invoice.No}:C0401:E");
        //                            }
        //                            models.SubmitChanges();
        //                            models.ExecuteCommand("delete [proc].C0401DispatchQueue where DocID={0} and StepID={1}",
        //                                item.DocID, item.StepID);
        //                        }

        //                        item = items.Where(c => c.DocID > item.DocID)
        //                                        .FirstOrDefault();
        //                    }
        //                }
        //            }
        //        });

        //        Task.Run(() =>
        //        {
        //            using (InvoiceManager models = new InvoiceManager())
        //            {
        //                using (TurnKey2DataContext turnkeyDB = new TurnKey2DataContext())
        //                {
        //                    var items = models.GetTable<C0501DispatchQueue>().Where(q => q.StepID == (int)Naming.InvoiceStepDefinition.MIG_P);
        //                    for (var item = items.FirstOrDefault(); item != null;)
        //                    {
        //                        var invoice = item.CDS_Document.DerivedDocument.ParentDocument.InvoiceItem;
        //                        var log = turnkeyDB.GetTable<V_Invoice>()
        //                                .Where(i => i.TrackCode == invoice.TrackCode)
        //                                .Where(i => i.No == invoice.No)
        //                                .Where(i => i.DocType == "C0501" || i.DocType == "A0501")
        //                                .OrderByDescending(i => i.MESSAGE_DTS)
        //                                .FirstOrDefault();

        //                        if (log != null)
        //                        {
        //                            if (log.STATUS == "C")
        //                            {
        //                                item.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_C, Naming.DataProcessStatus.Done);
        //                                Console.WriteLine($"{invoice.TrackCode}{invoice.No}:C0501:C");
        //                            }
        //                            else
        //                            {
        //                                item.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_E, Naming.DataProcessStatus.Done);
        //                                Console.WriteLine($"{invoice.TrackCode}{invoice.No}:C0501:E");
        //                            }
        //                            models.SubmitChanges();
        //                            models.ExecuteCommand("delete [proc].C0501DispatchQueue where DocID={0} and StepID={1}",
        //                                item.DocID, item.StepID);
        //                        }

        //                        item = items.Where(c => c.DocID > item.DocID)
        //                                        .FirstOrDefault();
        //                    }
        //                }
        //            }
        //        });

        //        Task.Run(() =>
        //        {
        //            using (InvoiceManager models = new InvoiceManager())
        //            {
        //                using (TurnKey2DataContext turnkeyDB = new TurnKey2DataContext())
        //                {
        //                    var items = models.GetTable<D0401DispatchQueue>().Where(q => q.StepID == (int)Naming.InvoiceStepDefinition.MIG_P);
        //                    for (var item = items.FirstOrDefault(); item != null;)
        //                    {
        //                        var invoice = item.CDS_Document.InvoiceAllowance;
        //                        var allowanceNo = invoice.AllowanceNumber;
        //                        var log = turnkeyDB.GetTable<V_Allowance>()
        //                                .Where(i => i.AllowanceNo == allowanceNo)
        //                                .Where(i => i.DocType == "D0401" || i.DocType == "B0401")
        //                                .OrderByDescending(i => i.MESSAGE_DTS)
        //                                .FirstOrDefault();

        //                        if (log != null)
        //                        {
        //                            if (log.STATUS == "C")
        //                            {
        //                                invoice.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_C, Naming.DataProcessStatus.Done);
        //                                Console.WriteLine($"{allowanceNo}:D0401:C");
        //                            }
        //                            else
        //                            {
        //                                invoice.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_E, Naming.DataProcessStatus.Done);
        //                                Console.WriteLine($"{allowanceNo}:D0401:E");
        //                            }
        //                            models.SubmitChanges();
        //                            models.ExecuteCommand("delete [proc].D0401DispatchQueue where DocID={0} and StepID={1}",
        //                                item.DocID, item.StepID);
        //                        }

        //                        item = items.Where(c => c.DocID > item.DocID)
        //                                        .FirstOrDefault();
        //                    }
        //                }
        //            }
        //        });

        //        Task.Run(() =>
        //        {
        //            using (InvoiceManager models = new InvoiceManager())
        //            {
        //                using (TurnKey2DataContext turnkeyDB = new TurnKey2DataContext())
        //                {
        //                    var items = models.GetTable<D0501DispatchQueue>().Where(q => q.StepID == (int)Naming.InvoiceStepDefinition.MIG_P);
        //                    for (var item = items.FirstOrDefault(); item != null;)
        //                    {
        //                        var invoice = item.CDS_Document.DerivedDocument.ParentDocument.InvoiceAllowance;
        //                        var allowanceNo = invoice.AllowanceNumber;
        //                        var log = turnkeyDB.GetTable<V_Allowance>()
        //                                .Where(i => i.AllowanceNo == allowanceNo)
        //                                .Where(i => i.DocType == "D0501" || i.DocType == "A0501")
        //                                .OrderByDescending(i => i.MESSAGE_DTS)
        //                                .FirstOrDefault();

        //                        if (log != null)
        //                        {
        //                            if (log.STATUS == "C")
        //                            {
        //                                item.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_C, Naming.DataProcessStatus.Done);
        //                                Console.WriteLine($"{allowanceNo}:D0501:C");
        //                            }
        //                            else
        //                            {
        //                                item.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_E, Naming.DataProcessStatus.Done);
        //                                Console.WriteLine($"{allowanceNo}:D0501:E");
        //                            }
        //                            models.SubmitChanges();
        //                            models.ExecuteCommand("delete [proc].D0501DispatchQueue where DocID={0} and StepID={1}",
        //                                item.DocID, item.StepID);
        //                        }

        //                        item = items.Where(c => c.DocID > item.DocID)
        //                                        .FirstOrDefault();
        //                    }
        //                }
        //            }
        //        });

        //        Task.Run(() =>
        //        {
        //            using (InvoiceManager models = new InvoiceManager())
        //            {
        //                using (TurnKey2DataContext turnkeyDB = new TurnKey2DataContext())
        //                {
        //                    var items = models.GetTable<A0401DispatchQueue>().Where(q => q.StepID == (int)Naming.InvoiceStepDefinition.MIG_P);
        //                    for (var item = items.FirstOrDefault(); item != null;)
        //                    {
        //                        var invoice = item.CDS_Document.InvoiceItem;
        //                        var log = turnkeyDB.GetTable<V_Invoice>()
        //                                .Where(i => i.TrackCode == invoice.TrackCode)
        //                                .Where(i => i.No == invoice.No)
        //                                .Where(i => i.DocType == "C0401" || i.DocType == "A0401")
        //                                .OrderByDescending(i => i.MESSAGE_DTS)
        //                                .FirstOrDefault();

        //                        if (log != null)
        //                        {
        //                            if (log.STATUS == "C")
        //                            {
        //                                invoice.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_C, Naming.DataProcessStatus.Done);
        //                                Console.WriteLine($"{invoice.TrackCode}{invoice.No}:A0401:C");
        //                            }
        //                            else
        //                            {
        //                                invoice.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_E, Naming.DataProcessStatus.Done);
        //                                Console.WriteLine($"{invoice.TrackCode}{invoice.No}:A0401:E");
        //                            }
        //                            models.SubmitChanges();
        //                            models.ExecuteCommand("delete [proc].A0401DispatchQueue where DocID={0} and StepID={1}",
        //                                item.DocID, item.StepID);
        //                        }

        //                        item = items.Where(c => c.DocID > item.DocID)
        //                                        .FirstOrDefault();
        //                    }
        //                }
        //            }
        //        });

        //        Task.Run(() =>
        //        {
        //            using (InvoiceManager models = new InvoiceManager())
        //            {
        //                using (TurnKey2DataContext turnkeyDB = new TurnKey2DataContext())
        //                {
        //                    var items = models.GetTable<A0501DispatchQueue>().Where(q => q.StepID == (int)Naming.InvoiceStepDefinition.MIG_P);
        //                    for (var item = items.FirstOrDefault(); item != null;)
        //                    {
        //                        var invoice = item.CDS_Document.DerivedDocument.ParentDocument.InvoiceItem;
        //                        var log = turnkeyDB.GetTable<V_Invoice>()
        //                                .Where(i => i.TrackCode == invoice.TrackCode)
        //                                .Where(i => i.No == invoice.No)
        //                                .Where(i => i.DocType == "C0501" || i.DocType == "A0501")
        //                                .OrderByDescending(i => i.MESSAGE_DTS)
        //                                .FirstOrDefault();

        //                        if (log != null)
        //                        {
        //                            if (log.STATUS == "C")
        //                            {
        //                                item.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_C, Naming.DataProcessStatus.Done);
        //                                Console.WriteLine($"{invoice.TrackCode}{invoice.No}:A0501:C");
        //                            }
        //                            else
        //                            {
        //                                item.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_E, Naming.DataProcessStatus.Done);
        //                                Console.WriteLine($"{invoice.TrackCode}{invoice.No}:A0501:E");
        //                            }
        //                            models.SubmitChanges();
        //                            models.ExecuteCommand("delete [proc].A0501DispatchQueue where DocID={0} and StepID={1}",
        //                                item.DocID, item.StepID);
        //                        }

        //                        item = items.Where(c => c.DocID > item.DocID)
        //                                        .FirstOrDefault();
        //                    }
        //                }
        //            }
        //        });

        //        Task.Run(() =>
        //        {
        //            using (InvoiceManager models = new InvoiceManager())
        //            {
        //                using (TurnKey2DataContext turnkeyDB = new TurnKey2DataContext())
        //                {
        //                    var items = models.GetTable<B0401DispatchQueue>().Where(q => q.StepID == (int)Naming.InvoiceStepDefinition.MIG_P);
        //                    for (var item = items.FirstOrDefault(); item != null;)
        //                    {
        //                        var invoice = item.CDS_Document.InvoiceAllowance;
        //                        var allowanceNo = invoice.AllowanceNumber;
        //                        var log = turnkeyDB.GetTable<V_Allowance>()
        //                                .Where(i => i.AllowanceNo == allowanceNo)
        //                                .Where(i => i.DocType == "D0401" || i.DocType == "B0401")
        //                                .OrderByDescending(i => i.MESSAGE_DTS)
        //                                .FirstOrDefault();

        //                        if (log != null)
        //                        {
        //                            if (log.STATUS == "C")
        //                            {
        //                                invoice.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_C, Naming.DataProcessStatus.Done);
        //                                Console.WriteLine($"{allowanceNo}:B0401:C");
        //                            }
        //                            else
        //                            {
        //                                invoice.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_E, Naming.DataProcessStatus.Done);
        //                                Console.WriteLine($"{allowanceNo}:B0401:E");
        //                            }
        //                            models.SubmitChanges();
        //                            models.ExecuteCommand("delete [proc].B0401DispatchQueue where DocID={0} and StepID={1}",
        //                                item.DocID, item.StepID);
        //                        }

        //                        item = items.Where(c => c.DocID > item.DocID)
        //                                        .FirstOrDefault();
        //                    }
        //                }
        //            }
        //        });

        //        Task.Run(() =>
        //        {
        //            using (InvoiceManager models = new InvoiceManager())
        //            {
        //                using (TurnKey2DataContext turnkeyDB = new TurnKey2DataContext())
        //                {
        //                    var items = models.GetTable<B0501DispatchQueue>().Where(q => q.StepID == (int)Naming.InvoiceStepDefinition.MIG_P);
        //                    for (var item = items.FirstOrDefault(); item != null;)
        //                    {
        //                        var invoice = item.CDS_Document.DerivedDocument.ParentDocument.InvoiceAllowance;
        //                        var allowanceNo = invoice.AllowanceNumber;
        //                        var log = turnkeyDB.GetTable<V_Allowance>()
        //                                .Where(i => i.AllowanceNo == allowanceNo)
        //                                .Where(i => i.DocType == "D0501" || i.DocType == "A0501")
        //                                .OrderByDescending(i => i.MESSAGE_DTS)
        //                                .FirstOrDefault();

        //                        if (log != null)
        //                        {
        //                            if (log.STATUS == "C")
        //                            {
        //                                item.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_C, Naming.DataProcessStatus.Done);
        //                                Console.WriteLine($"{allowanceNo}:B0501:C");
        //                            }
        //                            else
        //                            {
        //                                item.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_E, Naming.DataProcessStatus.Done);
        //                                Console.WriteLine($"{allowanceNo}:B0501:E");
        //                            }
        //                            models.SubmitChanges();
        //                            models.ExecuteCommand("delete [proc].B0501DispatchQueue where DocID={0} and StepID={1}",
        //                                item.DocID, item.StepID);
        //                        }

        //                        item = items.Where(c => c.DocID > item.DocID)
        //                                        .FirstOrDefault();
        //                    }
        //                }
        //            }
        //        });

        //    },
        //    PeriodInMinutes = 60,

        //};

        static EIVOTurnkeyFactory()
        {
            lock (typeof(EIVOTurnkeyFactory))
            {
                if (__Handler == null && ModelExtension.Properties.AppSettings.Default.EnableEIVOPlatform)
                {
                    //System.Diagnostics.Debugger.Launch();

                    __Handler = new QueuedProcessHandler(FileLogger.Logger)
                    {
                        MaxWaitingCount = 8,
                        Process = () =>
                        {
                            C0401Handler.WriteToTurnkeyInBatches();
                            A0101Handler.ReceiveFiles();

                            //__Tasks.Add(Task.Run(() =>
                            //{
                            //    using (InvoiceManager models = new InvoiceManager())
                            //    {
                            //        C0401Handler c0401 = new C0401Handler(models);
                            //        c0401.NotifyIssued();
                            //    }
                            //}));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    C0501Handler c0501 = new C0501Handler(models);
                                    //c0501.NotifyIssued();
                                    c0501.WriteToTurnkey();
                                }
                            }));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    D0401Handler d0401 = new D0401Handler(models);
                                    //d0401.NotifyIssued();
                                    d0401.WriteToTurnkey();
                                }
                            }));
                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    D0501Handler d0501 = new D0501Handler(models);
                                    //d0501.NotifyIssued();
                                    d0501.WriteToTurnkey();
                                }

                            }));
                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    A0401Handler a0401 = new A0401Handler(models);
                                    //a0401.ProcessToIssue();
                                    //a0401.NotifyToReceive();
                                    //a0401.NotifyIssued();
                                    a0401.WriteToTurnkey();
                                    //a0401.MatchDocumentAttachment();
                                }

                            }));
                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    A0501Handler a0501 = new A0501Handler(models);
                                    //a0501.ProcessToIssue();
                                    //a0501.NotifyIssued();
                                    a0501.WriteToTurnkey();
                                }

                            }));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    A0101Handler a0101 = new A0101Handler(models);
                                    a0101.WriteToTurnkey();
                                }

                            }));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    B0401Handler b0401 = new B0401Handler(models);
                                    //b0401.NotifyIssued();
                                    b0401.WriteToTurnkey();
                                }

                            }));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    B0501Handler b0501 = new B0501Handler(models);
                                    //b0501.NotifyIssued();
                                    b0501.WriteToTurnkey();
                                }

                            }));


                            var t = Task.Factory.ContinueWhenAll(__Tasks.ToArray(), ts =>
                            {
                                Logger.Info($"EIVOTurnkeyFatory finished:{DateTime.Now}");
                                __Tasks.Clear();
                            });
                            t.Wait();

                            //__Tasks[5] = Task.Run(() =>
                            //{
                            //    using (InvoiceManager models = new InvoiceManager())
                            //    {

                            //    }

                            //});
                            //__Tasks[6] = Task.Run(() =>
                            //{
                            //    using (InvoiceManager models = new InvoiceManager())
                            //    {

                            //    }

                            //});
                            //using (InvoiceManager models = new InvoiceManager())
                            //{
                            //    C0401Handler c0401 = new C0401Handler(models);
                            //    c0401.NotifyIssued();
                            //    c0401.WriteToTurnkey();


                            //    C0501Handler c0501 = new C0501Handler(models);
                            //    c0501.NotifyIssued();
                            //    c0501.WriteToTurnkey();

                            //    D0401Handler d0401 = new D0401Handler(models);
                            //    d0401.NotifyIssued();
                            //    d0401.WriteToTurnkey();

                            //    D0501Handler d0501 = new D0501Handler(models);
                            //    d0501.NotifyIssued();
                            //    d0501.WriteToTurnkey();

                            //    A0401Handler a0401 = new A0401Handler(models);
                            //    a0401.ProcessToIssue();
                            //    a0401.NotifyToReceive();
                            //    a0401.NotifyIssued();
                            //    a0401.WriteToTurnkey();
                            //    a0401.MatchDocumentAttachment();

                            //    A0501Handler a0501 = new A0501Handler(models);
                            //    a0501.ProcessToIssue();
                            //    a0501.NotifyIssued();
                            //    a0501.WriteToTurnkey();
                            //}
                        }
                    };
                }
            }
        }

        public static EventHandler<EventArgs<InvoiceItem>> NotifyReceivedInvoice
        {
            get;
            set;
        }

        public static int ResetBusyCount()
        {
            return __Handler.ResetBusyCount();
        }

        public static void Notify()
        {
            if (ModelExtension.Properties.AppSettings.Default.EnableEIVOPlatform)
                __Handler.Notify();
        }

        public static dynamic CurrentStatus =>
            new
            {
                Enabled = ModelExtension.Properties.AppSettings.Default.EnableEIVOPlatform,
                HasInstance = __Handler != null,
                MaxWaitingCount = __Handler?.MaxWaitingCount ?? -1,
                ReportDate = DateTime.Now,
            };

        //private static void notifyToProcess(object stateInfo)
        //{
        //    try
        //    {
        //        processEventQueue();
        //        Logger.Info("傳送至IFS資料處理完成!!");
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex);
        //    }
        //}

        //private static void processEventQueue()
        //{
        //    EIVOPlatformManager mgr = new EIVOPlatformManager();

        //    //傳送待傳送資料
        //    mgr.TransmitInvoice();
        //    //自動接收
        //    mgr.CommissionedToReceive();
        //    //自動開立
        //    mgr.CommissionedToIssue();
        //    mgr.MatchDocumentAttachment();
        //    mgr.NotifyToProcess();
        //}
    }
}
