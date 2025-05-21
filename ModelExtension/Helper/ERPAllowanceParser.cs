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
using ModelCore.Locale;

namespace ModelCore.Helper
{
    public class ERPAllowanceParser
    {

        String[]? _seller = null;
        XElement? _root,_invoice = null;
        public Naming.InvoiceProcessType PreferredProcessType { get; set; } = Naming.InvoiceProcessType.G0401;

        public XElement? ParseData(String fileName, Encoding encoding)
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
                        column = parser.Record!;
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
                                if (column.Length < 11)
                                {
                                    Array.Resize(ref column, 11);
                                }
                                buildContent(column);
                                break;

                            case "D":
                                if (column.Length < 6)
                                {
                                    Array.Resize(ref column, 6);
                                }
                                buildDetails(column);
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
                _root = new XElement("AllowanceRoot");
            }

            _invoice = new XElement("Allowance",
                            new XElement("AllowanceNumber", column[1]),
                            new XElement("AllowanceDate", column[2]),
                            new XElement("BuyerId", column[3]),
                            new XElement("BuyerName", column[4]),
                            new XElement("Address", column[5]),
                            new XElement("SellerId", column[6]),
                            new XElement("SellerName", column[7]),
                            new XElement("TaxAmount", column[9]),
                            new XElement("TotalAmount", column[10])
                        );

            _root.Add(_invoice);
            _root.Add(new XElement("ProcessType", PreferredProcessType.ToString()));
        }

        private void buildDetails(string[] column)
        {
            if (_invoice == null)
            {
                return;
            }

            XElement item = new XElement("AllowanceItem",
                                new XElement("OriginalDescription", column[1]),
                                new XElement("OriginalInvoiceNumber", column[2]),
                                new XElement("OriginalInvoiceDate", column[3]),
                                new XElement("Quantity", column[4]),
                                new XElement("UnitPrice", column[5]),
                                new XElement("Amount", column[6]),
                                new XElement("Tax", column[7]),
                                new XElement("TaxType", column[8])
                            );

            _invoice.Add(item);
        }

        public static XElement? ConvertToXml(String csvFile,Encoding? encoding = null)
        {
            return (new ERPAllowanceParser()).ParseData(csvFile, encoding ?? Encoding.UTF8);
        }
    }
}
