using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class POSInvoiceNoSegment
{
    public int SegmentID { get; set; }

    public int DeviceID { get; set; }

    public DateTime? RequestDate { get; set; }

    public virtual POSDevice Device { get; set; } = null!;

    public virtual InvoiceNoSegment Segment { get; set; } = null!;
}
