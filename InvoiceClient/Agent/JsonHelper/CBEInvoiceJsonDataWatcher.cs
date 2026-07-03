using System.IO;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using InvoiceClient.TransferManagement;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using ModelCore.Schema.TXN;

namespace InvoiceClient.Agent.JsonHelper
{
    public class CBEInvoiceJsonDataWatcher : JsonDataWatcher<Root>
    {
        public CBEInvoiceJsonDataWatcher(string fullPath)
            : base(fullPath)
        {
            ApiUrl = $"{ServerInspector.ServiceInfo.TaskCenterUrl}/InvoiceService/UploadInvoiceAutoTrackNo";
            PreferredProcessType = Naming.InvoiceProcessType.F0401_Json_CBE;
        }

        protected override Root? processUpload(string requestFile)
        {
            InvoiceRequestViewModel request = requestFile.DeserializeObjectFromFile<InvoiceRequestViewModel>();
            request.AccessToken = ServerInspector.ServiceInfo.AgentToken;
            request.ProcessType = PreferredProcessType;
            return UploadJsonTo(request.JsonStringify(), ApiUrl);
        }

        protected override void LogFault(string fault, string failedPath, string fileName)
        {
            File.WriteAllText(Path.Combine(failedPath, $"{fileName}.err"), fault);
        }
    }
}
