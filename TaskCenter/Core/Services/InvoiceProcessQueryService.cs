using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CommonLib.Core.DataWork;
using CommonLib.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelCore.DataEntity;
using ModelCore.DataEntityWrapper;
using ModelCore.DTOs;
using ModelCore.Helper;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using ModelCore.Security.MembershipManagement;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// 發票資料查詢服務實作（遷移自 WebHome InvoiceProcessController.Index/Inquire）。
    /// 查詢重用 ModelSource&lt;InvoiceItem&gt;.BuildInvoiceQuery（ModelExtension.EF），
    /// 角色資料範圍以登入者 UID 還原 UserProfileWrapper 後由管線內 FilterInvoiceByRole 套用。
    /// </summary>
    public class InvoiceProcessQueryService : IInvoiceProcessQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InvoiceProcessQueryService> _logger;

        public InvoiceProcessQueryService(IUnitOfWork unitOfWork, ILogger<InvoiceProcessQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ── 查詢建立 ────────────────────────────────────────────────

        /// <summary>
        /// 依角色範圍 + 查詢條件建立過濾後（未排序）之 IQueryable。
        /// 回傳 null 表示登入者無有效角色（視為無資料）。
        /// </summary>
        private IQueryable<InvoiceItem>? BuildFilteredQuery(
            InvoiceProcessQueryDto dto, int uid, out GenericDbContext<ApplicationDbContext> models)
        {
            models = new GenericDbContext<ApplicationDbContext>(_unitOfWork.Context);

            var profile = InvoiceQueryPipeline.GetProfile(models, uid);
            if (profile?.CurrentUserRole == null)
            {
                return null;
            }

            return InvoiceQueryPipeline.BuildQuery(models, dto, profile);
        }

        /// <summary>排序（移植自 ItemListSorting.cshtml 之 SortName → 運算式；預設依日期新到舊）。</summary>
        private static IQueryable<InvoiceItem> ApplySort(IQueryable<InvoiceItem> query, string? sortName, int? sortType)
        {
            // sortType：1 遞增、2 遞減；其餘視為未指定。
            bool asc = sortType == 1;
            bool desc = sortType == 2;
            if (!string.IsNullOrEmpty(sortName) && (asc || desc))
            {
                switch (sortName)
                {
                    case "InvoiceDate": return desc ? query.OrderByDescending(i => i.InvoiceDate) : query.OrderBy(i => i.InvoiceDate);
                    case "CompanyName": return desc ? query.OrderByDescending(i => i.InvoiceSeller!.CustomerName) : query.OrderBy(i => i.InvoiceSeller!.CustomerName);
                    case "ReceiptNo": return desc ? query.OrderByDescending(i => i.InvoiceSeller!.ReceiptNo) : query.OrderBy(i => i.InvoiceSeller!.ReceiptNo);
                    case "BuyerNo": return desc ? query.OrderByDescending(i => i.InvoiceBuyer!.ReceiptNo) : query.OrderBy(i => i.InvoiceBuyer!.ReceiptNo);
                    case "CustomerID": return desc ? query.OrderByDescending(i => i.InvoiceBuyer!.CustomerID) : query.OrderBy(i => i.InvoiceBuyer!.CustomerID);
                    case "OrderNo": return desc ? query.OrderByDescending(i => i.InvoicePurchaseOrder!.OrderNo) : query.OrderBy(i => i.InvoicePurchaseOrder!.OrderNo);
                    case "SalesAmount": return desc ? query.OrderByDescending(i => i.InvoiceAmountType!.SalesAmount) : query.OrderBy(i => i.InvoiceAmountType!.SalesAmount);
                    case "TaxAmount": return desc ? query.OrderByDescending(i => i.InvoiceAmountType!.TaxAmount) : query.OrderBy(i => i.InvoiceAmountType!.TaxAmount);
                    case "TotalAmount": return desc ? query.OrderByDescending(i => i.InvoiceAmountType!.TotalAmount) : query.OrderBy(i => i.InvoiceAmountType!.TotalAmount);
                    case "InvoiceNo": return desc ? query.OrderByDescending(i => i.TrackCode + i.No) : query.OrderBy(i => i.TrackCode + i.No);
                    case "IsWinning": return desc ? query.OrderByDescending(i => i.InvoiceWinningNumber!.PrizeType) : query.OrderBy(i => i.InvoiceWinningNumber!.PrizeType);
                }
            }
            // 預設：日期新到舊（原版依賴 DB 隱含順序，此處明確化以穩定分頁）。
            return query.OrderByDescending(i => i.InvoiceDate).ThenByDescending(i => i.InvoiceID);
        }

        private static readonly int MigCStep = (int)Naming.InvoiceStepDefinition.MIG_C;
        private static readonly int MigEStep = (int)Naming.InvoiceStepDefinition.MIG_E;

        // ── 查詢清單 ────────────────────────────────────────────────

        public async Task<PagedResultDto<InvoiceItemDatatableDto>> GetPagedAsync(InvoiceProcessQueryDto dto, int uid)
        {
            var query = BuildFilteredQuery(dto, uid, out var models);
            if (query == null)
            {
                return Empty(dto);
            }

            var totalCount = await query.CountAsync();

            var logs = models.GetTable<DataProcessLog>();
            var notices = models.GetTable<IssuingNotice>();

            var rows = await ApplySort(query, dto.SortName, dto.SortType)
                .Skip(dto.Skip)
                .Take(dto.PageSize)
                .Select(i => new
                {
                    i.InvoiceID,
                    i.TrackCode,
                    i.No,
                    i.InvoiceDate,
                    i.Remark,
                    i.PrintMark,
                    ProcessType = i.CDS_Document.ProcessType,
                    SellerName = i.InvoiceSeller!.CustomerName,
                    SellerReceiptNo = i.InvoiceSeller!.ReceiptNo,
                    BuyerName = i.InvoiceBuyer!.CustomerName,
                    BuyerReceiptNo = i.InvoiceBuyer!.ReceiptNo,
                    BuyerCustomerID = i.InvoiceBuyer!.CustomerID,
                    BuyerEMail = i.InvoiceBuyer!.EMail,
                    BuyerAddress = i.InvoiceBuyer!.Address,
                    BuyerContact = i.InvoiceBuyer!.ContactName,
                    OrderNo = i.InvoicePurchaseOrder!.OrderNo,
                    CancelDate = (DateTime?)(i.InvoiceCancellation != null ? i.InvoiceCancellation.CancelDate : null),
                    Currency = i.InvoiceAmountType!.Currency!.AbbrevName,
                    i.InvoiceAmountType!.SalesAmount,
                    i.InvoiceAmountType!.TaxAmount,
                    i.InvoiceAmountType!.TotalAmount,
                    TaxType = i.InvoiceAmountType!.TaxType,
                    WinningPrize = (string?)(i.InvoiceWinningNumber != null ? i.InvoiceWinningNumber.PrizeType : null),
                    CarrierNo = i.InvoiceCarrier!.CarrierNo,
                    AgencyCode = i.InvoiceDonation!.AgencyCode,
                    IssuingNoticeDate = notices.Where(n => n.DocID == i.InvoiceID).Select(n => n.IssueDate).FirstOrDefault(),
                    HasMigC = logs.Any(l => l.DocID == i.InvoiceID && l.StepID == MigCStep),
                    HasMigE = logs.Any(l => l.DocID == i.InvoiceID && l.StepID == MigEStep),
                })
                .ToListAsync();

            var items = rows.Select(r => new InvoiceItemDatatableDto
            {
                InvoiceId = r.InvoiceID,
                KeyId = r.InvoiceID.EncryptKey(),
                SellerName = r.SellerName,
                SellerReceiptNo = r.SellerReceiptNo,
                InvoiceNo = $"{r.TrackCode}{r.No}",
                TransTypeLabel = TransTypeLabel(r.ProcessType),
                InvoiceDate = r.InvoiceDate,
                BuyerName = r.BuyerName,
                BuyerReceiptNo = r.BuyerReceiptNo,
                OrderNo = r.OrderNo,
                StatusLabel = r.CancelDate.HasValue ? $"已作廢({r.CancelDate:yyyy/MM/dd})" : null,
                MigStatus = MigStatus(r.ProcessType, r.HasMigC, r.HasMigE),
                Currency = r.Currency,
                SalesAmount = r.SalesAmount,
                TaxTypeLabel = TaxTypeLabel(r.TaxType),
                TaxAmount = r.TaxAmount,
                TotalAmount = r.TotalAmount,
                Remark = r.Remark,
                PrintMark = r.PrintMark,
                WinningLabel = r.WinningPrize ?? "N/A",
                CarrierNo = r.CarrierNo,
                AgencyCode = r.AgencyCode,
                CustomerId = r.BuyerCustomerID,
                Email = r.BuyerEMail,
                IssuingNoticeDate = r.IssuingNoticeDate,
                BuyerAddress = r.BuyerAddress,
                BuyerContact = r.BuyerContact,
                IsCancelled = r.CancelDate.HasValue,
                IsWinning = r.WinningPrize != null,
            }).ToList();

            return new PagedResultDto<InvoiceItemDatatableDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = dto.Page,
                PageSize = dto.PageSize,
            };
        }

        private static PagedResultDto<InvoiceItemDatatableDto> Empty(InvoiceProcessQueryDto dto) => new()
        {
            Items = new List<InvoiceItemDatatableDto>(),
            TotalCount = 0,
            PageNumber = dto.Page,
            PageSize = dto.PageSize,
        };

        // ── 幣別統計 ────────────────────────────────────────────────

        public async Task<List<CurrencySummaryDto>> GetSummaryAsync(InvoiceProcessQueryDto dto, int uid)
        {
            var query = BuildFilteredQuery(dto, uid, out _);
            if (query == null)
            {
                return new List<CurrencySummaryDto>();
            }

            var rows = await query
                .GroupBy(i => i.InvoiceAmountType!.Currency!.AbbrevName)
                .Select(g => new CurrencySummaryDto
                {
                    Currency = g.Key,
                    Count = g.Count(),
                    SalesAmount = g.Sum(i => i.InvoiceAmountType!.SalesAmount) ?? 0,
                    TaxAmount = g.Sum(i => i.InvoiceAmountType!.TaxAmount) ?? 0,
                    TotalAmount = g.Sum(i => i.InvoiceAmountType!.TotalAmount) ?? 0,
                })
                .ToListAsync();

            return rows;
        }

        // ── 發票明細 ────────────────────────────────────────────────

        public async Task<InvoiceDetailDto?> GetDetailAsync(int invoiceId, int uid)
        {
            // 以空條件建立角色範圍查詢，再限縮至指定發票（越權查詢自動落空）。
            var query = BuildFilteredQuery(new InvoiceProcessQueryDto(), uid, out _);
            if (query == null)
            {
                return null;
            }

            var item = await query.Where(i => i.InvoiceID == invoiceId).FirstOrDefaultAsync();
            if (item == null)
            {
                return null;
            }

            var detail = new InvoiceDetailDto
            {
                InvoiceNo = $"{item.TrackCode}{item.No}",
                InvoiceDate = item.InvoiceDate,
                RandomNo = item.RandomNo,
                SellerName = item.InvoiceSeller?.CustomerName,
                SellerReceiptNo = item.InvoiceSeller?.ReceiptNo,
                BuyerName = item.InvoiceBuyer?.CustomerName,
                BuyerReceiptNo = item.InvoiceBuyer?.ReceiptNo,
                BuyerAddress = item.InvoiceBuyer?.Address,
                BuyerEmail = item.InvoiceBuyer?.EMail,
                Currency = item.InvoiceAmountType?.Currency?.AbbrevName,
                TaxTypeLabel = TaxTypeLabel(item.InvoiceAmountType?.TaxType),
                SalesAmount = item.InvoiceAmountType?.SalesAmount,
                TaxAmount = item.InvoiceAmountType?.TaxAmount,
                TotalAmount = item.InvoiceAmountType?.TotalAmount,
                CarrierType = item.InvoiceCarrier?.CarrierType,
                CarrierNo = item.InvoiceCarrier?.CarrierNo,
                AgencyCode = item.InvoiceDonation?.AgencyCode,
                StatusLabel = item.InvoiceCancellation != null
                    ? $"已作廢({item.InvoiceCancellation.CancelDate:yyyy/MM/dd})"
                    : "有效",
                Remark = item.Remark,
            };

            // 品項列（透過導覽屬性，lazy loading 於請求範圍內載入）。
            var seq = 0;
            foreach (var product in item.Product)
            {
                foreach (var pItem in product.InvoiceProductItem.OrderBy(x => x.No))
                {
                    detail.Lines.Add(new InvoiceDetailLineDto
                    {
                        Seq = ++seq,
                        Description = string.IsNullOrEmpty(pItem.Spec) ? product.Brief : pItem.Spec,
                        Quantity = pItem.Piece,
                        Unit = pItem.PieceUnit,
                        UnitPrice = pItem.UnitCost,
                        Amount = pItem.CostAmount,
                        Remark = pItem.Remark,
                    });
                }
            }

            return detail;
        }

        // ── 匯出：發票資料明細 Excel ────────────────────────────────
        // 欄位沿用舊版 CreateXlsx2021 之 admin / 非 admin 投影（穩定且完整規格）。

        public async Task<DataTable> BuildXlsxTableAsync(InvoiceProcessQueryDto dto, int uid, bool isAdmin)
        {
            var table = new DataTable("發票資料明細");
            string[] adminOnly = { "連絡人名稱", "連絡人地址", "買受人EMail" };
            string[] columns =
            {
                "發票號碼","發票日期","客戶ID","序號","發票開立人","開立人統編","營業人店別",
                "未稅金額","稅額","含稅金額","幣別","買受人名稱","買受人統編",
                "連絡人名稱","連絡人地址","買受人EMail","愛心碼","是否中獎","備註","發票狀態","載具類別","載具號碼",
            };
            foreach (var c in columns)
            {
                if (!isAdmin && adminOnly.Contains(c)) continue;
                table.Columns.Add(c);
            }

            var query = BuildFilteredQuery(dto, uid, out _);
            if (query == null) return table;

            var rows = await ApplySort(query, dto.SortName, dto.SortType)
                .Select(i => new
                {
                    i.TrackCode,
                    i.No,
                    i.InvoiceDate,
                    BuyerCustomerID = i.InvoiceBuyer!.CustomerID,
                    OrderNo = i.InvoicePurchaseOrder!.OrderNo,
                    SellerName = i.InvoiceSeller!.CustomerName,
                    SellerReceiptNo = i.InvoiceSeller!.ReceiptNo,
                    CustomerNo = i.Seller!.OrganizationExtension!.CustomerNo,
                    i.InvoiceAmountType!.SalesAmount,
                    i.InvoiceAmountType!.TaxAmount,
                    i.InvoiceAmountType!.TotalAmount,
                    Currency = i.InvoiceAmountType!.Currency!.AbbrevName,
                    BuyerName = i.InvoiceBuyer!.CustomerName,
                    BuyerReceiptNo = i.InvoiceBuyer!.ReceiptNo,
                    BuyerContact = i.InvoiceBuyer!.ContactName,
                    BuyerAddress = i.InvoiceBuyer!.Address,
                    BuyerEMail = i.InvoiceBuyer!.EMail,
                    AgencyCode = i.InvoiceDonation!.AgencyCode,
                    WinningPrize = (string?)(i.InvoiceWinningNumber != null ? i.InvoiceWinningNumber.PrizeType : null),
                    i.Remark,
                    IsCancelled = i.InvoiceCancellation != null,
                    CarrierType = i.InvoiceCarrier!.CarrierType,
                    CarrierNo = i.InvoiceCarrier!.CarrierNo,
                })
                .OrderBy(x => x.TrackCode).ThenBy(x => x.No)
                .ToListAsync();

            foreach (var r in rows)
            {
                var row = table.NewRow();
                row["發票號碼"] = $"{r.TrackCode}{r.No}";
                row["發票日期"] = r.InvoiceDate?.ToString("yyyy/MM/dd") ?? "";
                row["客戶ID"] = r.BuyerCustomerID ?? "";
                row["序號"] = r.OrderNo ?? "";
                row["發票開立人"] = r.SellerName ?? "";
                row["開立人統編"] = r.SellerReceiptNo ?? "";
                row["營業人店別"] = r.CustomerNo ?? "";
                row["未稅金額"] = r.SalesAmount;
                row["稅額"] = r.TaxAmount;
                row["含稅金額"] = r.TotalAmount;
                row["幣別"] = r.Currency ?? "";
                row["買受人名稱"] = r.BuyerName ?? "";
                row["買受人統編"] = "0000000000".Equals(r.BuyerReceiptNo) ? "" : (r.BuyerReceiptNo ?? "");
                if (isAdmin)
                {
                    row["連絡人名稱"] = r.BuyerContact ?? "";
                    row["連絡人地址"] = r.BuyerAddress ?? "";
                    row["買受人EMail"] = r.BuyerEMail ?? "";
                }
                row["愛心碼"] = r.AgencyCode ?? "";
                row["是否中獎"] = r.WinningPrize ?? "";
                row["備註"] = r.Remark ?? "";
                row["發票狀態"] = r.IsCancelled ? "已作廢" : "";
                row["載具類別"] = r.CarrierType ?? "";
                row["載具號碼"] = r.CarrierNo ?? "";
                table.Rows.Add(row);
            }

            return table;
        }

        // ── 匯出：買受人資料 Excel（6 欄）────────────────────────────

        public async Task<DataTable> BuildBuyerTableAsync(InvoiceProcessQueryDto dto, int uid)
        {
            var table = new DataTable("買受人");
            foreach (var c in new[] { "發票號碼", "營業人名稱", "收件人姓名", "地址", "電話", "EMail" })
            {
                table.Columns.Add(c);
            }

            var query = BuildFilteredQuery(dto, uid, out _);
            if (query == null) return table;

            var rows = await query
                .OrderBy(i => i.InvoiceID)
                .Select(i => new
                {
                    InvoiceNo = i.TrackCode + i.No,
                    Name = i.InvoiceBuyer!.CustomerName,
                    Contact = i.InvoiceBuyer!.ContactName,
                    Address = i.InvoiceBuyer!.Address,
                    Phone = i.InvoiceBuyer!.Phone,
                    Email = i.InvoiceBuyer!.EMail,
                })
                .ToListAsync();

            foreach (var r in rows)
            {
                table.Rows.Add(r.InvoiceNo, r.Name ?? "", r.Contact ?? "", r.Address ?? "", r.Phone ?? "", r.Email ?? "");
            }

            return table;
        }

        // ── 匯出：ERP 固定寬度文字（POSINV.dat）─────────────────────

        public async Task<string> BuildErpTextAsync(InvoiceProcessQueryDto dto, int uid)
        {
            var query = BuildFilteredQuery(dto, uid, out _);
            if (query == null) return string.Empty;

            var rows = await query
                .OrderBy(i => i.TrackCode).ThenBy(i => i.No)
                .Select(i => new
                {
                    CustomerNo = i.Seller!.OrganizationExtension!.CustomerNo,
                    i.InvoiceDate,
                    i.TrackCode,
                    i.No,
                    BuyerReceiptNo = i.InvoiceBuyer!.ReceiptNo,
                    i.InvoiceAmountType!.SalesAmount,
                    i.InvoiceAmountType!.TaxAmount,
                    i.InvoiceAmountType!.TotalAmount,
                    IsCancelled = i.InvoiceCancellation != null,
                })
                .ToListAsync();

            var sb = new StringBuilder();
            foreach (var r in rows)
            {
                var rocDate = r.InvoiceDate.HasValue
                    ? $"{r.InvoiceDate.Value.Year - 1911:0000}{r.InvoiceDate.Value:MMdd}"
                    : new string(' ', 8);
                var buyer = string.IsNullOrEmpty(r.BuyerReceiptNo) || r.BuyerReceiptNo == "0000000000"
                    ? new string(' ', 8)
                    : r.BuyerReceiptNo.PadRight(8);
                sb.AppendLine(
                    $"{(r.CustomerNo ?? "").PadRight(8)}" +
                    $"{rocDate}" +
                    $"{r.TrackCode}{r.No}" +
                    $"{buyer}" +
                    $"{((int)(r.SalesAmount ?? 0)),8}" +
                    $"{((int)(r.TaxAmount ?? 0)),8}" +
                    $"{((int)(r.TotalAmount ?? 0)),8}" +
                    $"{(r.IsCancelled ? "Y" : " ")}");
            }

            return sb.ToString();
        }

        // ── 下載：MIG XML 壓縮檔 ────────────────────────────────────
        // 遷移自舊版 DownloadF0401 / DownloadF0701 / DownloadF0501 + zipItems，
        // 差異：改在記憶體組壓縮檔（不落地 ~/temp），且作業對象先以角色範圍限縮。

        /// <summary>支援下載的 MIG 格式（對應舊版 DownloadMIG.cshtml 的三個按鈕）。</summary>
        public static readonly string[] MigDocTypes = { "F0401", "F0701", "F0501" };

        public async Task<MigZipResultDto> BuildMigZipAsync(string docType, IEnumerable<string> keyIds, int uid)
        {
            var result = new MigZipResultDto { DocType = docType };

            // 解密請求鍵；保留原鍵以便回報查無 / 越權者。
            var requested = (keyIds ?? Enumerable.Empty<string>())
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Distinct()
                .Select(k => new { KeyId = k, InvoiceId = TryDecryptKey(k) })
                .ToList();
            result.RequestedCount = requested.Count;
            if (requested.Count == 0)
            {
                return result;
            }

            // 以空條件建立角色範圍查詢，再限縮至選取的發票（越權者自動落空）。
            var query = BuildFilteredQuery(new InvoiceProcessQueryDto(), uid, out _);
            if (query == null)
            {
                result.SkippedNos.AddRange(requested.Select(r => r.KeyId));
                return result;
            }

            var ids = requested.Where(r => r.InvoiceId.HasValue).Select(r => r.InvoiceId!.Value).ToList();
            // 明確載入 MIG 轉檔所需導覽屬性（對應原版 DataLoadOptions），避免逐筆 lazy loading；
            // 其餘較少用到的導覽屬性仍由 lazy loading proxies 補齊。
            var items = ids.Count > 0
                ? await query.Where(i => ids.Contains(i.InvoiceID))
                    .Include(i => i.InvoiceBuyer)
                    .Include(i => i.InvoiceSeller)
                    .Include(i => i.InvoiceAmountType!).ThenInclude(a => a.Currency)
                    .Include(i => i.Seller)
                    .Include(i => i.InvoiceCarrier)
                    .Include(i => i.InvoiceDonation)
                    .Include(i => i.InvoiceCancellation)
                    .Include(i => i.Product).ThenInclude(p => p.InvoiceProductItem)
                    .AsSplitQuery()
                    .OrderBy(i => i.TrackCode).ThenBy(i => i.No)
                    .ToListAsync()
                : new List<InvoiceItem>();

            var found = new HashSet<int>(items.Select(i => i.InvoiceID));
            foreach (var r in requested)
            {
                if (!r.InvoiceId.HasValue || !found.Contains(r.InvoiceId.Value))
                {
                    result.SkippedNos.Add(r.KeyId);
                }
            }

            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var item in items)
                {
                    var invoiceNo = $"{item.TrackCode}{item.No}";
                    XmlDocument? doc = null;
                    try
                    {
                        doc = CreateMigDocument(docType, item);
                    }
                    catch (Exception ex)
                    {
                        // 單筆轉檔失敗不影響其他發票（原版 zipItems 亦以 null 略過）。
                        _logger.LogError(ex, "Error creating {DocType} for invoice {InvoiceNo}", docType, invoiceNo);
                    }

                    if (doc == null)
                    {
                        result.SkippedNos.Add(invoiceNo);
                        continue;
                    }

                    var entry = zip.CreateEntry($"{docType}_{invoiceNo}.xml");
                    using var outStream = entry.Open();
                    doc.Save(outStream);
                    result.IncludedCount++;
                }
            }

            if (result.IncludedCount > 0)
            {
                result.Content = ms.ToArray();
            }
            return result;
        }

        /// <summary>
        /// 產生單張發票的 MIG XML（重用 ModelExtension.EF 之 CreateF0401 / CreateF0701 / CreateF0501）。
        /// 回傳 null 表示該發票不適用此格式。
        /// </summary>
        private static XmlDocument? CreateMigDocument(string docType, InvoiceItem item) => docType switch
        {
            "F0401" => item.CreateF0401(),
            "F0701" => item.CreateF0701(),
            // F0501（作廢）僅適用已作廢發票；未作廢者無作廢資料可轉出。
            "F0501" => item.InvoiceCancellation != null ? item.CreateF0501() : null,
            _ => null,
        };

        private static int? TryDecryptKey(string keyId)
        {
            try
            {
                return keyId.DecryptKeyValue();
            }
            catch
            {
                return null;
            }
        }

        // ── 選擇器 ──────────────────────────────────────────────────

        public async Task<List<InvoiceQuerySellerOptionDto>> SearchSellersAsync(
            string? keyword, bool isAdmin, int? categoryId, int? companyId)
        {
            const int maxResults = 20;
            var models = new GenericDbContext<ApplicationDbContext>(_unitOfWork.Context);

            IQueryable<Organization> query;
            if (isAdmin)
            {
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
                query = OrganizationScope.AllowedOrganizations(models, categoryId ?? 0, companyId ?? 0);
            }

            query = query.AsNoTracking();
            var trimmed = keyword?.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                query = query.Where(o => o.ReceiptNo!.StartsWith(trimmed) || o.CompanyName!.Contains(trimmed));
            }

            var sellers = await query
                .OrderBy(o => o.ReceiptNo)
                .Take(maxResults)
                .Select(o => new { o.CompanyID, o.ReceiptNo, o.CompanyName })
                .ToListAsync();

            return sellers.Select(o => new InvoiceQuerySellerOptionDto
            {
                SellerKey = o.CompanyID.EncryptKey(),
                ReceiptNo = o.ReceiptNo,
                CompanyName = o.CompanyName,
            }).ToList();
        }

        public async Task<List<InvoiceQueryAgentOptionDto>> SearchAgentsAsync(string? keyword)
        {
            const int maxResults = 20;
            var models = new GenericDbContext<ApplicationDbContext>(_unitOfWork.Context);

            IQueryable<Organization> query = models.GetTable<Organization>()
                .Where(o => o.OrganizationCategory.Any(c => c.CategoryID == (int)Naming.CategoryID.COMP_INVOICE_AGENT))
                .AsNoTracking();

            var trimmed = keyword?.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                query = query.Where(o => o.ReceiptNo!.StartsWith(trimmed) || o.CompanyName!.Contains(trimmed));
            }

            var agents = await query
                .OrderBy(o => o.ReceiptNo)
                .Take(maxResults)
                .Select(o => new { o.CompanyID, o.ReceiptNo, o.CompanyName })
                .ToListAsync();

            return agents.Select(o => new InvoiceQueryAgentOptionDto
            {
                AgentKey = o.CompanyID.EncryptKey(),
                ReceiptNo = o.ReceiptNo,
                CompanyName = o.CompanyName,
            }).ToList();
        }

        // ── 標籤格式化 ──────────────────────────────────────────────

        private static string TransTypeLabel(int? processType)
        {
            var pt = (Naming.InvoiceProcessType?)processType;
            return pt == Naming.InvoiceProcessType.A0101 ? "交換" : "存證";
        }

        private static string? TaxTypeLabel(byte? taxType)
        {
            if (!taxType.HasValue) return null;
            return ((Naming.TaxTypeDefinition)taxType.Value).ToString();
        }

        private static string? MigStatus(int? processType, bool hasMigC, bool hasMigE)
        {
            var code = ((Naming.InvoiceProcessType?)processType)?.ToString() ?? "C0401";
            var state = hasMigC ? ":C" : hasMigE ? ":E" : ":P";
            return code + state;
        }
    }
}
