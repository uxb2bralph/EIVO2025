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
    public class InvoiceCancellationWatcher : InvoiceWatcher
    {
        public InvoiceCancellationWatcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            var result = invSvc.UploadInvoiceCancellation(docInv).ConvertTo<Root>();
            return result;
        }

        public override String ReportError()
        {
            int count = Directory.GetFiles(_failedTxnPath).Length;
            return count > 0 ? String.Format("{0}筆作廢發票資料傳送失敗!!\r\n", count) : null;
        }

        protected override bool processError(IEnumerable<RootResponseInvoiceNo> rootInvoiceNo, XmlDocument docInv, string fileName)
        {
            if (rootInvoiceNo != null && rootInvoiceNo.Count() > 0)
            {
                IEnumerable<String> message = rootInvoiceNo.Select(i => String.Format("作廢發票號碼:{0}=>{1}", i.Value, i.Description));
                Logger.Warn(String.Format("在上傳作廢發票檔({0})時,傳送失敗!!原因如下:\r\n{1}", fileName, String.Join("\r\n", message.ToArray())));

                CancelInvoiceRoot invoice = docInv.TrimAll().ConvertTo<CancelInvoiceRoot>();
                CancelInvoiceRoot stored = new CancelInvoiceRoot();
                CancelInvoiceRoot retry = new CancelInvoiceRoot();

                if (Settings.Default.RetryCancellationWhenInvoiceNotFound)
                {
                    retry.CancelInvoice = rootInvoiceNo.Where(i => i.ItemIndexSpecified && i.StatusCode == "R01").Select(i => invoice.CancelInvoice[i.ItemIndex]).ToArray();
                    stored.CancelInvoice = rootInvoiceNo.Where(i => i.ItemIndexSpecified 
                            && (i.StatusCode == null || i.StatusCode != "R01")).Select(i => invoice.CancelInvoice[i.ItemIndex]).ToArray();
                }
                else
                {
                    stored.CancelInvoice = rootInvoiceNo.Where(i => i.ItemIndexSpecified).Select(i => invoice.CancelInvoice[i.ItemIndex]).ToArray();
                }

                if (stored.CancelInvoice != null && stored.CancelInvoice.Length > 0)
                    stored.ConvertToXml().SaveDocumentWithEncoding(Path.Combine(_failedTxnPath, $"{DateTime.Now.Ticks}.xml"));
                if (retry.CancelInvoice != null && retry.CancelInvoice.Length > 0)
                    retry.ConvertToXml().SaveDocumentWithEncoding(Path.Combine(_requestPath, $"{DateTime.Now.Ticks}.xml"));
            }
            return true;
        }

        protected override void processError(string message, XmlDocument docInv, string fileName)
        {
            Logger.Warn(String.Format("在上傳作廢發票檔({0})時,傳送失敗!!原因如下:\r\n{1}", fileName, message));
        }

    }
}
