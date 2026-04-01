using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class UserRole
{
    public int UID { get; set; }

    public int RoleID { get; set; }

    public int OrgaCateID { get; set; }

    public virtual OrganizationCategory OrgaCate { get; set; } = null!;

    public virtual UserRoleDefinition Role { get; set; } = null!;

    public virtual UserProfile UIDNavigation { get; set; } = null!;
}
