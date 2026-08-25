using System.Data;
using System.Threading.Tasks;
using ModelCore.DTOs;
using TaskCenter.Core.DTOs;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// 中獎統計表服務（遷移自 WebHome WinningInvoiceController.ReportIndex / InquireReport /
    /// ReportGridPage / CreateXlsx）。發票條件過濾重用 InvoiceQueryPipeline（BuildInvoiceQuery，
    /// 內含 FilterInvoiceByRole 角色資料範圍），再強制只取中獎發票並依開立人彙總。服務僅查詢，不做寫入。
    /// </summary>
    public interface IWinningInvoiceReportService
    {
        /// <summary>依開立人彙總中獎 / 捐贈張數（分頁）。uid 為登入者 UID，用以還原角色資料範圍。</summary>
        Task<PagedResultDto<WinningInvoiceReportRowDto>> GetPagedAsync(WinningInvoiceReportQueryDto dto, int uid);

        /// <summary>
        /// 組裝中獎統計表內容（單一工作表「中獎統計」，不分頁；對應舊版 Module/CreateXlsx.cshtml）。
        /// 回傳 DataSet 供控制器轉為 Excel 下載。
        /// </summary>
        Task<DataSet> BuildReportAsync(WinningInvoiceReportQueryDto dto, int uid);
    }
}
