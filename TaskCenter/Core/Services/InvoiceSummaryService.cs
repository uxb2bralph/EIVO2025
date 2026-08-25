using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CommonLib.Core.DataWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelCore.DataEntity;
using ModelCore.DataEntityWrapper;
using ModelCore.DTOs;
using ModelCore.Helper;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// 發票統計表服務實作（遷移自 WebHome InvoiceQueryController.InquireSummary /
    /// CreateMonthlyReportXlsx / SaveAsExcel）。
    /// 發票條件過濾重用 <see cref="InvoiceQueryPipeline"/>（內含 FilterInvoiceByRole 角色範圍），
    /// 營業人清單另以 FilterOrganizationByRole 依角色限縮，再套用所選開立人 / 代理業者。
    /// </summary>
    public class InvoiceSummaryService : IInvoiceSummaryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InvoiceSummaryService> _logger;

        public InvoiceSummaryService(IUnitOfWork unitOfWork, ILogger<InvoiceSummaryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ── 查詢範圍建立 ────────────────────────────────────────────

        /// <summary>
        /// 建立查詢範圍：發票（依條件 + 角色）與營業人（依角色 + 所選開立人 / 代理）。
        /// 回傳 false 表示登入者無有效角色（視為無資料）。
        /// </summary>
        private bool TryBuildScope(
            InvoiceSummaryQueryDto dto, int uid,
            out GenericDbContext<ApplicationDbContext> models,
            out IQueryable<InvoiceItem> invoices,
            out IQueryable<Organization> sellers)
        {
            models = new GenericDbContext<ApplicationDbContext>(_unitOfWork.Context);
            invoices = null!;
            sellers = null!;

            var profile = InvoiceQueryPipeline.GetProfile(models, uid);
            if (profile?.CurrentUserRole == null)
            {
                return false;
            }

            invoices = InvoiceQueryPipeline.BuildQuery(models, dto, profile);

            // 營業人清單（對應舊版 InquireSummary 之 sellerItems）。
            var sellerItems = models.FilterOrganizationByRole(profile, models.GetTable<Organization>());

            if (!string.IsNullOrEmpty(dto.SellerKey))
            {
                var sellerId = dto.SellerKey.DecryptKeyValue();
                sellerItems = sellerItems.Where(o => o.CompanyID == sellerId);
            }

            if (!string.IsNullOrEmpty(dto.AgentKey))
            {
                var agentId = dto.AgentKey.DecryptKeyValue();
                sellerItems = sellerItems
                    .Join(models.GetTable<InvoiceIssuerAgent>().Where(a => a.AgentID == agentId),
                        o => o.CompanyID, a => a.IssuerID, (o, a) => o);
            }

            sellers = sellerItems.AsNoTracking();
            return true;
        }

        /// <summary>排序（對應舊版 TableBody.cshtml；預設依統一編號遞增）。</summary>
        private static IQueryable<Organization> ApplySort(IQueryable<Organization> query, string? sortName, int? sortType)
        {
            // sortType：1 遞增、2 遞減；其餘視為未指定。
            bool desc = sortType == 2;
            if (sortType == 1 || desc)
            {
                switch (sortName)
                {
                    case "CompanyName":
                        return desc ? query.OrderByDescending(o => o.CompanyName) : query.OrderBy(o => o.CompanyName);
                    case "ReceiptNo":
                        return desc ? query.OrderByDescending(o => o.ReceiptNo) : query.OrderBy(o => o.ReceiptNo);
                }
            }
            return query.OrderBy(o => o.ReceiptNo);
        }

        /// <summary>取得指定營業人於查詢範圍內的發票筆數（以單一 GROUP BY 取代舊版逐列 Count）。</summary>
        private static async Task<Dictionary<int, int>> CountBySellerAsync(
            IQueryable<InvoiceItem> invoices, IReadOnlyCollection<int> companyIds)
        {
            if (companyIds.Count == 0)
            {
                return new Dictionary<int, int>();
            }

            var idList = companyIds.ToList();
            var counts = await invoices
                .Where(i => i.SellerID.HasValue && idList.Contains(i.SellerID.Value))
                .GroupBy(i => i.SellerID!.Value)
                .Select(g => new { SellerId = g.Key, Count = g.Count() })
                .ToListAsync();

            return counts.ToDictionary(c => c.SellerId, c => c.Count);
        }

        // ── 統計清單 ────────────────────────────────────────────────

        public async Task<PagedResultDto<InvoiceSummaryRowDto>> GetPagedAsync(InvoiceSummaryQueryDto dto, int uid)
        {
            if (!TryBuildScope(dto, uid, out _, out var invoices, out var sellers))
            {
                return Empty(dto);
            }

            var totalCount = await sellers.CountAsync();

            var rows = await ApplySort(sellers, dto.SortName, dto.SortType)
                .Skip(dto.Skip)
                .Take(dto.PageSize)
                .Select(o => new
                {
                    o.CompanyID,
                    o.CompanyName,
                    o.ReceiptNo,
                    ExpirationDate = (DateTime?)(o.OrganizationExtension != null ? o.OrganizationExtension.ExpirationDate : null),
                })
                .ToListAsync();

            var counts = await CountBySellerAsync(invoices, rows.Select(r => r.CompanyID).ToList());

            var items = rows.Select(r => new InvoiceSummaryRowDto
            {
                SellerKey = r.CompanyID.EncryptKey(),
                CompanyName = r.CompanyName,
                ReceiptNo = r.ReceiptNo,
                InvoiceCount = counts.TryGetValue(r.CompanyID, out var c) ? c : 0,
                ExpirationDate = r.ExpirationDate,
            }).ToList();

            return new PagedResultDto<InvoiceSummaryRowDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = dto.Page,
                PageSize = dto.PageSize,
            };
        }

        private static PagedResultDto<InvoiceSummaryRowDto> Empty(InvoiceSummaryQueryDto dto) => new()
        {
            Items = new List<InvoiceSummaryRowDto>(),
            TotalCount = 0,
            PageNumber = dto.Page,
            PageSize = dto.PageSize,
        };

        // ── 開立發票月報表 ──────────────────────────────────────────

        public async Task<DataSet> BuildMonthlyReportAsync(InvoiceSummaryQueryDto dto, int uid)
        {
            var ds = new DataSet();

            var statistics = new DataTable(
                $"發票資料統計({dto.DateFrom:yyyy-MM-dd}~{dto.DateTo:yyyy-MM-dd})");
            statistics.Columns.Add(new DataColumn("開立發票營業人", typeof(string)));
            statistics.Columns.Add(new DataColumn("統編", typeof(string)));
            statistics.Columns.Add(new DataColumn("上線日期", typeof(string)));
            statistics.Columns.Add(new DataColumn("發票筆數", typeof(int)));
            statistics.Columns.Add(new DataColumn("註記停用日期", typeof(string)));
            ds.Tables.Add(statistics);

            if (!TryBuildScope(dto, uid, out _, out var invoices, out var sellers))
            {
                return ds;
            }

            // 第一張表：營業人統計（不分頁，依統編排序）。
            var orgs = await sellers
                .OrderBy(o => o.ReceiptNo)
                .Select(o => new
                {
                    o.CompanyID,
                    o.CompanyName,
                    o.ReceiptNo,
                    GoLiveDate = (DateTime?)(o.OrganizationExtension != null ? o.OrganizationExtension.GoLiveDate : null),
                    ExpirationDate = (DateTime?)(o.OrganizationExtension != null ? o.OrganizationExtension.ExpirationDate : null),
                })
                .ToListAsync();

            var counts = await CountBySellerAsync(invoices, orgs.Select(o => o.CompanyID).ToList());

            foreach (var org in orgs)
            {
                statistics.Rows.Add(
                    org.CompanyName ?? "",
                    org.ReceiptNo ?? "",
                    org.GoLiveDate.HasValue ? $"{org.GoLiveDate:yyyy/MM/dd}" : "",
                    counts.TryGetValue(org.CompanyID, out var c) ? c : 0,
                    org.ExpirationDate.HasValue ? $"{org.ExpirationDate:yyyy/MM/dd}" : "");
            }

            // 其後各表：每個年月一張日別統計（未作廢 / 已作廢之筆數與金額）。
            var daily = await invoices
                .Where(i => i.InvoiceDate.HasValue)
                .GroupBy(i => new { i.InvoiceDate!.Value.Year, i.InvoiceDate!.Value.Month, i.InvoiceDate!.Value.Day })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Day,
                    ActiveCount = g.Count(i => i.InvoiceCancellation == null),
                    ActiveAmount = g.Sum(i => i.InvoiceCancellation == null ? i.InvoiceAmountType!.TotalAmount : 0),
                    VoidCount = g.Count(i => i.InvoiceCancellation != null),
                    VoidAmount = g.Sum(i => i.InvoiceCancellation != null ? i.InvoiceAmountType!.TotalAmount : 0),
                })
                .ToListAsync();

            foreach (var month in daily.GroupBy(d => new { d.Year, d.Month })
                        .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month))
            {
                var table = new DataTable($"月報表({month.Key.Year}-{month.Key.Month})");
                table.Columns.Add(new DataColumn("日期", typeof(string)));
                table.Columns.Add(new DataColumn("未作廢總筆數", typeof(int)));
                table.Columns.Add(new DataColumn("未作廢總金額", typeof(decimal)));
                table.Columns.Add(new DataColumn("已作廢總筆數", typeof(int)));
                table.Columns.Add(new DataColumn("已作廢總金額", typeof(decimal)));
                ds.Tables.Add(table);

                foreach (var day in month.OrderBy(d => d.Day))
                {
                    table.Rows.Add(
                        day.Day.ToString(),
                        day.ActiveCount, day.ActiveAmount ?? 0m,
                        day.VoidCount, day.VoidAmount ?? 0m);
                }

                table.Rows.Add(
                    "總計",
                    month.Sum(d => d.ActiveCount), month.Sum(d => d.ActiveAmount ?? 0m),
                    month.Sum(d => d.VoidCount), month.Sum(d => d.VoidAmount ?? 0m));
            }

            return ds;
        }
    }
}
