using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelCore.DataEntity;
using ModelCore.DTOs;
using ModelCore.Helper;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// 營業人資料查詢服務實作。
    /// 查詢邏輯對應舊版 OrganizationQueryController.InquireCompany 與 InquireOrganization 系列 Inquiry。
    /// </summary>
    public class OrganizationQueryService : IOrganizationQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrganizationQueryService> _logger;

        public OrganizationQueryService(IUnitOfWork unitOfWork, ILogger<OrganizationQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PagedResultDto<OrganizationDatatableDto>> GetPagedAsync(OrganizationQueryDto queryDto)
        {
            // 基礎條件：僅含有狀態的營業人（對應舊版 Items.Where(o => o.OrganizationStatus != null)）
            var query = _unitOfWork.Context.Set<Organization>()
                .AsNoTracking()
                .Where(o => o.OrganizationStatus != null);

            // 統編：前綴比對（InquireOrganizationReceiptNo）
            var receiptNo = queryDto.ReceiptNo?.Trim();
            if (!string.IsNullOrEmpty(receiptNo))
                query = query.Where(o => o.ReceiptNo!.StartsWith(receiptNo));

            // 營業人名稱：包含比對（InquireCompanyName）
            var companyName = queryDto.CompanyName?.Trim();
            if (!string.IsNullOrEmpty(companyName))
                query = query.Where(o => o.CompanyName!.Contains(companyName));

            // 營業人狀態（InquireOrganizationStatus）
            if (queryDto.OrganizationStatus.HasValue)
                query = query.Where(o => o.OrganizationStatus!.CurrentLevel == queryDto.OrganizationStatus.Value);

            // 營業人類別
            if (queryDto.CategoryId.HasValue)
                query = query.Where(o => o.OrganizationCategory.Any(c => c.CategoryID == queryDto.CategoryId.Value));

            // 所屬經銷商 / 分支機構（此營業人為某經銷商之開立人 Issuer）
            if (queryDto.AgentId.HasValue)
            {
                var agentId = queryDto.AgentId.Value;
                if (queryDto.BranchRelation == true)
                {
                    var masterBranch = (int)InvoiceIssuerAgent.RelationTypeEnum.MasterBranch;
                    query = query.Where(o => o.AsInvoiceIssuer
                        .Any(a => a.AgentID == agentId && a.RelationType == masterBranch));
                }
                else
                {
                    query = query.Where(o => o.AsInvoiceIssuer.Any(a => a.AgentID == agentId));
                }
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(o => o.ReceiptNo)
                .Skip(queryDto.Skip)
                .Take(queryDto.PageSize)
                .Select(o => new OrganizationDatatableDto
                {
                    CompanyId = o.CompanyID,
                    CompanyName = o.CompanyName,
                    ReceiptNo = o.ReceiptNo,
                    UndertakerName = o.UndertakerName,
                    ContactEmail = o.ContactEmail,
                    StatusLevel = o.OrganizationStatus!.CurrentLevel,
                    StatusName = o.OrganizationStatus!.CurrentLevelNavigation!.Description,
                    GoLiveDate = o.OrganizationExtension!.GoLiveDate,
                    ExpirationDate = o.OrganizationExtension!.ExpirationDate,
                    IsMaster = o.MasterOrganization != null,
                })
                .ToListAsync();

            // EncryptKey() 為記憶體運算，無法在 EF 查詢中翻譯，故於 materialize 後逐筆填入加密 KeyID。
            foreach (var item in items)
            {
                item.KeyId = item.CompanyId.EncryptKey();
            }

            return new PagedResultDto<OrganizationDatatableDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = queryDto.Page,
                PageSize = queryDto.PageSize,
            };
        }

        public async Task<List<OrganizationAgentDto>> SearchAgentsAsync(string? keyword)
        {
            // autocomplete 一次最多回傳的候選筆數（避免關鍵字過短時撈回全部經銷商）。
            const int maxResults = 20;

            // 對應舊版 InquireOrganization.cshtml：取類別為「經銷商」的營業人。
            var dealerCategory = (int)CategoryDefinition.CategoryEnum.經銷商;

            var query = _unitOfWork.Context.Set<Organization>()
                .AsNoTracking()
                .Where(o => o.OrganizationCategory.Any(c => c.CategoryID == dealerCategory));

            // 關鍵字：統編前綴或名稱包含比對（沿用查詢列表的比對慣例）。
            var trimmed = keyword?.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                query = query.Where(o =>
                    o.ReceiptNo!.StartsWith(trimmed) || o.CompanyName!.Contains(trimmed));
            }

            return await query
                .OrderBy(o => o.ReceiptNo)
                .Take(maxResults)
                .Select(o => new OrganizationAgentDto
                {
                    CompanyId = o.CompanyID,
                    ReceiptNo = o.ReceiptNo,
                    CompanyName = o.CompanyName,
                })
                .ToListAsync();
        }

        public async Task<List<HeadquarterDto>> SearchHeadquartersAsync(string? keyword)
        {
            // autocomplete 一次最多回傳的候選筆數。
            const int maxResults = 20;

            // 沿用舊版 Home/SearchHeadquarter：關鍵字為空時回傳空集合（items.Where(f => false)）。
            var trimmed = keyword?.Trim();
            if (string.IsNullOrEmpty(trimmed))
                return new List<HeadquarterDto>();

            // 對應舊版：僅取已設定為主機構（MasterOrganization != null）的營業人，
            // 以統編前綴或名稱包含比對。
            var items = await _unitOfWork.Context.Set<Organization>()
                .AsNoTracking()
                .Where(o => o.MasterOrganization != null)
                .Where(o => o.ReceiptNo!.StartsWith(trimmed) || o.CompanyName!.Contains(trimmed))
                .OrderBy(o => o.ReceiptNo)
                .Take(maxResults)
                .Select(o => new { o.CompanyID, o.ReceiptNo, o.CompanyName })
                .ToListAsync();

            // EncryptKey() 為記憶體運算，無法在 EF 查詢中翻譯，故 materialize 後逐筆填入加密 KeyID。
            return items
                .Select(o => new HeadquarterDto
                {
                    KeyId = o.CompanyID.EncryptKey(),
                    ReceiptNo = o.ReceiptNo,
                    CompanyName = o.CompanyName,
                })
                .ToList();
        }
    }
}
