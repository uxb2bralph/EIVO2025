using System.Threading.Tasks;
using ModelCore.DTOs;
using TaskCenter.Core.DTOs;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// 期別匯率查詢服務（遷移自舊版 PeriodicalExchangeRateController.Inquire）。
    /// </summary>
    public interface IPeriodicalExchangeRateService
    {
        /// <summary>
        /// 依發票年度（+ 期別 / 幣別）取得期別匯率列表（分頁）。
        /// </summary>
        Task<PagedResultDto<ExchangeRateDatatableDto>> GetPagedAsync(ExchangeRateQueryDto queryDto);
    }
}
