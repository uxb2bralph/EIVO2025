using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

using InvoiceClient.Helper;
using InvoiceClient.Properties;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TXN;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using CsvHelper;

using System.Xml.Linq;

namespace InvoiceClient.Agent.CsvRequestHelper
{
    public class CsvC0401RequestWatcher : CsvInvoiceRequestWatcher
    {
        public CsvC0401RequestWatcher(String fullPath)
            : base(fullPath)
        {

        }

        protected String CheckBuyerId(String buyerId)
        {
            return buyerId == "00000000"
                ? "0000000000"
                : buyerId;
        }

        protected override void processUpload()
        {
            _root = new XElement("InvoiceRoot");
            foreach (var invRow in _master)
            {
                _invoice = new XElement("Invoice",
                    new XElement("InvoiceNumber", invRow[0]),
                    new XElement("InvoiceDate", invRow[1]),
                    new XElement("InvoiceTime", invRow[2]),
                    new XElement("SellerId", invRow[3].PadLeft(8, '0')),
                    new XElement("BuyerName", invRow[13]),
                    new XElement("BuyerId", CheckBuyerId(invRow[12].PadLeft(8, '0'))),
                    new XElement("InvoiceType", "07"),
                    new XElement("BuyerMark", invRow[22]),
                    new XElement("CustomsClearanceMark", invRow[24]),
                    new XElement("DonateMark", !String.IsNullOrEmpty(invRow[29]) && "1Yy".Contains(invRow[29]) ? "1" : "0"),
                    new XElement("CarrierType", invRow[30]),
                    new XElement("CarrierId1", invRow[31]),
                    new XElement("CarrierId2", invRow[32]),
                    new XElement("PrintMark", "1Yy".Contains(invRow[33]) ? "Y" : "N"),
                    new XElement("NPOBAN", invRow[34]),
                    new XElement("RandomNumber", invRow[35].GetEfficientString() ?? invRow[0].GetEfficientString()?.Right(4)),
                    new XElement("SalesAmount", invRow[36]),
                    new XElement("FreeTaxSalesAmount", invRow[37]),
                    new XElement("ZeroTaxSalesAmount", invRow[38]),
                    new XElement("TaxType", invRow[39]),
                    new XElement("TaxRate", invRow[40]),
                    new XElement("TaxAmount", invRow[41]),
                    new XElement("TotalAmount", invRow[42]),
                    new XElement("Currency", invRow[46]),
                    new XElement("DiscountAmount", invRow[43]),
                    new XElement("CustomerID", invRow[19]),
                    new XElement("ContactName", invRow[15]),
                    new XElement("EMail", invRow[18]),
                    new XElement("Address", invRow[14]),
                    new XElement("Phone", invRow[16]),
                    new XElement("MainRemark", invRow[23]),
                    new XElement("DataNumber", ExtractDataNo(invRow[23])),
                    new XElement(new XElement("Contact",
                        new XElement("Name", invRow[15]),
                        new XElement("Address", invRow[14]),
                        new XElement("TEL", invRow[16]),
                        new XElement("Email", invRow[18]))),
                    new XElement(new XElement("CustomerDefined",
                        new XElement("ProjectNo", !String.IsNullOrEmpty(invRow[50]) && "1Yy".Contains(invRow[50]) ? null : "NoPrintReceipt")))
                );

                int seqNo = 1;
                foreach(var col in _details.Where(d => d[0] == invRow[0]))
                {
                    _invoice.Add(new XElement("InvoiceItem",
                        new XElement("Description", col[1]),
                        new XElement("Quantity", col[2]),
                        new XElement("Unit", col[3]),
                        new XElement("UnitPrice", col[4]),
                        new XElement("Amount", col[5]),
                        new XElement("SequenceNumber", seqNo++),
                        new XElement("Item", col[7]),
                        new XElement("Remark", col[6])
                    ));
                }

                _root.Add(_invoice);
            }
        }

        protected override void processFile(string invFile)
        {
            base.processFile(invFile);

        }


    }
}
