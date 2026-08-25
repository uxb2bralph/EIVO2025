using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 使用者帳號查詢條件（遷移自舊版 UserAccountQueryViewModel / AccountController.Inquire）。
    /// 以 [FromQuery] 繫結；營業人以加密 OrgKeyId 傳遞，避免外露原始 CompanyID。
    /// </summary>
    public class UserAccountQueryDto
    {
        /// <summary>加密後的營業人 CompanyID（來自營業人列表 keyId 欄位，對應舊版 EncSellerID）</summary>
        public string? OrgKeyId { get; set; }

        /// <summary>帳號（PID，前綴比對）</summary>
        public string? Pid { get; set; }

        /// <summary>會員名稱（UserName，包含比對）</summary>
        public string? UserName { get; set; }

        /// <summary>身份角色（Naming.RoleID 值；對應舊版 RoleID 篩選）</summary>
        public int? RoleId { get; set; }

        /// <summary>會員狀態（UserProfileStatus.CurrentLevel，對應舊版 LevelID 篩選）</summary>
        public int? LevelId { get; set; }

        /// <summary>頁碼（1-based）</summary>
        public int Page { get; set; } = 1;

        /// <summary>每頁筆數</summary>
        public int PageSize { get; set; } = 10;

        /// <summary>略過筆數</summary>
        [JsonIgnore]
        public int Skip => (Page - 1) * PageSize;

        /// <summary>由 Controller 解密 OrgKeyId 後填入的營業人 CompanyID（供 Service 篩選）。</summary>
        [JsonIgnore]
        public int CompanyId { get; set; }
    }

    /// <summary>
    /// 使用者帳號列表項目（對應舊版 Account/Module/DataItem.cshtml 之列欄位）。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class UserAccountDatatableDto
    {
        /// <summary>加密後的 UID（沿用 UID.EncryptKey()），供啟用 / 停用 / 刪除等動作傳遞。</summary>
        [JsonPropertyName("keyId")]
        public string? KeyId { get; set; }

        /// <summary>所屬營業人名稱（UserRole.OrganizationCategory.Company.CompanyName）</summary>
        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }

        /// <summary>身份角色代碼（首個 UserRole.RoleID）</summary>
        [JsonPropertyName("roleId")]
        public int? RoleId { get; set; }

        /// <summary>身份角色名稱（對應舊版 (Naming.EIVOUserRoleID)RoleID 顯示）</summary>
        [JsonPropertyName("roleName")]
        public string? RoleName { get; set; }

        /// <summary>會員名稱（UserProfile.UserName）</summary>
        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        /// <summary>帳號（UserProfile.PID）</summary>
        [JsonPropertyName("pid")]
        public string? Pid { get; set; }

        /// <summary>電子郵件（UserProfile.EMail）</summary>
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        /// <summary>
        /// 會員狀態（UserProfile.LevelID）；用於決定列管理選單顯示「啟用」或「停用 / 重送確認 / 刪除」，
        /// 沿用舊版 DataItem.cshtml 以 LevelID == Mark_To_Delete 判斷。
        /// </summary>
        [JsonPropertyName("levelId")]
        public int? LevelId { get; set; }
    }
}
