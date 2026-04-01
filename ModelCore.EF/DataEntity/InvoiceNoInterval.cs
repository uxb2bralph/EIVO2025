using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceNoInterval
{
    public int IntervalID { get; set; }

    public int TrackID { get; set; }

    public int SellerID { get; set; }

    public int StartNo { get; set; }

    public int EndNo { get; set; }

    public int? LockID { get; set; }

    public virtual ICollection<InvoiceNoAllocation> InvoiceNoAllocation { get; set; } = new List<InvoiceNoAllocation>();

    public virtual ICollection<InvoiceNoAssignment> InvoiceNoAssignment { get; set; } = new List<InvoiceNoAssignment>();

    public virtual InvoiceNoSegment? InvoiceNoSegment { get; set; }

    public virtual InvoiceTrackCodeAssignment InvoiceTrackCodeAssignment { get; set; } = null!;

    public virtual ICollection<VacantInvoiceNo> VacantInvoiceNo { get; set; } = new List<VacantInvoiceNo>();
}
