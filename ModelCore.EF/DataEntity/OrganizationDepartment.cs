using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class OrganizationDepartment
{
    public int DepartmentID { get; set; }

    public string? Department { get; set; }

    public int CompanyID { get; set; }

    public virtual Organization Company { get; set; } = null!;

    public virtual ICollection<InvoiceNoSegmentDisposition> InvoiceNoSegmentDisposition { get; set; } = new List<InvoiceNoSegmentDisposition>();

    public virtual ICollection<UserProfile> UID { get; set; } = new List<UserProfile>();
}
