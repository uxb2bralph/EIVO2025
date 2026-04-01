using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceNoMainAssignment
{
    public int AssignmentID { get; set; }

    public int TrackID { get; set; }

    public int MasterID { get; set; }

    public int StartNo { get; set; }

    public int EndNo { get; set; }

    public virtual InvoiceTrackCodeAssignment InvoiceTrackCodeAssignment { get; set; } = null!;

    public virtual ICollection<InvoiceTrackCodeAssignment> InvoiceTrackCodeAssignmentNavigation { get; set; } = new List<InvoiceTrackCodeAssignment>();
}
