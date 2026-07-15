using System.Collections.Generic;
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

        /// <summary>
        /// 依關鍵字（統編前綴或名稱包含）搜尋所屬經銷商候選清單，供 autocomplete 使用。
        /// keyword 為空時回傳前 N 筆經銷商；結果數量上限由實作決定。
        /// </summary>
        Task<List<OrganizationAgentDto>> SearchAgentsAsync(string? keyword);

        /// <summary>
        /// 依關鍵字（統編前綴或名稱包含）搜尋主機構候選清單（已設定為主機構 MasterOrganization 的營業人），
        /// 供「設為分支機構」選擇主機構的 autocomplete 使用。
        /// 沿用舊版 Home/SearchHeadquarter：keyword 為空時回傳空集合；結果數量上限由實作決定。
        /// </summary>
        Task<List<HeadquarterDto>> SearchHeadquartersAsync(string? keyword);
    }
}
