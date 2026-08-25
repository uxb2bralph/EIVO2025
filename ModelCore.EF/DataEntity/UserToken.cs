using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class UserToken
{
    public Guid Token { get; set; }

    public int UID { get; set; }

    public DateTime LogonTime { get; set; }

    public string? X509Certificate { get; set; }

    public string? Thumbprint { get; set; }

    public string? PKCS12 { get; set; }

    public virtual ICollection<OrganizationStatus> OrganizationStatus { get; set; } = new List<OrganizationStatus>();

    public virtual UserProfile UserProfile { get; set; } = null!;
}
