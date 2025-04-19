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
using System.Security.Cryptography.Pkcs;

namespace InvoiceClient.Agent
{
    public class InvoiceBuyerWatcher : CsvInvoiceWatcher
    {

        public InvoiceBuyerWatcher(String fullPath)
            : base(fullPath)
        {

        }


        protected override Root processUpload(eInvoiceServiceClient invSvc, SignedCms docInv)
        {
            var result = invSvc.UploadInvoiceBuyerCmsCSV(docInv.Encode()).ConvertTo<Root>();
            return result;
        }

    }
}
