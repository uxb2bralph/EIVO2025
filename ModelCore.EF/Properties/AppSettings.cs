using CommonLib.Utility.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelCore.Properties
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

        public String SystemKeyName { get; set; } = "SystemKey.new.json";
        public String? ConnectionString { get; set; }
        public String? EINVTurnkey2ConnectionString { get; set; }
        public int PageSize { get; set; } = 10;
        public MIGSettings MIG { get; set; } = new MIGSettings();

    }

    public class MIGSettings
    {
        public String A0101 { get; set; } = "urn:GEINV:eInvoiceMessage:A0101:4.1";
        public String A0102 { get; set; } = "urn:GEINV:eInvoiceMessage:A0102:4.1";
        public String A0301 { get; set; } = "urn:GEINV:eInvoiceMessage:A0301:4.1";
        public String A0302 { get; set; } = "urn:GEINV:eInvoiceMessage:A0302:4.1";
        public String A0103 { get; set; } = "urn:GEINV:eInvoiceMessage:A0103:4.1";
        public String A0201 { get; set; } = "urn:GEINV:eInvoiceMessage:A0201:4.1";
        public String A0202 { get; set; } = "urn:GEINV:eInvoiceMessage:A0202:4.1";
        public String B0101 { get; set; } = "urn:GEINV:eInvoiceMessage:B0101:4.1";
        public String B0102 { get; set; } = "urn:GEINV:eInvoiceMessage:B0102:4.1";  
        public String B0201 { get; set; } = "urn:GEINV:eInvoiceMessage:B0201:4.1";
        public String B0202 { get; set; } = "urn:GEINV:eInvoiceMessage:B0202:4.1";
        public String F0401 { get; set; } = "urn:GEINV:eInvoiceMessage:F0401:4.1";
        public String F0501 { get; set; } = "urn:GEINV:eInvoiceMessage:F0501:4.1";
        public String F0701 { get; set; } = "urn:GEINV:eInvoiceMessage:F0701:4.1";
        public String G0401 { get; set; } = "urn:GEINV:eInvoiceMessage:G0401:4.1";
        public String G0501 { get; set; } = "urn:GEINV:eInvoiceMessage:G0501:4.1";
    }
}
