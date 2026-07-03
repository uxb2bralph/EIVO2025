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
        public string[] AllowCORS { get; set; } = { "http://localhost:5000", "http://localhost:5050", "http://localhost:5173", "https://egui.uxifs.com", "https://eguitest.uxifs.com" };
        public double LoginExpireMinutes { get; set; } = 1440 * 7;
        public JwtSettings Jwt { get; set; } = new JwtSettings();   
        public LicenseSettings License { get; set; } = new LicenseSettings();
        public bool EnableRequestDump { get; set; } = false;

    }

    public class JwtSettings
    {
        public string Issuer { get; set; } = "TaskCenter";
        public string Audience { get; set; } = "TaskCenterAudience";
        public string SecurityKey { get; set; } = "EIVO03SecretKeyForJwtToken70762419";
        public int ExpirationInMinutes { get; set; } = 60;
        /// <summary>Refresh token 有效時間（分鐘），預設 7 天。</summary>
        public int RefreshExpirationInMinutes { get; set; } = 1440 * 7;
    }

    public class LicenseSettings
    {

    }

    public static class Settings
    {
        public static AppSettings Default => AppSettings.Default;

    }
}