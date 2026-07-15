using System;
using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 電子發票字軌號碼申請查詢條件（遷移自舊版 InvoiceNumberApplyQueryViewModel / Query）。
    /// 以 [FromQuery] 繫結（大小寫不敏感，不需標註 [JsonPropertyName]）。
    /// </summary>
    public class InvoiceNumberApplyQueryDto
    {
        /// <summary>統一編號（部分比對，對應舊版以檔名 Contains 過濾）；null / 空白表示全部。</summary>
        public string? BusinessId { get; set; }
    }

    /// <summary>
    /// 申請 JSON 檔列表項目（對應舊版 QueryItemList2024.cshtml 之列）。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class InvoiceNumberApplyItemDto
    {
        /// <summary>統一編號（由檔名 apply_{BusinessID}.json 解析而得）。</summary>
        [JsonPropertyName("businessId")]
        public string BusinessId { get; set; } = string.Empty;

        /// <summary>最新填表日（檔案 LastWriteTime）。</summary>
        [JsonPropertyName("applyUpdateTime")]
        public DateTime ApplyUpdateTime { get; set; }

        /// <summary>加密後的申請 JSON 檔完整路徑（供歸檔 / 轉營業人動作傳遞，對應舊版 KeyID）。</summary>
        [JsonPropertyName("keyId")]
        public string KeyId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 列動作請求（歸檔 / 轉營業人）。KeyId 為加密後的申請 JSON 檔路徑（對應舊版 QueryViewModel.KeyID）。
    /// </summary>
    public class InvoiceNumberApplyActionDto
    {
        public string? KeyId { get; set; }
    }
}
