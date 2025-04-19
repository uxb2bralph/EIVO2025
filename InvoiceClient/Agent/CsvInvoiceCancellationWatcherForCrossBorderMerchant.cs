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
using System.Xml.Linq;
using XmlLib;

namespace InvoiceClient.Agent
{
    public class CsvInvoiceCancellationWatcherForCrossBorderMerchant : CsvInvoiceWatcherForCrossBorderMerchant
    {
        enum FieldIndex
        {
            InvoiceDate = 0,
            CancelInvoiceNumber,
            CancelReason,
            EndOfField
        }

        public CsvInvoiceCancellationWatcherForCrossBorderMerchant(String fullPath) : base(fullPath)
        {

        }

        protected override bool validate(CsvLine item)
        {
            var column = item.Columns;
            if (column.Length < (int)FieldIndex.EndOfField)
            {
                item.Status = "data with wrong fields.";
                return false;
            }

            for (int i = 0; i < (int)FieldIndex.EndOfField; i++)
            {
                column[i] = column[i].GetEfficientString();
            }

            if (column[(int)FieldIndex.CancelInvoiceNumber] == null)
            {
                item.Status = "empty Cancel InvoiceNumber.";
                return false;
            }

            return true;
        }

        protected override XmlDocument buildDocument()
        {
            XElement root = new XElement("CancelInvoiceRoot");

            if (_items.Count > 0)
            {
                foreach (var d in _items)
                {
                    XElement invoice = new XElement("CancelInvoice",
                        new XElement("CancelInvoiceNumber", d.Columns[(int)FieldIndex.CancelInvoiceNumber]),
                        new XElement("CancelReason", d.Columns[(int)FieldIndex.CancelReason]),
                        new XElement("CancelDate", $"{DateTime.Now:yyyy/MM/dd}"),
                        new XElement("CancelTime", $"{DateTime.Now:HH:mm:ss}"),
                        new XElement("InvoiceDate", d.Columns[(int)FieldIndex.InvoiceDate]),
                        new XElement("SellerId", Settings.Default.SellerReceiptNo),
                        new XElement("Remark", d.Columns[(int)FieldIndex.CancelReason]),
                        new XElement("LineNo", d.LineIndex));

                    root.Add(invoice);
                }

            }

            return root.ToXmlDocument();
        }

        protected override bool processError(IEnumerable<RootResponseInvoiceNo> rootInvoiceNo, XmlDocument docInv, string fileName)
        {
            if (rootInvoiceNo != null && rootInvoiceNo.Count() > 0)
            {
                IEnumerable<String> message = rootInvoiceNo.Select(i => String.Format("作廢發票號碼:{0}=>{1}", i.Value, i.Description));
                Logger.Warn(String.Format("在上傳作廢發票檔({0})時,傳送失敗!!原因如下:\r\n{1}", fileName, String.Join("\r\n", message.ToArray())));

                CancelInvoiceRoot invoice = docInv.TrimAll().ConvertTo<CancelInvoiceRoot>();
                int errorIdx = 0;
                foreach (var rejectItem in rootInvoiceNo.Where(i => i.ItemIndexSpecified))
                {
                    var err = invoice.CancelInvoice[rejectItem.ItemIndex];
                    if (err.LineNo.HasValue)
                    {
                        var sentItem = _items.Where(i => i.LineIndex == err.LineNo).FirstOrDefault();
                        if (sentItem != null)
                        {
                            sentItem.Status = rejectItem.Description;
                            _items.Remove(sentItem);
                            _errors.Insert(errorIdx++, sentItem);
                        }
                    }
                }

                _errorFile = Path.Combine(_failedTxnPath, Path.GetFileName(fileName));

                foreach (var csvItem in _errors)
                {
                    reportError(csvItem);
                }

            }
            return true;
        }

    }
}
