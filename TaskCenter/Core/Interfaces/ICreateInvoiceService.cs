using System.Collections.Generic;
using System.Threading.Tasks;
using TaskCenter.Core.DTOs;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// 線上開立發票服務（遷移自 WebHome InvoiceBusinessController.CreateInvoice /
    /// CommitInvoice（F0401 存證）/ CommitA0101（B2B 交換），及其表單依賴的
    /// 開立人 / 相對營業人 / 產品快速查詢）。
    /// 開立邏輯重用 ModelExtension.EF 之 InvoiceViewModelValidator / A0101ViewModelValidator。
    /// </summary>
    public interface ICreateInvoiceService
    {
        /// <summary>開立人候選清單（依角色範圍限縮；沿用 InvoiceProcessQuery 之作法）。</summary>
        Task<List<CreateInvoiceSellerOptionDto>> SearchSellersAsync(string? keyword, bool isAdmin, int? categoryId, int? companyId);

        /// <summary>
        /// 相對營業人（買受人）查詢：以開立人的相對營業人關係為範圍，供表單自動帶入 / autocomplete。
        /// term 為統編前綴或名稱關鍵字。
        /// </summary>
        Task<List<CounterpartOptionDto>> SearchCounterpartsAsync(int sellerId, string? term);

        /// <summary>產品快速查詢（依登入者角色與所選開立人限縮；對應舊版 ProductCatalog/QuickSearch）。</summary>
        Task<List<ProductOptionDto>> SearchProductsAsync(int uid, int sellerId, string? productName);

        /// <summary>
        /// 開立單張發票（F0401 存證或 A0101 交換，由 dto.ProcessType 決定）。
        /// dto.ForPreview 為 true 時僅回傳內容預覽（不寫入）。owner 即所選開立人（seller）。
        /// </summary>
        Task<CreateInvoiceCommitResult> CommitAsync(CreateInvoiceRequestDto dto, int sellerId);
    }
}
