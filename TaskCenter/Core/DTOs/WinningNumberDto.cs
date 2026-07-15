using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 中獎號碼查詢條件（遷移自舊版 InquireNoIntervalViewModel / WinningNumberController.Inquire）。
    /// 以 [FromQuery] 繫結；Year 與 PeriodNo 皆為必填（對應舊版查詢須同時選年份與期別）。
    /// </summary>
    public class WinningNumberQueryDto
    {
        /// <summary>發票年度（西元年；前端以民國年顯示 = Year - 1911）</summary>
        public int? Year { get; set; }

        /// <summary>發票期別（1~6 對應雙月）</summary>
        public int? PeriodNo { get; set; }
    }

    /// <summary>
    /// 中獎號碼列表項目（對應舊版 WinningNumber/Module/DataItem.cshtml 之列欄位）。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class WinningNumberDatatableDto
    {
        /// <summary>中獎號碼識別碼（UniformInvoiceWinningNumber.WinningID），供修改 / 刪除傳遞。</summary>
        [JsonPropertyName("winningId")]
        public int WinningId { get; set; }

        /// <summary>發票年度（西元年）</summary>
        [JsonPropertyName("year")]
        public int Year { get; set; }

        /// <summary>發票期別（1~6）</summary>
        [JsonPropertyName("period")]
        public int Period { get; set; }

        /// <summary>獎別代碼（Naming.WinningPrizeType）</summary>
        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        /// <summary>獎別名稱（如「頭獎」）</summary>
        [JsonPropertyName("prizeType")]
        public string? PrizeType { get; set; }

        /// <summary>中獎金額</summary>
        [JsonPropertyName("bonus")]
        public int? Bonus { get; set; }

        /// <summary>中獎號碼</summary>
        [JsonPropertyName("winningNo")]
        public string? WinningNo { get; set; }

        /// <summary>
        /// 是否可修改 / 刪除（對應舊版 DataItem.cshtml isEditable：
        /// 特別獎 / 特獎 / 頭獎 / 增開六獎 可維護，二~六獎為頭獎自動衍生不可直接維護）。
        /// </summary>
        [JsonPropertyName("editable")]
        public bool Editable { get; set; }
    }

    /// <summary>
    /// 新增 / 修改中獎號碼（遷移自舊版 WinningNumberViewModel / WinningNumberController.CommitItem）。
    /// WinningId 為 null 時新增，否則修改。頭獎會自動衍生二~六獎。
    /// </summary>
    public class WinningNumberEditDto
    {
        /// <summary>中獎號碼識別碼；新增時為 null。</summary>
        public int? WinningId { get; set; }

        /// <summary>發票年度（西元年）；新增時必填。</summary>
        public int? Year { get; set; }

        /// <summary>發票期別（1~6）；新增時必填。</summary>
        public int? Period { get; set; }

        /// <summary>獎別代碼（Naming.EditableWinningPrizeType：1 特別獎 / 2 特獎 / 3 頭獎 / 9 增開六獎）。</summary>
        public int? Rank { get; set; }

        /// <summary>中獎號碼（特別獎 / 特獎 / 頭獎為 8 碼數字；增開六獎為 3 碼數字）。</summary>
        public string? WinningNo { get; set; }
    }

    /// <summary>
    /// 對獎 / 清除作業條件（遷移自舊版 InquireNoIntervalViewModel）。
    /// 以 [FromBody] 繫結；Year 與 PeriodNo 皆為必填。
    /// </summary>
    public class WinningActionDto
    {
        /// <summary>發票年度（西元年）</summary>
        public int? Year { get; set; }

        /// <summary>發票期別（1~6）</summary>
        public int? PeriodNo { get; set; }
    }

    /// <summary>
    /// 雲端發票中獎清冊上傳結果（遷移自舊版 WinningNumberController.UploadWinningNo 之 PromptCheckDownload）。
    /// 上傳成功後回傳工作識別碼，前端據此輪詢處理狀態並於完成後下載結果檔。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class WinningNoUploadResultDto
    {
        /// <summary>處理工作識別碼（proc.ProcessRequest.TaskID）。</summary>
        [JsonPropertyName("taskId")]
        public int TaskId { get; set; }

        /// <summary>結果檔下載時的建議檔名（沿用舊版「中獎發票回應.xlsx」）。</summary>
        [JsonPropertyName("fileDownloadName")]
        public string? FileDownloadName { get; set; }
    }

    /// <summary>
    /// 雲端發票中獎清冊處理狀態（對應舊版 DataExchange.CheckResource 之輪詢回應）。
    /// </summary>
    public class WinningNoProcessStatusDto
    {
        /// <summary>是否已完成（結果檔已產生，可供下載）。</summary>
        [JsonPropertyName("completed")]
        public bool Completed { get; set; }

        /// <summary>處理過程是否有例外（對應舊版 ExceptionLog；仍可下載含處理狀態之結果檔）。</summary>
        [JsonPropertyName("failed")]
        public bool Failed { get; set; }

        /// <summary>例外訊息（沿用舊版 ExceptionLog.DataContent；無則為 null）。</summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
