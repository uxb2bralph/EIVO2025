using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class UnassignedInvoiceNo
{
    public int UAID { get; set; }

    public int TrackID { get; set; }

    public int SellerID { get; set; }

    public int InvoiceBeginNo { get; set; }

    public int InvoiceEndNo { get; set; }

    public virtual InvoiceTrackCodeAssignment InvoiceTrackCodeAssignment { get; set; } = null!;
}
