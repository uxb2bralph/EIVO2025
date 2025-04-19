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

namespace InvoiceClient.Agent
{
    public class CsvInvoiceWatcherForCrossBorderMerchant : InvoiceWatcherV2
    {
        protected String _errorFile;
        protected List<CsvLine> _items;
        protected List<CsvLine> _errors;
        enum FieldIndex
        {
            SerialNumber = 0,
            DataDate,
            CustomerID,
            SequenceNumber,
            Description,
            UnitPrice,
            Quantity,
            Amount,
            Remark,
            BuyerId,
            SalesAmount,
            TaxAmount,
            TotalAmount,
            CurrencyCode,
            BuyerName,
            Email,
            CarrieType,
            CarrieId1,
            CarrieId2,
            EndOfField
        }

        public CsvInvoiceWatcherForCrossBorderMerchant(String fullPath) : base(fullPath)
        {
            _items = new List<CsvLine>();
            _errors = new List<CsvLine>();
        }


        protected override XmlDocument prepareInvoiceDocument(string invoiceFile)
        {
            processCsv(invoiceFile);
            return buildDocument();
        }

        protected virtual XmlDocument buildDocument()
        {
            XElement root = new XElement("InvoiceRoot");

            if (_items.Count > 0)
            {
                var items = _items.GroupBy(d => new { SerialNumber = d.Columns[(int)FieldIndex.SerialNumber], CustomerID = d.Columns[(int)FieldIndex.CustomerID] });

                foreach (var item in items)
                {
                    var d = item.First();
                    XElement invoice = new XElement("Invoice",
                        new XElement("DataNumber", item.Key.SerialNumber + item.Key.CustomerID),
                        new XElement("DataDate", d.Columns[(int)FieldIndex.DataDate]),
                        new XElement("SellerId", Settings.Default.SellerReceiptNo),
                        new XElement("BuyerName", d.Columns[(int)FieldIndex.BuyerName]),
                        new XElement("BuyerId", d.Columns[(int)FieldIndex.BuyerId]),
                        new XElement("InvoiceType", "07"),
                        new XElement("CarrierType", d.Columns[(int)FieldIndex.CarrieType]),
                        new XElement("CarrierId1", d.Columns[(int)FieldIndex.CarrieId1]),
                        new XElement("CarrierId2", d.Columns[(int)FieldIndex.CarrieId2]),
                        new XElement("SalesAmount", d.Columns[(int)FieldIndex.SalesAmount]),
                        new XElement("FreeTaxSalesAmount", 0),
                        new XElement("ZeroTaxSalesAmount", 0),
                        new XElement("TaxType", 1),
                        new XElement("TaxRate", 0.05m),
                        new XElement("TaxAmount", d.Columns[(int)FieldIndex.TaxAmount]),
                        new XElement("TotalAmount", d.Columns[(int)FieldIndex.TotalAmount]),
                        new XElement("LineNo", d.LineIndex));

                    foreach (var v in item)
                    {
                        invoice.Add(new XElement("InvoiceItem",
                            new XElement("Description", v.Columns[(int)FieldIndex.Description]),
                            new XElement("Quantity", v.Columns[(int)FieldIndex.Quantity]),
                            new XElement("UnitPrice", v.Columns[(int)FieldIndex.UnitPrice]),
                            new XElement("Amount", v.Columns[(int)FieldIndex.Amount]),
                            new XElement("SequenceNumber", v.Columns[(int)FieldIndex.SequenceNumber]),
                            new XElement("Remark", v.Columns[(int)FieldIndex.Remark])));
                    }

                    root.Add(invoice);
                }

            }

            return root.ToXmlDocument();
        }

        protected virtual void processCsv(String invoiceFile)
        {
            _items.Clear();
            _errors.Clear();

            parseData(invoiceFile, Encoding.GetEncoding(Settings.Default.CsvEncoding));
        }

        protected override bool processError(IEnumerable<RootResponseInvoiceNo> rootInvoiceNo, XmlDocument docInv, string fileName)
        {
            if (rootInvoiceNo != null && rootInvoiceNo.Count() > 0)
            {
                IEnumerable<String> message = rootInvoiceNo.Select(i => String.Format("發票號碼:{0}=>{1}", i.Value, i.Description));
                Logger.Warn(String.Format("在上傳發票檔({0})時,傳送失敗!!原因如下:\r\n{1}", fileName, String.Join("\r\n", message.ToArray())));

                InvoiceRoot invoice = docInv.TrimAll().ConvertTo<InvoiceRoot>();
                int errorIdx = 0;
                foreach(var rejectItem in rootInvoiceNo.Where(i => i.ItemIndexSpecified))
                {
                    var err = invoice.Invoice[rejectItem.ItemIndex];
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

                foreach(var csvItem in _errors)
                {
                    reportError(csvItem);
                }

            }
            return true;
        }

        void parseData(String fileName, Encoding encoding)
        {
            using (StreamReader sr = new StreamReader(fileName, encoding))
            {
                sr.ReadLine();
                int lineIdx = 0;
                using (CsvParser parser = new CsvParser(sr,CultureInfo.CurrentCulture))
                {
                    String[] column;
                    while (parser.Read())
                    {
                        column = parser.Record!;
                        lineIdx++;
                        CsvLine item = new CsvLine
                        {
                            Columns = column,
                            LineIndex = lineIdx
                        };

                        if(validate(item))
                        {
                            _items.Add(item);
                        }
                        else
                        {
                            _errors.Add(item);
                        }
                    }
                }
            }
        }

        protected void reportError(CsvLine item)
        {
            String[] column = item.Columns;

            if (column!=null && column.Length>0)
            {
                File.AppendAllText(_errorFile,
                    String.Concat(String.Join(",", column), "\r\n",
                        $"=> line {item.LineIndex} => ", item.Status, "\r\n"));
            }
            else
            {
                File.AppendAllText(_errorFile,
                    String.Concat($"line {item.LineIndex} => ", item.Status, "\r\n"));
            }
        }

        protected virtual bool validate(CsvLine item)
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

            if (column[(int)FieldIndex.SerialNumber] == null)
            {
                item.Status = "empty Serial Number.";
                return false;
            }

            return true;

        }
    }
}
