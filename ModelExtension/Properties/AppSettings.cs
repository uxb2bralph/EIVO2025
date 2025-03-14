using CommonLib.Utility.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelExtension.Properties
{
    public partial class AppSettings : AppSettingsBase
    {
        static AppSettings()
        {
            _default = Initialize<AppSettings>(typeof(AppSettings).Namespace);
        }

        public AppSettings() : base()
        {

        }

        static AppSettings _default;
        public static AppSettings Default => _default;

        public static void Reload()
        {
            Reload<AppSettings>(ref _default, typeof(AppSettings).Namespace);
        }

        public String LineToken { get; set; } = "Hp41egh8G3O4rDKXL3TxwrB4gWtuGh87ZNfYTxdr1Ls";
        public String LineNotify { get; set; } = "https://notify-api.line.me/api/notify";
        public string BuyerOrderPrefix { get; set; } = "30";
        public string CsvUploadEncoding { get; set; } = "Big5";
        public string DefaultUILanguage { get; set; } = "zh-TW";
        public string DefaultUserCarrierType { get; set; } = "3J0001";
        public int EIVO_Service { get; set; } = 1;
        public bool EnableGovPlatform { get; set; } = true;
        public bool EnableEIVOPlatform { get; set; } = true;
        public bool EnableJobScheduler { get; set; } = true;
        public string ExceptionNotificationUrl { get; set; } = "~/Notification/DataUploadExceptionList";
        public string GenerateMemberCodeUrl { get; set; } = "~/Published/GenerateMemberCode.ashx";
        public string MailServer { get; set; } = "localhost";
        public int MaxResponseCountPerBatch { get; set; } = 1024;
        public string ReplyTo { get; set; } = "it_test@uxb2b.com";
        public string ServiceMailBox { get; set; } = "e-invoicevasc@uxb2b.com";
        public bool ShowAuthorizationNoInMail { get; set; } = true;
        public string SystemAdmin { get; set; } = "ifsadmin";
        public string TaskCenter { get; set; } = "TaskCenter";
        public string ThermalPOS { get; set; } = "0 0 162 792";
        public string WebMaster { get; set; } = "系統管理員 <invoice_test@uxb2b.com>";
        public int UserTimeoutInMinutes = 20160;
        public int SessionTimeoutInMinutes = 180;
        public int ResourceMaxWidth = 300;
        public String StoreRoot { get; set; } = "WebStore";
        public String AttachmentTempStore { get; set; } = "TempAttachment";
        public String A0101Outbound { get; set; } = "C:\\EINVTurnkey\\UpCast\\B2BSTORAGE\\A0101\\SRC";
        public String A0201Outbound { get; set; } = "C:\\EINVTurnkey\\UpCast\\B2BSTORAGE\\A0201\\SRC";
        public String A0401Outbound { get; set; } = "C:\\EINVTurnkey\\UpCast\\B2BSTORAGE\\A0401\\SRC";
        public String A0501Outbound { get; set; } = "C:\\EINVTurnkey\\UpCast\\B2BSTORAGE\\A0501\\SRC";
        public String A1401Outbound { get; set; } = "C:\\EINVTurnkey\\UpCast\\B2BSTORAGE\\A1401\\SRC";
        public String B0401Outbound { get; set; } = "C:\\EINVTurnkey\\UpCast\\B2BSTORAGE\\B0401\\SRC";
        public String B0501Outbound { get; set; } = "C:\\EINVTurnkey\\UpCast\\B2BSTORAGE\\B0501\\SRC";
        public String B1401Outbound { get; set; } = "C:\\EINVTurnkey\\UpCast\\B2BSTORAGE\\B1401\\SRC";
        public String C0401Outbound { get; set; } = "C:\\EINVTurnkey\\UpCast\\B2CSTORAGE\\C0401\\SRC";
        public String C0501Outbound { get; set; } = "C:\\EINVTurnkey\\UpCast\\B2CSTORAGE\\C0501\\SRC";
        public String D0401Outbound { get; set; } = "C:\\EINVTurnkey\\UpCast\\B2CSTORAGE\\D0401\\SRC";
        public String D0501Outbound { get; set; } = "C:\\EINVTurnkey\\UpCast\\B2CSTORAGE\\D0501\\SRC";
        public String E0401Outbound { get; set; } = "C:\\EINVTurnkey\\UpCast\\B2CSTORAGE\\E0401\\SRC";
        public String E0402Outbound { get; set; } = "C:\\EINVTurnkey\\UpCast\\B2CSTORAGE\\E0402\\SRC";
        public String C0701Outbound { get; set; } = "C:\\EINVTurnkey\\UpCast\\B2CSTORAGE\\C0701\\SRC";
        public string UploadedFilePath { get; set; } = "TempForPDF";
        public string A0101Receipt { get; set; } = "C:\\EINVTurnkey\\RecvTarget\\A0101";
        public int CommonParallelProcessCount { get; set; } = 100;
        public int TaskDelayInMilliseconds { get; set; } = 5000;
        public double SessionTimeout { get; set; } = 20;
        public String EINVTurnkeyRoot { get; set; } = "C:\\EINVTurnkey";
        public bool UseMIG40 { get; set; } = false;
        public int UserPasswordValidDays { get; set; } = 180;
        public string EV8D_ID { get; set; } = "";
        public string EV8D_PASSWORD { get; set; } = "";
        public String EV8D_SMS { get; set; } = "http://api.every8d.com/API21/SOAP/SMS.asmx";
        public double MinimumCreditAlert { get; set; } = 100;
        public String HostUrl { get; set; } = "http://localhost:2598";
        public decimal? DefaultTaxRate { get; set; } = 0.05M;

    }
}