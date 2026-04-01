using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class EnterpriseGroupMemberToken
{
    public int EnterpriseID { get; set; }

    public int CompanyID { get; set; }

    public string X509Certificate { get; set; } = null!;

    public string Thumbprint { get; set; } = null!;

    public string? PKCS12 { get; set; }

    public Guid? KeyID { get; set; }

    public virtual EnterpriseGroupMember EnterpriseGroupMember { get; set; } = null!;
}
