using System.Data;
using System.Threading.Tasks;
using TaskCenter.Core.DTOs;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// 發票月報表服務（遷移自 WebHome InvoiceQueryController.InquireMonthlyReport +
    /// Business.EF 之 InvoiceDataReportExtensions.CreateReport）。
    /// 依所選開立人 / 代理業者旗下開立人，逐月統計發票、作廢發票、折讓、作廢折讓筆數與計費。
    /// 服務僅查詢，不做寫入。
    /// </summary>
    public interface IMonthlyReportService
    {
        /// <summary>
        /// 組裝月報表內容（單一工作表「發票資料明細」，每個營業人 × 月份一列，
        /// 末端附上「月服務費」合計列）。uid 為登入者 UID，用以還原角色資料範圍。
        /// 回傳 DataSet 供控制器轉為 Excel 下載。
        /// </summary>
        Task<DataSet> BuildReportAsync(MonthlyReportQueryDto dto, int uid);
    }
}
