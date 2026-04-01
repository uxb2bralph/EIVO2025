using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class EnterpriseGroup
{
    public int EnterpriseID { get; set; }

    public string EnterpriseName { get; set; } = null!;

    public virtual ICollection<EnterpriseGroupMember> EnterpriseGroupMember { get; set; } = new List<EnterpriseGroupMember>();
}
