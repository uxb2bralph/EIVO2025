using System.Collections.Generic;
using System.Threading.Tasks;
using TaskCenter.Core.DTOs;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// 中獎號碼查詢服務（遷移自舊版 WinningNumberController.Inquire）。
    /// </summary>
    public interface IWinningNumberService
    {
        /// <summary>
        /// 依發票年度 + 期別取得中獎號碼列表（依獎別排序，對應舊版 ItemList.cshtml）。
        /// </summary>
        Task<List<WinningNumberDatatableDto>> GetListAsync(WinningNumberQueryDto queryDto);
    }
}
