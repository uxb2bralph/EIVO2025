using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CommonLib.Core.DataWork;
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
    /// 中獎統計表服務實作（遷移自 WebHome WinningInvoiceController.InquireReport /
    /// ReportGridPage / Module/CreateXlsx.cshtml）。
    ///
    /// 與舊版的差異（刻意）：
    /// (1) 舊版匯出是「建 ProcessRequest → 背景 Task 產檔 → 前端輪詢下載」；此處改為同步產檔直接
    ///     回傳 Excel（與已遷移的發票統計表 / 發票月報表一致）。
    /// (2) 舊版 ReportGridPage 先 Skip/Take 再 Join Organization，若開立人已不存在會使該頁少列；
    ///     此處先 Join 再分頁，總筆數與分頁一致。
    /// (3) 發票日期起迄改為必填（同已遷移之發票統計表 / 發票明細查詢），避免無條件全表 GROUP BY。
    /// </summary>
    public class WinningInvoiceReportService : IWinningInvoiceReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WinningInvoiceReportService> _logger;

        public WinningInvoiceReportService(IUnitOfWork unitOfWork, ILogger<WinningInvoiceReportService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>彙總後的統計列（查詢投影用；SellerId 保留供排序與加密鍵使用）。</summary>
        private sealed class WinningRow
        {
            public int SellerId { get; set; }
            public string? SellerReceiptNo { get; set; }
            public string? SellerName { get; set; }
            public string? Addr { get; set; }
            public int WinningCount { get; set; }
            public int DonationCount { get; set; }
        }

        // ── 查詢範圍建立 ────────────────────────────────────────────

        /// <summary>
        /// 建立「中獎發票依開立人彙總」之查詢（未排序、未分頁）。
        /// 回傳 null 表示登入者無有效角色（視為無資料）。
        /// </summary>
        private IQueryable<WinningRow>? BuildScope(WinningInvoiceReportQueryDto dto, int uid)
        {
            var models = new GenericDbContext<ApplicationDbContext>(_unitOfWork.Context);

            var profile = InvoiceQueryPipeline.GetProfile(models, uid);
            if (profile?.CurrentUserRole == null)
            {
                return null;
            }

            // 條件過濾（含角色資料範圍）後，僅保留中獎發票（對應舊版 InvoiceWinningNumber != null）。
            var invoices = InvoiceQueryPipeline.BuildQuery(models, dto, profile)
                .AsNoTracking()
                .Where(i => i.InvoiceWinningNumber != null && i.SellerID.HasValue);

            return invoices
                .GroupBy(i => i.SellerID!.Value)
                .Select(g => new
                {
                    SellerId = g.Key,
                    WinningCount = g.Count(),
                    DonationCount = g.Count(i => i.InvoiceDonation != null),
                })
                .Join(models.GetTable<Organization>(),
                    g => g.SellerId, o => o.CompanyID,
                    (g, o) => new WinningRow
                    {
                        SellerId = g.SellerId,
                        SellerReceiptNo = o.ReceiptNo,
                        SellerName = o.CompanyName,
                        Addr = o.Addr,
                        WinningCount = g.WinningCount,
                        DonationCount = g.DonationCount,
                    });
        }

        /// <summary>排序；未指定時沿用舊版預設（依 SellerID 遞增）。</summary>
        private static IQueryable<WinningRow> ApplySort(IQueryable<WinningRow> query, string? sortName, int? sortType)
        {
            // sortType：1 遞增、2 遞減；其餘視為未指定。
            bool desc = sortType == 2;
            if (sortType == 1 || desc)
            {
                switch (sortName)
                {
                    case "SellerReceiptNo":
                        return desc ? query.OrderByDescending(r => r.SellerReceiptNo) : query.OrderBy(r => r.SellerReceiptNo);
                    case "SellerName":
                        return desc ? query.OrderByDescending(r => r.SellerName) : query.OrderBy(r => r.SellerName);
                    case "WinningCount":
                        return desc ? query.OrderByDescending(r => r.WinningCount) : query.OrderBy(r => r.WinningCount);
                    case "DonationCount":
                        return desc ? query.OrderByDescending(r => r.DonationCount) : query.OrderBy(r => r.DonationCount);
                }
            }
            return query.OrderBy(r => r.SellerId);
        }

        private static WinningInvoiceReportRowDto ToDto(WinningRow row) => new()
        {
            SellerKey = row.SellerId.EncryptKey(),
            SellerReceiptNo = row.SellerReceiptNo,
            SellerName = row.SellerName,
            Addr = row.Addr,
            WinningCount = row.WinningCount,
            DonationCount = row.DonationCount,
        };

        // ── 統計清單 ────────────────────────────────────────────────

        public async Task<PagedResultDto<WinningInvoiceReportRowDto>> GetPagedAsync(WinningInvoiceReportQueryDto dto, int uid)
        {
            var query = BuildScope(dto, uid);
            if (query == null)
            {
                return Empty(dto);
            }

            var totalCount = await query.CountAsync();

            var rows = await ApplySort(query, dto.SortName, dto.SortType)
                .Skip(dto.Skip)
                .Take(dto.PageSize)
                .ToListAsync();

            return new PagedResultDto<WinningInvoiceReportRowDto>
            {
                Items = rows.Select(ToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = dto.Page,
                PageSize = dto.PageSize,
            };
        }

        private static PagedResultDto<WinningInvoiceReportRowDto> Empty(WinningInvoiceReportQueryDto dto) => new()
        {
            Items = new List<WinningInvoiceReportRowDto>(),
            TotalCount = 0,
            PageNumber = dto.Page,
            PageSize = dto.PageSize,
        };

        // ── 中獎統計表 Excel ────────────────────────────────────────

        public async Task<DataSet> BuildReportAsync(WinningInvoiceReportQueryDto dto, int uid)
        {
            var ds = new DataSet();

            // 欄位名稱沿用舊版 CreateXlsx.cshtml 之工作表與欄位標題。
            var table = new DataTable("中獎統計");
            table.Columns.Add(new DataColumn("賣方統一編號", typeof(string)));
            table.Columns.Add(new DataColumn("賣方名稱", typeof(string)));
            table.Columns.Add(new DataColumn("賣方地址", typeof(string)));
            table.Columns.Add(new DataColumn("中獎張數", typeof(int)));
            table.Columns.Add(new DataColumn("捐贈張數", typeof(int)));
            ds.Tables.Add(table);

            var query = BuildScope(dto, uid);
            if (query == null)
            {
                return ds;
            }

            var rows = await ApplySort(query, dto.SortName, dto.SortType).ToListAsync();

            foreach (var row in rows)
            {
                table.Rows.Add(
                    row.SellerReceiptNo ?? "",
                    row.SellerName ?? "",
                    row.Addr ?? "",
                    row.WinningCount,
                    row.DonationCount);
            }

            return ds;
        }
    }
}
