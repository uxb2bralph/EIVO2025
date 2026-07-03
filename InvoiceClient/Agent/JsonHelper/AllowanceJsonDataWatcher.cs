using System.IO;
using CommonLib.Core.Utility;
using InvoiceClient.TransferManagement;
using ModelCore.Locale;
using ModelCore.Schema.TXN;

namespace InvoiceClient.Agent.JsonHelper
{
    public class AllowanceJsonDataWatcher : CBEInvoiceJsonDataWatcher
    {
        public AllowanceJsonDataWatcher(string fullPath)
            : base(fullPath)
        {
            PreferredProcessType = Naming.InvoiceProcessType.G0401_Json;
            ApiUrl = $"{ServerInspector.ServiceInfo.TaskCenterUrl}/InvoiceService/UploadAllowance";
        }
    }
}
