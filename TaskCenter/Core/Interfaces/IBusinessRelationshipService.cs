using System.Collections.Generic;
using System.Threading.Tasks;
using ModelCore.DTOs;
using TaskCenter.Core.DTOs;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// 相對營業人查詢服務（遷移自舊版 BusinessRelationshipController.InquireBusinessRelationship）。
    /// </summary>
    public interface IBusinessRelationshipService
    {
        /// <summary>
        /// 依查詢條件取得相對營業人關係列表（分頁），並依登入者角色範圍限制主營業人（MasterID）。
        /// </summary>
        Task<PagedResultDto<BusinessRelationshipDatatableDto>> GetPagedAsync(
            BusinessRelationshipQueryDto queryDto, bool isAdmin, int? categoryId, int? companyId);

        /// <summary>
        /// 取得集團成員（主營業人）下拉選項（對應舊版 GroupMemberSelector），並依登入者角色範圍限制。
        /// </summary>
        Task<List<GroupMemberDto>> GetGroupMembersAsync(bool isAdmin, int? categoryId, int? companyId);
    }
}
