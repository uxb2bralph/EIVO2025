using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoicePeriodExchangeRate
{
    public int PeriodID { get; set; }

    public int CurrencyID { get; set; }

    /// <summary>
    /// 匯率
    /// </summary>
    public decimal ExchangeRate { get; set; }

    public virtual CurrencyType Currency { get; set; } = null!;

    public virtual InvoicePeriod Period { get; set; } = null!;
}
