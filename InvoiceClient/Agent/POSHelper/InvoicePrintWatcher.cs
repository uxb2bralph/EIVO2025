using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

using InvoiceClient.Helper;
using InvoiceClient.Properties;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TXN;
using CommonLib.Core.Utility;
using CommonLib.Utility;

namespace InvoiceClient.Agent.POSHelper
{
    public class InvoicePrintWatcher : InvoiceWatcher
    {
        public InvoicePrintWatcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            var result = invSvc.UploadInvoiceV2(docInv).ConvertTo<Root>();
            return result;
        }

    }
}
