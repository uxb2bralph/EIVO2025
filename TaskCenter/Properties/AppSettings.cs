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

    }

    public class JwtSettings
    {
        public string Issuer { get; set; } = "TaskCenter";
        public string Audience { get; set; } = "TaskCenterAudience";
        public string SecurityKey { get; set; } = "EIVO03SecretKeyForJwtToken70762419";
        public int ExpirationInMinutes { get; set; } = 60;
    }

    public class LicenseSettings
    {
        public string AutoMapperLicenseKey { get; set; } = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxODAyMzkwNDAwIiwiaWF0IjoiMTc3MDg3Nzg5OSIsImFjY291bnRfaWQiOiIwMTljNTA4YTlmN2Q3NTRmODAzNTY1ZGNhODZlMTYwNCIsImN1c3RvbWVyX2lkIjoiY3RtXzAxa2g4OHJtYjl5d2Q4MjhmMm5jZXh5M2Q1Iiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.PGi8WHkho--FZmDNigws2_dFT1jseBLNtryTT81TJa2Po4a0gfLF7Fxqu9RXsdJhgyLmssyABKVEIwlHW-iv1eDKUyAiEC722FCj0y-xqJbBGo8ykYJIM3NdYbldGi7e-RMLStGCsmakReZqIrlis7XlV9twns-FREV_McnhJZ7Nrrbo01z-tRG-vGLgFSChcS0HqJgtRbydrPM5aN3ZvvR52BEkcQvrMlKi__x71cueNIoqs_-llbGH9iChww17ohWI1i6sl9fnPswnS2hAyesmsLfuy0SbJ0ur2m_H53S4PSDhjg3IJXD3BK3TNaHcjR5QUILhiDplfe09UR_wdQ";
    }

    public static class Settings
    {
        public static AppSettings Default => AppSettings.Default;

    }
}