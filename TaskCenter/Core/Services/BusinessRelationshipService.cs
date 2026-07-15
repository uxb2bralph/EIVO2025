using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonLib.Core.DataWork;
using CommonLib.Utility;
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
    /// 相對營業人查詢服務實作。
    /// 查詢邏輯對應舊版 BusinessRelationshipController.InquireBusinessRelationship
    /// （以主營業人 / 相對營業人統編 / 相對營業人名稱 / 營業人類別篩選）。
    /// 本站以系統管理身份運作，不套用舊版「非系統管理員限本身集團」之過濾（沿用其他已遷移頁面之做法）。
    /// </summary>
    public class BusinessRelationshipService : IBusinessRelationshipService
    {
        private const int MarkToDelete = (int)Naming.MemberStatusDefinition.Mark_To_Delete;

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BusinessRelationshipService> _logger;

        public BusinessRelationshipService(IUnitOfWork unitOfWork, ILogger<BusinessRelationshipService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PagedResultDto<BusinessRelationshipDatatableDto>> GetPagedAsync(
            BusinessRelationshipQueryDto queryDto, bool isAdmin, int? categoryId, int? companyId)
        {
            var query = _unitOfWork.Context.Set<BusinessRelationship>()
                .AsNoTracking()
                .AsQueryable();

            // 角色範圍：非系統管理者僅限自己（代理商含下轄）為主營業人的關係。
            if (!isAdmin)
            {
                var gdb = new GenericDbContext<ApplicationDbContext>(_unitOfWork.Context);
                var allowedIds = OrganizationScope.AllowedOrganizations(gdb, categoryId ?? 0, companyId ?? 0)
                    .Select(o => o.CompanyID);
                query = query.Where(r => allowedIds.Contains(r.MasterID));
            }

            // 主營業人（集團成員）篩選。
            if (queryDto.CompanyId.HasValue)
            {
                query = query.Where(r => r.MasterID == queryDto.CompanyId.Value);
            }

            // 營業人類別篩選（銷項 / 進項）。
            if (queryDto.BusinessType.HasValue)
            {
                query = query.Where(r => r.BusinessID == queryDto.BusinessType.Value);
            }

            // 相對營業人統一編號（精確比對；沿用舊版 PromptBusinessRelationship）。
            var receiptNo = queryDto.ReceiptNo.GetEfficientString();
            if (receiptNo != null)
            {
                query = query.Where(r => r.Relative.ReceiptNo == receiptNo);
            }

            // 相對營業人名稱（模糊比對，含分店名稱；沿用舊版 Inquire 對 orgItems 的條件）。
            var companyName = queryDto.CompanyName.GetEfficientString();
            if (companyName != null)
            {
                query = query.Where(r =>
                    (r.Relative.CompanyName != null && r.Relative.CompanyName.Contains(companyName))
                    || r.Relative.OrganizationBranch.Any(b => b.BranchName != null && b.BranchName.Contains(companyName)));
            }

            var totalCount = await query.CountAsync();

            // 沿用舊版 CreateXlsx 之排序（MasterID、RelativeID）。
            var rows = await query
                .OrderBy(r => r.MasterID)
                .ThenBy(r => r.RelativeID)
                .Skip(queryDto.Skip)
                .Take(queryDto.PageSize)
                .Select(r => new
                {
                    r.MasterID,
                    r.RelativeID,
                    r.BusinessID,
                    r.CompanyName,
                    MasterName = r.Master.CompanyName,
                    ReceiptNo = r.Relative.ReceiptNo,
                    BusinessTypeName = r.Business.Business,
                    r.ContactEmail,
                    r.Addr,
                    r.Phone,
                    r.CustomerNo,
                    r.CurrentLevel,
                    Expression = r.CurrentLevelNavigation != null ? r.CurrentLevelNavigation.Expression : null,
                    Entrusting = r.Relative.OrganizationStatus != null ? r.Relative.OrganizationStatus.Entrusting : null,
                    EntrustToPrint = r.Relative.OrganizationStatus != null ? r.Relative.OrganizationStatus.EntrustToPrint : null,
                })
                .ToListAsync();

            // EncryptKey 無法於 EF 查詢中翻譯，故 materialize 後再投影為 DTO。
            var items = rows.Select(r => new BusinessRelationshipDatatableDto
            {
                MasterId = r.MasterID,
                RelativeId = r.RelativeID,
                BusinessId = r.BusinessID,
                RelativeKeyId = r.RelativeID.EncryptKey(),
                MasterName = r.MasterName,
                CompanyName = r.CompanyName,
                ReceiptNo = r.ReceiptNo,
                BusinessTypeName = r.BusinessTypeName,
                ContactEmail = r.ContactEmail,
                Addr = r.Addr,
                Phone = r.Phone,
                CustomerNo = r.CustomerNo,
                // 沿用舊版 DataItem.cshtml：CurrentLevel 為 null 顯示「已啟用」，否則採 LevelExpression.Expression。
                StatusText = !r.CurrentLevel.HasValue ? "已啟用" : (r.Expression ?? string.Empty),
                Deactivated = r.CurrentLevel == MarkToDelete,
                Entrusting = r.Entrusting,
                EntrustToPrint = r.EntrustToPrint,
            }).ToList();

            return new PagedResultDto<BusinessRelationshipDatatableDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = queryDto.Page,
                PageSize = queryDto.PageSize,
            };
        }

        public async Task<List<GroupMemberDto>> GetGroupMembersAsync(bool isAdmin, int? categoryId, int? companyId)
        {
            // 系統管理員：所有企業群組成員；非系統管理者：僅限其角色範圍內之群組成員（對應舊版 GroupMemberSelector 兩分支）。
            var memberIds = _unitOfWork.Context.Set<EnterpriseGroupMember>()
                .AsNoTracking()
                .Select(m => m.CompanyID)
                .Distinct();

            if (!isAdmin)
            {
                var gdb = new GenericDbContext<ApplicationDbContext>(_unitOfWork.Context);
                var allowedIds = OrganizationScope.AllowedOrganizations(gdb, categoryId ?? 0, companyId ?? 0)
                    .Select(o => o.CompanyID);
                memberIds = memberIds.Where(id => allowedIds.Contains(id));
            }

            var members = await memberIds
                .Join(_unitOfWork.Context.Set<Organization>(),
                    id => id, o => o.CompanyID, (id, o) => new GroupMemberDto
                    {
                        CompanyId = o.CompanyID,
                        ReceiptNo = o.ReceiptNo,
                        CompanyName = o.CompanyName,
                    })
                .OrderBy(o => o.ReceiptNo)
                .ToListAsync();

            return members;
        }
    }
}
