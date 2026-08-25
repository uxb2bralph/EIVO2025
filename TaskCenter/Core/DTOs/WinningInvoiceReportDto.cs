using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 中獎統計表查詢條件（遷移自 WebHome WinningInvoiceController.ReportIndex / InquireReport /
    /// CreateXlsx）。舊版查詢表單（Module/InquireWinning.cshtml）僅呈現「開立人 / 發票日期 / 代理業者」，
    /// 但底層走的是共用的 InquireInvoiceViewModel 查詢管線，故此處直接沿用
    /// <see cref="InvoiceProcessQueryDto"/>；差別在於結果強制只計中獎發票，並依「開立人」彙總。
    /// </summary>
    public class WinningInvoiceReportQueryDto : InvoiceProcessQueryDto
    {
    }

    /// <summary>
    /// 中獎統計表結果列（對應舊版 WebHome.Models.WinningInvoiceReportItem 與
    /// Views/WinningInvoice/DataField/*.cshtml 各欄位）。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class WinningInvoiceReportRowDto
    {
        /// <summary>加密後的 CompanyID（不透明鍵，供前端傳遞開立人身分）。</summary>
        [JsonPropertyName("sellerKey")] public string? SellerKey { get; set; }
        /// <summary>賣方統一編號。</summary>
        [JsonPropertyName("sellerReceiptNo")] public string? SellerReceiptNo { get; set; }
        /// <summary>賣方名稱。</summary>
        [JsonPropertyName("sellerName")] public string? SellerName { get; set; }
        /// <summary>賣方地址。</summary>
        [JsonPropertyName("addr")] public string? Addr { get; set; }
        /// <summary>中獎張數。</summary>
        [JsonPropertyName("winningCount")] public int WinningCount { get; set; }
        /// <summary>捐贈張數（中獎發票中同時具捐贈註記者）。</summary>
        [JsonPropertyName("donationCount")] public int DonationCount { get; set; }
    }
}
