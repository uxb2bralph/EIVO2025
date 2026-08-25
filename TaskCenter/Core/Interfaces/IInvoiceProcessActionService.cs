using System.Collections.Generic;
using System.Threading.Tasks;
using TaskCenter.Core.DTOs;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// 發票逐列作業服務（遷移自 WebHome InvoiceProcessController 之 CommitAction 系列）。
    /// 目前提供線上作廢（CancelInvoice）。查詢仍由 IInvoiceProcessQueryService 負責；
    /// 此服務僅處理寫入動作，並以登入者角色範圍限縮可作業對象。
    /// </summary>
    public interface IInvoiceProcessActionService
    {
        /// <summary>
        /// 作廢指定發票。keyIds 為加密後 InvoiceID；uid 為登入者 UID，用以還原角色資料範圍。
        /// 僅作廢登入者可視範圍內、且尚未作廢之發票。
        /// </summary>
        Task<CancelInvoiceResultDto> CancelAsync(IEnumerable<string> keyIds, int uid);
    }
}
