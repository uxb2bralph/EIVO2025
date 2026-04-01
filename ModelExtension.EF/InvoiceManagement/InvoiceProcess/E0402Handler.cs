using CommonLib.Utility;
using ModelCore.DataEntity;
using ModelCore.Schema.TurnKey.E0402;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelCore.InvoiceManagement.InvoiceProcess
{
    public class E0402Handler
    {
        public static void WriteToTurnkey(BranchTrackBlank item)
        {
            ModelExtension.Properties.AppSettings.Default.E0402Outbound.CheckStoredPath();
            String fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.E0402Outbound, $"E0402-{Process.GetCurrentProcess().Id}-{Thread.CurrentThread.ManagedThreadId}-{DateTime.Now.Ticks}.xml");
            item.ConvertToXml().Save(fileName);
        }
    }
}
