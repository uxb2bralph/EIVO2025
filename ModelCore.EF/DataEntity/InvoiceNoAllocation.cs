using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceNoAllocation
{
    public int IntervalID { get; set; }

    public int InvoiceNo { get; set; }

    public int Status { get; set; }

    public string? RandomNo { get; set; }

    public string? EncryptedContent { get; set; }

    public DateTime? AllocateDate { get; set; }

    public virtual InvoiceNoInterval Interval { get; set; } = null!;
}
