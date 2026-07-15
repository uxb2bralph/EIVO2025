using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonLib.Core.DataWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelCore.DataEntity;
using ModelCore.DTOs;
using ModelCore.Helper;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// 電子發票配號區間查詢服務實作。
    /// 查詢邏輯對應舊版 InvoiceNoController.InquireInterval（重用 QueryExtensions.InquireInvoiceNoInterval）。
    /// 目前以系統管理範圍查詢（profile=null）：僅依所選開立人 + 年度 + 期別篩選。
    /// </summary>
    public class InvoiceNoIntervalService : IInvoiceNoIntervalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InvoiceNoIntervalService> _logger;

        public InvoiceNoIntervalService(IUnitOfWork unitOfWork, ILogger<InvoiceNoIntervalService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PagedResultDto<InvoiceNoIntervalDatatableDto>> GetPagedAsync(
            InvoiceNoIntervalQueryDto queryDto, bool isAdmin, int? categoryId, int? companyId)
        {
            var models = new GenericDbContext<ApplicationDbContext>(_unitOfWork.Context);

            var chosenSeller = string.IsNullOrEmpty(queryDto.SellerKey) ? (int?)null : queryDto.SellerKey.DecryptKeyValue();

            // 依角色範圍建立基礎查詢（移植自 QueryExtensions.InquireInvoiceNoInterval 的 admin / 非 admin 分支）。
            IQueryable<InvoiceNoInterval> query = models.GetTable<InvoiceNoInterval>();
            if (isAdmin)
            {
                if (chosenSeller.HasValue)
                {
                    query = queryDto.BranchRelation == true
                        ? query.Join(models.GetQueryByAgent(chosenSeller.Value), n => n.SellerID, o => o.CompanyID, (n, o) => n)
                        : query.Where(t => t.SellerID == chosenSeller.Value);
                }
            }
            else
            {
                // 非系統管理：限制在登入者可存取的營業人範圍內；join 即強制範圍（越權查詢自動落空）。
                var allowed = OrganizationScope.AllowedOrganizations(models, categoryId ?? 0, companyId ?? 0);
                if (queryDto.BranchRelation != true && chosenSeller.HasValue)
                {
                    allowed = allowed.Where(o => o.CompanyID == chosenSeller.Value);
                }
                query = query.Join(allowed, n => n.SellerID, o => o.CompanyID, (n, o) => n);
            }

            if (queryDto.Year.HasValue)
            {
                var yr = (short)queryDto.Year.Value;
                query = query.Where(i => i.InvoiceTrackCodeAssignment.Track.Year == yr);
            }
            if (queryDto.PeriodNo.HasValue)
            {
                var pd = (short)queryDto.PeriodNo.Value;
                query = query.Where(i => i.InvoiceTrackCodeAssignment.Track.PeriodNo == pd);
            }

            var totalCount = await query.CountAsync();

            // CurrentAllocatingNo() 與「是否已使用」需以已配發 / 已指派號碼計算，改於投影中以相關子查詢取回
            // 純量（避免載入整個明細集合），materialize 後再於記憶體換算目前給號 / 剩餘 / 可編輯。
            var rows = await query
                .OrderBy(i => i.InvoiceTrackCodeAssignment.Seller.ReceiptNo)
                .ThenBy(i => i.InvoiceTrackCodeAssignment.Track.TrackCode)
                .ThenBy(i => i.StartNo)
                .Skip(queryDto.Skip)
                .Take(queryDto.PageSize)
                .Select(i => new
                {
                    i.IntervalID,
                    ReceiptNo = i.InvoiceTrackCodeAssignment.Seller.ReceiptNo,
                    i.InvoiceTrackCodeAssignment.Track.Year,
                    i.InvoiceTrackCodeAssignment.Track.PeriodNo,
                    i.InvoiceTrackCodeAssignment.Track.TrackCode,
                    i.InvoiceTrackCodeAssignment.Track.InvoiceType,
                    i.StartNo,
                    i.EndNo,
                    DeviceName = i.InvoiceNoSegment != null ? i.InvoiceNoSegment.DeviceName : null,
                    i.LockID,
                    MaxAllocated = i.InvoiceNoAllocation.Max(a => (int?)a.InvoiceNo),
                    MaxAssigned = i.InvoiceNoAssignment.Max(a => (int?)a.InvoiceNo),
                    HasAllocation = i.InvoiceNoAllocation.Any(),
                    HasAssignment = i.InvoiceNoAssignment.Any(),
                    // 開立人是否為主機構（沿用 OrganizationQuery 的 IsMaster 判定）。
                    IsMaster = i.InvoiceTrackCodeAssignment.Seller.MasterOrganization != null,
                    // 是否已有涵蓋本區間之主機構配號（InvoiceNoMainAssignment）。
                    HasMainAssignment = i.InvoiceTrackCodeAssignment.InvoiceNoMainAssignment.Any(m =>
                        (m.StartNo <= i.StartNo && m.EndNo >= i.StartNo) || (m.StartNo <= i.EndNo && m.EndNo >= i.EndNo)),
                })
                .ToListAsync();

            var items = rows.Select(r =>
            {
                var currentNo = Math.Max((r.MaxAllocated + 1) ?? r.StartNo, (r.MaxAssigned + 1) ?? r.StartNo);
                var total = r.EndNo - r.StartNo + 1;
                return new InvoiceNoIntervalDatatableDto
                {
                    IntervalId = r.IntervalID,
                    ReceiptNo = r.ReceiptNo,
                    Year = r.Year,
                    PeriodNo = r.PeriodNo,
                    TrackCode = r.TrackCode,
                    InvoiceType = r.InvoiceType,
                    StartNo = r.StartNo,
                    EndNo = r.EndNo,
                    DeviceName = r.DeviceName,
                    TotalCount = total,
                    BookletCount = total / 50,
                    CurrentNo = currentNo,
                    Remaining = r.EndNo - currentNo + 1,
                    Locked = r.LockID != null,
                    Editable = !r.HasAllocation && !r.HasAssignment,
                    IsMaster = r.IsMaster,
                    HasMainAssignment = r.HasMainAssignment,
                };
            }).ToList();

            return new PagedResultDto<InvoiceNoIntervalDatatableDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = queryDto.Page,
                PageSize = queryDto.PageSize,
            };
        }

        public async Task<List<InvoiceNoSellerOptionDto>> SearchSellersAsync(
            string? keyword, bool isAdmin, int? categoryId, int? companyId)
        {
            const int maxResults = 20;

            var models = new GenericDbContext<ApplicationDbContext>(_unitOfWork.Context);

            IQueryable<Organization> query;
            if (isAdmin)
            {
                // 系統管理可見之開立人類別（對應 InitializeOrganizationQuery 之 COMP_SYS 分支）。
                var categories = new[]
                {
                    (int)Naming.CategoryID.COMP_E_INVOICE_B2C_SELLER,
                    (int)Naming.CategoryID.COMP_VIRTUAL_CHANNEL,
                    (int)Naming.CategoryID.COMP_E_INVOICE_GOOGLE_TW,
                    (int)Naming.CategoryID.COMP_INVOICE_AGENT,
                };
                query = models.GetTable<Organization>()
                    .Where(o => o.OrganizationCategory.Any(c => categories.Contains(c.CategoryID)));
            }
            else
            {
                // 非系統管理：僅限登入者可存取的營業人範圍。
                query = OrganizationScope.AllowedOrganizations(models, categoryId ?? 0, companyId ?? 0);
            }

            query = query.AsNoTracking();

            var trimmed = keyword?.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                query = query.Where(o =>
                    o.ReceiptNo!.StartsWith(trimmed) || o.CompanyName!.Contains(trimmed));
            }

            var sellers = await query
                .OrderBy(o => o.ReceiptNo)
                .Take(maxResults)
                .Select(o => new { o.CompanyID, o.ReceiptNo, o.CompanyName })
                .ToListAsync();

            // EncryptKey() 為記憶體運算，無法於 EF 查詢中翻譯，materialize 後逐筆填入加密 sellerKey。
            return sellers
                .Select(o => new InvoiceNoSellerOptionDto
                {
                    SellerKey = o.CompanyID.EncryptKey(),
                    ReceiptNo = o.ReceiptNo,
                    CompanyName = o.CompanyName,
                })
                .ToList();
        }

        public async Task<List<InvoiceTrackCodeOptionDto>> GetTrackCodeOptionsAsync(int? year, int? periodNo)
        {
            var y = (short)(year ?? 0);

            var query = _unitOfWork.Context.Set<InvoiceTrackCode>()
                .AsNoTracking()
                .Where(t => t.Year == y);

            if (periodNo.HasValue)
            {
                var p = (short)periodNo.Value;
                query = query.Where(t => t.PeriodNo == p);
            }

            return await query
                .OrderBy(t => t.PeriodNo)
                .ThenBy(t => t.TrackCode)
                .Select(t => new InvoiceTrackCodeOptionDto
                {
                    TrackId = t.TrackID,
                    TrackCode = t.TrackCode,
                    InvoiceType = t.InvoiceType,
                })
                .ToListAsync();
        }
    }
}
