using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 作廢發票請求（遷移自 WebHome InvoiceProcessController.CancelInvoiceAsync）。
    /// KeyIds 為結果列的加密 InvoiceID（沿用查詢結果 InvoiceItemDatatableDto.KeyId）。
    /// </summary>
    public class CancelInvoiceRequestDto
    {
        /// <summary>欲作廢的發票（加密後 InvoiceID 清單）。</summary>
        public List<string> KeyIds { get; set; } = new();
    }

    /// <summary>
    /// 作廢發票結果（對應舊版 AlertMessage 的「下列發票已作廢完成」文字，改回結構化資料）。
    /// </summary>
    public class CancelInvoiceResultDto
    {
        /// <summary>請求作廢的筆數。</summary>
        [JsonPropertyName("requestedCount")] public int RequestedCount { get; set; }

        /// <summary>實際完成作廢的筆數。</summary>
        [JsonPropertyName("cancelledCount")] public int CancelledCount { get; set; }

        /// <summary>已完成作廢的發票號碼（字軌 + 號碼）。</summary>
        [JsonPropertyName("cancelledNos")] public List<string> CancelledNos { get; set; } = new();

        /// <summary>
        /// 未能作廢的發票號碼（已作廢、查無或無權存取）。
        /// 越權 / 查無者以加密 KeyId 呈現（無法還原號碼）。
        /// </summary>
        [JsonPropertyName("skippedNos")] public List<string> SkippedNos { get; set; } = new();
    }
}
