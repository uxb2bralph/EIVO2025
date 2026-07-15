using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 客製化服務設定 — 發送通知郵件伺服器(SMTP)設定
    /// （遷移自舊版 OrganizationController.CustomSettings / LoadSmtpSettings 之 CustomSmtpHost）。
    /// 沿用舊版以加密 KeyID 傳遞 CompanyID；密碼不回傳，僅以 HasPassword 表示是否已設定。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class CustomSmtpSettingsDto
    {
        /// <summary>加密後的 CompanyID（回傳供後續動作沿用）</summary>
        [JsonPropertyName("keyId")]
        public string? KeyId { get; set; }

        /// <summary>是否已有啟用中的 SMTP 設定（Disabled 視為未設定）</summary>
        [JsonPropertyName("configured")]
        public bool Configured { get; set; }

        /// <summary>伺服器 domain name 或 IP</summary>
        [JsonPropertyName("host")]
        public string? Host { get; set; }

        /// <summary>Port（未設定時舊版預設 25）</summary>
        [JsonPropertyName("port")]
        public int? Port { get; set; }

        /// <summary>是否使用 SSL 連線</summary>
        [JsonPropertyName("enableSsl")]
        public bool? EnableSsl { get; set; }

        /// <summary>登入帳號</summary>
        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        /// <summary>寄件人 email</summary>
        [JsonPropertyName("mailFrom")]
        public string? MailFrom { get; set; }

        /// <summary>是否已設定登入密碼（密碼本身不回傳）</summary>
        [JsonPropertyName("hasPassword")]
        public bool HasPassword { get; set; }
    }

    /// <summary>
    /// 儲存客製化 SMTP 設定的請求（遷移自舊版 CommitSmtpSettings → CommitCustomSmtpHost）。
    /// 沿用舊版以加密 KeyID 傳遞 CompanyID；Password 留空表示不變更原密碼。
    /// </summary>
    public class CommitCustomSmtpDto
    {
        /// <summary>加密後的 CompanyID（來自列表 keyId 欄位）</summary>
        [JsonPropertyName("keyId")]
        public string? KeyId { get; set; }

        /// <summary>伺服器 domain name 或 IP（必填）</summary>
        [JsonPropertyName("host")]
        public string? Host { get; set; }

        /// <summary>Port（未填時後端預設 25）</summary>
        [JsonPropertyName("port")]
        public int? Port { get; set; }

        /// <summary>是否使用 SSL 連線</summary>
        [JsonPropertyName("enableSsl")]
        public bool? EnableSsl { get; set; }

        /// <summary>登入帳號</summary>
        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        /// <summary>登入密碼（留空表示不變更原密碼）</summary>
        [JsonPropertyName("password")]
        public string? Password { get; set; }

        /// <summary>寄件人 email（必填）</summary>
        [JsonPropertyName("mailFrom")]
        public string? MailFrom { get; set; }
    }
}
