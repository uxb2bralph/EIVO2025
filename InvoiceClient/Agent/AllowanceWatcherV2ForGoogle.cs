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
    public class AllowanceWatcherV2ForGoogle : InvoicePGPWatcherForGoogle
    {
        private String _pathToPDF;

        public AllowanceWatcherV2ForGoogle(String fullPath)
            : base(fullPath)
        {
            _pathToPDF = fullPath + "(ToPDF)";
            _pathToPDF.CheckStoredPath();
        }

        public String PathToPDF => _pathToPDF;

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            var result = invSvc.UploadAllowanceByClient(docInv, Settings.Default.ClientID, (int)_channelID).ConvertTo<Root>();
            return result;
        }
        public override String ReportError()
        {
            int count = Directory.GetFiles(_failedTxnPath).Length;
            return count > 0 ? String.Format("{0} Allowance Data Transfer Failure!!\r\n", count) : null;
        }

        protected override bool processError(IEnumerable<RootResponseInvoiceNo> rootInvoiceNo, XmlDocument docInv, string fileName)
        {
            if (rootInvoiceNo != null && rootInvoiceNo.Count() > 0)
            {
                IEnumerable<String> message = rootInvoiceNo.Select(i => String.Format("Allowance Number:{0}=>{1}", i.Value, i.Description));
                Logger.Warn(String.Format("Failed to Send an Allowance ({0}) When Uploading Files!!For the Following Reasons:\r\n{1}", fileName, String.Join("\r\n", message.ToArray())));

                AllowanceRoot invoice = docInv.TrimAll().ConvertTo<AllowanceRoot>();
                AllowanceRoot stored = new AllowanceRoot();
                AllowanceRoot tryToPDF = new AllowanceRoot
                {
                    Allowance = rootInvoiceNo.Where(i => i.ItemIndexSpecified && i.StatusCode == "I01").Select(i => invoice.Allowance[i.ItemIndex]).ToArray()
                };
                stored.Allowance = rootInvoiceNo.Where(i => i.ItemIndexSpecified
                    && (i.StatusCode == null || i.StatusCode != "I01")).Select(i => invoice.Allowance[i.ItemIndex]).ToArray();

                if (stored.Allowance != null && stored.Allowance.Length > 0)
                    stored.ConvertToXml().SaveDocumentWithEncoding(Path.Combine(_failedTxnPath, fileName));
                if (tryToPDF.Allowance != null && tryToPDF.Allowance.Length > 0)
                    tryToPDF.ConvertToXml().SaveDocumentWithEncoding(Path.Combine(_pathToPDF, fileName));

            }
            return true;
        }

        protected override void processError(string message, XmlDocument docInv, string fileName)
        {
            Logger.Warn(String.Format("Failed to Send an Allowance ({0}) When Uploading Files!!For the Following Reasons:\r\n{1}", fileName, message));
        }
    
    }
}
