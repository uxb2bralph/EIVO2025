using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 發票經銷商候選項目（對應舊版 Organization/Module/ApplyIssuerAgent.cshtml 的候選清單）。
    /// 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法，避免外露原始 CompanyID。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class IssuerAgentDto
    {
        /// <summary>加密後的經銷商 CompanyID（沿用 CompanyID.EncryptKey()）</summary>
        [JsonPropertyName("keyId")]
        public string? KeyId { get; set; }

        /// <summary>經銷商統一編號</summary>
        [JsonPropertyName("receiptNo")]
        public string? ReceiptNo { get; set; }

        /// <summary>經銷商名稱</summary>
        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }

        /// <summary>該經銷商是否已指派給目標開立人</summary>
        [JsonPropertyName("selected")]
        public bool Selected { get; set; }
    }

    /// <summary>
    /// 設定發票經銷商的請求（對應舊版 OrganizationController.CommitIssuerAgent）。
    /// 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法：開立人與各經銷商均以 KeyID 傳遞。
    /// </summary>
    public class CommitIssuerAgentDto
    {
        /// <summary>加密後的開立人 CompanyID（來自列表 keyId 欄位）</summary>
        [JsonPropertyName("keyId")]
        public string? KeyId { get; set; }

        /// <summary>勾選的經銷商加密 KeyID 清單</summary>
        [JsonPropertyName("agentKeyIds")]
        public List<string>? AgentKeyIds { get; set; }
    }
}
