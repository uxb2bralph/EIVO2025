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
        private static QueuedProcessHandler? __Handler;
        private static List<Task> __Tasks = new List<Task>();

        public static Func<XmlDocument, bool>? Sign
        {
            get;
            set;
        }

        public static String? DefaultUserCarrierType
        {
            get;
            set;
        } = ModelExtension.Properties.AppSettings.Default.DefaultUserCarrierType;

        public static Func<String, SignedCms>? SignCms
        {
            get;
            set;
        }

        public static EventHandler? SendNotification
        {
            get;
            set;
        }


        public static Action<NotifyToProcessID>? NotifyIssuedInvoice
        {
            get;
            set;
        }

        public static Action<NotifyToProcessID>? NotifyWinningInvoice
        {
            get;
            set;
        }

        public static Action<OrganizationViewModel>? NotifyLowerInvoiceNoStock
        {
            get;
            set;
        }

        public static Action<OrganizationViewModel>? NotifyInvoiceNotUpload
        {
            get;
            set;
        }

        public static Action<int>? NotifyIssuedAllowance
        {
            get;
            set;
        }


        public static Action<NotifyToProcessID>? NotifyIssuedInvoiceCancellation
        {
            get;
            set;
        }

        public static Action<int>? NotifyIssuedAllowanceCancellation
        {
            get;
            set;
        }


        public static Action<NotifyToProcessID>? NotifyIssuedA0401
        {
            get;
            set;
        }

        public static Action<int>? NotifyToReceiveA0401
        {
            get;
            set;
        }

        public static EventHandler<EventArgs<NotifyToProcessID>>? NotifyCommissionedToReceive
        {
            get;
            set;
        }

        public static Action<NotifyToProcessID>? NotifyCommissionedToReceiveA0401
        {
            get;
            set;
        }

        public static EventHandler<EventArgs<NotifyToProcessID>>? NotifyCommissionedToReceiveInvoiceCancellation
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
                    __Handler = new QueuedProcessHandler(FileLogger.Logger)
                    {
                        MaxWaitingCount = 8,
                        Process = () =>
                        {

                            __Tasks.Add(Task.Run(() =>
                            {
                                Parallel.For(0, ModelExtension.Properties.AppSettings.Default.CommonParallelProcessCount, idx =>
                                {
                                    using (InvoiceManager models = new InvoiceManager())
                                    {
                                        InvoiceHandler c0401 = new InvoiceHandler(models);
                                        c0401.WriteF0401ToTurnkey();
                                    }
                                });
                            }));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    InvoiceHandler c0501 = new InvoiceHandler(models);
                                    //c0501.NotifyIssued();
                                    c0501.WriteF0501ToTurnkey();
                                }
                            }));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    InvoiceHandler d0401 = new InvoiceHandler(models);
                                    //d0401.NotifyIssued();
                                    d0401.WriteG0401ToTurnkey();
                                }
                            }));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    InvoiceHandler d0501 = new InvoiceHandler(models);
                                    //d0501.NotifyIssued();
                                    d0501.WriteG0501ToTurnkey();
                                }

                            }));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    InvoiceHandler handler = new InvoiceHandler(models);
                                    handler.WriteA0101ToTurnkey();
                                }

                            }));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    InvoiceHandler handler = new InvoiceHandler(models);
                                    handler.WriteA0102ToTurnkey();
                                }
                            }));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    InvoiceHandler handler = new InvoiceHandler(models);
                                    handler.WriteA0301ToTurnkey();
                                }
                            }));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    InvoiceHandler handler = new InvoiceHandler(models);
                                    handler.WriteA0302ToTurnkey();
                                }
                            }));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    InvoiceHandler handler = new InvoiceHandler(models);
                                    handler.WriteA0201ToTurnkey();
                                }
                            }));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    InvoiceHandler handler = new InvoiceHandler(models);
                                    handler.WriteA0202ToTurnkey();
                                }
                            }));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    InvoiceHandler handler = new InvoiceHandler(models);
                                    handler.CompleteReceivedA0302();
                                }
                            }));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    InvoiceHandler handler = new InvoiceHandler(models);
                                    handler.WriteB0101ToTurnkey();
                                }

                            }));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    InvoiceHandler handler = new InvoiceHandler(models);
                                    handler.WriteB0102ToTurnkey();
                                }
                            }));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    InvoiceHandler handler = new InvoiceHandler(models);
                                    handler.WriteB0201ToTurnkey();
                                }

                            }));

                            __Tasks.Add(Task.Run(() =>
                            {
                                using (InvoiceManager models = new InvoiceManager())
                                {
                                    InvoiceHandler handler = new InvoiceHandler(models);
                                    handler.WriteB0202ToTurnkey();
                                }

                            }));



                            var t = Task.Factory.ContinueWhenAll(__Tasks.ToArray(), ts =>
                            {
                                Logger.Info($"EIVOTurnkeyFatory finished:{DateTime.Now}");
                                __Tasks.Clear();
                            });
                            t.Wait();
                            
                        },
                        MilliSecondsWait = ModelExtension.Properties.AppSettings.Default.TaskDelayInMilliseconds,
                    };
                }
            }
        }

        public static EventHandler<EventArgs<InvoiceItem>>? NotifyReceivedInvoice
        {
            get;
            set;
        }

        public static int ResetBusyCount()
        {
            return __Handler?.ResetBusyCount() ?? -1;
        }

        public static void Notify()
        {
            if (ModelExtension.Properties.AppSettings.Default.EnableEIVOPlatform)
                __Handler?.Notify();
        }

        public static dynamic CurrentStatus =>
            new
            {
                Enabled = ModelExtension.Properties.AppSettings.Default.EnableEIVOPlatform,
                HasInstance = __Handler != null,
                MaxWaitingCount = __Handler?.MaxWaitingCount ?? -1,
                ReportDate = DateTime.Now,
            };
    }
}
