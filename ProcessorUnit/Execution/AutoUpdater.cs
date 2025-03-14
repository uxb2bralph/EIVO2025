using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Helper;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using CommonLib.Utility;
using ModelCore.InvoiceManagement;
using ClosedXML.Excel;
using System.Net;

using ProcessorUnit.Properties;
using ModelCore.Models.ViewModel;
using Business.Helper.ReportProcessor;
using System.Diagnostics;
using CommonLib.Core.Utility;

namespace ProcessorUnit.Execution
{
    public class AutoUpdater : ExecutorForeverBase
    {
        const String UpdateBatch = "AutoUpdate.bat";
        public AutoUpdater()
        {

        }

        protected override void DoSomething()
        {
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, UpdateBatch)))
            {
                RunBatch(UpdateBatch,null);
                Program.Terminate();
            }

            base.DoSomething();
        }

        public static Process RunBatch(String batchFileName, String args)
        {
            Logger.Info($"{batchFileName} {args}");
            ProcessStartInfo info = new ProcessStartInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, batchFileName), args)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            return Process.Start(info);
        }
    }
}
