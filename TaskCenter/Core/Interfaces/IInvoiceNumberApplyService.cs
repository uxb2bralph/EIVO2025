using System.Collections.Generic;
using ModelCore.Models;
using ModelCore.Models.ViewModel;
using TaskCenter.Core.DTOs;

namespace TaskCenter.Core.Interfaces
{
    /// <summary>
    /// 電子發票字軌號碼申請查詢 / 維護服務（遷移自舊版 WebHome InvoiceNumberApplyController + InvoiceNumberApplyService）。
    /// 申請資料以檔案系統上的 JSON 檔保存，非資料庫實體。
    /// 需資料庫寫入之「轉營業人」提交由控制器以其 DbContext 執行；本服務僅負責檔案與轉換 / 打包邏輯。
    /// </summary>
    public interface IInvoiceNumberApplyService
    {
        /// <summary>依統一編號（部分比對）列出申請 JSON 檔（對應舊版 Query / GetApplyJsonFiles）。</summary>
        List<InvoiceNumberApplyItemDto> GetApplyFiles(string? businessId);

        /// <summary>解密列動作傳入的 KeyId 取得申請 JSON 檔路徑；檔案不存在則回傳 null（對應舊版 KeyID.DecryptData）。</summary>
        string? ResolveApplyFilePath(string? keyId);

        /// <summary>讀取申請 JSON 檔為模型；失敗回傳 null。</summary>
        InvoiceNumberApply? LoadApply(string filePath);

        /// <summary>將申請資料轉換為營業人 ViewModel（對應舊版 ApplyConvertedOrganization）。</summary>
        OrganizationViewModel ConvertToOrganization(InvoiceNumberApply apply);

        /// <summary>將申請 JSON 檔歸檔（搬移至備份資料夾）（對應舊版 MoveJsonFile）。</summary>
        void MoveJsonFile(string filePath);

        /// <summary>
        /// 依統一編號讀取申請資料，逐一套用 Word 範本產出 .doc（XML）並打包為 zip（對應舊版 SetAll）。
        /// 找不到申請資料時回傳 null。
        /// </summary>
        (byte[] Content, string FileName)? BuildWordZip(string businessId);
    }
}
