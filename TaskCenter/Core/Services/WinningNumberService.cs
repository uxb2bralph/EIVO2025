using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelCore.DataEntity;
using ModelCore.Locale;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// 中獎號碼查詢服務實作。
    /// 查詢邏輯對應舊版 WinningNumberController.Inquire（依發票年度 + 期別篩選，依獎別排序）。
    /// </summary>
    public class WinningNumberService : IWinningNumberService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WinningNumberService> _logger;

        public WinningNumberService(IUnitOfWork unitOfWork, ILogger<WinningNumberService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<WinningNumberDatatableDto>> GetListAsync(WinningNumberQueryDto queryDto)
        {
            var year = queryDto.Year ?? 0;
            var period = queryDto.PeriodNo ?? 0;

            // 對應舊版 Where(w => w.Year == viewModel.Year && w.Period == viewModel.PeriodNo)。
            var items = await _unitOfWork.Context.Set<UniformInvoiceWinningNumber>()
                .AsNoTracking()
                .Where(w => w.Year == year && w.Period == period)
                .OrderBy(w => w.Rank) // 對應 ItemList.cshtml 之 OrderBy(w => w.Rank)
                .Select(w => new WinningNumberDatatableDto
                {
                    WinningId = w.WinningID,
                    Year = w.Year,
                    Period = w.Period,
                    Rank = w.Rank,
                    PrizeType = w.PrizeType,
                    Bonus = w.Bonus,
                    WinningNo = w.WinningNO,
                    // 特別獎(1) / 特獎(2) / 頭獎(3) / 增開六獎(9) 可維護，其餘為頭獎自動衍生。
                    Editable = w.Rank == (int)Naming.EditableWinningPrizeType.特別獎
                        || w.Rank == (int)Naming.EditableWinningPrizeType.特獎
                        || w.Rank == (int)Naming.EditableWinningPrizeType.頭獎
                        || w.Rank == (int)Naming.EditableWinningPrizeType.增開六獎,
                })
                .ToListAsync();

            return items;
        }
    }
}
