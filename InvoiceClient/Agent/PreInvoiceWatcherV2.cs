using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

using InvoiceClient.Helper;
using InvoiceClient.Properties;
using ModelCore.Locale;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TXN;
using CommonLib.Core.Utility;
using CommonLib.Utility;

namespace InvoiceClient.Agent
{
    public class PreInvoiceWatcherV2 : InvoiceWatcher
    {
        public PreInvoiceWatcherV2(String fullPath)
            : base(fullPath)
        {

        }

        protected override Root processUpload(eInvoiceServiceClient invSvc, XmlDocument docInv)
        {
            Root result;
            if(PreferredProcessType.HasValue)
            {
                result = invSvc.UploadInvoiceByClient(docInv, Settings.Default.ClientID, (int)_channelID, true, (int)PreferredProcessType).ConvertTo<Root>();
            }
            else
            {
                result = invSvc.UploadInvoiceAutoTrackNoV2(docInv).ConvertTo<Root>();
            }
            return result;
        }

    }
}
