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
    public class ERPInvoiceParser
    {

        String[] _seller = null;
        XElement _root, _invoice = null;

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

                        switch (column[0].ToUpper())
                        {
                            case "H":
                                _seller = column;
                                break;

                            case "M":
                                if (column.Length < 15)
                                {
                                    Array.Resize(ref column, 15);
                                }
                                buildInvoice(column);
                                break;

                            case "D":
                                if (column.Length < 6)
                                {
                                    Array.Resize(ref column, 6);
                                }
                                buildInvoiceDetails(column);
                                break;
                        }
                    }
                }
            }

            return _root;
        }

        private void buildInvoice(string[] column)
        {
            if (_root == null)
            {
                _root = new XElement("InvoiceRoot");
            }

            DateTime invoiceDate = DateTime.MaxValue;
            DateTime.TryParse(column[2], out invoiceDate);

            _invoice = new XElement("Invoice",
                    new XElement("InvoiceNumber", column[1]),
                    new XElement("InvoiceDate", $"{invoiceDate:yyyy/MM/dd}"),
                    new XElement("InvoiceTime", $"{invoiceDate:HH:mm:ss}"),
                    new XElement("InvoiceType", column[3]),
                    new XElement("SellerId", _seller[1]),
                    new XElement("BuyerId", column[4]),
                    new XElement("BuyerName", column[5]),
                    new XElement("Address", column[6]),
                    new XElement("DonateMark", "0"),
                    new XElement("PrintMark", "Y"),
                    new XElement("TaxType", column[7]),
                    new XElement("TaxType", column[7]),
                    new XElement("TaxRate", column[8]),
                    new XElement("SalesAmount", column[9]),
                    new XElement("TaxAmount", column[10]),
                    new XElement("TotalAmount", column[11]),
                    new XElement("CustomsClearanceMark", column[12].GetEfficientString()),
                    new XElement("BondedAreaConfirm", column[13].GetEfficientString()),
                    new XElement("MainRemark", column[14]));

            _root.Add(_invoice);
        }

        private void buildInvoiceDetails(string[] column)
        {
            if (_invoice == null)
            {
                return;
            }

            XElement item = new XElement("InvoiceItem",
                            new XElement("Description", column[1]),
                            new XElement("Quantity", column[2]),
                            new XElement("UnitPrice", column[3]),
                            new XElement("Amount", column[4]),
                            new XElement("Remark", column[5]));

            _invoice.Add(item);
        }

        public static XElement ConvertToXml(String csvFile, Encoding encoding = null)
        {
            return (new ERPInvoiceParser()).ParseData(csvFile, encoding ?? Encoding.UTF8);
        }
    }
}
