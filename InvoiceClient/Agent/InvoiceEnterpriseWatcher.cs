using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using InvoiceClient.Properties;
using System.Xml;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using InvoiceClient.Helper;
using ModelCore.Schema.EIVO;
using System.Threading;
using ModelCore.Schema.TXN;

namespace InvoiceClient.Agent
{
    public class InvoiceEnterpriseWatcher : InvoiceWatcher
    {
        public InvoiceEnterpriseWatcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            var result = invSvc.UploadInvoiceEnterprise(docInv).ConvertTo<Root>();
            return result;
        }

        public override String ReportError()
        {
            int count = Directory.GetFiles(_failedTxnPath).Length;
            return count > 0 ? String.Format("{0}筆發票開立營業人資料傳送失敗!!\r\n", count) : null;
        }

        protected override bool processError(IEnumerable<RootResponseInvoiceNo> rootInvoiceNo, XmlDocument docInv, string fileName)
        {
            if (rootInvoiceNo != null && rootInvoiceNo.Count() > 0)
            {
                IEnumerable<String> message = rootInvoiceNo.Select(i => String.Format("發票開立營業人統編:{0}=>{1}", i.Value, i.Description));
                Logger.Warn(String.Format("在上傳發票開立營業人檔({0})時,傳送失敗!!原因如下:\r\n{1}", fileName, String.Join("\r\n", message.ToArray())));

                InvoiceEnterpriseRoot invoice = docInv.TrimAll().ConvertTo<InvoiceEnterpriseRoot>();
                InvoiceEnterpriseRoot stored = new InvoiceEnterpriseRoot();
                stored.InvoiceEnterprise = rootInvoiceNo.Where(i => i.ItemIndexSpecified).Select(i => invoice.InvoiceEnterprise[i.ItemIndex]).ToArray();

                stored.ConvertToXml().SaveDocumentWithEncoding(Path.Combine(_failedTxnPath, fileName));
            }
            return true;
        }

        protected override void processError(string message, XmlDocument docInv, string fileName)
        {
            Logger.Warn(String.Format("在上傳發票開立營業人檔({0})時,傳送失敗!!原因如下:\r\n{1}", fileName, message));
        }

    }
}
