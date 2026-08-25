using System.Collections.Generic;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// MIG 檔案下載請求（遷移自 WebHome InvoiceProcessController.DownloadF0401 / DownloadF0701 / DownloadF0501）。
    /// KeyIds 為結果列的加密 InvoiceID（沿用查詢結果 InvoiceItemDatatableDto.KeyId）。
    /// </summary>
    public class MigDownloadRequestDto
    {
        /// <summary>MIG 格式（F0401 / F0701 / F0501）。</summary>
        public string? DocType { get; set; }

        /// <summary>欲下載的發票（加密後 InvoiceID 清單）。</summary>
        public List<string> KeyIds { get; set; } = new();
    }

    /// <summary>
    /// MIG 壓縮檔組裝結果（僅供服務層回傳給控制器，不直接序列化為 JSON）。
    /// </summary>
    public class MigZipResultDto
    {
        /// <summary>MIG 格式（F0401 / F0701 / F0501）。</summary>
        public string DocType { get; set; } = string.Empty;

        /// <summary>請求下載的筆數（去重後）。</summary>
        public int RequestedCount { get; set; }

        /// <summary>實際納入壓縮檔的筆數。</summary>
        public int IncludedCount { get; set; }

        /// <summary>
        /// 未能產出 MIG 的發票（查無 / 無權存取者以加密 KeyId 呈現，其餘為發票號碼）。
        /// </summary>
        public List<string> SkippedNos { get; set; } = new();

        /// <summary>壓縮檔內容；IncludedCount 為 0 時為 null。</summary>
        public byte[]? Content { get; set; }
    }
}
