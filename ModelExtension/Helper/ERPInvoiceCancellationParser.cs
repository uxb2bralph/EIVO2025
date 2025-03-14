using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommonLib.Utility;
using System.Globalization;

namespace ModelCore.Helper
{
    public class ERPInvoiceCancellationParser
    {

        String[] _seller = null;
        XElement _root,_invoice = null;

        public XElement ParseData(String fileName, Encoding encoding)
        {
             _seller = null;
            _root = _invoice = null;

            using (StreamReader sr = new StreamReader(fileName, encoding))
            {
                using (CsvParser parser = new CsvParser(sr, CultureInfo.CurrentCulture))
                {
                    String[] column;
                    while (parser.Read())
                    {
                        column = parser.Record;
                        if (column == null || column.Length < 1)
                        {
                            continue;
                        }

                        switch(column[0].ToUpper())
                        {
                            case "H":
                                _seller = column;
                                break;

                            case "M":
                                if (column.Length < 9)
                                {
                                    Array.Resize(ref column, 9);
                                }
                                buildContent(column);
                                break;

                        }
                    }
                }
            }

            return _root;
        }

        private void buildContent(string[] column)
        {
            if (_root == null)
            {
                _root = new XElement("CancelInvoiceRoot");
            }

            String[] dateInfo = column[6]?.Split(' ');

            _invoice = new XElement("CancelInvoice",
                    new XElement("CancelInvoiceNumber", column[1]),
                    new XElement("InvoiceDate", column[2]),
                    new XElement("BuyerId", column[3]),
                    new XElement("SellerId", _seller[1]),
                    new XElement("CancelDate", dateInfo?[0]),
                    new XElement("CancelTime", dateInfo?.Length>1 ? dateInfo[1] : ""),
                    new XElement("CancelReason", column[7]),
                    new XElement("ReturnTaxDocumentNumber", column[8]),
                    new XElement("Remark", ""));

            _root.Add(_invoice);
        }

        public static XElement ConvertToXml(String csvFile,Encoding encoding = null)
        {
            return (new ERPInvoiceCancellationParser()).ParseData(csvFile, encoding ?? Encoding.UTF8);
        }
    }
}
