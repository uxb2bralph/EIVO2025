using CommonLib.Utility.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole.Properties
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
            get => _default;
        }

        public TurnkeySettings Turnkey { get; set; } = new TurnkeySettings { };


        public class TurnkeySettings
        {
            public String UpCastPath { get; set; } = "C:\\EINVTurnkey\\UpCast";
            public String RecyclePath { get; set; } = "C:\\EINVTurnkey\\Recycle";
            public String Command { get; set; } = "C:\\Program Files\\7-Zip\\7z.exe";
            public String ArgsPattern { get; set; } = "a -r -sdel {0}.7z {0}";

        }
    }
}
