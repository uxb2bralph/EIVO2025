using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 發票消售金額明細檔
/// </summary>
public partial class InvoiceAmountType
{
    public int InvoiceID { get; set; }

    /// <summary>
    /// 課稅別
    /// 1：應稅
    /// 2：零稅率
    /// 3：免稅
    /// 9：混合應稅與免稅或零稅率 (限收銀機發票無法分辨時使用)
    /// </summary>
    public byte? TaxType { get; set; }

    /// <summary>
    /// 應稅銷售額合計(新台幣)
    /// </summary>
    public decimal? SalesAmount { get; set; }

    /// <summary>
    /// 營業稅額
    /// </summary>
    public decimal? TaxAmount { get; set; }

    /// <summary>
    /// 稅率
    /// </summary>
    public decimal? TaxRate { get; set; }

    /// <summary>
    /// 總計
    /// 整數
    /// (應稅銷售額合計+免稅銷售額合計+零稅率銷售額合計+營業稅額=此總計欄位) ，可為負數
    /// </summary>
    public decimal? TotalAmount { get; set; }

    /// <summary>
    /// 中文國字大寫金額
    /// </summary>
    public string? TotalAmountInChinese { get; set; }

    /// <summary>
    /// 扣抵金額
    /// </summary>
    public decimal? DiscountAmount { get; set; }

    /// <summary>
    /// 角分調整
    /// </summary>
    public decimal? Adjustment { get; set; }

    /// <summary>
    /// 原幣金額
    /// </summary>
    public decimal? OriginalCurrencyAmount { get; set; }

    /// <summary>
    /// 匯率
    /// </summary>
    public decimal? ExchangeRate { get; set; }

    public int? CurrencyID { get; set; }

    public decimal? FreeTaxSalesAmount { get; set; }

    public decimal? ZeroTaxSalesAmount { get; set; }

    public byte? BondedAreaConfirm { get; set; }

    public string? ZeroTaxRateReason { get; set; }

    public virtual CurrencyType? Currency { get; set; }

    public virtual InvoiceItem Invoice { get; set; } = null!;
}
