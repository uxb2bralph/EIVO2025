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
    public class BranchTrackWatcher : InvoiceWatcher
    {
        public BranchTrackWatcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            var result = invSvc.UploadBranchTrack(docInv).ConvertTo<Root>();
            return result;
        }

        public override String ReportError()
        {
            int count = Directory.GetFiles(_failedTxnPath).Length;
            return count > 0 ? String.Format("{0}筆開立發票配號資料傳送失敗!!\r\n", count) : null;
        }

        protected override bool processError(IEnumerable<RootResponseInvoiceNo> rootInvoiceNo, XmlDocument docInv, string fileName)
        {
            if (rootInvoiceNo != null && rootInvoiceNo.Count() > 0)
            {
                IEnumerable<String> message = rootInvoiceNo.Select(i => String.Format("開立發票配號號碼:{0}=>{1}", i.Value, i.Description));
                Logger.Warn(String.Format("在上傳開立發票配號檔({0})時,傳送失敗!!原因如下:\r\n{1}", fileName, String.Join("\r\n", message.ToArray())));

                BranchTrack invoice = docInv.TrimAll().ConvertTo<BranchTrack>();
                BranchTrack stored = new BranchTrack();
                stored.Main = rootInvoiceNo.Where(i => i.ItemIndexSpecified).Select(i => invoice.Main[i.ItemIndex]).ToArray();

                stored.ConvertToXml().SaveDocumentWithEncoding(Path.Combine(_failedTxnPath, fileName));
            }
            return true;
        }

        protected override void processError(string message, XmlDocument docInv, string fileName)
        {
            Logger.Warn(String.Format("在上傳開立發票配號檔({0})時,傳送失敗!!原因如下:\r\n{1}", fileName, message));
        }

    }
}
