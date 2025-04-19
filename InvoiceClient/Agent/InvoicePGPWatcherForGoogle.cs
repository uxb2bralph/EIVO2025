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
using System.Diagnostics;
using System.Globalization;
using ModelCore.Locale;

namespace InvoiceClient.Agent
{
    public class InvoicePGPWatcherForGoogle : InvoiceWatcherV2ForGoogle
    {

        public InvoicePGPWatcherForGoogle(String fullPath) : base(fullPath)
        {
            _ResponsedPath = fullPath + "(ResponseToPGP)";
            _ResponsedPath.CheckStoredPath();
        }

        protected override void processFile(String invFile)
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
                Logger.Warn("move file error: " + invFile);
                Logger.Error(ex);
                return;
            }

            if (fullPath.EndsWith(".gpg", StringComparison.CurrentCultureIgnoreCase)
                    || fullPath.EndsWith(".pgp", StringComparison.CurrentCultureIgnoreCase))
            {
                processPGP(fullPath);
                return;
            }

            eInvoiceServiceClient invSvc = CreateInvoiceService();
            Root result = new Root();
            try
            {
                XmlDocument docInv = prepareInvoiceDocument(fullPath);

                docInv.Sign();
                result = processUpload(invSvc, docInv);

                if (result.Result.value != 1)
                {
                    if (result.Response != null && result.Response.InvoiceNo != null && result.Response.InvoiceNo.Length > 0)
                    {
                        processError(result.Response.InvoiceNo, docInv, fileName);
                        storeFile(fullPath, Path.Combine(Logger.LogDailyPath, fileName));
                    }
                    else
                    {
                        processError(result.Result.message, docInv, fileName);
                        storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
                    }
                }
                else
                {
                    storeFile(fullPath, Path.Combine(Logger.LogDailyPath, fileName));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
            }
            finally
            {
                if (result.Automation != null)
                {
                    Automation auto = new Automation { Item = result.Automation };
                    String responseName = fileName.Replace("request", "response")
                            .Replace("_OUT_", "_IN_");
                    responseName = Path.Combine(_ResponsedPath, responseName);
                    auto.ConvertToXml().SaveDocumentWithEncoding(responseName);
                }
            }
        }

        protected void processPGP(String invoiceFile)
        {
            String fileName = Path.GetFileName(invoiceFile);
            String contentFile = fileName;
            contentFile = Path.Combine(_requestPath, contentFile.Substring(0, contentFile.Length - 4));

            //String args = $"{invoiceFile} {contentFile} {Logger.LogDailyPath}";
            //Process proc = "pgp_decrypt.bat".RunBatch(args);
            ////Process proc = Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pgp_decrypt.bat"), args);
            //proc.WaitForExit();
            invoiceFile.DecryptFile(contentFile);
            if(File.Exists(contentFile))
            {
                storeFile(invoiceFile, Path.Combine(Logger.LogDailyPath, fileName));
            }
            else
            {
                storeFile(invoiceFile, Path.Combine(_requestPath, fileName));
            }
        }

    }
}
