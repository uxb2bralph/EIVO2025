using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

using InvoiceClient.Helper;
using InvoiceClient.Properties;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TXN;
using CommonLib.Core.Utility;
using CommonLib.Utility;

namespace InvoiceClient.Agent
{
    public class B2BInvoiceWatcher : InvoiceWatcherV2
    {
        public B2BInvoiceWatcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override XmlDocument prepareInvoiceDocument(string invoiceFile)
        {
            XmlDocument docInv = base.prepareInvoiceDocument(invoiceFile);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?><InvoiceRoot></InvoiceRoot>");
            XmlNodeList? nodes = docInv.DocumentElement?.ChildNodes;
            if (nodes != null)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    doc.DocumentElement!.AppendChild(doc.ImportNode(nodes.Item(i)!, true));
                }
            }
            return doc;
        }

        protected override bool processError(IEnumerable<RootResponseInvoiceNo> rootInvoiceNo, XmlDocument docInv, string fileName)
        {
            if (rootInvoiceNo != null && rootInvoiceNo.Count() > 0)
            {
                IEnumerable<String> message = rootInvoiceNo.Select(i => String.Format("發票號碼:{0}=>{1}", i.Value, i.Description));
                Logger.Warn(String.Format("在上傳發票檔({0})時,傳送失敗!!原因如下:\r\n{1}", fileName, String.Join("\r\n", message.ToArray())));

                SellerInvoiceRoot invoice = docInv.TrimAll().ConvertTo<SellerInvoiceRoot>();
                SellerInvoiceRoot stored = new SellerInvoiceRoot();
                stored.Invoice = rootInvoiceNo.Where(i=>i.ItemIndexSpecified).Select(i => invoice.Invoice[i.ItemIndex]).ToArray();
                stored.ConvertToXml().SaveDocumentWithEncoding(Path.Combine(_failedTxnPath, String.Format("{0}-{1:yyyyMMddHHmmssfff}.xml", Path.GetFileNameWithoutExtension(fileName), DateTime.Now)));
            }
            return true;
        }

    }
}
