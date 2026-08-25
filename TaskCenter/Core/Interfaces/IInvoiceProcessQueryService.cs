using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ModelCore.DTOs;
using TaskCenter.Core.DTOs;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// 發票資料查詢服務（遷移自 WebHome InvoiceProcessController.Index/Inquire）。
    /// 查詢管線重用 ModelExtension.EF 的 ModelSource&lt;InvoiceItem&gt;.BuildInvoiceQuery，
    /// 角色範圍以登入者 UID 還原 UserProfileWrapper 後由 FilterInvoiceByRole 套用。
    /// 服務僅負責查詢與匯出內容組裝；不做寫入。
    /// </summary>
    public interface IInvoiceProcessQueryService
    {
        /// <summary>查詢發票（分頁）。uid 為登入者 UID，用以還原角色資料範圍。</summary>
        Task<PagedResultDto<InvoiceItemDatatableDto>> GetPagedAsync(InvoiceProcessQueryDto dto, int uid);

        /// <summary>對完整過濾集依幣別彙總（對應舊版 CurrencySummary footer）。</summary>
        Task<List<CurrencySummaryDto>> GetSummaryAsync(InvoiceProcessQueryDto dto, int uid);

        /// <summary>取得單張發票明細（先確認在登入者可視範圍內，否則回 null）。</summary>
        Task<InvoiceDetailDto?> GetDetailAsync(int invoiceId, int uid);

        /// <summary>組裝發票資料明細 Excel 內容（admin 25 欄 / 非 admin 去除連絡人 3 欄）。</summary>
        Task<DataTable> BuildXlsxTableAsync(InvoiceProcessQueryDto dto, int uid, bool isAdmin);

        /// <summary>組裝發票買受人資料 Excel 內容（6 欄）。</summary>
        Task<DataTable> BuildBuyerTableAsync(InvoiceProcessQueryDto dto, int uid);

        /// <summary>組裝 ERP 匯出固定寬度文字內容（POSINV.dat）。</summary>
        Task<string> BuildErpTextAsync(InvoiceProcessQueryDto dto, int uid);

        /// <summary>
        /// 組裝選取發票的 MIG（F0401 / F0701 / F0501）XML 壓縮檔。
        /// keyIds 為加密後 InvoiceID；uid 為登入者 UID，用以還原角色資料範圍（範圍外者略過）。
        /// </summary>
        Task<MigZipResultDto> BuildMigZipAsync(string docType, IEnumerable<string> keyIds, int uid);

        /// <summary>開立人候選清單（依角色範圍限縮）。</summary>
        Task<List<InvoiceQuerySellerOptionDto>> SearchSellersAsync(string? keyword, bool isAdmin, int? categoryId, int? companyId);

        /// <summary>代理業者候選清單（僅系統管理使用）。</summary>
        Task<List<InvoiceQueryAgentOptionDto>> SearchAgentsAsync(string? keyword);
    }
}
