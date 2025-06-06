﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading;
using System.Xml;

using InvoiceClient.Helper;
using InvoiceClient.Properties;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TXN;
using CommonLib.Core.Utility;
using CommonLib.Utility;

namespace InvoiceClient.Agent
{
    public class CsvInvoiceWatcherV2ForProxy : CsvInvoiceWatcher
    {

        public CsvInvoiceWatcherV2ForProxy(String fullPath)
            : base(fullPath)
        {

        }


        protected override Root processUpload(eInvoiceServiceClient invSvc, SignedCms docInv)
        {
            var result = invSvc.UploadInvoiceCmsCSVAutoTrackNoV2(docInv.Encode()).ConvertTo<Root>();
            return result;
        }

    }
}
