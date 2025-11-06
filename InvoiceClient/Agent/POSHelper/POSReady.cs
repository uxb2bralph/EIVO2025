using CommonLib.Core.Utility;
using CommonLib.Utility;
using CommonLib.Utility.Properties;
using InvoiceClient.Helper;
using InvoiceClient.Properties;
using InvoiceClient.TransferManagement;
using ModelCore.Schema.TXN;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;


namespace InvoiceClient.Agent.POSHelper
{

    public class POSReady : InvoiceServerInspector
    {

        static POSReady()
        {
            Settings.InvoiceNoPreload.CheckStoredPath();
            Settings.PreparedInvoice.CheckStoredPath();
            Settings.PreparedAllowance.CheckStoredPath();
            Settings.SellerInvoice.CheckStoredPath();
            Settings.PrintInvoice.CheckStoredPath();
            Settings.MIGResponse.CheckStoredPath();
        }

        public static LocalSettings Settings => LocalSettings.Default;

        public class LocalSettings : AppSettingsBase
        {
            static LocalSettings()
            {
                _default = Initialize<LocalSettings>(typeof(LocalSettings).Namespace);
            }

            public LocalSettings() : base()
            {

            }

            static LocalSettings _default;
            public static LocalSettings Default
            {
                get
                {
                    return _default;
                }
            }

            public static void Reload()
            {
                Reload<LocalSettings>(ref _default, typeof(LocalSettings).Namespace);
            }

            public String ServiceHost { get; } = ServiceInfo.ServiceHost;  //"https://eguitest.uxifs.com/cbe";

            public String InvoiceNoPreload { get; set; } = Path.Combine(Logger.LogPath, "InvoiceNoInspector", "InvoiceNo");
            public int LowVolumeAlert { get; set; } = 500;
            public int Booklet { get; set; } = 10;
            public String? DeviceNo { get; set; }
            public String LoadInvoiceNoUrl { get; set; } = "/POSDevice/AllocateInvoiceNo";
            public String PreparedInvoice { get; set; } = Path.Combine(Logger.LogPath, "InvoiceNoInspector", "PreparedInvoice");
            public String BlindReturn { get; set; } = Path.Combine(Logger.LogPath, "InvoiceNoInspector", "BlindReturn");
            public String ZeroAmount { get; set; } = Path.Combine(Logger.LogPath, "InvoiceNoInspector", "ZeroAmountReceipt");
            public String Replacement { get; set; } = Path.Combine(Logger.LogPath, "InvoiceNoInspector", "Replacement");
            public String SellerInvoice { get; set; } = Path.Combine(Logger.LogPath, "InvoiceNoInspector", "SellerInvoice");
            public String B2BSellerInvoice { get; set; } = Path.Combine(Logger.LogPath, "InvoiceNoInspector", "B2BSellerInvoice");
            public String PrintInvoice { get; set; } = Path.Combine(Logger.LogPath, "InvoiceNoInspector", "PrintInvoice");
            public String ReprintReceipt { get; set; } = Path.Combine(Logger.LogPath, "InvoiceNoInspector", "ReprintReceipt");
            public String ReprintInvoice { get; set; } = Path.Combine(Logger.LogPath, "InvoiceNoInspector", "ReprintInvoice");
            public String ReprintAllowance { get; set; } = Path.Combine(Logger.LogPath, "InvoiceNoInspector", "ReprintAllowance");
            public String PrintC0401 { get; set; } = "https://eguitest.uxifs.com/Kiosk/FrontEnd/PrintC0401";
            public String PrintBlindReturn { get; set; } = "https://eguitest.uxifs.com/Kiosk/FrontEnd/BlindReturn";
            public String PrintZeroAmount { get; set; } = "https://eguitest.uxifs.com/Kiosk/FrontEnd/PrintZeroAmount";
            public String PrintReplacement { get; set; } = "https://eguitest.uxifs.com/Kiosk/FrontEnd/Replacement";
            public String PrintD0401 { get; set; } = "https://eguitest.uxifs.com/Kiosk/FrontEnd/PrintD0401";
            public String VerifyAllowance { get; set; } = "/POSDevice/VerifyAllowance";
            public String PreparedAllowance { get; set; } = Path.Combine(Logger.LogPath, "InvoiceNoInspector", "PreparedAllowance");
            public String MIGResponse { get; set; } = Path.Combine(Logger.LogPath, "MIGResponse");
            public bool UserPOSPrinter { get; set; } = false;
            public bool PreloadInvoiceNo { get; set; } = false;
            public string C0401 { get; set; } = "C0401";
            public string C0501 { get; set; } = "C0501";
            public string A0401 { get; set; } = "A0401";
            public string A0501 { get; set; } = "A0501";
            public bool Initialized { get; set; } = false;
            public string InitBatch { get; set; } = "Init.bat";
            public string? SellerReceiptNo { get; set; }
            public bool UserClose { get; set; } = true;
            public String? DefaultPOSPrinter { get; set; }
            public int InvoiceFileEncodingPage { get; set; } = Encoding.Unicode.CodePage;
        }

        public POSReady()
        {

        }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class InvoiceIssue
    {
        public string sn { get; set; }
        public string random { get; set; }
        public string aesbase64 { get; set; }
    }

    public class InvoiceNoRoot
    {
        public int? SellerID { get; set; }
        public int? Year { get; set; }
        public int? PeriodNo { get; set; }
        public List<InvoiceIssue> invoice_issue { get; set; }
    }


}
