using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Utility;
using CommonLib.Utility.Properties;

namespace JobHelper.Properties
{
    public class AppSettings : AppSettingsBase
    {
        static AppSettings()
        {
            _default = Initialize<AppSettings>(typeof(AppSettings).Namespace);
        }

        public AppSettings() : base()
        {

        }

        static AppSettings _default;
        public static AppSettings Default
        {
            get => _default;
        }

        public String CommandDecryptGPG { get; set; } = "call pgp_decrypt.bat {0} {1}";
        public String Workspace { get; set; } = Path.Combine(AppRoot, "Workspace").CheckStoredPath();
        public String ResponseReady { get; set; } = Path.Combine(AppRoot, "ResponseReady").CheckStoredPath();
        public String InvoiceTransactionRoot { get; set; } = "C:\\Users\\10090557\\sftp";
        public String[] RequestType { get; set; } = { "SellerInvoice", "CancelInvoice", "Allowance" };
        public String CommandEncryptGPG { get; set; } = "call pgp_encrypt.bat {0} {1}";
        public String[] ActiveEIVODBConnection { get; set; } = { };
    }

}
