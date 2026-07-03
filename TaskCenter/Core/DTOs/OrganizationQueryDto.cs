using System;
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
}
