using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 期別匯率查詢條件（遷移自舊版 ExchangeRateQueryViewModel / PeriodicalExchangeRateController.Inquire）。
    /// 以 [FromQuery] 繫結；Year 為必填，PeriodNo 省略表示查詢該年度全部期別。
    /// 註：舊版期別以 PeriodID = Year*100 + PeriodNo 表示（PeriodNo 為 1~6 之雙月期別）。
    /// </summary>
    public class ExchangeRateQueryDto
    {
        /// <summary>發票年度（西元年；前端以民國年顯示 = Year - 1911）</summary>
        public int? Year { get; set; }

        /// <summary>發票期別（1~6 對應雙月；null 表示該年度全部期別）</summary>
        public int? PeriodNo { get; set; }

        /// <summary>幣別（AbbrevName，前綴比對；NTD 視同 TWD）；null 表示全部幣別</summary>
        public string? Currency { get; set; }

        /// <summary>頁碼（1-based）</summary>
        public int Page { get; set; } = 1;

        /// <summary>每頁筆數</summary>
        public int PageSize { get; set; } = 10;

        /// <summary>略過筆數</summary>
        [JsonIgnore]
        public int Skip => (Page - 1) * PageSize;
    }

    /// <summary>
    /// 期別匯率列表項目（對應舊版 PeriodicalExchangeRate/DataQuery/ExchangeRateList.cshtml 之列欄位）。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class ExchangeRateDatatableDto
    {
        /// <summary>期別識別碼（InvoicePeriodExchangeRate.PeriodID = 年度*100 + 期別）。與 CurrencyId 共同組成複合鍵，供修改 / 刪除傳遞。</summary>
        [JsonPropertyName("periodId")]
        public int PeriodId { get; set; }

        /// <summary>發票年度（西元年；= PeriodID / 100）</summary>
        [JsonPropertyName("year")]
        public int Year { get; set; }

        /// <summary>發票期別（1~6；= PeriodID % 100）</summary>
        [JsonPropertyName("periodNo")]
        public int PeriodNo { get; set; }

        /// <summary>幣別識別碼（CurrencyType.CurrencyID）。與 PeriodId 共同組成複合鍵，供修改 / 刪除傳遞。</summary>
        [JsonPropertyName("currencyId")]
        public int CurrencyId { get; set; }

        /// <summary>幣別代碼（CurrencyType.AbbrevName，如「USD」）</summary>
        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        /// <summary>幣別名稱（CurrencyType.CurrencyName）</summary>
        [JsonPropertyName("currencyName")]
        public string? CurrencyName { get; set; }

        /// <summary>匯率</summary>
        [JsonPropertyName("exchangeRate")]
        public decimal ExchangeRate { get; set; }
    }

    /// <summary>
    /// 新增 / 修改期別匯率（遷移自舊版 ExchangeRateQueryViewModel / PeriodicalExchangeRateController.CommitItem）。
    /// OrigPeriodId / OrigCurrencyId 為修改前的複合鍵（新增時為 null）；當幣別 / 期別變更時，
    /// 後端會將原資料列移動至新的複合鍵（對應舊版以 KeyID 帶入原識別後的搬移邏輯）。
    /// </summary>
    public class ExchangeRateEditDto
    {
        /// <summary>修改前的期別識別碼；新增時為 null。</summary>
        public int? OrigPeriodId { get; set; }

        /// <summary>修改前的幣別識別碼；新增時為 null。</summary>
        public int? OrigCurrencyId { get; set; }

        /// <summary>發票年度（西元年）；與 PeriodNo 共同組成 PeriodID。</summary>
        public int? Year { get; set; }

        /// <summary>發票期別（1~6）；與 Year 共同組成 PeriodID。</summary>
        public int? PeriodNo { get; set; }

        /// <summary>期別識別碼（= 年度*100 + 期別）；若有值則優先於 Year / PeriodNo。</summary>
        public int? PeriodId { get; set; }

        /// <summary>幣別代碼（CurrencyType.AbbrevName；NTD 視同 TWD）。</summary>
        public string? Currency { get; set; }

        /// <summary>匯率（須大於 0）。</summary>
        public decimal? ExchangeRate { get; set; }
    }
}
