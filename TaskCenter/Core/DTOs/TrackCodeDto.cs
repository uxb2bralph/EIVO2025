using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 電子發票字軌查詢條件（遷移自舊版 TrackCodeQueryViewModel / TrackCodeController.Inquire）。
    /// 以 [FromQuery] 繫結；Year 為必填（對應舊版查詢表單一律帶入發票年度）。
    /// </summary>
    public class TrackCodeQueryDto
    {
        /// <summary>發票年度（西元年；前端以民國年顯示 = Year - 1911）</summary>
        public int? Year { get; set; }

        /// <summary>發票期別（1~6 對應雙月；null 表示全部）</summary>
        public int? PeriodNo { get; set; }

        /// <summary>頁碼（1-based）</summary>
        public int Page { get; set; } = 1;

        /// <summary>每頁筆數</summary>
        public int PageSize { get; set; } = 10;

        /// <summary>略過筆數</summary>
        [JsonIgnore]
        public int Skip => (Page - 1) * PageSize;
    }

    /// <summary>
    /// 電子發票字軌列表項目（對應舊版 TrackCode/Module/DataItem.cshtml 之列欄位）。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class TrackCodeDatatableDto
    {
        /// <summary>字軌識別碼（InvoiceTrackCode.TrackID），供修改 / 刪除傳遞。</summary>
        [JsonPropertyName("trackId")]
        public int TrackId { get; set; }

        /// <summary>發票年度（西元年）</summary>
        [JsonPropertyName("year")]
        public int Year { get; set; }

        /// <summary>發票期別（1~6）</summary>
        [JsonPropertyName("periodNo")]
        public int PeriodNo { get; set; }

        /// <summary>字軌（二位英文字母）</summary>
        [JsonPropertyName("trackCode")]
        public string? TrackCode { get; set; }

        /// <summary>發票類別（Naming.InvoiceTypeDefinition：7 一般 / 8 特種）；前端以此對照顯示名稱。</summary>
        [JsonPropertyName("invoiceType")]
        public int? InvoiceType { get; set; }
    }

    /// <summary>
    /// 新增 / 修改電子發票字軌（遷移自舊版 TrackCodeViewModel / TrackCodeController.CommitItem）。
    /// TrackId 為 null 時新增，否則修改；修改時 Year / PeriodNo 不可變更（沿用舊版）。
    /// </summary>
    public class TrackCodeEditDto
    {
        /// <summary>字軌識別碼；新增時為 null。</summary>
        public int? TrackId { get; set; }

        /// <summary>發票年度（西元年）；新增時必填。</summary>
        public int? Year { get; set; }

        /// <summary>發票期別（1~6）；新增時必填。</summary>
        public int? PeriodNo { get; set; }

        /// <summary>字軌（二位英文字母）。</summary>
        public string? TrackCode { get; set; }

        /// <summary>發票類別（Naming.InvoiceTypeDefinition：7 一般 / 8 特種）；未指定時預設一般。</summary>
        public int? InvoiceType { get; set; }
    }
}
