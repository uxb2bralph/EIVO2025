using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelCore.DataEntity;
using ModelCore.DTOs;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// 電子發票字軌查詢服務實作。
    /// 查詢邏輯對應舊版 TrackCodeController.Inquire（依發票年度 + 期別篩選）。
    /// </summary>
    public class TrackCodeService : ITrackCodeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TrackCodeService> _logger;

        public TrackCodeService(IUnitOfWork unitOfWork, ILogger<TrackCodeService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PagedResultDto<TrackCodeDatatableDto>> GetPagedAsync(TrackCodeQueryDto queryDto)
        {
            // 發票年度（對應舊版 Where(t => t.Year == viewModel.Year)）。實體 Year / PeriodNo 為 short。
            var year = (short)(queryDto.Year ?? 0);

            var query = _unitOfWork.Context.Set<InvoiceTrackCode>()
                .AsNoTracking()
                .Where(t => t.Year == year);

            // 期別（對應舊版 PeriodNo.HasValue 時再篩選）。
            if (queryDto.PeriodNo.HasValue)
            {
                var periodNo = (short)queryDto.PeriodNo.Value;
                query = query.Where(t => t.PeriodNo == periodNo);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(t => t.PeriodNo)
                .ThenBy(t => t.TrackCode)
                .Skip(queryDto.Skip)
                .Take(queryDto.PageSize)
                .Select(t => new TrackCodeDatatableDto
                {
                    TrackId = t.TrackID,
                    Year = t.Year,
                    PeriodNo = t.PeriodNo,
                    TrackCode = t.TrackCode,
                    InvoiceType = t.InvoiceType,
                })
                .ToListAsync();

            return new PagedResultDto<TrackCodeDatatableDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = queryDto.Page,
                PageSize = queryDto.PageSize,
            };
        }
    }
}
