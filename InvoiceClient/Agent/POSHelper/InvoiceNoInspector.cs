using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

using InvoiceClient.Properties;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using ModelCore.Schema.TXN;
using InvoiceClient.Helper;
using InvoiceClient.TransferManagement;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Net;

using ModelCore.Helper;
using System.Security.Cryptography;

namespace InvoiceClient.Agent.POSHelper
{

    public class InvoiceNoInspector : InvoiceServerInspector
    {
        private static InvoiceNoInspector _Instance;

        public InvoiceNoInspector()
        {
            _Instance = this;
        }

        public override void StartUp()
        {
            InvokeService(CheckToPreload);
        }

        private void CheckToPreload()
        {
            if(POSReady.Settings.PreloadInvoiceNo)
            {
                ReceiveInvoiceNo();
            }
        }

        private void ReceiveInvoiceNo()
        {
            if (POSReady.Settings.SellerReceiptNo == null)
            {
                return;
            }

            String path = GetInvoiceNoPreloadPath(DateTime.Today).CheckStoredPath();

            if (Directory.EnumerateFiles(path).Count() > POSReady._Settings.LowVolumeAlert)
                return;

            String url = $"{POSReady._Settings.ServiceHost}{POSReady._Settings.LoadInvoiceNoUrl}";
            using (WebClientEx client = new WebClientEx())
            {
                client.Timeout = 43200000;
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                var seed = $"{DateTime.Now.Ticks % 100000000:00000000}";
                client.Headers["Seed"] = seed ;
                using (SHA256 hash = SHA256.Create())
                {
                    client.Headers["Authorization"] = Settings.Default.ActivationKey.ComputeAuthorization(Settings.Default.SellerReceiptNo, hash, seed);
                }

                String result = client.UploadString(url,
                    JsonConvert.SerializeObject(
                        new
                        {
                            SellerID = POSReady.Settings.SellerReceiptNo,
                            POSReady._Settings.Booklet,
                        }));
                try
                {
                    InvoiceNoRoot invoiceNo = JsonConvert.DeserializeObject<InvoiceNoRoot>(result);

                    if (invoiceNo?.invoice_issue!=null)
                    {
                        path = GetInvoiceNoPreloadPath(invoiceNo.Year, invoiceNo.PeriodNo).CheckStoredPath();

                        foreach (var item in invoiceNo.invoice_issue)
                        {
                            String fileName = Path.Combine(path, $"{item.sn}.json");
                            File.WriteAllText(fileName, JsonConvert.SerializeObject(item));
                        }
                    }
                }
                catch(Exception ex)
                {
                    Logger.Error(ex);
                }

            }
        }

        public override Type UIConfigType
        {
            get { return null; }
        }

        public static String GetInvoiceNoPreloadPath(DateTime invoiceDate)
        {
            return GetInvoiceNoPreloadPath(invoiceDate.Year, (invoiceDate.Month + 1) / 2);
        }

        public static String GetInvoiceNoPreloadPath(int? year,int? periodNo)
        {
            return Path.Combine(POSReady._Settings.InvoiceNoPreload, POSReady.Settings.SellerReceiptNo, $"{year:0000}", $"{periodNo:00}");
        }


        public static InvoiceIssue ConsumeInvoiceNo(DateTime invoiceDate)
        {
            InvoiceIssue result = null;
            if (_Instance != null)
            {
                String path = GetInvoiceNoPreloadPath(invoiceDate);
                path.CheckStoredPath();
                lock (_Instance)
                {
                    var item = Directory.EnumerateFiles(path).FirstOrDefault();
                    if (item == null)
                    {
                        _Instance.ReceiveInvoiceNo();
                    }
                    item = Directory.EnumerateFiles(path).FirstOrDefault();
                    if (item != null)
                    {
                        result = JsonConvert.DeserializeObject<InvoiceIssue>(File.ReadAllText(item));
                        item.StoreFile(Path.Combine(Logger.LogDailyPath, Path.GetFileName(item)));
                    }
                }
            }
            return result;
        }

    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 

}
