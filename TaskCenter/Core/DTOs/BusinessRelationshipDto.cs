using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 相對營業人查詢條件（遷移自舊版 BusinessRelationshipQueryViewModel /
    /// BusinessRelationshipController.InquireBusinessRelationship）。
    /// 以 [FromQuery] 繫結（大小寫不敏感，故不需標註 [JsonPropertyName]）。
    /// </summary>
    public class BusinessRelationshipQueryDto
    {
        /// <summary>集團成員（主營業人 CompanyID）；null 表示全部集團成員。對應舊版 GroupMemberSelector 選取值。</summary>
        public int? CompanyId { get; set; }

        /// <summary>相對營業人統一編號（精確比對）；對應舊版 ReceiptNo。</summary>
        public string? ReceiptNo { get; set; }

        /// <summary>相對營業人名稱（模糊比對，含分店名稱）；對應舊版 CompanyName。</summary>
        public string? CompanyName { get; set; }

        /// <summary>營業人類別（Naming.InvoiceCenterBusinessType：1=銷項、2=進項）；null 表示全部。對應舊版 BusinessType。</summary>
        public int? BusinessType { get; set; }

        /// <summary>頁碼（1-based）</summary>
        public int Page { get; set; } = 1;

        /// <summary>每頁筆數</summary>
        public int PageSize { get; set; } = 10;

        /// <summary>略過筆數</summary>
        [JsonIgnore]
        public int Skip => (Page - 1) * PageSize;
    }

    /// <summary>
    /// 相對營業人列表項目（對應舊版 BusinessRelationship/Module/DataItem.cshtml 之列欄位）。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// 註：本次遷移每筆關係呈現單列（對應舊版 DataItem.cshtml 無分店時的 else 區塊）；
    /// 舊版「相對營業人具分店時逐分店展開」之情境不在本次範圍。
    /// </summary>
    public class BusinessRelationshipDatatableDto
    {
        /// <summary>主營業人 CompanyID（BusinessRelationship.MasterID）；供列動作傳遞。</summary>
        [JsonPropertyName("masterId")]
        public int MasterId { get; set; }

        /// <summary>相對營業人 CompanyID（BusinessRelationship.RelativeID）；供列動作傳遞。</summary>
        [JsonPropertyName("relativeId")]
        public int RelativeId { get; set; }

        /// <summary>營業人類別識別碼（BusinessRelationship.BusinessID：1=銷項、2=進項）；供列動作傳遞。</summary>
        [JsonPropertyName("businessId")]
        public int BusinessId { get; set; }

        /// <summary>加密後的相對營業人 CompanyID（供「管理使用者」導向 UserAccount 頁使用）。</summary>
        [JsonPropertyName("relativeKeyId")]
        public string? RelativeKeyId { get; set; }

        /// <summary>主營業人名稱（Master.CompanyName）。</summary>
        [JsonPropertyName("masterName")]
        public string? MasterName { get; set; }

        /// <summary>相對營業人名稱（BusinessRelationship.CompanyName，沿用舊版顯示欄位）。</summary>
        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }

        /// <summary>相對營業人統一編號（Relative.ReceiptNo）。</summary>
        [JsonPropertyName("receiptNo")]
        public string? ReceiptNo { get; set; }

        /// <summary>營業人類別名稱（BusinessType.Business，如「銷項」）。</summary>
        [JsonPropertyName("businessTypeName")]
        public string? BusinessTypeName { get; set; }

        /// <summary>聯絡人電子郵件</summary>
        [JsonPropertyName("contactEmail")]
        public string? ContactEmail { get; set; }

        /// <summary>地址</summary>
        [JsonPropertyName("addr")]
        public string? Addr { get; set; }

        /// <summary>電話</summary>
        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        /// <summary>客戶代碼</summary>
        [JsonPropertyName("customerNo")]
        public string? CustomerNo { get; set; }

        /// <summary>關係狀態顯示文字（CurrentLevel 對應之 LevelExpression.Expression；null 時為「已啟用」）。</summary>
        [JsonPropertyName("statusText")]
        public string? StatusText { get; set; }

        /// <summary>是否已註記停用（CurrentLevel == Mark_To_Delete）；供列選單顯示「啟用 / 停用」。</summary>
        [JsonPropertyName("deactivated")]
        public bool Deactivated { get; set; }

        /// <summary>相對營業人是否設定自動接收（Relative.OrganizationStatus.Entrusting）。</summary>
        [JsonPropertyName("entrusting")]
        public bool? Entrusting { get; set; }

        /// <summary>相對營業人是否設定主動列印（Relative.OrganizationStatus.EntrustToPrint；null 表示未設定）。</summary>
        [JsonPropertyName("entrustToPrint")]
        public bool? EntrustToPrint { get; set; }
    }

    /// <summary>
    /// 集團成員（主營業人）下拉選項（對應舊版 GroupMemberSelector.cshtml）。
    /// 本站以系統管理身份運作，回傳所有企業群組成員（沿用舊版系統管理員分支）。
    /// </summary>
    public class GroupMemberDto
    {
        [JsonPropertyName("companyId")]
        public int CompanyId { get; set; }

        [JsonPropertyName("receiptNo")]
        public string? ReceiptNo { get; set; }

        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }
    }

    /// <summary>
    /// 修改相對營業人（遷移自舊版 BusinessRelationshipController.CommitItem）。
    /// 以複合鍵（MasterID + RelativeID + BusinessID）定位既有關係，僅更新可編輯欄位。
    /// </summary>
    public class BusinessRelationshipEditDto
    {
        public int MasterId { get; set; }
        public int RelativeId { get; set; }
        public int BusinessId { get; set; }
        public string? CompanyName { get; set; }
        public string? ContactEmail { get; set; }
        public string? Addr { get; set; }
        public string? Phone { get; set; }
        public string? CustomerNo { get; set; }
    }

    /// <summary>
    /// 新增相對營業人（對應舊版 AddItem 列 → CommitBusinessRelationshipViewModel）。
    /// MasterCompanyId 為所選集團成員；ReceiptNo 為相對營業人統一編號（不存在則新建 Organization）。
    /// </summary>
    public class BusinessRelationshipAddDto
    {
        /// <summary>主營業人（集團成員）CompanyID。</summary>
        public int? MasterCompanyId { get; set; }

        /// <summary>相對營業人統一編號（必填）。</summary>
        public string? ReceiptNo { get; set; }

        /// <summary>相對營業人名稱（必填）。</summary>
        public string? CompanyName { get; set; }

        /// <summary>營業人類別（Naming.InvoiceCenterBusinessType：1=銷項、2=進項）；null 預設銷項。</summary>
        public int? BusinessType { get; set; }

        public string? ContactEmail { get; set; }
        public string? Addr { get; set; }
        public string? Phone { get; set; }
        public string? CustomerNo { get; set; }
    }
}
