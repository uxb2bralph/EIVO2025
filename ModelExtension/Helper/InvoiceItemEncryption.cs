using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ModelCore.DataEntity;
using CommonLib.Utility;
using CommonLib.Core.Utility;

namespace ModelCore.Helper
{
    public static class InvoiceItemEncryption
    {
        public static String Key
        {
            private set;
            get;
        }

        static InvoiceItemEncryption()
        {
            ResetKey();
        } 

        public static void ResetKey()
        {
            lock (typeof(InvoiceItemEncryption))
            {
                Key = null;
                String keyFile = Path.Combine(Logger.LogPath, "ORCodeKey.txt");
                if (!File.Exists(keyFile))
                    return;

                Key = File.ReadAllText(keyFile);
            }
        }

        public static String EncryptContent(this InvoiceItem item)
        {
            if(!String.IsNullOrEmpty(Key) && item!=null)
            {
                String content = item.TrackCode + item.No + item.RandomNo;
                return EncryptContent(content);
            }
            return null;
        }

        public static String EncryptContent(this String invoiceNo, String randomNo)
        {
            return EncryptContent(invoiceNo + randomNo);
        }


        public static string EncryptContent(this string content)
        {
            com.tradevan.qrutil.QREncrypter qrencrypter = new com.tradevan.qrutil.QREncrypter();
            return qrencrypter.AESEncrypt(content, Key);
        }

        public static string BuildEncryptedData(this InvoiceItem item)
        {
            return item.EncryptContent();
        }
    }
}