using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelCore.DataEntity;
using ModelCore.DTOs;
using ModelCore.Helper;
using ModelCore.Locale;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// 使用者帳號查詢服務實作。
    /// 查詢邏輯對應舊版 AccountController.Inquire（以營業人限定範圍 + PID/名稱/角色/狀態篩選）。
    /// </summary>
    public class UserAccountService : IUserAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserAccountService> _logger;

        public UserAccountService(IUnitOfWork unitOfWork, ILogger<UserAccountService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PagedResultDto<UserAccountDatatableDto>> GetPagedAsync(UserAccountQueryDto queryDto)
        {
            var companyId = queryDto.CompanyId;

            // 限定於指定營業人（對應舊版 FilterByOrganization：UserRole → OrgaCate.CompanyID）。
            var query = _unitOfWork.Context.Set<UserProfile>()
                .AsNoTracking()
                .Where(u => u.UserRole.Any(r => r.OrgaCate.CompanyID == companyId));

            // 帳號（PID）：前綴比對
            var pid = queryDto.Pid?.Trim();
            if (!string.IsNullOrEmpty(pid))
                query = query.Where(u => u.PID.StartsWith(pid));

            // 會員名稱：包含比對
            var userName = queryDto.UserName?.Trim();
            if (!string.IsNullOrEmpty(userName))
                query = query.Where(u => u.UserName!.Contains(userName));

            // 身份角色
            if (queryDto.RoleId.HasValue)
                query = query.Where(u => u.UserRole.Any(r => r.RoleID == queryDto.RoleId.Value));

            // 會員狀態：沿用舊版 Inquire，篩選 UserProfileStatus.CurrentLevel
            if (queryDto.LevelId.HasValue)
                query = query.Where(u => u.UserProfileStatus!.CurrentLevel == queryDto.LevelId.Value);

            var totalCount = await query.CountAsync();

            var rows = await query
                .OrderBy(u => u.PID)
                .Skip(queryDto.Skip)
                .Take(queryDto.PageSize)
                .Select(u => new
                {
                    u.UID,
                    u.UserName,
                    u.PID,
                    u.EMail,
                    u.LevelID,
                    // 首個 UserRole 之角色與所屬營業人名稱（沿用舊版 First() 取法）。
                    Role = u.UserRole
                        .Select(r => new { r.RoleID, CompanyName = r.OrgaCate.Company.CompanyName })
                        .FirstOrDefault(),
                })
                .ToListAsync();

            // EncryptKey() 為記憶體運算，無法在 EF 查詢中翻譯，故 materialize 後逐筆投影。
            var items = rows.Select(r => new UserAccountDatatableDto
            {
                KeyId = r.UID.EncryptKey(),
                CompanyName = r.Role?.CompanyName,
                RoleId = r.Role?.RoleID,
                RoleName = ResolveRoleName(r.Role?.RoleID),
                UserName = r.UserName,
                Pid = r.PID,
                Email = r.EMail,
                LevelId = r.LevelID,
            }).ToList();

            return new PagedResultDto<UserAccountDatatableDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = queryDto.Page,
                PageSize = queryDto.PageSize,
            };
        }

        /// <summary>
        /// 將 RoleID 轉為顯示名稱（沿用舊版 (Naming.EIVOUserRoleID)RoleID.ToString()）；
        /// 未定義的角色代碼則回傳原始數值。
        /// </summary>
        private static string? ResolveRoleName(int? roleId)
        {
            if (!roleId.HasValue)
                return null;

            return Enum.IsDefined(typeof(Naming.EIVOUserRoleID), roleId.Value)
                ? ((Naming.EIVOUserRoleID)roleId.Value).ToString()
                : roleId.Value.ToString();
        }
    }
}
