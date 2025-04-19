using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;

using InvoiceClient.Helper;
using InvoiceClient.Properties;
using InvoiceClient.TransferManagement;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TXN;
using Newtonsoft.Json;
using CommonLib.Core.Utility;
using CommonLib.Utility;


namespace InvoiceClient.Agent
{
    public class XmlProcessRequestWatcher : ProcessRequestWatcher
    {
        protected String _waitingForResponse;

        public XmlProcessRequestWatcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override void prepareStorePath(string fullPath)
        {
            base.prepareStorePath(fullPath);

            _waitingForResponse = Path.Combine(Logger.LogPath, $"{Path.GetFileName(fullPath)}(WaitingForResponse)");
            _waitingForResponse.CheckStoredPath();
        }

        protected override void processFile(string invFile)
        {
            if (!File.Exists(invFile))
                return;

            String fileName = Path.GetFileName(invFile);
            String fullPath = Path.Combine(_inProgressPath, fileName);
            try
            {
                File.Move(invFile, fullPath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return;
            }

            try
            {
                XmlDocument docInv = prepareInvoiceDocument(fullPath);
                docInv.Sign();
                String tmpPath = Path.Combine(Logger.LogDailyPath, $"{fileName}(Signed).xml");
                docInv.Save(tmpPath);

                var result = processUpload(tmpPath);

                if (result.result != true)
                {
                    storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
                }
                else
                {
                    storeFile(fullPath, Path.Combine(_waitingForResponse, fileName));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
            }
        }
    }

}
