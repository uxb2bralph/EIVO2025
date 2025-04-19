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
using System.Net;

namespace InvoiceClient.Agent
{
    public class AllowanceContentToPDFWatcher : InvoiceWatcher
    {
        public static readonly String[] ThermalPOSPaper = new string[] { "0 0 162 792" };

        public AllowanceContentToPDFWatcher(String fullPath) : base(fullPath)
        {

        }

        protected override void prepareStorePath(String fullPath)
        {
            _inProgressPath = Path.Combine(fullPath + "(Processing)", $"{Process.GetCurrentProcess().Id}");
            _inProgressPath.CheckStoredPath();
        }

        protected override void processFile(String invFile)
        {
            if (!File.Exists(invFile))
                return;

            String fileName = Path.GetFileName(invFile);
            String fullPath = Path.Combine(_inProgressPath, fileName);

            try
            {
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
                File.Move(invFile, fullPath);
            }
            catch (Exception ex)
            {
                Logger.Warn("move file error: " + invFile);
                Logger.Error(ex);
                return;
            }

            try
            {
                XmlDocument docInv = new XmlDocument();
                docInv.Load(fullPath);

                AllowanceRoot root = docInv.TrimAll().ConvertTo<AllowanceRoot>();
                AllowanceRoot dummy = new AllowanceRoot();

                String tmpPath = Path.Combine(Logger.LogDailyPath, $"{Guid.NewGuid()}");
                tmpPath.CheckStoredPath();

                using (WebClientEx client = new WebClientEx { Timeout = 43200000 })
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/xml";
                    client.Encoding = Encoding.UTF8;
                    foreach (var item in root.Allowance)
                    {
                        dummy.Allowance = new AllowanceRootAllowance[] { item };
                        String tmpHtml = Path.Combine(tmpPath, $"{item.AllowanceNumber}.htm");
                        do
                        {
                            try
                            {
                                File.WriteAllText(tmpHtml, client.UploadString(Settings.Default.ConvertDataToAllowance, dummy.ConvertToXml().OuterXml));
                            }
                            catch(Exception ex)
                            {
                                Logger.Error(ex);
                            }
                        } while (!File.Exists(tmpHtml));
                        String pdfFile = Path.Combine(tmpPath, $"taiwan_uxb2b_scanned_sac_pdf_{item.AllowanceNumber}.pdf");
                        do
                        {
                            tmpPath.CheckStoredPath();
                            Logger.Info($"Allowance Content:{tmpHtml} => {pdfFile}");
                            tmpHtml.ConvertHtmlToPDF(pdfFile, 1);
                        } while (!File.Exists(pdfFile));
                    }
                }

                String args = $"taiwan_uxb2b_scanned_sac_pdf_{DateTime.Now:yyyyMMddHHmmssffff}_{root.Allowance.Length} \"{tmpPath}\" \"{_ResponsedPath}\"";

                Logger.Info($"zip PDF:{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ZipAllowancePDF.bat")} {args}");
                "ZipAllowancePDF.bat".RunBatch(args);

                storeFile(fullPath, Path.Combine(Logger.LogDailyPath, fileName));

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

        }

    }
}
