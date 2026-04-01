using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class OrganizationToken
{
    public int CompanyID { get; set; }

    public string X509Certificate { get; set; } = null!;

    public string Thumbprint { get; set; } = null!;

    public string? PKCS12 { get; set; }

    public Guid? KeyID { get; set; }

    public bool? IsActivated { get; set; }

    public virtual Organization Company { get; set; } = null!;
}
