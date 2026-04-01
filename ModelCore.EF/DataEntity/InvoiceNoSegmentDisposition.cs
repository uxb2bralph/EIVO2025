using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceNoSegmentDisposition
{
    public int SegmentID { get; set; }

    public int UID { get; set; }

    public int DepartmentID { get; set; }

    public virtual OrganizationDepartment Department { get; set; } = null!;

    public virtual InvoiceNoSegment Segment { get; set; } = null!;

    public virtual UserProfile UIDNavigation { get; set; } = null!;
}
