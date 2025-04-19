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
    public class InvoiceCancellationWatcherV2ForGoogle : InvoiceWatcherForGoogle
    {
        public InvoiceCancellationWatcherV2ForGoogle(String fullPath)
            : base(fullPath)
        {

        }

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            var result = invSvc.UploadInvoiceCancellationV2(docInv).ConvertTo<Root>();
            return result;
        }
        public override String ReportError()
        {
            int count = Directory.GetFiles(_failedTxnPath).Length;
            return count > 0 ? String.Format("{0} InvoiceCancellation Data Transfer Failure!!\r\n", count) : null;
        }

        protected override bool processError(IEnumerable<RootResponseInvoiceNo> rootInvoiceNo, XmlDocument docInv, string fileName)
        {
            if (rootInvoiceNo != null && rootInvoiceNo.Count() > 0)
            {
                IEnumerable<String> message = rootInvoiceNo.Select(i => String.Format("InvoiceCancellation Number:{0}=>{1}", i.Value, i.Description));
                Logger.Warn(String.Format("Failed to Send an InvoiceCancellation ({0}) When Uploading Files!!For the Following Reasons:\r\n{1}", fileName, String.Join("\r\n", message.ToArray())));

                CancelInvoiceRoot invoice = docInv.TrimAll().ConvertTo<CancelInvoiceRoot>();
                CancelInvoiceRoot stored = new CancelInvoiceRoot();
                stored.CancelInvoice = rootInvoiceNo.Where(i => i.ItemIndexSpecified).Select(i => invoice.CancelInvoice[i.ItemIndex]).ToArray();

                stored.ConvertToXml().SaveDocumentWithEncoding(Path.Combine(_failedTxnPath, fileName));
            }
            return true;
        }

        protected override void processError(string message, XmlDocument docInv, string fileName)
        {
            Logger.Warn(String.Format("Failed to Send an InvoiceCancellation ({0}) When Uploading Files!!For the Following Reasons:\r\n{1}", fileName, message));
        }

    }
}
