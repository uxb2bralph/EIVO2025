using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class EnterpriseGroupMember
{
    public int EnterpriseID { get; set; }

    public int CompanyID { get; set; }

    public virtual Organization Company { get; set; } = null!;

    public virtual EnterpriseGroup Enterprise { get; set; } = null!;

    public virtual EnterpriseGroupMemberToken? EnterpriseGroupMemberToken { get; set; }
}
