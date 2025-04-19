using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using System.Xml;

using InvoiceClient.Helper;
using InvoiceClient.Properties;
using InvoiceClient.TransferManagement;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TXN;
using Newtonsoft.Json;
using CommonLib.Core.Utility;
using CommonLib.Utility;


namespace InvoiceClient.Agent
{
    public class XlsxInvoiceWatcher : ProcessRequestWatcher
    {

        public XlsxInvoiceWatcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override JsonResult processUpload(String requestFile)
        {
            List<KeyValuePair<String, String>> queryParams = new List<KeyValuePair<string, string>>();
            queryParams.Add(new KeyValuePair<string, String>("KeyID", ServerInspector.ServiceInfo.AgentToken));
            queryParams.Add(new KeyValuePair<string, String>("Sender", $"{ServerInspector.ServiceInfo.AgentUID}"));
            queryParams.Add(new KeyValuePair<string, String>("ProcessType", $"{(int?)XlsxInvoiceTransferManager.Default?.Settings.DefaultProcessType ?? (int?)ServerInspector.ServiceInfo.DefaultProcessType}"));
            queryParams.Add(new KeyValuePair<string, String>("StoragePath", TxnPath));
            return UploadTo(requestFile, $"{ServerInspector.ServiceInfo.TaskCenterUrl}/InvoiceData/UploadInvoiceRequestXlsx", queryParams);
        }

    }
}
