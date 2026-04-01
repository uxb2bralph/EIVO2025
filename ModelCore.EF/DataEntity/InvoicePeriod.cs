using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoicePeriod
{
    public int PeriodID { get; set; }

    public virtual ICollection<InvoicePeriodExchangeRate> InvoicePeriodExchangeRate { get; set; } = new List<InvoicePeriodExchangeRate>();

    public virtual ICollection<InvoiceTrackCode> InvoiceTrackCode { get; set; } = new List<InvoiceTrackCode>();
}
