using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 發票資料查詢條件（遷移自 WebHome InvoiceProcessController.Index/Inquire 之 InquireInvoiceViewModel）。
    /// 以 [FromQuery]（清單/統計/匯出用 [FromBody]）繫結；欄位對應原版查詢表單各篩選項目。
    /// SellerKey / AgentKey 為加密後的 CompanyID（沿用其他遷移做法）。
    /// </summary>
    public class InvoiceProcessQueryDto
    {
        /// <summary>開立人（加密後的 Organization.CompanyID；對應舊版 EncSellerID）。</summary>
        public string? SellerKey { get; set; }

        /// <summary>代理業者（加密後的 CompanyID；對應舊版 AgentID，僅系統管理可用）。</summary>
        public string? AgentKey { get; set; }

        /// <summary>買受人統一編號（支援前綴 "!" 表示不等於；對應舊版 BuyerReceiptNo）。</summary>
        public string? BuyerReceiptNo { get; set; }

        /// <summary>買受人名稱（模糊比對；對應舊版 BuyerName）。</summary>
        public string? BuyerName { get; set; }

        /// <summary>客戶 ID（對應舊版 CustomerID）。</summary>
        public string? CustomerId { get; set; }

        /// <summary>發票日期起（含）；對應舊版 InvoiceDateFrom。</summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>發票日期迄（含當日）；對應舊版 InvoiceDateTo。</summary>
        public DateTime? DateTo { get; set; }

        /// <summary>發票號碼（單一或起）；對應舊版 InvoiceNo。</summary>
        public string? InvoiceNo { get; set; }

        /// <summary>發票號碼迄（範圍查詢）；對應舊版 EndNo。</summary>
        public string? EndNo { get; set; }

        /// <summary>單據號碼（採購單序號前綴）；對應舊版 DataNo。</summary>
        public string? DataNo { get; set; }

        /// <summary>交易類型（1 銷項 / 2 進項；對應 Naming.InvoiceCenterBusinessType）。</summary>
        public int? BusinessType { get; set; }

        /// <summary>附件檔（1 有 / 0 無 / null 全部）。</summary>
        public int? Attachment { get; set; }

        /// <summary>是否中獎（1 是 / null 全部）。</summary>
        public int? Winning { get; set; }

        /// <summary>單據狀態（true 已作廢 / false 未作廢 / null 全部）。</summary>
        public bool? Cancelled { get; set; }

        /// <summary>列印註記（Y / N）。</summary>
        public string? PrintMark { get; set; }

        /// <summary>列印狀態（true 已列印 / false 未列印 / null 全部）。</summary>
        public bool? Printed { get; set; }

        /// <summary>僅顯示有買受人地址（對應舊版 HasAddr）。</summary>
        public bool? HasAddr { get; set; }

        /// <summary>載具類型。</summary>
        public string? CarrierType { get; set; }

        /// <summary>載具號碼。</summary>
        public string? CarrierNo { get; set; }

        /// <summary>開立通知未送出（false 表示只查未通知；對應舊版 IsNoticed）。</summary>
        public bool? IsNoticed { get; set; }

        /// <summary>頁碼（1-based）。</summary>
        public int Page { get; set; } = 1;

        /// <summary>每頁筆數。</summary>
        public int PageSize { get; set; } = 10;

        /// <summary>排序欄位（對應舊版 SortName，如 InvoiceDate / TotalAmount）。</summary>
        public string? SortName { get; set; }

        /// <summary>排序方向（1 遞增 / 2 遞減；0 或 null 不排序）。</summary>
        public int? SortType { get; set; }

        /// <summary>略過筆數。</summary>
        [JsonIgnore]
        public int Skip => (Page - 1) * PageSize;
    }

    /// <summary>
    /// 發票查詢結果列（對應 WebHome InvoiceProcess 結果表格各欄位）。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class InvoiceItemDatatableDto
    {
        [JsonPropertyName("invoiceId")] public int InvoiceId { get; set; }
        /// <summary>加密後的 InvoiceID，供明細 / 後續動作傳遞。</summary>
        [JsonPropertyName("keyId")] public string? KeyId { get; set; }
        /// <summary>開立發票營業人名稱。</summary>
        [JsonPropertyName("sellerName")] public string? SellerName { get; set; }
        /// <summary>開立人統編。</summary>
        [JsonPropertyName("sellerReceiptNo")] public string? SellerReceiptNo { get; set; }
        /// <summary>發票號碼（字軌 + 號碼）。</summary>
        [JsonPropertyName("invoiceNo")] public string? InvoiceNo { get; set; }
        /// <summary>傳輸類型（交換 / 存證）。</summary>
        [JsonPropertyName("transTypeLabel")] public string? TransTypeLabel { get; set; }
        /// <summary>發票日期。</summary>
        [JsonPropertyName("invoiceDate")] public DateTime? InvoiceDate { get; set; }
        /// <summary>買受人名稱。</summary>
        [JsonPropertyName("buyerName")] public string? BuyerName { get; set; }
        /// <summary>買受人統編。</summary>
        [JsonPropertyName("buyerReceiptNo")] public string? BuyerReceiptNo { get; set; }
        /// <summary>序號（採購單號）。</summary>
        [JsonPropertyName("orderNo")] public string? OrderNo { get; set; }
        /// <summary>發票狀態（已作廢 + 日期）。</summary>
        [JsonPropertyName("statusLabel")] public string? StatusLabel { get; set; }
        /// <summary>大平台處理狀態（MIG；如 C0401:C）。</summary>
        [JsonPropertyName("migStatus")] public string? MigStatus { get; set; }
        /// <summary>幣別。</summary>
        [JsonPropertyName("currency")] public string? Currency { get; set; }
        /// <summary>未稅金額。</summary>
        [JsonPropertyName("salesAmount")] public decimal? SalesAmount { get; set; }
        /// <summary>稅別。</summary>
        [JsonPropertyName("taxTypeLabel")] public string? TaxTypeLabel { get; set; }
        /// <summary>稅額。</summary>
        [JsonPropertyName("taxAmount")] public decimal? TaxAmount { get; set; }
        /// <summary>含稅金額。</summary>
        [JsonPropertyName("totalAmount")] public decimal? TotalAmount { get; set; }
        /// <summary>備註。</summary>
        [JsonPropertyName("remark")] public string? Remark { get; set; }
        /// <summary>列印註記。</summary>
        [JsonPropertyName("printMark")] public string? PrintMark { get; set; }
        /// <summary>是否中獎（獎別 / N/A）。</summary>
        [JsonPropertyName("winningLabel")] public string? WinningLabel { get; set; }
        /// <summary>載具號碼。</summary>
        [JsonPropertyName("carrierNo")] public string? CarrierNo { get; set; }
        /// <summary>捐贈愛心碼。</summary>
        [JsonPropertyName("agencyCode")] public string? AgencyCode { get; set; }
        /// <summary>客戶 ID。</summary>
        [JsonPropertyName("customerId")] public string? CustomerId { get; set; }
        /// <summary>買受人 email。</summary>
        [JsonPropertyName("email")] public string? Email { get; set; }
        /// <summary>開立通知送出日期（null 表示未通知）。</summary>
        [JsonPropertyName("issuingNoticeDate")] public DateTime? IssuingNoticeDate { get; set; }
        /// <summary>買受人地址（僅系統管理有值）。</summary>
        [JsonPropertyName("buyerAddress")] public string? BuyerAddress { get; set; }
        /// <summary>買受人連絡人（僅系統管理有值）。</summary>
        [JsonPropertyName("buyerContact")] public string? BuyerContact { get; set; }
        /// <summary>是否已作廢。</summary>
        [JsonPropertyName("isCancelled")] public bool IsCancelled { get; set; }
        /// <summary>是否中獎。</summary>
        [JsonPropertyName("isWinning")] public bool IsWinning { get; set; }
    }

    /// <summary>幣別統計（對應舊版 CurrencySummary footer）。</summary>
    public class CurrencySummaryDto
    {
        [JsonPropertyName("currency")] public string? Currency { get; set; }
        [JsonPropertyName("count")] public int Count { get; set; }
        [JsonPropertyName("salesAmount")] public decimal SalesAmount { get; set; }
        [JsonPropertyName("taxAmount")] public decimal TaxAmount { get; set; }
        [JsonPropertyName("totalAmount")] public decimal TotalAmount { get; set; }
    }

    /// <summary>發票明細（對應舊版 DataView/Module/CDS_Document.cshtml 之預覽內容）。</summary>
    public class InvoiceDetailDto
    {
        [JsonPropertyName("invoiceNo")] public string? InvoiceNo { get; set; }
        [JsonPropertyName("invoiceDate")] public DateTime? InvoiceDate { get; set; }
        [JsonPropertyName("randomNo")] public string? RandomNo { get; set; }
        [JsonPropertyName("sellerName")] public string? SellerName { get; set; }
        [JsonPropertyName("sellerReceiptNo")] public string? SellerReceiptNo { get; set; }
        [JsonPropertyName("buyerName")] public string? BuyerName { get; set; }
        [JsonPropertyName("buyerReceiptNo")] public string? BuyerReceiptNo { get; set; }
        [JsonPropertyName("buyerAddress")] public string? BuyerAddress { get; set; }
        [JsonPropertyName("buyerEmail")] public string? BuyerEmail { get; set; }
        [JsonPropertyName("currency")] public string? Currency { get; set; }
        [JsonPropertyName("taxTypeLabel")] public string? TaxTypeLabel { get; set; }
        [JsonPropertyName("salesAmount")] public decimal? SalesAmount { get; set; }
        [JsonPropertyName("taxAmount")] public decimal? TaxAmount { get; set; }
        [JsonPropertyName("totalAmount")] public decimal? TotalAmount { get; set; }
        [JsonPropertyName("carrierType")] public string? CarrierType { get; set; }
        [JsonPropertyName("carrierNo")] public string? CarrierNo { get; set; }
        [JsonPropertyName("agencyCode")] public string? AgencyCode { get; set; }
        [JsonPropertyName("statusLabel")] public string? StatusLabel { get; set; }
        [JsonPropertyName("remark")] public string? Remark { get; set; }
        [JsonPropertyName("lines")] public List<InvoiceDetailLineDto> Lines { get; set; } = new();
    }

    /// <summary>發票明細品項列。</summary>
    public class InvoiceDetailLineDto
    {
        [JsonPropertyName("seq")] public int Seq { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("quantity")] public decimal? Quantity { get; set; }
        [JsonPropertyName("unit")] public string? Unit { get; set; }
        [JsonPropertyName("unitPrice")] public decimal? UnitPrice { get; set; }
        [JsonPropertyName("amount")] public decimal? Amount { get; set; }
        [JsonPropertyName("remark")] public string? Remark { get; set; }
    }

    /// <summary>開立人候選項目（供前端選擇開立人 autocomplete）。</summary>
    public class InvoiceQuerySellerOptionDto
    {
        [JsonPropertyName("sellerKey")] public string? SellerKey { get; set; }
        [JsonPropertyName("receiptNo")] public string? ReceiptNo { get; set; }
        [JsonPropertyName("companyName")] public string? CompanyName { get; set; }
    }

    /// <summary>代理業者候選項目（供系統管理選擇代理 autocomplete）。</summary>
    public class InvoiceQueryAgentOptionDto
    {
        [JsonPropertyName("agentKey")] public string? AgentKey { get; set; }
        [JsonPropertyName("receiptNo")] public string? ReceiptNo { get; set; }
        [JsonPropertyName("companyName")] public string? CompanyName { get; set; }
    }
}
