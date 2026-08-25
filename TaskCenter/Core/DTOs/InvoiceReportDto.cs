using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 發票明細查詢條件（遷移自 WebHome InvoiceQueryController.InvoiceReport 之查詢表單
    /// Views/InvoiceQuery/Module/InvoiceReport.cshtml「發票報表匯出」）。
    /// 篩選欄位與「資料查詢／列印／匯出」相同（皆對應 InquireInvoiceViewModel），故直接沿用
    /// <see cref="InvoiceProcessQueryDto"/>；差別在於結果／匯出欄位採原版 CreateXlsxAsync
    /// 之「發票資料明細」19 欄，且強制須指定發票日期起迄（避免全表明細匯出）。
    /// </summary>
    public class InvoiceReportQueryDto : InvoiceProcessQueryDto
    {
    }

    /// <summary>
    /// 發票明細結果列。欄位對應原版 InvoiceQueryController.CreateXlsxAsync 之匯出投影
    /// （所見即所得：畫面表格與匯出 Excel／CSV 同一組欄位）。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class InvoiceReportRowDto
    {
        /// <summary>加密後的 InvoiceID，供附件下載等後續動作傳遞。</summary>
        [JsonPropertyName("keyId")] public string? KeyId { get; set; }
        /// <summary>發票號碼（字軌 + 號碼）。</summary>
        [JsonPropertyName("invoiceNo")] public string? InvoiceNo { get; set; }
        /// <summary>發票日期。</summary>
        [JsonPropertyName("invoiceDate")] public DateTime? InvoiceDate { get; set; }
        /// <summary>附件檔名（原版取第一個附件的 KeyName）。</summary>
        [JsonPropertyName("attachmentName")] public string? AttachmentName { get; set; }
        /// <summary>附件檔數（供前端判斷可否下載附件）。</summary>
        [JsonPropertyName("attachmentCount")] public int AttachmentCount { get; set; }
        /// <summary>客戶 ID。</summary>
        [JsonPropertyName("customerId")] public string? CustomerId { get; set; }
        /// <summary>序號（採購單號）。</summary>
        [JsonPropertyName("orderNo")] public string? OrderNo { get; set; }
        /// <summary>發票開立人名稱。</summary>
        [JsonPropertyName("sellerName")] public string? SellerName { get; set; }
        /// <summary>開立人統編。</summary>
        [JsonPropertyName("sellerReceiptNo")] public string? SellerReceiptNo { get; set; }
        /// <summary>未稅金額。</summary>
        [JsonPropertyName("salesAmount")] public decimal? SalesAmount { get; set; }
        /// <summary>稅額。</summary>
        [JsonPropertyName("taxAmount")] public decimal? TaxAmount { get; set; }
        /// <summary>含稅金額。</summary>
        [JsonPropertyName("totalAmount")] public decimal? TotalAmount { get; set; }
        /// <summary>買受人名稱。</summary>
        [JsonPropertyName("buyerName")] public string? BuyerName { get; set; }
        /// <summary>買受人統編。</summary>
        [JsonPropertyName("buyerReceiptNo")] public string? BuyerReceiptNo { get; set; }
        /// <summary>連絡人名稱。</summary>
        [JsonPropertyName("contactName")] public string? ContactName { get; set; }
        /// <summary>連絡人地址。</summary>
        [JsonPropertyName("address")] public string? Address { get; set; }
        /// <summary>買受人 EMail。</summary>
        [JsonPropertyName("email")] public string? Email { get; set; }
        /// <summary>捐贈愛心碼。</summary>
        [JsonPropertyName("agencyCode")] public string? AgencyCode { get; set; }
        /// <summary>是否中獎（獎別；未中獎為 null）。</summary>
        [JsonPropertyName("winningLabel")] public string? WinningLabel { get; set; }
        /// <summary>載具類別。</summary>
        [JsonPropertyName("carrierType")] public string? CarrierType { get; set; }
        /// <summary>載具號碼。</summary>
        [JsonPropertyName("carrierNo")] public string? CarrierNo { get; set; }
        /// <summary>是否已作廢（供結果列樣式標示，非匯出欄位）。</summary>
        [JsonPropertyName("isCancelled")] public bool IsCancelled { get; set; }
    }

    /// <summary>
    /// 附件壓縮檔下載請求（遷移自 WebHome InvoiceQueryController.DownloadAttachment）。
    /// KeyIds 為結果列的加密 InvoiceID。
    /// </summary>
    public class InvoiceAttachmentZipRequestDto
    {
        /// <summary>欲下載附件的發票（加密後 InvoiceID 清單）。</summary>
        public List<string> KeyIds { get; set; } = new();
    }

    /// <summary>
    /// 附件壓縮檔組裝結果（僅供服務層回傳給控制器，不直接序列化為 JSON）。
    /// </summary>
    public class InvoiceAttachmentZipResultDto
    {
        /// <summary>納入壓縮檔的附件檔數。</summary>
        public int IncludedCount { get; set; }

        /// <summary>有附件的發票筆數（「下載全部附件」用以判斷是否超量）。</summary>
        public int MatchedInvoiceCount { get; set; }

        /// <summary>是否因超過單次處理上限而未產檔。</summary>
        public bool ExceededLimit { get; set; }

        /// <summary>有附件紀錄但實體檔案不存在／無權存取而未納入者（以發票號碼或加密 KeyId 呈現）。</summary>
        public List<string> SkippedNos { get; set; } = new();

        /// <summary>壓縮檔內容；IncludedCount 為 0 時為 null。</summary>
        public byte[]? Content { get; set; }
    }
}
