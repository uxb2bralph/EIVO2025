using ProcessorUnit.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using CommonLib.Core.Utility;
using ModelCore.DataEntity;
using ModelCore.ProcessorUnitHelper;
using CommonLib.Utility;

namespace ProcessorUnit
{
    internal class Program
    {
        static void Main(string[] args)
        {
            InitializeApp.StartUp();
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey();
            } while (key.Key != ConsoleKey.Q);
            Logger.Info("Process terminated..");
        }

        internal static void Terminate()
        {
            Process.GetCurrentProcess().Kill();
        }
    }

    public static class InitializeApp
    {

        private static void Initialize()
        {
            Console.WriteLine($"Settings:{ProcessorUnit.Properties.AppSettings.Default.JsonStringify()}");
            using (ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>())
            {
                var item = ProcessorUnit.Properties.AppSettings.Default.InstanceID.RegisterProcessorUnit(models);
                ProcessorUnit.Properties.AppSettings.Default.ProcessorID = item.ProcessorID;
                Console.WriteLine($"Instance ID:{ProcessorUnit.Properties.AppSettings.Default.InstanceID}");
                Console.WriteLine($"Processor ID:{item.ProcessorID}");
            }
        }


        public static void StartUp()
        {
            FileLogger.Logger.OutputWriter = Console.Out;
            Logger.Info($"Process start at {DateTime.Now}");

            /// SSL憑證信任設定
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

            Initialize();
            //Process proc = Process.GetCurrentProcess();
            //proc.EnableRaisingEvents = true;
            //proc.Exited += Proc_Exited;
            //(new InvoiceExcelRequestProcessor
            //{
            //    ChainedExecutor = new InvoiceExcelRequestForCBEProcessor
            //    {
            //        ChainedExecutor = new InvoiceExcelRequestForVACProcessor
            //        {
            //            ChainedExecutor = new InvoiceExcelRequestForIssuerProcessor
            //            {
            //                ChainedExecutor = new VoidInvoiceExcelRequestProcessor
            //                {
            //                    ChainedExecutor = new AllowanceExcelRequestProcessor
            //                    {
            //                        ChainedExecutor = new VoidAllowanceExcelRequestProcessor { },
            //                    },
            //                }
            //            }
            //        }
            //    }
            //}).ReadyToGo();

            ExecutorForeverBase processorStart = new InvoiceExcelRequestProcessor
            {

            };

            ExecutorForeverBase chainedProcessor = new InvoiceExcelRequestForCBEProcessor
            {

            };

            processorStart.ChainedExecutor = chainedProcessor;

            chainedProcessor.ChainedExecutor = new InvoiceExcelRequestForVACProcessor
            {

            };
            chainedProcessor = chainedProcessor.ChainedExecutor;

            chainedProcessor.ChainedExecutor = new InvoiceExcelRequestForIssuerProcessor
            {

            };
            chainedProcessor = chainedProcessor.ChainedExecutor;

            chainedProcessor.ChainedExecutor = new InvoiceExcelRequestForIssuerA0401Processor
            {

            };
            chainedProcessor = chainedProcessor.ChainedExecutor;

            chainedProcessor.ChainedExecutor = new VoidInvoiceExcelRequestProcessor
            {

            };
            chainedProcessor = chainedProcessor.ChainedExecutor;

            chainedProcessor.ChainedExecutor = new AllowanceExcelRequestProcessor
            {

            };
            chainedProcessor = chainedProcessor.ChainedExecutor;

            chainedProcessor.ChainedExecutor = new AllowanceJsonRequestProcessor
            {
            };
            chainedProcessor = chainedProcessor.ChainedExecutor;

            chainedProcessor.ChainedExecutor = new FullAllowanceExcelRequestProcessor
            {
            };
            chainedProcessor = chainedProcessor.ChainedExecutor;


            chainedProcessor.ChainedExecutor = new VoidAllowanceExcelRequestProcessor { };
            chainedProcessor = chainedProcessor.ChainedExecutor;

            chainedProcessor.ChainedExecutor = new ProcessExceptionNotificationProcessor { };
            chainedProcessor = chainedProcessor.ChainedExecutor;

            chainedProcessor.ChainedExecutor = new InvoiceJsonRequestForCBEProcessor { };
            chainedProcessor = chainedProcessor.ChainedExecutor;

            chainedProcessor.ChainedExecutor = new VoidInvoiceJsonRequestProcessor { };
            chainedProcessor = chainedProcessor.ChainedExecutor;

            chainedProcessor.ChainedExecutor = new UnassignedInvoiceNOSettlementProcessor { };
            chainedProcessor = chainedProcessor.ChainedExecutor;

            chainedProcessor.ChainedExecutor = new DataReportProcessor { };
            chainedProcessor = chainedProcessor.ChainedExecutor;

            chainedProcessor.ChainedExecutor = new AutoUpdater { };
            chainedProcessor = chainedProcessor.ChainedExecutor;

            processorStart.ReadyToGo();

            //(new InvoiceExcelRequestForIssuerProcessor
            //{

            //}).ReadyToGo();

        }

        //private static void Proc_Exited(object sender, EventArgs e)
        //{
        //    ProcessorUnit.Properties.AppSettings.Default.Save();
        //}
    }
}
