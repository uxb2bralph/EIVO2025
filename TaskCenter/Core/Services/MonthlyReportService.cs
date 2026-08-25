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
using ModelCore.Helper;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// 發票月報表服務實作（遷移自 WebHome InvoiceQueryController.InquireMonthlyReport，
    /// 報表內容對應 Business.EF 之 InvoiceDataReportExtensions.CreateReport）。
    ///
    /// 與舊版的差異（刻意）：
    /// (1) 舊版是「建 ProcessRequest → 推入佇列 → ProcessorUnit 背景產檔 → 前端輪詢下載」，
    ///     此處改為同步產檔直接回傳 Excel（與已遷移的發票統計表月報表一致）。
    /// (2) 舊版逐「營業人 × 月份」各下 5 道 Count 查詢（N+1）；此處改為一次 GROUP BY 取回
    ///     整個期間的統計後於記憶體組表，統計結果相同。
    /// (3) 舊版此頁僅系統管理可用且未限縮資料範圍；此處另以 FilterOrganizationByRole
    ///     依登入者角色限縮營業人清單。
    /// </summary>
    public class MonthlyReportService : IMonthlyReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MonthlyReportService> _logger;

        public MonthlyReportService(IUnitOfWork unitOfWork, ILogger<MonthlyReportService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>營業人 × 年月之統計值（各筆數查詢的共同回傳形狀）。</summary>
        private sealed class MonthlyCount
        {
            public int SellerId { get; set; }
            public int Year { get; set; }
            public int Month { get; set; }
            public int Count { get; set; }
        }

        /// <summary>報表列（排序後才寫入 DataTable）。</summary>
        private sealed class ReportRow
        {
            public string CompanyName { get; set; } = "";
            public string ReceiptNo { get; set; } = "";
            public int InvoiceCount { get; set; }
            public int CancellationCount { get; set; }
            public int AllowanceCount { get; set; }
            public int AllowanceCancellationCount { get; set; }
            public string Month { get; set; } = "";
            public string GoLiveDate { get; set; } = "";
            public string ExpirationDate { get; set; } = "";
            public int TotalIssueCount { get; set; }
            public int ChargeAmount { get; set; }
        }

        public async Task<DataSet> BuildReportAsync(MonthlyReportQueryDto dto, int uid)
        {
            var (dateFrom, dateTo) = NormalizeRange(dto.DateFrom, dto.DateTo);

            var ds = new DataSet();
            var table = CreateTable();
            ds.Tables.Add(table);

            var models = new GenericDbContext<ApplicationDbContext>(_unitOfWork.Context);

            var profile = InvoiceQueryPipeline.GetProfile(models, uid);
            if (profile?.CurrentUserRole == null)
            {
                AppendSummaryRows(table, 0);
                return ds;
            }

            var sellers = BuildSellerScope(models, dto, profile);
            if (sellers == null)
            {
                AppendSummaryRows(table, 0);
                return ds;
            }

            var sellerIds = sellers.Select(o => o.CompanyID);

            // 營業人清單（舊版未排序；此處固定依統一編號取回以求輸出穩定）。
            var orgs = await sellers.AsNoTracking()
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

            if (orgs.Count == 0)
            {
                AppendSummaryRows(table, 0);
                return ds;
            }

            var months = EnumerateMonths(dateFrom, dateTo).ToList();

            var invoices = await CountInvoicesAsync(models, sellerIds, dateFrom, dateTo);
            var cancellations = await CountInvoiceCancellationsAsync(models, sellerIds, dateFrom, dateTo);
            var allowances = await CountAllowancesAsync(models, sellerIds, dateFrom, dateTo);
            var allowanceCancellations = await CountAllowanceCancellationsAsync(models, sellerIds, dateFrom, dateTo);
            var billings = await GetMonthlyBillingAsync(models, sellerIds, months);

            var rows = new List<ReportRow>();
            var chargeAmountByAgent = 0;

            foreach (var org in orgs)
            {
                foreach (var month in months)
                {
                    var key = (org.CompanyID, month.Year, month.Month);
                    var hasBilling = billings.TryGetValue(key, out var billing);

                    var goLiveDate = org.GoLiveDate.HasValue ? $"{org.GoLiveDate:yyyy/MM/dd}" : "";
                    var charge = hasBilling ? billing.ChargeAmount : 0;

                    // 上線月份不計費（沿用舊版規則）。
                    if (org.GoLiveDate?.ToString("yyyyMM") == $"{month:yyyyMM}")
                    {
                        charge = 0;
                    }
                    chargeAmountByAgent += charge;

                    rows.Add(new ReportRow
                    {
                        CompanyName = org.CompanyName ?? "",
                        ReceiptNo = org.ReceiptNo ?? "",
                        InvoiceCount = Lookup(invoices, key),
                        CancellationCount = Lookup(cancellations, key),
                        AllowanceCount = Lookup(allowances, key),
                        AllowanceCancellationCount = Lookup(allowanceCancellations, key),
                        Month = $"{month:yyyyMM}",
                        GoLiveDate = goLiveDate,
                        ExpirationDate = org.ExpirationDate.HasValue ? $"{org.ExpirationDate:yyyy/MM/dd}" : "",
                        TotalIssueCount = hasBilling ? billing.TotalIssueCount : 0,
                        ChargeAmount = charge,
                    });
                }
            }

            // 排序（沿用舊版）：有上線日期者在前，再依上線日期遞增；同上線日期維持原順序。
            foreach (var row in rows
                        .OrderByDescending(r => !string.IsNullOrWhiteSpace(r.GoLiveDate))
                        .ThenBy(r => r.GoLiveDate, StringComparer.Ordinal))
            {
                table.Rows.Add(
                    row.CompanyName, row.ReceiptNo,
                    row.InvoiceCount, row.CancellationCount,
                    row.AllowanceCount, row.AllowanceCancellationCount,
                    row.Month, row.GoLiveDate, row.ExpirationDate,
                    row.TotalIssueCount, row.ChargeAmount);
            }

            AppendSummaryRows(table, chargeAmountByAgent);
            return ds;
        }

        // ── 報表結構 ────────────────────────────────────────────────

        /// <summary>建立報表工作表結構（欄位順序沿用舊版 CreateReport）。</summary>
        private static DataTable CreateTable()
        {
            var table = new DataTable("發票資料明細");
            table.Columns.Add("營業人名稱");
            table.Columns.Add("統一編號");
            table.Columns.Add("發票", typeof(int));
            table.Columns.Add("作廢發票", typeof(int));
            table.Columns.Add("折讓", typeof(int));
            table.Columns.Add("作廢折讓", typeof(int));
            table.Columns.Add("月份");
            table.Columns.Add("上線日期", typeof(string));
            table.Columns.Add("註記停用日期", typeof(string));
            table.Columns.Add("總張數", typeof(int));
            table.Columns.Add("計費", typeof(int));
            return table;
        }

        /// <summary>補上舊版報表末端的空白列與「月服務費」合計列。</summary>
        private static void AppendSummaryRows(DataTable table, int chargeAmountByAgent)
        {
            table.Rows.Add("");
            table.Rows.Add("月服務費", chargeAmountByAgent);
        }

        // ── 查詢範圍 ────────────────────────────────────────────────

        /// <summary>
        /// 統計日期區間正規化（沿用舊版 CreateReport）：起日取當月 1 日，
        /// 迄日取「所屬月份 + 1 個月」的 1 日（即含迄日整月），回傳半開區間 [From, To)。
        /// </summary>
        private static (DateTime From, DateTime To) NormalizeRange(DateTime? from, DateTime? to)
        {
            var start = from ?? DateTime.Today;
            var end = to.HasValue ? to.Value.AddMonths(1) : start.AddMonths(1);
            return (new DateTime(start.Year, start.Month, 1), new DateTime(end.Year, end.Month, 1));
        }

        private static IEnumerable<DateTime> EnumerateMonths(DateTime from, DateTime to)
        {
            for (var idx = from; idx < to; idx = idx.AddMonths(1))
            {
                yield return idx;
            }
        }

        /// <summary>
        /// 營業人範圍：先依登入者角色限縮，再套用「所選開立人 或 所選代理業者旗下開立人」
        /// （沿用舊版 CreateReport 之 sellers 條件）。兩者皆未指定時回 null（控制器已先擋下）。
        /// </summary>
        private static IQueryable<Organization>? BuildSellerScope(
            GenericDbContext<ApplicationDbContext> models,
            MonthlyReportQueryDto dto,
            UserProfileWrapper profile)
        {
            var sellers = models.FilterOrganizationByRole(profile, models.GetTable<Organization>());

            int? sellerId = string.IsNullOrEmpty(dto.SellerKey) ? null : dto.SellerKey.DecryptKeyValue();
            int? agentId = string.IsNullOrEmpty(dto.AgentKey) ? null : dto.AgentKey.DecryptKeyValue();

            var issuers = models.GetTable<InvoiceIssuerAgent>().Where(a => a.AgentID == agentId);

            if (sellerId.HasValue && agentId.HasValue)
            {
                return sellers.Where(o => o.CompanyID == sellerId || issuers.Any(i => i.IssuerID == o.CompanyID));
            }
            if (sellerId.HasValue)
            {
                return sellers.Where(o => o.CompanyID == sellerId);
            }
            if (agentId.HasValue)
            {
                return sellers.Where(o => issuers.Any(i => i.IssuerID == o.CompanyID));
            }
            return null;
        }

        // ── 各項統計（皆以單次 GROUP BY 取代舊版逐月 Count）────────

        private static int Lookup(Dictionary<(int, int, int), int> source, (int, int, int) key)
            => source.TryGetValue(key, out var value) ? value : 0;

        private static Dictionary<(int, int, int), int> ToDictionary(IEnumerable<MonthlyCount> items)
            => items.ToDictionary(c => (c.SellerId, c.Year, c.Month), c => c.Count);

        /// <summary>發票筆數（對應舊版 GetInvoice）。</summary>
        private static async Task<Dictionary<(int, int, int), int>> CountInvoicesAsync(
            GenericDbContext<ApplicationDbContext> models, IQueryable<int> sellerIds, DateTime dateFrom, DateTime dateTo)
        {
            var items = await models.GetTable<InvoiceItem>()
                .Where(i => i.SellerID.HasValue && sellerIds.Contains(i.SellerID.Value))
                .Where(i => i.InvoiceDate >= dateFrom && i.InvoiceDate < dateTo)
                .GroupBy(i => new { SellerId = i.SellerID!.Value, i.InvoiceDate!.Value.Year, i.InvoiceDate!.Value.Month })
                .Select(g => new MonthlyCount
                {
                    SellerId = g.Key.SellerId,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count(),
                })
                .ToListAsync();

            return ToDictionary(items);
        }

        /// <summary>作廢發票筆數（對應舊版 GetInvoiceCancellation；以發票日期歸月）。</summary>
        private static async Task<Dictionary<(int, int, int), int>> CountInvoiceCancellationsAsync(
            GenericDbContext<ApplicationDbContext> models, IQueryable<int> sellerIds, DateTime dateFrom, DateTime dateTo)
        {
            var items = await models.GetTable<InvoiceCancellation>()
                .Join(models.GetTable<InvoiceItem>()
                        .Where(i => i.SellerID.HasValue && sellerIds.Contains(i.SellerID.Value))
                        .Where(i => i.InvoiceDate >= dateFrom && i.InvoiceDate < dateTo),
                    c => c.InvoiceID, i => i.InvoiceID, (c, i) => i)
                .GroupBy(i => new { SellerId = i.SellerID!.Value, i.InvoiceDate!.Value.Year, i.InvoiceDate!.Value.Month })
                .Select(g => new MonthlyCount
                {
                    SellerId = g.Key.SellerId,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count(),
                })
                .ToListAsync();

            return ToDictionary(items);
        }

        /// <summary>折讓筆數（對應舊版 GetAllowance；以折讓日期歸月）。</summary>
        private static async Task<Dictionary<(int, int, int), int>> CountAllowancesAsync(
            GenericDbContext<ApplicationDbContext> models, IQueryable<int> sellerIds, DateTime dateFrom, DateTime dateTo)
        {
            var items = await models.GetTable<InvoiceAllowance>()
                .Where(a => a.AllowanceDate >= dateFrom && a.AllowanceDate < dateTo)
                .Join(models.GetTable<InvoiceAllowanceSeller>()
                        .Where(s => s.SellerID.HasValue && sellerIds.Contains(s.SellerID.Value)),
                    a => a.AllowanceID, s => s.AllowanceID,
                    (a, s) => new { SellerId = s.SellerID!.Value, Date = a.AllowanceDate!.Value })
                .GroupBy(x => new { x.SellerId, x.Date.Year, x.Date.Month })
                .Select(g => new MonthlyCount
                {
                    SellerId = g.Key.SellerId,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count(),
                })
                .ToListAsync();

            return ToDictionary(items);
        }

        /// <summary>作廢折讓筆數（對應舊版 GetAllowanceCancellation；以作廢日期歸月）。</summary>
        private static async Task<Dictionary<(int, int, int), int>> CountAllowanceCancellationsAsync(
            GenericDbContext<ApplicationDbContext> models, IQueryable<int> sellerIds, DateTime dateFrom, DateTime dateTo)
        {
            var items = await models.GetTable<InvoiceAllowanceCancellation>()
                .Where(c => c.CancelDate >= dateFrom && c.CancelDate < dateTo)
                .Join(models.GetTable<InvoiceAllowanceSeller>()
                        .Where(s => s.SellerID.HasValue && sellerIds.Contains(s.SellerID.Value)),
                    c => c.AllowanceID, s => s.AllowanceID,
                    (c, s) => new { SellerId = s.SellerID!.Value, Date = c.CancelDate!.Value })
                .GroupBy(x => new { x.SellerId, x.Date.Year, x.Date.Month })
                .Select(g => new MonthlyCount
                {
                    SellerId = g.Key.SellerId,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count(),
                })
                .ToListAsync();

            return ToDictionary(items);
        }

        /// <summary>
        /// 各月計費資料（對應舊版 GetSellerMonthlyBilling：以 Settlement 之年月對應 MonthlyBilling）。
        /// 同一營業人同年月若有多筆，取其一（與舊版 FirstOrDefault 相同語意）。
        /// </summary>
        private static async Task<Dictionary<(int, int, int), (int TotalIssueCount, int ChargeAmount)>> GetMonthlyBillingAsync(
            GenericDbContext<ApplicationDbContext> models, IQueryable<int> sellerIds, IReadOnlyCollection<DateTime> months)
        {
            var monthKeys = months.Select(m => m.Year * 100 + m.Month).ToList();

            var items = await models.GetTable<MonthlyBilling>()
                .Where(b => sellerIds.Contains(b.CompanyID))
                .Join(models.GetTable<Settlement>(),
                    b => b.SettlementID, s => s.SettlementID,
                    (b, s) => new
                    {
                        b.CompanyID,
                        s.Year,
                        s.Month,
                        b.TotalIssueCount,
                        b.IssueChargeAmount,
                    })
                .Where(x => monthKeys.Contains(x.Year * 100 + x.Month))
                .ToListAsync();

            return items
                .GroupBy(x => (x.CompanyID, x.Year, x.Month))
                .ToDictionary(g => g.Key, g => (g.First().TotalIssueCount, g.First().IssueChargeAmount));
        }
    }
}
