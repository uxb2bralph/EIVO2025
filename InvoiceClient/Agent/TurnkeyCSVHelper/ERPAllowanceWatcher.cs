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
using System.Diagnostics;
using System.Globalization;
using ModelCore.Locale;
using CsvHelper;
using ModelCore.UploadManagement;
using XmlLib;
using System.Xml.Linq;
using ModelCore.Helper;

namespace InvoiceClient.Agent.TurnkeyCSVHelper
{
    public class ERPAllowanceWatcher : AllowanceWatcherV2
    {
        protected String _errorFile;

        public ERPAllowanceWatcher(String fullPath) : base(fullPath)
        {

        }


        protected override XmlDocument prepareInvoiceDocument(string invoiceFile)
        {
            XElement root = ERPAllowanceParser.ConvertToXml(invoiceFile);
            return root.ToXmlDocument();
        }
        protected override bool processError(IEnumerable<RootResponseInvoiceNo> rootInvoiceNo, XmlDocument docInv, string fileName)
        {
            if (rootInvoiceNo != null && rootInvoiceNo.Count() > 0)
            {
                IEnumerable<String> message = rootInvoiceNo.Select(i => String.Format("開立折讓號碼:{0}=>{1}", i.Value, i.Description));
                Logger.Warn(String.Format("在上傳開立折讓({0})時,傳送失敗!!原因如下:\r\n{1}", fileName, String.Join("\r\n", message.ToArray())));

            }
            return true;
        }
    }
}
