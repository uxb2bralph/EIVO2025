using System.Data;
using System.Threading.Tasks;
using ModelCore.DTOs;
using TaskCenter.Core.DTOs;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// 發票統計表服務（遷移自 WebHome InvoiceQueryController.InvoiceSummary / InquireSummary /
    /// CreateMonthlyReportXlsx）。發票條件過濾重用 InvoiceQueryPipeline（BuildInvoiceQuery），
    /// 營業人清單則以 FilterOrganizationByRole 依登入者角色限縮。服務僅查詢，不做寫入。
    /// </summary>
    public interface IInvoiceSummaryService
    {
        /// <summary>依開立發票營業人彙總查詢結果（分頁）。uid 為登入者 UID，用以還原角色資料範圍。</summary>
        Task<PagedResultDto<InvoiceSummaryRowDto>> GetPagedAsync(InvoiceSummaryQueryDto dto, int uid);

        /// <summary>
        /// 組裝開立發票月報表內容：第一張工作表為營業人統計，其後每個年月一張日別統計表
        /// （對應舊版 SaveAsExcel）。回傳 DataSet 供控制器轉為 Excel 下載。
        /// </summary>
        Task<DataSet> BuildMonthlyReportAsync(InvoiceSummaryQueryDto dto, int uid);
    }
}
