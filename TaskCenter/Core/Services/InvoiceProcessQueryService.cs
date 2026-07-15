using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var profile = new UserProfileManager(models).GetUserProfile(uid);
            if (profile?.CurrentUserRole == null)
            {
                return null;
            }

            var vm = MapToViewModel(dto);
            var ms = new ModelSource<InvoiceItem>(models);
            ms.BuildInvoiceQuery(vm, profile, "Common");
            return ms.Items;
        }

        /// <summary>DTO → InquireInvoiceViewModel（欄位對應舊版查詢表單）。</summary>
        private static InquireInvoiceViewModel MapToViewModel(InvoiceProcessQueryDto dto)
        {
            var vm = new InquireInvoiceViewModel
            {
                BuyerReceiptNo = dto.BuyerReceiptNo.GetEfficientString(),
                BuyerName = dto.BuyerName.GetEfficientString(),
                CustomerID = dto.CustomerId.GetEfficientString(),
                DateFrom = dto.DateFrom,
                DateTo = dto.DateTo,
                InvoiceNo = dto.InvoiceNo.GetEfficientString(),
                EndNo = dto.EndNo.GetEfficientString(),
                DataNo = dto.DataNo.GetEfficientString(),
                Attachment = dto.Attachment,
                Winning = dto.Winning,
                Cancelled = dto.Cancelled,
                PrintMark = dto.PrintMark.GetEfficientString(),
                Printed = dto.Printed,
                HasAddr = dto.HasAddr,
                CarrierType = dto.CarrierType.GetEfficientString(),
                CarrierNo = dto.CarrierNo.GetEfficientString(),
                IsNoticed = dto.IsNoticed,
            };

            if (!string.IsNullOrEmpty(dto.SellerKey))
            {
                vm.SellerID = dto.SellerKey.DecryptKeyValue();
            }
            if (!string.IsNullOrEmpty(dto.AgentKey))
            {
                vm.AgentID = dto.AgentKey.DecryptKeyValue();
            }
            if (dto.BusinessType.HasValue)
            {
                vm.BusinessType = (Naming.InvoiceCenterBusinessType)dto.BusinessType.Value;
            }

            return vm;
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
                    ProcessType = i.Invoice.ProcessType,
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
