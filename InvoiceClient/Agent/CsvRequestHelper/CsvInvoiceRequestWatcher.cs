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
using XmlLib;
using System.Text.RegularExpressions;

namespace InvoiceClient.Agent.CsvRequestHelper
{
    public class CsvInvoiceRequestWatcher : CsvRequestWatcher
    {
        protected XElement _invoice, _root;
        protected List<String[]> _master, _details;
        public CsvInvoiceRequestWatcher(String fullPath)
            : base(fullPath)
        {

        }

        public String PreparedPrintPath { get; set; }

        protected override void processFile(string invFile)
        {
            String fileName = Path.GetFileName(invFile);
            String originalPath = Path.GetDirectoryName(invFile);

            String master, detail;
            String tmpName = fileName.ToLower();
            if(tmpName.Contains("master"))
            {
                master = fileName;
                detail = master.Replace("master", "detail");
            }
            else if(tmpName.Contains("detail"))
            {
                detail = fileName;
                master = fileName.Replace("detail", "master");
            }
            else
            {
                return;
            }

            String masterPath = Path.Combine(originalPath, master);
            String detailPath = Path.Combine(originalPath, detail);
            String masterLog, detailLog;

            if(File.Exists(masterPath) && File.Exists(detailPath))
            {
                base.processFile(masterPath);
                _master = DataItems;
                masterLog = lastLogPath;

                base.processFile(detailPath);
                _details = DataItems;
                detailLog = lastLogPath;

                _root = _invoice = null;
                if (_master != null && _master.Any() && _details != null && _details.Any())
                {
                    try
                    {
                        foreach (var col in _master)
                        {
                            col[0] = col[0].GetEfficientString();
                        }

                        foreach (var col in _details)
                        {
                            col[0] = col[0].GetEfficientString();
                        }

                        processUpload();
                        _root.Add(new XElement("SourceLog", String.Join(";", masterLog, detailLog)));
                        _root.Add(new XElement("ToFail", this._failedTxnPath));
                        _root.Save(Path.Combine(_ResponsedPath, $"{Path.GetFileNameWithoutExtension(fileName)}.xml"));

                        if (PreparedPrintPath != null)
                        {
                            var docInv = _root.ToXmlDocument();
                            InvoiceRoot invoice = docInv.TrimAll().ConvertTo<InvoiceRoot>();
                            InvoiceRoot stored = new InvoiceRoot
                            {
                                Invoice = invoice.Invoice.Where(i => i.PrintMark == "Y" || i.PrintMark == "y").ToArray()
                            };

                            stored.ConvertToXml().SaveDocumentWithEncoding(Path.Combine(PreparedPrintPath, $"{Path.GetFileNameWithoutExtension(fileName)}.xml"));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        //MainForm.Alert($"檔案：{invFile}\r\n原因：{ex.Message}", "資料處理發生錯誤");
                        PutFailedPairs(masterLog, detailLog);
                    }
                }
                else
                {
                    //restore original request files
                    PutFailedPairs(masterLog, detailLog);
                }
            }
        }

        private void PutFailedPairs(string masterLog, string detailLog)
        {
            String master = Path.Combine(this._failedTxnPath, Path.GetFileName(masterLog));
            if (File.Exists(masterLog))
            {
                storeFile(masterLog, master);
            }

            String detail = Path.Combine(_failedTxnPath, Path.GetFileName(detailLog));
            if (File.Exists(detailLog))
            {
                storeFile(detailLog, detail);
            }

            MainForm.Alert($"檔案：\r\n{master}\r\n{detail}", "資料處理發生錯誤", transferManager: this.TransferManager);
        }

        protected virtual String ExtractDataNo(String data)
        {
            if (data == null)
                return null;

            var match = Regex.Match(data, @"A\d+");
            return match.Success ? match.Value : null;
        }

        protected virtual void processUpload()
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
                    new XElement("BuyerId", invRow[12].PadLeft(8, '0')),
                    new XElement("InvoiceType", "07"),
                    new XElement("BuyerMark", invRow[22]),
                    new XElement("CustomsClearanceMark", invRow[24]),
                    new XElement("DonateMark", "0"),
                    new XElement("PrintMark", "Y"),
                    new XElement("RandomNumber", $"{DateTime.Now.Ticks % 10000:0000}"),
                    new XElement("SalesAmount", invRow[31]),
                    new XElement("TaxType", invRow[32]),
                    new XElement("TaxRate", invRow[33]),
                    new XElement("TaxAmount", invRow[34]),
                    new XElement("TotalAmount", invRow[35]),
                    new XElement("Currency", invRow[39]),
                    new XElement("DiscountAmount", invRow[36]),
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
                    new XElement("BondedAreaConfirm", invRow[30])
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


    }
}
