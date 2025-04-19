using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading;
using System.Web;
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
    public class XlsxVoidInvoiceWatcher : ProcessRequestWatcher
    {

        public XlsxVoidInvoiceWatcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override JsonResult processUpload(String requestFile)
        {
            return UploadTo(requestFile, $"{ServerInspector.ServiceInfo.TaskCenterUrl}/InvoiceData/UploadVoidInvoiceRequestXlsx?keyID={HttpUtility.UrlEncode(ServerInspector.ServiceInfo.AgentToken)}&sender={ServerInspector.ServiceInfo.AgentUID}");
        }

    }

}
