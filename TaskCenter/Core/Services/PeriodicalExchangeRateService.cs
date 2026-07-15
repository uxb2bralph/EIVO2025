using System.Linq;
using System.Threading.Tasks;
using CommonLib.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelCore.DataEntity;
using ModelCore.DTOs;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// 期別匯率查詢服務實作。
    /// 查詢邏輯對應舊版 PeriodicalExchangeRateController.Inquire（依 PeriodID = 年度*100 + 期別 篩選）。
    /// </summary>
    public class PeriodicalExchangeRateService : IPeriodicalExchangeRateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PeriodicalExchangeRateService> _logger;

        public PeriodicalExchangeRateService(IUnitOfWork unitOfWork, ILogger<PeriodicalExchangeRateService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PagedResultDto<ExchangeRateDatatableDto>> GetPagedAsync(ExchangeRateQueryDto queryDto)
        {
            var year = queryDto.Year ?? 0;

            var query = _unitOfWork.Context.Set<InvoicePeriodExchangeRate>()
                .AsNoTracking()
                .AsQueryable();

            // 期別篩選（PeriodID = 年度*100 + 期別）。
            // 舊版於 PeriodNo 為空時不加期別條件（等同全部年度）；此處改為以所選年度為範圍（PeriodID 落在 年度*100+1 ~ 年度*100+6），
            // 更貼合「已選定年度」之使用者意圖，並與字軌 / 中獎號碼維護頁一致。
            if (queryDto.PeriodNo.HasValue)
            {
                var periodId = year * 100 + queryDto.PeriodNo.Value;
                query = query.Where(t => t.PeriodID == periodId);
            }
            else
            {
                query = query.Where(t => t.PeriodID >= year * 100 + 1 && t.PeriodID <= year * 100 + 6);
            }

            // 幣別篩選（AbbrevName 前綴比對；NTD 視同 TWD，沿用舊版 Inquire）。
            var currency = queryDto.Currency.GetEfficientString();
            if (currency != null)
            {
                if (currency == "NTD")
                {
                    currency = "TWD";
                }
                query = query.Where(t => t.Currency.AbbrevName!.StartsWith(currency));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(t => t.PeriodID)
                .ThenBy(t => t.Currency.AbbrevName)
                .Skip(queryDto.Skip)
                .Take(queryDto.PageSize)
                .Select(t => new ExchangeRateDatatableDto
                {
                    PeriodId = t.PeriodID,
                    Year = t.PeriodID / 100,
                    PeriodNo = t.PeriodID % 100,
                    CurrencyId = t.CurrencyID,
                    Currency = t.Currency.AbbrevName,
                    CurrencyName = t.Currency.CurrencyName,
                    ExchangeRate = t.ExchangeRate,
                })
                .ToListAsync();

            return new PagedResultDto<ExchangeRateDatatableDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = queryDto.Page,
                PageSize = queryDto.PageSize,
            };
        }
    }
}
