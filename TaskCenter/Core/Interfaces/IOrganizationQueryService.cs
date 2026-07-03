using System.Threading.Tasks;
using ModelCore.DTOs;
using TaskCenter.Core.DTOs;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// 營業人資料查詢服務。
    /// </summary>
    public interface IOrganizationQueryService
    {
        /// <summary>
        /// 依查詢條件取得分頁營業人資料列表。
        /// </summary>
        Task<PagedResultDto<OrganizationDatatableDto>> GetPagedAsync(OrganizationQueryDto queryDto);
    }
}
