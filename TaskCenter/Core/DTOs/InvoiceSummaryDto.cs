using System;
using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 發票統計表查詢條件（遷移自 WebHome InvoiceQueryController.InvoiceSummary/InquireSummary）。
    /// 篩選欄位與「資料查詢／列印／匯出」相同（皆對應 InquireInvoiceViewModel），故直接沿用
    /// <see cref="InvoiceProcessQueryDto"/>；差別在於結果以「開立發票營業人」為單位彙總，
    /// 且 SortName 僅支援 CompanyName / ReceiptNo，並強制須指定發票日期起迄。
    /// </summary>
    public class InvoiceSummaryQueryDto : InvoiceProcessQueryDto
    {
    }

    /// <summary>
    /// 發票統計表結果列（對應舊版 Module/InvoiceSummaryResult.cshtml + DataItem.cshtml）。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class InvoiceSummaryRowDto
    {
        /// <summary>加密後的 CompanyID（不透明鍵，供前端傳遞開立人身分）。</summary>
        [JsonPropertyName("sellerKey")] public string? SellerKey { get; set; }
        /// <summary>營業人名稱。</summary>
        [JsonPropertyName("companyName")] public string? CompanyName { get; set; }
        /// <summary>統一編號。</summary>
        [JsonPropertyName("receiptNo")] public string? ReceiptNo { get; set; }
        /// <summary>符合查詢條件之發票筆數。</summary>
        [JsonPropertyName("invoiceCount")] public int InvoiceCount { get; set; }
        /// <summary>註記停用日期。</summary>
        [JsonPropertyName("expirationDate")] public DateTime? ExpirationDate { get; set; }
    }
}
