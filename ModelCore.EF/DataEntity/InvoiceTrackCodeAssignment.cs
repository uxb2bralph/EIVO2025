using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceTrackCodeAssignment
{
    public int TrackID { get; set; }

    public int SellerID { get; set; }

    public int? AssignmentID { get; set; }

    public virtual InvoiceNoMainAssignment? Assignment { get; set; }

    public virtual ICollection<InvoiceNoInterval> InvoiceNoInterval { get; set; } = new List<InvoiceNoInterval>();

    public virtual ICollection<InvoiceNoMainAssignment> InvoiceNoMainAssignment { get; set; } = new List<InvoiceNoMainAssignment>();

    public virtual Organization Seller { get; set; } = null!;

    public virtual InvoiceTrackCode Track { get; set; } = null!;

    public virtual ICollection<UnassignedInvoiceNo> UnassignedInvoiceNo { get; set; } = new List<UnassignedInvoiceNo>();
}
