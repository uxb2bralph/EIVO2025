using System.IO;
using CommonLib.Core.Utility;
using InvoiceClient.TransferManagement;
using ModelCore.Locale;
using ModelCore.Schema.TXN;

namespace InvoiceClient.Agent.JsonHelper
{
    public class InvoiceCancellationJsonDataWatcher : CBEInvoiceJsonDataWatcher
    {
        public InvoiceCancellationJsonDataWatcher(string fullPath)
            : base(fullPath)
        {
            PreferredProcessType = Naming.InvoiceProcessType.F0501_Json;
            ApiUrl = $"{ServerInspector.ServiceInfo.TaskCenterUrl}/InvoiceService/UploadInvoiceCancellation";
        }
    }
}
