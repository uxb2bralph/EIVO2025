using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TXN;
using Newtonsoft.Json;
using CommonLib.Core.Utility;
using CommonLib.Utility;


namespace InvoiceClient.Agent
{
    public class XlsxDelayedInvoiceWatcher : ProcessRequestWatcher
    {

        public XlsxDelayedInvoiceWatcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override JsonResult processUpload(String requestFile)
        {
            if ((DateTime.Today.Month % 2) == 1 && DateTime.Today.Day < 11)
            {
                return UploadTo(requestFile, $"{ServerInspector.ServiceInfo.TaskCenterUrl}/InvoiceData/UploadInvoiceRequestXlsx?keyID={HttpUtility.UrlEncode(ServerInspector.ServiceInfo.AgentToken)}&sender={ServerInspector.ServiceInfo.AgentUID}&processType={(int?)XlsxInvoiceTransferManager.Default?.Settings.DefaultProcessType ?? (int?)ServerInspector.ServiceInfo.DefaultProcessType}&ConditionID={(int)ProcessRequestCondition.ConditionType.UseLastPeriodTrackCodeNo}");
            }
            else
            {
                return new JsonResult { result = false, message = "未在允許期間傳送上期發票!!" };
            }
        }

    }
}
