﻿using System;
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
    public class AllowanceCancellationWatcherV2ForGoogle : InvoiceWatcherV2ForGoogle
    {
        public AllowanceCancellationWatcherV2ForGoogle(String fullPath)
            : base(fullPath)
        {

        }

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            var result = invSvc.UploadAllowanceCancellationV2(docInv).ConvertTo<Root>();
            return result;
        }
        public override String ReportError()
        {
            int count = Directory.GetFiles(_failedTxnPath).Length;
            return count > 0 ? String.Format("{0} AllowanceCancellation Data Transfer Failure!!\r\n", count) : null;
        }

        protected override bool processError(IEnumerable<RootResponseInvoiceNo> rootInvoiceNo, XmlDocument docInv, string fileName)
        {
            if (rootInvoiceNo != null && rootInvoiceNo.Count() > 0)
            {
                IEnumerable<String> message = rootInvoiceNo.Select(i => String.Format("AllowanceCancellation Number:{0}=>{1}", i.Value, i.Description));
                Logger.Warn(String.Format("Failed to Send an AllowanceCancellation ({0}) When Uploading Files!!For the Following Reasons:\r\n{1}", fileName, String.Join("\r\n", message.ToArray())));

                CancelAllowanceRoot invoice = docInv.TrimAll().ConvertTo<CancelAllowanceRoot>();
                CancelAllowanceRoot stored = new CancelAllowanceRoot();
                stored.CancelAllowance = rootInvoiceNo.Where(i => i.ItemIndexSpecified).Select(i => invoice.CancelAllowance[i.ItemIndex]).ToArray();

                stored.ConvertToXml().SaveDocumentWithEncoding(Path.Combine(_failedTxnPath, fileName));
            }
            return true;
        }

        protected override void processError(string message, XmlDocument docInv, string fileName)
        {
            Logger.Warn(String.Format("Failed to Send an AllowanceCancellation ({0}) When Uploading Files!!For the Following Reasons:\r\n{1}", fileName, message));
        }

    }
}
