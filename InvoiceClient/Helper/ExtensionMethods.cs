using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Xml;

using InvoiceClient.Properties;
using ModelCore.Schema.TXN;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using System.Diagnostics;
using InvoiceClient.Agent.POSHelper;
using CommonLib.Security.UseCrypto;
using Microsoft.AspNetCore.StaticFiles;

namespace InvoiceClient.Helper
{
    public static class ExtensionMethods
    {
        public static XmlDocument Sign(this XmlDocument docMsg)
        {

            CryptoUtility.SignXml(docMsg, Settings.Default.SignerCspName,
                Settings.Default.SignerKeyPassword, AppSigner.SignerCertificate);

            return docMsg;
        }

        public static SignedCms Sign(this byte[] data, bool detached = false)
        {
            ContentInfo content = new ContentInfo(data);
            SignedCms signedCms = new SignedCms(content, detached);
            CmsSigner signer = new CmsSigner(AppSigner.SignerCertificate);

            signedCms.ComputeSignature(signer, true);
            return signedCms;
        }

        public static SignedCms SignFile(this String fileName, bool detached = false)
        {
            byte[] data;
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length > Int32.MaxValue)
                {
                    throw new Exception("檔案太大無法處理!!");
                }
                data = new byte[fs.Length];
                fs.Read(data, 0, (int)fs.Length);
            }
            return data.Sign(detached);
        }

        public static Root CreateMessageToken(this object obj, String actionName)
        {
            return new Root
            {
                UXB2B = "電子發票系統",
                Request = new RootRequest
                {
                    actionName = actionName,
                    periodicalIntervalSpecified = true,
                    periodicalInterval = Settings.Default.AutoInvServiceInterval > 0 ? Settings.Default.AutoInvServiceInterval * 60 : 1800,
                    processIndexSpecified = Settings.Default.ProcessArrayIndex > 0,
                    processIndex = Settings.Default.ProcessArrayIndex,
                }
            };
        }

        public static Process RunBatch(this String batchFileName, String args)
        {
            Logger.Info($"{batchFileName} {args}");
            ProcessStartInfo info = new ProcessStartInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, batchFileName), args)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            return Process.Start(info);
        }

        public static bool AssertFile(this String fileName)
        {
            return File.Exists(fileName) && (new FileInfo(fileName)).Length > 0;
        }

        public static void SaveDocumentWithEncoding(this XmlDocument doc, String path, Encoding? encoding = null)
        {
            encoding ??= Encoding.GetEncoding(Settings.Default.OutputEncoding);

            if (encoding.CodePage == 65001)
            {
                doc.Save(path);
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(path, false, encoding))
                {
                    doc.Save(writer);
                }
            }
        }

        public static void ReviseXmlContent(this string invoiceFile)
        {
            var content = File.ReadAllText(invoiceFile, Encoding.GetEncoding(POSReady.Settings.InvoiceFileEncodingPage));
            if (content.IndexOf('&') >= 0)
            {
                StringBuilder sb = new StringBuilder(content);
                sb.Replace("&", "&amp;");
                File.WriteAllText(invoiceFile, sb.ToString(), Encoding.GetEncoding(POSReady.Settings.InvoiceFileEncodingPage));
            }
        }

        public static void StoreFile(this String srcName, String destName)
        {
            try
            {
                if (File.Exists(destName))
                {
                    File.Delete(destName);
                }
                File.Move(srcName, destName);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public static string GetMimeType(this string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out string contentType))
            {
                contentType = "application/octet-stream"; // 預設值
            }
            return contentType;
        }
    }

    public interface ITabWorkItem
    {
        String TabName { get; }
        String TabText { get; }
        void ReportStatus();
    }
}
