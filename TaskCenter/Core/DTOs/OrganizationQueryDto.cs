using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 營業人資料查詢條件（對應舊版 OrganizationQueryViewModel）。
    /// 以 [FromQuery] 繫結，欄位名稱大小寫不敏感。
    /// </summary>
    public class OrganizationQueryDto
    {
        /// <summary>統一編號（前綴比對）</summary>
        public string? ReceiptNo { get; set; }

        /// <summary>營業人名稱（包含比對）</summary>
        public string? CompanyName { get; set; }

        /// <summary>營業人狀態（OrganizationStatus.CurrentLevel，1103 已啟用 / 1101 已停用）</summary>
        public int? OrganizationStatus { get; set; }

        /// <summary>營業人類別（CategoryDefinition.CategoryEnum 值）</summary>
        public int? CategoryId { get; set; }

        /// <summary>所屬經銷商（InvoiceIssuerAgent.AgentID）</summary>
        public int? AgentId { get; set; }

        /// <summary>是否僅查分支機構（RelationType = MasterBranch）</summary>
        public bool? BranchRelation { get; set; }

        /// <summary>頁碼（1-based）</summary>
        public int Page { get; set; } = 1;

        /// <summary>每頁筆數</summary>
        public int PageSize { get; set; } = 10;

        /// <summary>略過筆數</summary>
        [JsonIgnore]
        public int Skip => (Page - 1) * PageSize;
    }

    /// <summary>
    /// 營業人資料列表項目（對應舊版 CompanyList / SellerList 欄位）。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class OrganizationDatatableDto
    {
        [JsonPropertyName("companyId")]
        public int CompanyId { get; set; }

        /// <summary>
        /// 加密後的 CompanyID（沿用舊版 CompanyID.EncryptKey()），供編輯等動作以 KeyID 傳遞，
        /// 避免外露原始 CompanyID。於查詢結果materialize後填入（EncryptKey 無法在 EF 查詢中翻譯）。
        /// </summary>
        [JsonPropertyName("keyId")]
        public string? KeyId { get; set; }

        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }

        [JsonPropertyName("receiptNo")]
        public string? ReceiptNo { get; set; }

        [JsonPropertyName("undertakerName")]
        public string? UndertakerName { get; set; }

        [JsonPropertyName("contactEmail")]
        public string? ContactEmail { get; set; }

        /// <summary>狀態代碼（CurrentLevel）</summary>
        [JsonPropertyName("statusLevel")]
        public int? StatusLevel { get; set; }

        /// <summary>狀態說明（LevelExpression.Description）</summary>
        [JsonPropertyName("statusName")]
        public string? StatusName { get; set; }

        [JsonPropertyName("goLiveDate")]
        public DateTime? GoLiveDate { get; set; }

        [JsonPropertyName("expirationDate")]
        public DateTime? ExpirationDate { get; set; }

        /// <summary>是否為主機構（對應 Organization.MasterOrganization 是否存在）</summary>
        [JsonPropertyName("isMaster")]
        public bool IsMaster { get; set; }
    }

    /// <summary>
    /// 所屬經銷商下拉選項（類別為經銷商的營業人），供查詢條件使用
    /// （對應舊版 InquireOrganization.cshtml 之 AgentID 下拉）。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class OrganizationAgentDto
    {
        /// <summary>經銷商 CompanyID（作為 AgentId 查詢值，沿用舊版以原始 CompanyID 傳遞）</summary>
        [JsonPropertyName("companyId")]
        public int CompanyId { get; set; }

        [JsonPropertyName("receiptNo")]
        public string? ReceiptNo { get; set; }

        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }
    }

    /// <summary>
    /// 主機構下拉選項（已設定為主機構 MasterOrganization 的營業人），供「設為分支機構」選擇主機構使用
    /// （對應舊版 Home/SearchHeadquarter）。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class HeadquarterDto
    {
        /// <summary>
        /// 加密後的主機構 CompanyID（沿用本控制器以加密 KeyID 傳遞 CompanyID 的做法）。
        /// EncryptKey 無法於 EF 查詢中翻譯，故於 materialize 後填入。
        /// </summary>
        [JsonPropertyName("keyId")]
        public string? KeyId { get; set; }

        [JsonPropertyName("receiptNo")]
        public string? ReceiptNo { get; set; }

        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }
    }

    /// <summary>
    /// 設為分支機構請求（對應舊版 Organization/ApplyHeadquarter）。
    /// 將勾選的營業人設定為指定主機構的分支機構。
    /// 以 [FromBody] 繫結，欄位名稱大小寫不敏感。
    /// </summary>
    public class ApplyHeadquarterDto
    {
        /// <summary>主機構的加密 CompanyID（來自主機構 autocomplete）</summary>
        public string? HeadquarterKeyId { get; set; }

        /// <summary>要設為分支機構的營業人加密 CompanyID 清單（來自列表勾選）</summary>
        public List<string>? BranchKeyIds { get; set; }
    }

    /// <summary>
    /// 複製收費標準請求（對應舊版 Organization/CloneBillingPlan）。
    /// 將來源營業人的收費設定複製到勾選的目標營業人（目標原有設定將被取代）。
    /// 以 [FromBody] 繫結，欄位名稱大小寫不敏感。
    /// </summary>
    public class CloneBillingPlanDto
    {
        /// <summary>複製來源營業人的加密 CompanyID（來自主機構/來源 autocomplete）</summary>
        public string? SourceKeyId { get; set; }

        /// <summary>複製目標營業人的加密 CompanyID 清單（來自列表勾選）</summary>
        public List<string>? TargetKeyIds { get; set; }
    }
}
