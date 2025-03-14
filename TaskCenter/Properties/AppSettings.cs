using CommonLib.Utility.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaskCenter.Properties
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

        public static void Reload()
        {
            Reload<AppSettings>(ref _default, typeof(AppSettings).Namespace);
        }

        public long TimeoutTicks { get; set; } = 86400000000000;

    }

    public static class Settings
    {
        public static AppSettings Default => AppSettings.Default;

    }
}