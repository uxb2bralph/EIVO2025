using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;

using InvoiceClient.Helper;
using InvoiceClient.Properties;

using ModelCore.Schema.EIVO;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TXN;
using Newtonsoft.Json;
using CommonLib.Core.Utility;
using CommonLib.Utility;

namespace InvoiceClient.Agent.POSHelper
{
    public class ReprintReceiptWatcher : InvoiceWatcher
    {
        public ReprintReceiptWatcher(String fullPath)
            : base(fullPath)
        {

        }

        private Root processUploadCore(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            Root result = new Root
            {
                UXB2B = "電子發票系統",
                Result = new RootResult
                {
                    timeStamp = DateTime.Now,
                    value = 0
                }
            };

            if (docInv?.DocumentElement.Name == "InvoiceRoot")
            {
                docInv.Save(Path.Combine(POSReady._Settings.ReprintInvoice, $"{Guid.NewGuid()}.xml"));
                result.Result.value = 1;
            }
            else if (docInv?.DocumentElement.Name == "AllowanceRoot")
            {
                docInv.Save(Path.Combine(POSReady._Settings.ReprintAllowance, $"{Guid.NewGuid()}.xml"));
                result.Result.value = 1;
            }
            else
            {
                result.Result.message = "列印檔案資料格式錯誤";
            }

            return result;
        }

        protected override XmlDocument prepareInvoiceDocument(string invoiceFile)
        {
            invoiceFile.ReviseXmlContent();
            return base.prepareInvoiceDocument(invoiceFile);
        }

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            return processUploadCore(invSvc, docInv);
        }
    }
}
