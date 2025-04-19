using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

using InvoiceClient.Helper;
using InvoiceClient.Properties;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.TXN;
using CommonLib.Core.Utility;
using CommonLib.Utility;

namespace InvoiceClient.Agent
{
    public class B2BAllowanceCancellationWatcher : AllowanceCancellationWatcher
    {
        public B2BAllowanceCancellationWatcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            var result = invSvc.B2BUploadAllowanceCancellation(docInv).ConvertTo<Root>();
            return result;
        }

        protected override bool processError(IEnumerable<RootResponseInvoiceNo> rootInvoiceNo, XmlDocument docInv, string fileName)
        {
            if (rootInvoiceNo != null && rootInvoiceNo.Count() > 0)
            {
                IEnumerable<String> message = rootInvoiceNo.Select(i => String.Format("作廢折讓證明單號碼:{0}=>{1}", i.Value, i.Description));
                Logger.Warn(String.Format("在上傳作廢折讓證明單檔({0})時,傳送失敗!!原因如下:\r\n{1}", fileName, String.Join("\r\n", message.ToArray())));

                CancelAllowanceRoot invoice = docInv.TrimAll().ConvertTo<CancelAllowanceRoot>();
                CancelAllowanceRoot stored = new CancelAllowanceRoot();
                stored.CancelAllowance = rootInvoiceNo.Where(i => i.ItemIndexSpecified).Select(i => invoice.CancelAllowance[i.ItemIndex]).ToArray();

                stored.ConvertToXml().SaveDocumentWithEncoding(Path.Combine(_failedTxnPath, String.Format("{0}-{1:yyyyMMddHHmmssfff}.xml", Path.GetFileNameWithoutExtension(fileName), DateTime.Now)));
            }
            return true;
        }
    }
}
