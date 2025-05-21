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
    public class ERPAllowanceCancellationParser
    {

        String[]? _buyer = null;
        XElement? _root,_invoice = null;
        public Naming.InvoiceProcessType PreferredProcessType { get; set; } = Naming.InvoiceProcessType.G0501;

        public XElement? ParseData(String fileName, Encoding encoding)
        {
             _buyer = null;
            _root = _invoice = null;

            using (StreamReader sr = new StreamReader(fileName, encoding))
            {
                using (CsvParser parser = new CsvParser(sr,CultureInfo.CurrentCulture))
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
                                _buyer = column;
                                break;

                            case "M":
                                if (column.Length < 8)
                                {
                                    Array.Resize(ref column, 8);
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
                _root = new XElement("CancelAllowanceRoot");
            }

            String[]? dateInfo = column[6]?.Split(' ');

            _invoice = new XElement("CancelAllowance",
                    new XElement("CancelAllowanceNumber", column[1]),
                    new XElement("AllowanceDate", column[2]),
                    new XElement("SellerId", column[3]),
                    new XElement("BuyerId", _buyer?[1]),
                    new XElement("CancelDate", dateInfo?[0]),
                    new XElement("CancelTime", dateInfo?.Length>1 ? dateInfo[1] : ""),
                    new XElement("CancelReason", column[7]),
                    new XElement("Remark", ""));

            _root.Add(_invoice);
            _root.Add(new XElement("ProcessType", PreferredProcessType.ToString()));

        }

        public static XElement? ConvertToXml(String csvFile, Encoding? encoding = null)
        {
            return (new ERPInvoiceCancellationParser()).ParseData(csvFile, encoding ?? Encoding.UTF8);
        }
    }
}
