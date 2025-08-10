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
    public class ERPInvoiceParser
    {

        String[]? _seller = null;
        XElement? _root, _invoice = null;

        public Naming.InvoiceProcessType PreferredProcessType { get; set; } = Naming.InvoiceProcessType.F0401;

        // Invoice column index enum for CSV parsing
        private enum CSV_M
        {
            Code = 0,
            InvoiceNumber = 1,
            InvoiceDate = 2,
            InvoiceType = 3,
            BuyerId = 4,
            BuyerName = 5,
            BuyerAddress = 6,
            TaxType = 7,
            TaxRate = 8,
            SalesAmount = 9,
            TaxAmount = 10,
            TotalAmount = 11,
            CustomsClearanceMark = 12,
            BondedAreaConfirm = 13,
            MainRemark = 14,
            ZeroTaxRateReason = 15,
            RelatedNumber = 16,
            BuyerCustomerNumber = 17,
            BuyerRemark = 18,
            SummaryMark = 19,
            ExchangeRate = 20,
            Currency = 21
        }

        // Invoice detail column index enum for CSV parsing
        private enum CSV_D
        {
            Code = 0,
            Description = 1,
            Quantity = 2,
            UnitPrice = 3,
            Amount = 4,
            Remark = 5,
            Unit = 6,
            RelatedNumber = 7
        }

        // Seller column index enum for CSV parsing
        private enum CSV_H
        {
            Code = 0,
            SellerId = 1,
            SellerName = 2,
            SellerAddress = 3,
            SellerPhone = 4,
            InvoiceProcessType = 5
        }

        int CSV_H_Length = Enum.GetNames(typeof(CSV_H)).Length;
        int CSV_M_Length = Enum.GetNames(typeof(CSV_M)).Length;
        int CSV_D_Length = Enum.GetNames(typeof(CSV_D)).Length;

        String taxType = "1"; // Default tax type

        public XElement? ParseData(String fileName, Encoding encoding)
        {
            _seller = null;
            _root = _invoice = null;

            using (StreamReader sr = new StreamReader(fileName, encoding))
            {
                String[] column;
                String? line;
                List<String> lines = new List<string>();
                // Read the file and display it line by line.
                while ((line = sr.ReadLine()) != null)
                {
                    lines.Clear();
                    column = line.FromCsvLine(container: lines);
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
                            if (column.Length < CSV_M_Length)
                            {
                                Array.Resize(ref column, CSV_M_Length);
                            }
                            buildInvoice(column);
                            break;

                        case "D":
                            if (column.Length < CSV_D_Length)
                            {
                                Array.Resize(ref column, CSV_D_Length);
                            }
                            buildInvoiceDetails(column);
                            break;
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
                _root.Add(new XElement("ProcessType", PreferredProcessType.ToString()));
            }

            DateTime invoiceDate = DateTime.MaxValue;
            DateTime.TryParse(column[(int)CSV_M.InvoiceDate], out invoiceDate);
            taxType = column[(int)CSV_M.TaxType].GetEfficientString() ?? "1";

            _invoice = new XElement("Invoice",
                    new XElement("InvoiceNumber", column[(int)CSV_M.InvoiceNumber]),
                    new XElement("InvoiceDate", $"{invoiceDate:yyyy/MM/dd}"),
                    new XElement("InvoiceTime", $"{invoiceDate:HH:mm:ss}"),
                    new XElement("InvoiceType", column[(int)CSV_M.InvoiceType]),
                    new XElement("SellerId", _seller?[(int)CSV_H.SellerId]),
                    new XElement("BuyerId", column[(int)CSV_M.BuyerId]),
                    new XElement("BuyerName", column[(int)CSV_M.BuyerName]),
                    new XElement("Address", column[(int)CSV_M.BuyerAddress]),
                    new XElement("DonateMark", "0"),
                    new XElement("PrintMark", "Y"),
                    new XElement("TaxType", taxType),
                    new XElement("TaxRate", column[(int)CSV_M.TaxRate] == "5" ? "0.05" : column[(int)CSV_M.TaxRate]),
                    new XElement("SalesAmount", taxType == "1" || taxType == "4" ? column[(int)CSV_M.SalesAmount] : "0"),
                    new XElement("ZeroTaxSalesAmount", taxType == "2" ? column[(int)CSV_M.SalesAmount] : "0"),
                    new XElement("FreeTaxSalesAmount", taxType == "3" ? column[(int)CSV_M.SalesAmount] : "0"),
                    new XElement("TaxAmount", column[(int)CSV_M.TaxAmount]),
                    new XElement("TotalAmount", column[(int)CSV_M.TotalAmount]),
                    new XElement("CustomsClearanceMark", column[(int)CSV_M.CustomsClearanceMark].GetEfficientString()),
                    new XElement("BondedAreaConfirm", column[(int)CSV_M.BondedAreaConfirm].GetEfficientString()),
                    new XElement("BuyerMark", column[(int)CSV_M.BuyerRemark].GetEfficientString()),
                    new XElement("ZeroTaxRateReason", column[(int)CSV_M.ZeroTaxRateReason].GetEfficientString()),
                    new XElement("MainRemark", column[(int)CSV_M.MainRemark]));

            _root.Add(_invoice);
        }

        private void buildInvoiceDetails(string[] column)
        {
            if (_invoice == null)
            {
                return;
            }

            XElement item = new XElement("InvoiceItem",
                            new XElement("Description", column[(int)CSV_D.Description]),
                            new XElement("Quantity", column[(int)CSV_D.Quantity]),
                            new XElement("UnitPrice", column[(int)CSV_D.UnitPrice]),
                            new XElement("Amount", column[(int)CSV_D.Amount]),
                            new XElement("TaxType", taxType),
                            new XElement("Remark", column[(int)CSV_D.Remark]),
                            new XElement("Unit", column[(int)CSV_D.Unit]),
                            new XElement("RelatedNumber", column[(int)CSV_D.RelatedNumber])
            );

            _invoice.Add(item);
        }

        public static XElement? ConvertToXml(String csvFile, Encoding? encoding = null, Naming.InvoiceProcessType processType = Naming.InvoiceProcessType.F0401)
        {
            return (new ERPInvoiceParser() { PreferredProcessType = processType }).ParseData(csvFile, encoding ?? Encoding.UTF8);
        }
    }
}
