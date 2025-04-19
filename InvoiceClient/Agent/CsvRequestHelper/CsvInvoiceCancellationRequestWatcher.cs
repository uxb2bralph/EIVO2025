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

namespace InvoiceClient.Agent.CsvRequestHelper
{
    public class CsvInvoiceCancellationRequestWatcher : CsvRequestWatcher
    {
        XElement _cancellation, _root;
        public CsvInvoiceCancellationRequestWatcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override void processFile(string invFile)
        {
            base.processFile(invFile);
            _root = _cancellation = null;
            if (DataItems != null && DataItems.Any())
            {
                foreach(var col in DataItems)
                {
                    col[0] = col[0].GetEfficientString();
                }

                processUpload();
                _root.Save(Path.Combine(_ResponsedPath, $"{Path.GetFileNameWithoutExtension(invFile)}.xml"));
            }
        }

        protected virtual void processUpload()
        {
            _root = new XElement("CancelInvoiceRoot");
            foreach (var col in DataItems)
            {
                _cancellation = new XElement("CancelInvoice",
                    new XElement("CancelInvoiceNumber", col[0]),
                    new XElement("InvoiceDate", col[1]),
                    new XElement("BuyerId", col[2]),
                    new XElement("SellerId", col[3]),
                    new XElement("CancelDate", col[4]),
                    new XElement("CancelTime", col[5]),
                    new XElement("CancelReason", col[6]),
                    new XElement("ReturnTaxDocumentNumber", col[7]),
                    new XElement("Remark", col[8])
                );

                _root.Add(_cancellation);
            }
        }


    }
}
