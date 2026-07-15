using CommonLib.Utility.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ModelCore.Properties;
using TaskCenter.Core.DTOs;

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

        /// <summary>
        /// 角色側邊選單設定（roleId 字串 → 選單群組清單）。
        /// 未在 App.settings.json 覆寫時，使用 <see cref="MenuDefaults.Build"/> 的內建預設。
        /// 登入時由後端依使用者角色回傳對應選單給前端。
        /// </summary>
        public Dictionary<string, List<MenuGroupDto>> MenusByRole { get; set; } = MenuDefaults.Build();

        /// <summary>
        /// 電子發票字軌號碼申請功能設定（遷移自 WebHome.Properties.AppSettings.InvoiceNumberApplySetting）。
        /// 預設路徑以記錄目錄為基底；Word 範本資料夾與 WebHome 相同（部署時可於 App.settings.json 覆寫）。
        /// </summary>
        public InvoiceNumberApplySetting InvoiceNumberApplySetting { get; set; } = new InvoiceNumberApplySetting
        {
            ApplyFileBaseFolder = Path.Combine(CommonLib.Core.Utility.Logger.LogPath, "InvoiceNumberApply"),
            ApplyFileBackupFolder = Path.Combine(CommonLib.Core.Utility.Logger.LogPath, "history"),
            WordTemplateFolder = @"C:\Project\GitHub\IFS-EIVO03\eIVOGo\resource\InvoiceNumberApply",
            ApplyFileNameFormat = "apply_{0}.json",
            ApplyFileNameFormatReg = @"apply_\d{8}.json$",
            ZipFileNameFormat = "ApplyWord_{0}.zip",
            NotifyEnable = true,
        };

        /// <summary>
        /// 電子發票字軌號碼申請 Word 範本清單（遷移自 WebHome.Properties.AppSettings.InvoiceNumberApplyWordSetting）。
        /// 產生「下載Word」zip 時，逐一以各範本產出 .doc（XML）並打包。
        /// </summary>
        public IEnumerable<InvoiceNumberApplyWordSetting> InvoiceNumberApplyWordSetting { get; set; } = new List<InvoiceNumberApplyWordSetting>
        {
            new() { ID = "apply", OutputName = "(中文版)電子發票字軌號碼申請書.doc", TemplateFileName = "invoiceNumberApplyWord.xml" },
            new() { ID = "commit", OutputName = "(中文版)附表1-使用電子發票承諾書.doc", TemplateFileName = "invoiceNumberCommitWord.xml" },
            new() { ID = "sysTest", OutputName = "(中文版)附表2-電子發票開立系統自行檢測表.doc", TemplateFileName = "invoiceNumberSysTestWord.xml" },
            new() { ID = "paperTest", OutputName = "(中文版)附表3-電子發票證明聯採用感熱紙切結書.doc", TemplateFileName = "invoiceNumberPaperTestWord.xml" },
            new() { ID = "appoint", OutputName = "(中文版)附表5-委託加值服務中心事務委任書.doc", TemplateFileName = "invoiceNumberAppointWord.xml" },
            new() { ID = "agent", OutputName = "(中文版)書表6-委託專業代理人事務委任書.doc", TemplateFileName = "invoiceNumberAgentApply.xml" },
        };

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