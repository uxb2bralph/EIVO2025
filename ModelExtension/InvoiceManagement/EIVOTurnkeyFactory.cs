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
                        },
                        MilliSecondsWait = 10000,
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
