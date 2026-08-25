using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ModelCore.DTOs;
using TaskCenter.Core.DTOs;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// 發票明細查詢服務（遷移自 WebHome InvoiceQueryController.InvoiceReport 頁面之
    /// Inquire / CreateXlsxAsync / DownloadCSV / DownloadAttachment / DownloadAll）。
    /// 發票條件過濾與角色資料範圍一律重用 InvoiceQueryPipeline（BuildInvoiceQuery +
    /// FilterInvoiceByRole）。服務僅查詢與組檔，不做寫入。
    /// </summary>
    public interface IInvoiceReportService
    {
        /// <summary>查詢發票明細（分頁）。uid 為登入者 UID，用以還原角色資料範圍。</summary>
        Task<PagedResultDto<InvoiceReportRowDto>> GetPagedAsync(InvoiceReportQueryDto dto, int uid);

        /// <summary>
        /// 組裝「發票資料明細」資料表（欄位同結果表格；對應舊版 CreateXlsxAsync 投影）。
        /// 回傳 DataTable 供控制器轉為 Excel 下載。
        /// </summary>
        Task<DataTable> BuildDetailTableAsync(InvoiceReportQueryDto dto, int uid);

        /// <summary>組裝「發票資料明細」CSV 內容（欄位同 Excel；對應舊版 DownloadCSV）。</summary>
        Task<string> BuildDetailCsvAsync(InvoiceReportQueryDto dto, int uid);

        /// <summary>
        /// 組裝選取發票的附件壓縮檔（對應舊版 DownloadAttachment）。
        /// 下載對象先以角色範圍限縮，越權者自動落空。
        /// </summary>
        Task<InvoiceAttachmentZipResultDto> BuildSelectedAttachmentZipAsync(IEnumerable<string> keyIds, int uid);

        /// <summary>
        /// 組裝查詢結果全部發票的附件壓縮檔（對應舊版 DownloadAll）。
        /// 為避免一次壓縮過量檔案，超過 InvoiceReportService.AttachmentZipInvoiceLimit 筆時不產檔，
        /// 由控制器回報請縮小查詢範圍。
        /// </summary>
        Task<InvoiceAttachmentZipResultDto> BuildAllAttachmentZipAsync(InvoiceReportQueryDto dto, int uid);
    }
}
