using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading;
using System.Xml;

using InvoiceClient.Helper;
using InvoiceClient.Properties;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TXN;
using CommonLib.Core.Utility;
using CommonLib.Utility;

namespace InvoiceClient.Agent
{
    public class D0401Watcher : AllowanceWatcher
    {

        public D0401Watcher(String fullPath)
            : base(fullPath)
        {

        }


        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
           
            var result = invSvc.UploadAllowance_D0401(docInv).ConvertTo<Root>();
            return result;
        }
        protected override  bool processError(IEnumerable<RootResponseInvoiceNo> rootInvoiceNo, XmlDocument docInv, string fileName)
        {
            if (rootInvoiceNo != null && rootInvoiceNo.Count() > 0)
            {
                IEnumerable<String> message = rootInvoiceNo.Select(i => String.Format("Allowance Number:{0}=>{1}", i.Value, i.Description));
                Logger.Warn(String.Format("在上傳開立折讓({0})時,傳送失敗!!原因如下:\r\n{1}", fileName, String.Join("\r\n", message.ToArray())));

                ModelCore.Schema.TurnKey.D0401.Allowance invoice = docInv.TrimAll().ConvertTo<ModelCore.Schema.TurnKey.D0401.Allowance>();
                ModelCore.Schema.TurnKey.D0401.Allowance stored = docInv.TrimAll().ConvertTo<ModelCore.Schema.TurnKey.D0401.Allowance>();
                
                stored.ConvertToXml().SaveDocumentWithEncoding(Path.Combine(_failedTxnPath, Path.GetFileName(fileName)));
            }
            return true;
        }
    }
}
