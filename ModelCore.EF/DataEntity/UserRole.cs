using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class UserRole
{
    public int UID { get; set; }

    public int RoleID { get; set; }

    public int OrgaCateID { get; set; }

    public virtual OrganizationCategory OrganizationCategory { get; set; } = null!;

    public virtual UserRoleDefinition UserRoleDefinition { get; set; } = null!;

    public virtual UserProfile UserProfile { get; set; } = null!;
}
