using System.Threading.Tasks;
using ModelCore.DTOs;
using TaskCenter.Core.DTOs;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// 使用者帳號查詢服務（遷移自舊版 AccountController.Inquire）。
    /// </summary>
    public interface IUserAccountService
    {
        /// <summary>
        /// 依查詢條件取得分頁使用者帳號列表（限定於指定營業人）。
        /// </summary>
        Task<PagedResultDto<UserAccountDatatableDto>> GetPagedAsync(UserAccountQueryDto queryDto);
    }
}
