using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Net;

using InvoiceClient.Properties;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using ModelCore.Schema.TXN;
using InvoiceClient.Helper;
using InvoiceClient.TransferManagement;
using System.Diagnostics;
using System.Threading.Tasks;
using CommonLib.Helper;


namespace InvoiceClient.Helper
{
    public static class ZipPDFFactory
    {
        private static QueuedProcessHandler __Handler;

        private static Queue<String> __ArgsItems;

        static ZipPDFFactory()
        {
            lock (typeof(ZipPDFFactory))
            {
                if (__Handler == null)
                {
                    __ArgsItems = new Queue<string>();

                    __Handler = new QueuedProcessHandler(FileLogger.Logger)
                    {
                        Process = () =>
                        {
                            while (__ArgsItems.Count > 0)
                            {
                                String args;
                                lock(__ArgsItems)
                                {
                                    args = __ArgsItems.Dequeue();
                                }

                                Logger.Info($"zip PDF:{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ZipPDF.bat")} {args}");
                                Process proc = "ZipPDF.bat".RunBatch(args);
                                proc.WaitForExit(300000);
                            }
                        }
                    };
                }
            }
        }

        public static int ResetBusyCount()
        {
            return __Handler.ResetBusyCount();
        }

        public static void Notify(String args)
        {
            lock(__ArgsItems)
            {
                __ArgsItems.Enqueue(args);
            }
            __Handler.Notify();
        }
    }
}
