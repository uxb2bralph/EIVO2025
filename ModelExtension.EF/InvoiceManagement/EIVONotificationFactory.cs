using CommonLib.Core.Utility;
using CommonLib.Helper;
using CommonLib.Utility;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement.InvoiceProcess;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using ModelExtension.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace ModelCore.InvoiceManagement
{
    public static class EIVONotificationFactory
    {
        public static Func<XmlDocument, bool>? Sign
        {
            get;
            set;
        }

        public static Func<String, SignedCms>? SignCms
        {
            get;
            set;
        }

        public static void NotifyIssuedInvoice(this IEnumerable<int> docID, bool? appendAttachment, String? mailTo = null, bool forceTodo = true)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                foreach (var invoiceID in docID)
                {
                    EIVONotificationFactory.NotifyIssuedInvoice(new RenderStyleViewModel
                    {
                        DocID = invoiceID,
                        AppendAttachment = appendAttachment,
                        MailTo = mailTo,
                        ForceTodo = forceTodo,
                    });
                    Console.WriteLine($"通知開立發票:{invoiceID}完成");
                }
            });
        }

        private static void PushMailQueue(RenderStyleViewModel viewModel)
        {
            string queueItem = Path.Combine(AppSettings.Default.MailQueuePath, $"{viewModel!.DocID}.json");
            if (File.Exists(queueItem))
                return;
            
            File.WriteAllText(queueItem, viewModel.JsonStringify());
        }

        public static bool NotifyIssuedInvoice(RenderStyleViewModel? viewModel, bool notifyImmediately = false)
        {
            if (viewModel == null)
                return false;

            if (notifyImmediately)
            {
                return SendNotification(viewModel);
            }
            else if (viewModel?.DocID > 0)
            {
                viewModel.ProcessType ??= Naming.InvoiceProcessType.F0401;
                viewModel.Title ??= "電子發票開立通知";
                viewModel.MailUrl ??= ModelExtension.Properties.AppSettings.Default.NotifyIssuedInvoiceUrl;
                PushMailQueue(viewModel);
                return true;
            }
            return false;
        }

        public static bool SendNotification(RenderStyleViewModel viewModel)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.UploadString($"{viewModel.MailUrl ?? ModelExtension.Properties.AppSettings.Default.NotifyIssuedInvoiceUrl}", viewModel.JsonStringify());
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Warn($"{viewModel.Title}傳送失敗:\r\n{viewModel.JsonStringify()}");
                    Logger.Error(ex);
                }
            }

            return false;
        }

        public static bool NotifyWinningInvoice(RenderStyleViewModel? viewModel, bool notifyImmediately = false)
        {
            if (viewModel == null)
                return false;
            viewModel.StepID = Naming.InvoiceStepDefinition.中獎通知;
            viewModel.Title = "發票中獎通知";
            viewModel.MailUrl = ModelExtension.Properties.AppSettings.Default.NotifyWinningInvoiceUrl;
            return NotifyIssuedInvoice(viewModel, notifyImmediately);
        }

        public static void NotifyLowerInvoiceNoStock(OrganizationViewModel? viewModel)
        {
            if (viewModel == null)
                return;
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.UploadString($"{ModelExtension.Properties.AppSettings.Default.NotifyLowerInvoiceNoStockUrl}", viewModel.JsonStringify());
                }
                catch (Exception ex)
                {
                    Logger.Warn(String.Format("電子發票可用號碼存量不足通知傳送失敗,ID:{0}", viewModel.CompanyID));
                    Logger.Error(ex);
                }
            }
        }

        public static bool NotifyIssuedAllowance(int? allowanceID, bool notifyImmediately = false)
        {
            return NotifyIssuedInvoice(new RenderStyleViewModel
            {
                DocID = allowanceID,
                StepID = Naming.InvoiceStepDefinition.開立通知,
                ProcessType = Naming.InvoiceProcessType.G0401,
                Title = "電子發票折讓證明開立通知",
                MailUrl = ModelExtension.Properties.AppSettings.Default.NotifyIssuedAllowanceUrl,
            }, notifyImmediately);
        }


        public static bool NotifyIssuedInvoiceCancellation(RenderStyleViewModel? viewModel, bool notifyImmediately = false)
        {
            if (viewModel == null)
                return false;
            if (notifyImmediately)
            {
                return SendNotification(viewModel);
            }
            else if (viewModel?.DocID > 0)
            {
                viewModel.ProcessType ??= Naming.InvoiceProcessType.F0501;
                viewModel.Title = "電子發票作廢通知";
                viewModel.MailUrl = ModelExtension.Properties.AppSettings.Default.NotifyIssuedInvoiceCancellationUrl;
                PushMailQueue(viewModel);
                return true;
            }
            return false;
        }

        public static bool NotifyIssuedAllowanceCancellation(RenderStyleViewModel? viewModel, bool notifyImmediately = false)
        {
            if (viewModel == null)
                return false;
            if (notifyImmediately)
            {
                return SendNotification(viewModel);
            }
            else if (viewModel.DocID > 0)
            {
                viewModel.ProcessType ??= Naming.InvoiceProcessType.G0501;
                viewModel.Title = "電子發票折讓證明作廢通知";
                viewModel.MailUrl = ModelExtension.Properties.AppSettings.Default.NotifyIssuedAllowanceCancellationUrl;
                PushMailQueue(viewModel);
                return true;
            }
            return false;
        }


        public static bool NotifyIssuedA0401(RenderStyleViewModel? viewModel, bool notifyImmediately = false)
        {
            viewModel ??= new RenderStyleViewModel();
            viewModel.PaperStyle = "B2B";
            viewModel.MailUrl = ModelExtension.Properties.AppSettings.Default.NotifyIssuedA0401Url;
            return NotifyIssuedInvoice(viewModel, notifyImmediately);
        }

        public static bool NotifyToReceiveA0401(RenderStyleViewModel? viewModel, bool notifyImmediately = false)
        {
            if (viewModel == null)
                return false;
            viewModel.StepID = Naming.InvoiceStepDefinition.未接收資料待通知;
            viewModel.MailUrl = ModelExtension.Properties.AppSettings.Default.NotifyToReceiveA0401Url;
            return NotifyIssuedInvoice(viewModel, notifyImmediately);
        }

        public static void NotifyIssuedA0401(this IEnumerable<int> docID, String? mailTo = null, bool forceTodo = true)
        {
            ThreadPool.QueueUserWorkItem(t =>
            {
                foreach (var id in docID)
                {
                    EIVONotificationFactory.NotifyIssuedA0401(new RenderStyleViewModel
                    {
                        DocID = id,
                        MailTo = mailTo,
                        ForceTodo = forceTodo
                    });
                }
            });
        }

        public static void NotifyWinningInvoice(this IEnumerable<int> docID, bool? appendAttachment, String? mailTo = null)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                foreach (var invoiceID in docID)
                {
                    EIVONotificationFactory.NotifyWinningInvoice(new RenderStyleViewModel
                    {
                        DocID = invoiceID,
                        AppendAttachment = appendAttachment,
                        MailTo = mailTo,
                    });
                }
            });
        }

        public static void NotifyIssuedInvoiceCancellation(this IEnumerable<int> docID, String? mailTo = null)
        {
            ThreadPool.QueueUserWorkItem(t =>
            {
                foreach (var id in docID)
                {
                    EIVONotificationFactory.NotifyIssuedInvoiceCancellation(new RenderStyleViewModel
                    {
                        DocID = id,
                        MailTo = mailTo,
                    });
                }
            });

        }

        public static void NotifyIssuedAllowance(this IEnumerable<int> docID)
        {
            ThreadPool.QueueUserWorkItem(t =>
            {
                foreach (var id in docID)
                {
                    EIVONotificationFactory.NotifyIssuedAllowance(id);
                }
            });
        }

        public static void NotifyIssuedAllowanceCancellation(this IEnumerable<int> docID)
        {
            ThreadPool.QueueUserWorkItem(t =>
            {
                foreach (var id in docID)
                {
                    EIVONotificationFactory.NotifyIssuedAllowanceCancellation(new RenderStyleViewModel { DocID = id });
                }
            });
        }

        public static void NotifyIssuedInvoiceCancellation(int docID)
        {
            EIVONotificationFactory.NotifyIssuedInvoiceCancellation(new RenderStyleViewModel { DocID = docID }, false);
        }
        public static void NotifyIssuedAllowanceCancellation(int docID)
        {
            EIVONotificationFactory.NotifyIssuedAllowanceCancellation(new RenderStyleViewModel { DocID = docID }, false);
        }

        //public static EventHandler<EventArgs<DocumentQueryViewModel>>? NotifyCommissionedToReceive
        //{
        //    get;
        //    set;
        //}

        //public static bool NotifyCommissionedToReceiveA0401(RenderStyleViewModel? viewModel, bool notifyImmediately = false)
        //{
        //    if (viewModel == null)
        //        return false;
        //    viewModel.StepID = Naming.InvoiceStepDefinition.已接收資料待通知;
        //    viewModel.MailUrl = ModelExtension.Properties.AppSettings.Default.NotifyCommissionedToReceiveA0401Url;
        //    return NotifyIssuedInvoice(viewModel, notifyImmediately);
        //}

        public static void Notify()
        {
            __Handler?.Notify();
        }

        public static EventHandler<EventArgs<DocumentQueryViewModel>>? NotifyCommissionedToReceiveInvoiceCancellation
        {
            get;
            set;
        }

        private static QueuedProcessHandler? __Handler;

        static EIVONotificationFactory()
        {
            lock (typeof(EIVONotificationFactory))
            {
                if (__Handler == null)
                {
                    __Handler = new QueuedProcessHandler(FileLogger.Logger)
                    {
                        MaxWaitingCount = 1,
                        Process = () =>
                        {
                            using (InvoiceManager models = new InvoiceManager())
                            {
                                InvoiceHandler handler = new InvoiceHandler(models);
                                InvoiceHandler.SendMailNotification();
                                handler.NotifyIssuedInvoice();
                                handler.NotifyIssuedAllowance();
                                handler.NotifyIssuedInvoiceCancellation();
                                handler.NotifyIssuedAllowanceCancellation();
                            }
                        },
                        MilliSecondsWait = ModelExtension.Properties.AppSettings.Default.TaskDelayInMilliseconds,
                    };
                }
            }
        }


    }
}
