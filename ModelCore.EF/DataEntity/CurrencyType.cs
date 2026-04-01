using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 幣別主檔
/// </summary>
public partial class CurrencyType
{
    public int CurrencyID { get; set; }

    /// <summary>
    /// 貨幣完整名稱
    /// </summary>
    public string? CurrencyName { get; set; }

    /// <summary>
    /// 簡稱
    /// </summary>
    public string? AbbrevName { get; set; }

    public string? FormatPattern { get; set; }

    public short? Decimals { get; set; }

    public virtual ICollection<InvoiceAllowance> InvoiceAllowance { get; set; } = new List<InvoiceAllowance>();

    public virtual ICollection<InvoiceAmountType> InvoiceAmountType { get; set; } = new List<InvoiceAmountType>();

    public virtual ICollection<InvoicePeriodExchangeRate> InvoicePeriodExchangeRate { get; set; } = new List<InvoicePeriodExchangeRate>();
}
