using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// E0501（配號結果檔）自動取號設定
    /// （遷移自舊版 Organization/ApplyE0501Settings.cshtml 之 OrganizationCustomSetting.Settings）。
    /// 載入與儲存共用同一組欄位；沿用舊版以加密 KeyID 傳遞 CompanyID。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class E0501SettingsDto
    {
        /// <summary>加密後的 CompanyID（來自列表 / 編輯表單的 keyId 欄位）</summary>
        [JsonPropertyName("keyId")]
        public string? KeyId { get; set; }

        /// <summary>停用 E0501 自動取號（對應 Settings.DisableE0501AutoUpdate）</summary>
        [JsonPropertyName("disableAutoUpdate")]
        public bool DisableAutoUpdate { get; set; }

        /// <summary>自動取號後預設鎖定字軌號碼區間（對應 Settings.E0501InitialLock）</summary>
        [JsonPropertyName("initialLock")]
        public bool InitialLock { get; set; }

        /// <summary>保留本組數（每組 50 號；對應 Settings.E0501ReservedBooklets）</summary>
        [JsonPropertyName("reservedBooklets")]
        public int? ReservedBooklets { get; set; }

        /// <summary>
        /// 主機構批次配號設定（對應 Settings.BranchInvoiceNoAssignments）。
        /// 僅供顯示，由 Excel 匯入（UploadBranchAssignments）維護，CommitE0501 不異動此項。
        /// </summary>
        [JsonPropertyName("branchAssignments")]
        public BranchInvoiceNoAssignmentDto[] BranchAssignments { get; set; } = [];
    }
}
