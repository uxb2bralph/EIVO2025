using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
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
    /// 發票明細查詢服務實作（遷移自 WebHome InvoiceQueryController.InvoiceReport 頁面之
    /// Inquire / CreateXlsxAsync / DownloadCSV / DownloadAttachment / DownloadAll）。
    /// 查詢條件與角色資料範圍重用 <see cref="InvoiceQueryPipeline"/>（內含 BuildInvoiceQuery
    /// 與 FilterInvoiceByRole）；結果與匯出欄位共用同一組投影，故畫面與檔案內容一致。
    /// </summary>
    public class InvoiceReportService : IInvoiceReportService
    {
        /// <summary>「下載全部附件」單次可處理的發票筆數上限（超過請縮小查詢範圍）。</summary>
        public const int AttachmentZipInvoiceLimit = 1000;

        /// <summary>
        /// 匯出欄位定義（名稱 + 型別；沿用舊版 CreateXlsxAsync 投影順序）。
        /// 金額欄以 decimal 輸出，Excel 才能直接加總（其餘欄位為字串）。
        /// </summary>
        private static readonly (string Name, Type Type)[] DetailColumns =
        {
            ("發票號碼", typeof(string)),
            ("發票日期", typeof(string)),
            ("附件檔名", typeof(string)),
            ("客戶ID", typeof(string)),
            ("序號", typeof(string)),
            ("發票開立人", typeof(string)),
            ("開立人統編", typeof(string)),
            ("未稅金額", typeof(decimal)),
            ("稅額", typeof(decimal)),
            ("含稅金額", typeof(decimal)),
            ("買受人名稱", typeof(string)),
            ("買受人統編", typeof(string)),
            ("連絡人名稱", typeof(string)),
            ("連絡人地址", typeof(string)),
            ("買受人EMail", typeof(string)),
            ("愛心碼", typeof(string)),
            ("是否中獎", typeof(string)),
            ("載具類別", typeof(string)),
            ("載具號碼", typeof(string)),
        };

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InvoiceReportService> _logger;

        public InvoiceReportService(IUnitOfWork unitOfWork, ILogger<InvoiceReportService> logger)
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

        /// <summary>
        /// 排序（欄位限本頁結果表格所呈現者；預設依發票號碼遞增，對應舊版匯出的固定順序）。
        /// 匯出與畫面共用，故檔案內容順序與畫面一致。
        /// </summary>
        private static IQueryable<InvoiceItem> ApplySort(IQueryable<InvoiceItem> query, string? sortName, int? sortType)
        {
            // sortType：1 遞增、2 遞減；其餘視為未指定。
            bool asc = sortType == 1;
            bool desc = sortType == 2;
            if (!string.IsNullOrEmpty(sortName) && (asc || desc))
            {
                switch (sortName)
                {
                    case "InvoiceNo": return desc ? query.OrderByDescending(i => i.TrackCode + i.No) : query.OrderBy(i => i.TrackCode + i.No);
                    case "InvoiceDate": return desc ? query.OrderByDescending(i => i.InvoiceDate) : query.OrderBy(i => i.InvoiceDate);
                    case "CompanyName": return desc ? query.OrderByDescending(i => i.InvoiceSeller!.CustomerName) : query.OrderBy(i => i.InvoiceSeller!.CustomerName);
                    case "ReceiptNo": return desc ? query.OrderByDescending(i => i.InvoiceSeller!.ReceiptNo) : query.OrderBy(i => i.InvoiceSeller!.ReceiptNo);
                    case "BuyerNo": return desc ? query.OrderByDescending(i => i.InvoiceBuyer!.ReceiptNo) : query.OrderBy(i => i.InvoiceBuyer!.ReceiptNo);
                    case "CustomerID": return desc ? query.OrderByDescending(i => i.InvoiceBuyer!.CustomerID) : query.OrderBy(i => i.InvoiceBuyer!.CustomerID);
                    case "OrderNo": return desc ? query.OrderByDescending(i => i.InvoicePurchaseOrder!.OrderNo) : query.OrderBy(i => i.InvoicePurchaseOrder!.OrderNo);
                    case "SalesAmount": return desc ? query.OrderByDescending(i => i.InvoiceAmountType!.SalesAmount) : query.OrderBy(i => i.InvoiceAmountType!.SalesAmount);
                    case "TaxAmount": return desc ? query.OrderByDescending(i => i.InvoiceAmountType!.TaxAmount) : query.OrderBy(i => i.InvoiceAmountType!.TaxAmount);
                    case "TotalAmount": return desc ? query.OrderByDescending(i => i.InvoiceAmountType!.TotalAmount) : query.OrderBy(i => i.InvoiceAmountType!.TotalAmount);
                }
            }
            // 預設：發票號碼遞增（舊版匯出為 OrderBy(InvoiceID)，此處明確化以穩定分頁）。
            return query.OrderBy(i => i.TrackCode).ThenBy(i => i.No).ThenBy(i => i.InvoiceID);
        }

        /// <summary>結果／匯出共用投影（欄位對應舊版 CreateXlsxAsync）。</summary>
        private static async Task<List<InvoiceReportRowDto>> ProjectAsync(IQueryable<InvoiceItem> query)
        {
            var rows = await query
                .Select(i => new
                {
                    i.InvoiceID,
                    i.TrackCode,
                    i.No,
                    i.InvoiceDate,
                    AttachmentName = i.CDS_Document.Attachment.Select(a => a.KeyName).FirstOrDefault(),
                    AttachmentCount = i.CDS_Document.Attachment.Count(),
                    CustomerID = i.InvoiceBuyer!.CustomerID,
                    OrderNo = i.InvoicePurchaseOrder!.OrderNo,
                    SellerName = i.InvoiceSeller!.CustomerName,
                    SellerReceiptNo = i.InvoiceSeller!.ReceiptNo,
                    i.InvoiceAmountType!.SalesAmount,
                    i.InvoiceAmountType!.TaxAmount,
                    i.InvoiceAmountType!.TotalAmount,
                    BuyerName = i.InvoiceBuyer!.CustomerName,
                    BuyerReceiptNo = i.InvoiceBuyer!.ReceiptNo,
                    ContactName = i.InvoiceBuyer!.ContactName,
                    Address = i.InvoiceBuyer!.Address,
                    Email = i.InvoiceBuyer!.EMail,
                    AgencyCode = i.InvoiceDonation!.AgencyCode,
                    // 舊版取中獎號碼主檔的 PrizeType；此處優先取發票中獎紀錄自身欄位，null 時回退主檔。
                    WinningLabel = i.InvoiceWinningNumber != null
                        ? (i.InvoiceWinningNumber.PrizeType ?? (i.InvoiceWinningNumber.Winning != null ? i.InvoiceWinningNumber.Winning.PrizeType : null))
                        : null,
                    CarrierType = i.InvoiceCarrier!.CarrierType,
                    CarrierNo = i.InvoiceCarrier!.CarrierNo,
                    IsCancelled = i.InvoiceCancellation != null,
                })
                .ToListAsync();

            return rows.Select(r => new InvoiceReportRowDto
            {
                KeyId = r.InvoiceID.EncryptKey(),
                InvoiceNo = $"{r.TrackCode}{r.No}",
                InvoiceDate = r.InvoiceDate,
                AttachmentName = r.AttachmentName,
                AttachmentCount = r.AttachmentCount,
                CustomerId = r.CustomerID,
                OrderNo = r.OrderNo,
                SellerName = r.SellerName,
                SellerReceiptNo = r.SellerReceiptNo,
                SalesAmount = r.SalesAmount,
                TaxAmount = r.TaxAmount,
                TotalAmount = r.TotalAmount,
                BuyerName = r.BuyerName,
                BuyerReceiptNo = r.BuyerReceiptNo,
                ContactName = r.ContactName,
                Address = r.Address,
                Email = r.Email,
                AgencyCode = r.AgencyCode,
                WinningLabel = r.WinningLabel,
                CarrierType = r.CarrierType,
                CarrierNo = r.CarrierNo,
                IsCancelled = r.IsCancelled,
            }).ToList();
        }

        // ── 明細清單 ────────────────────────────────────────────────

        public async Task<PagedResultDto<InvoiceReportRowDto>> GetPagedAsync(InvoiceReportQueryDto dto, int uid)
        {
            var query = BuildFilteredQuery(dto, uid, out _);
            if (query == null)
            {
                return Empty(dto);
            }

            var totalCount = await query.CountAsync();

            var items = await ProjectAsync(
                ApplySort(query, dto.SortName, dto.SortType).Skip(dto.Skip).Take(dto.PageSize));

            return new PagedResultDto<InvoiceReportRowDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = dto.Page,
                PageSize = dto.PageSize,
            };
        }

        private static PagedResultDto<InvoiceReportRowDto> Empty(InvoiceReportQueryDto dto) => new()
        {
            Items = new List<InvoiceReportRowDto>(),
            TotalCount = 0,
            PageNumber = dto.Page,
            PageSize = dto.PageSize,
        };

        // ── 匯出：發票資料明細（Excel / CSV 共用同一組欄位）─────────

        /// <summary>取得匯出用的完整結果（不分頁，套用畫面排序）。</summary>
        private async Task<List<InvoiceReportRowDto>> GetExportRowsAsync(InvoiceReportQueryDto dto, int uid)
        {
            var query = BuildFilteredQuery(dto, uid, out _);
            if (query == null)
            {
                return new List<InvoiceReportRowDto>();
            }

            return await ProjectAsync(ApplySort(query, dto.SortName, dto.SortType));
        }

        /// <summary>
        /// 依匯出欄位順序取出單列各欄值（Excel / CSV 共用，確保兩者內容一致）。
        /// 金額回傳原始 decimal（Excel 直接存為數值），空值以 DBNull 表示。
        /// </summary>
        private static object[] ToCellValues(InvoiceReportRowDto r) => new object[]
        {
            r.InvoiceNo ?? "",
            r.InvoiceDate.HasValue ? $"{r.InvoiceDate:yyyy/MM/dd}" : "",
            r.AttachmentName ?? "",
            r.CustomerId ?? "",
            r.OrderNo ?? "",
            r.SellerName ?? "",
            r.SellerReceiptNo ?? "",
            r.SalesAmount ?? (object)DBNull.Value,
            r.TaxAmount ?? (object)DBNull.Value,
            r.TotalAmount ?? (object)DBNull.Value,
            r.BuyerName ?? "",
            r.BuyerReceiptNo ?? "",
            r.ContactName ?? "",
            r.Address ?? "",
            r.Email ?? "",
            r.AgencyCode ?? "",
            r.WinningLabel ?? "",
            r.CarrierType ?? "",
            r.CarrierNo ?? "",
        };

        public async Task<DataTable> BuildDetailTableAsync(InvoiceReportQueryDto dto, int uid)
        {
            var table = new DataTable("發票資料明細");
            foreach (var (name, type) in DetailColumns)
            {
                table.Columns.Add(new DataColumn(name, type));
            }

            foreach (var r in await GetExportRowsAsync(dto, uid))
            {
                table.Rows.Add(ToCellValues(r));
            }

            return table;
        }

        public async Task<string> BuildDetailCsvAsync(InvoiceReportQueryDto dto, int uid)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", DetailColumns.Select(c => CsvField(c.Name))));

            foreach (var r in await GetExportRowsAsync(dto, uid))
            {
                // 金額以不因伺服器地區設定改變小數符號的方式輸出，避免破壞 CSV 欄位切分。
                sb.AppendLine(string.Join(",", ToCellValues(r).Select(v =>
                    CsvField(v == DBNull.Value ? "" : Convert.ToString(v, CultureInfo.InvariantCulture) ?? ""))));
            }

            return sb.ToString();
        }

        /// <summary>CSV 欄位轉義（含逗號 / 引號 / 換行時以雙引號包覆）。</summary>
        private static string CsvField(string value)
        {
            if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
            {
                return value;
            }
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        // ── 下載：發票附件壓縮檔 ────────────────────────────────────
        // 遷移自舊版 DownloadAttachment / DownloadAll + zipAttachment，
        // 差異：改在記憶體組壓縮檔（不落地 ~/temp），且對象先以角色範圍限縮。

        public async Task<InvoiceAttachmentZipResultDto> BuildSelectedAttachmentZipAsync(
            IEnumerable<string> keyIds, int uid)
        {
            var result = new InvoiceAttachmentZipResultDto();

            // 解密請求鍵；保留原鍵以便回報查無 / 越權者。
            var requested = (keyIds ?? Enumerable.Empty<string>())
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Distinct()
                .Select(k => new { KeyId = k, InvoiceId = TryDecryptKey(k) })
                .ToList();
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
            var attachments = ids.Count > 0
                ? await LoadAttachmentsAsync(query.Where(i => ids.Contains(i.InvoiceID))
                    .OrderBy(i => i.TrackCode).ThenBy(i => i.No))
                : new List<InvoiceAttachmentSource>();

            var found = new HashSet<int>(attachments.Select(a => a.InvoiceId));
            foreach (var r in requested)
            {
                if (!r.InvoiceId.HasValue || !found.Contains(r.InvoiceId.Value))
                {
                    // 查無、無權存取，或該發票無附件。
                    result.SkippedNos.Add(r.KeyId);
                }
            }

            result.MatchedInvoiceCount = attachments.Count;
            WriteZip(attachments, result);
            return result;
        }

        public async Task<InvoiceAttachmentZipResultDto> BuildAllAttachmentZipAsync(
            InvoiceReportQueryDto dto, int uid)
        {
            var result = new InvoiceAttachmentZipResultDto();

            var query = BuildFilteredQuery(dto, uid, out _);
            if (query == null)
            {
                return result;
            }

            // 僅需有附件的發票；先計數避免一次壓縮過量檔案。
            var withAttachment = query.Where(i => i.CDS_Document.Attachment.Any());
            result.MatchedInvoiceCount = await withAttachment.CountAsync();
            if (result.MatchedInvoiceCount == 0)
            {
                return result;
            }
            if (result.MatchedInvoiceCount > AttachmentZipInvoiceLimit)
            {
                result.ExceededLimit = true;
                return result;
            }

            var attachments = await LoadAttachmentsAsync(
                ApplySort(withAttachment, dto.SortName, dto.SortType));

            WriteZip(attachments, result);
            return result;
        }

        /// <summary>單張發票的附件來源（發票號碼 + 各附件實體路徑）。</summary>
        private sealed class InvoiceAttachmentSource
        {
            public int InvoiceId { get; init; }
            public string InvoiceNo { get; init; } = string.Empty;
            public List<string> StoredPaths { get; init; } = new();
        }

        /// <summary>取出各發票的附件實體路徑（僅含有附件者）。</summary>
        private static async Task<List<InvoiceAttachmentSource>> LoadAttachmentsAsync(IQueryable<InvoiceItem> query)
        {
            var rows = await query
                .Where(i => i.CDS_Document.Attachment.Any())
                .Select(i => new
                {
                    i.InvoiceID,
                    i.TrackCode,
                    i.No,
                    Paths = i.CDS_Document.Attachment.Select(a => a.StoredPath).ToList(),
                })
                .ToListAsync();

            return rows.Select(r => new InvoiceAttachmentSource
            {
                InvoiceId = r.InvoiceID,
                InvoiceNo = $"{r.TrackCode}{r.No}",
                StoredPaths = r.Paths,
            }).ToList();
        }

        /// <summary>
        /// 將附件寫入壓縮檔（沿用舊版 zipAttachment 之命名：第一份為「發票號碼.pdf」，
        /// 其後為「發票號碼-序號.pdf」）。實體檔案不存在者記入 SkippedNos。
        /// 若有略過項目，另加入清單文字檔說明，避免使用者誤以為已下載完整。
        /// </summary>
        private void WriteZip(List<InvoiceAttachmentSource> sources, InvoiceAttachmentZipResultDto result)
        {
            var missing = new List<string>();

            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var source in sources)
                {
                    var index = 0;
                    var written = 0;
                    foreach (var path in source.StoredPaths)
                    {
                        var entryName = index == 0 ? $"{source.InvoiceNo}.pdf" : $"{source.InvoiceNo}-{index}.pdf";
                        index++;

                        if (string.IsNullOrEmpty(path) || !File.Exists(path))
                        {
                            continue;
                        }

                        try
                        {
                            var entry = zip.CreateEntry(entryName);
                            using var outStream = entry.Open();
                            using var inStream = File.OpenRead(path);
                            inStream.CopyTo(outStream);
                            written++;
                            result.IncludedCount++;
                        }
                        catch (Exception ex)
                        {
                            // 單一附件讀取失敗不影響其他發票（原版亦僅在檔案存在時寫入）。
                            _logger.LogError(ex, "Error zipping attachment for invoice {InvoiceNo}", source.InvoiceNo);
                        }
                    }

                    if (written == 0)
                    {
                        missing.Add(source.InvoiceNo);
                    }
                }

                if (missing.Count > 0 && result.IncludedCount > 0)
                {
                    var entry = zip.CreateEntry("_未取得附件清單.txt");
                    using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(true));
                    writer.WriteLine("以下發票有附件紀錄，但實體檔案不存在或讀取失敗：");
                    foreach (var no in missing)
                    {
                        writer.WriteLine(no);
                    }
                }
            }

            result.SkippedNos.AddRange(missing);
            if (result.IncludedCount > 0)
            {
                result.Content = ms.ToArray();
            }
        }

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
    }
}
