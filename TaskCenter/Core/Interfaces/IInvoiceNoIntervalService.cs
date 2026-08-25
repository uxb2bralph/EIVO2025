using System.Collections.Generic;
using System.Threading.Tasks;
using ModelCore.DTOs;
using TaskCenter.Core.DTOs;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// 電子發票配號區間查詢服務（遷移自舊版 InvoiceNoController.InquireInterval 及其選擇器）。
    /// 寫入（新增 / 修改 / 刪除 / 鎖定）沿用 TaskCenter 慣例，於 Controller 內以 _models 直接處理。
    /// </summary>
    public interface IInvoiceNoIntervalService
    {
        /// <summary>
        /// 依開立人 + 年度 + 期別取得分頁配號區間列表，並依登入者角色範圍限制
        /// （系統管理看全部；賣方/代理僅限自己與下轄分支）。
        /// </summary>
        Task<PagedResultDto<InvoiceNoIntervalDatatableDto>> GetPagedAsync(
            InvoiceNoIntervalQueryDto queryDto, bool isAdmin, int? categoryId, int? companyId);

        /// <summary>
        /// 依關鍵字搜尋開立人候選清單（統編前綴 / 名稱包含），並依登入者角色範圍限制候選結果。
        /// </summary>
        Task<List<InvoiceNoSellerOptionDto>> SearchSellersAsync(
            string? keyword, bool isAdmin, int? categoryId, int? companyId);

        /// <summary>依年度 + 期別取得字軌候選清單（新增配號區間時選擇字軌）。</summary>
        Task<List<InvoiceTrackCodeOptionDto>> GetTrackCodeOptionsAsync(int? year, int? periodNo);
    }
}
