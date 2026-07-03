using System.IO;
using CommonLib.Core.Utility;
using InvoiceClient.TransferManagement;
using ModelCore.Locale;
using ModelCore.Schema.TXN;

namespace InvoiceClient.Agent.JsonHelper
{
    public class AllowanceCancellationJsonDataWatcher : CBEInvoiceJsonDataWatcher
    {
        public AllowanceCancellationJsonDataWatcher(string fullPath)
            : base(fullPath)
        {
            PreferredProcessType = Naming.InvoiceProcessType.G0501_Json;
            ApiUrl = $"{ServerInspector.ServiceInfo.TaskCenterUrl}/InvoiceService/UploadAllowanceCancellation";
        }
    }
}
