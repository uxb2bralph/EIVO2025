using System.Threading.Tasks;
using ModelCore.DTOs;
using TaskCenter.Core.DTOs;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// 電子發票字軌查詢服務（遷移自舊版 TrackCodeController.Inquire）。
    /// </summary>
    public interface ITrackCodeService
    {
        /// <summary>
        /// 依查詢條件取得分頁字軌列表（限定於指定發票年度）。
        /// </summary>
        Task<PagedResultDto<TrackCodeDatatableDto>> GetPagedAsync(TrackCodeQueryDto queryDto);
    }
}
