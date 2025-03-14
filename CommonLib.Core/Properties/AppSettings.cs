using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Utility.Properties;

namespace CommonLib.Core.Properties
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
        public static AppSettings Default
        {
            get
            {
                return _default;
            }
        }

        public String IPdfUtilityImpl { get; set; } = "WKPdfWrapper.PdfUtility,WKPdfWrapper, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        public bool SqlLog { get; set; } = true;
        public bool EnableJobScheduler { get; set; } = true;
        public String LogPath { get; set; }
        public bool IgnoreCertificateRevoked { get; set; } = true;
    }

}
