using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 使用者帳號編輯 / 新增資料（遷移自舊版 UserProfileController.EditItem / Commit 與 UserProfileViewModel）。
    /// 沿用舊版以加密 KeyID 傳遞識別碼：KeyId = 加密 UID（新增時為 null）、OrgKeyId = 加密所屬營業人 CompanyID。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class UserAccountEditDto
    {
        /// <summary>加密後的 UID；新增帳號時為 null。</summary>
        [JsonPropertyName("keyId")]
        public string? KeyId { get; set; }

        /// <summary>加密後的所屬營業人 CompanyID（來自使用者管理頁的 orgKeyId）。</summary>
        [JsonPropertyName("orgKeyId")]
        public string? OrgKeyId { get; set; }

        /// <summary>帳號（PID）</summary>
        [JsonPropertyName("pid")]
        public string? Pid { get; set; }

        /// <summary>會員名稱（UserName）</summary>
        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        /// <summary>常用電子郵件（EMail）</summary>
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        /// <summary>住址</summary>
        [JsonPropertyName("address")]
        public string? Address { get; set; }

        /// <summary>電話（日）</summary>
        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        /// <summary>行動電話</summary>
        [JsonPropertyName("mobilePhone")]
        public string? MobilePhone { get; set; }

        /// <summary>電話（夜）</summary>
        [JsonPropertyName("phone2")]
        public string? Phone2 { get; set; }

        /// <summary>身份設定（Naming.RoleID 值）</summary>
        [JsonPropertyName("roleId")]
        public int? RoleId { get; set; }

        /// <summary>密碼（留空表示不修改原密碼；新增時必填）。僅用於 Commit，載入時不回傳。</summary>
        [JsonPropertyName("password")]
        public string? Password { get; set; }

        /// <summary>確認密碼。僅用於 Commit。</summary>
        [JsonPropertyName("password1")]
        public string? Password1 { get; set; }

        /// <summary>所屬營業人名稱（唯讀，載入時回傳供顯示）。</summary>
        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }
    }
}
