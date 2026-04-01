using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceNoSegment
{
    public int SegmentID { get; set; }

    public string? DeviceName { get; set; }

    public virtual InvoiceNoSegmentDisposition? InvoiceNoSegmentDisposition { get; set; }

    public virtual POSInvoiceNoSegment? POSInvoiceNoSegment { get; set; }

    public virtual InvoiceNoInterval Segment { get; set; } = null!;
}
