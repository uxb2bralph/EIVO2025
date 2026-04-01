using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class OrganizationCategoryUserRole
{
    public int OrgaCateID { get; set; }

    public int RoleID { get; set; }

    public string? MainMenu { get; set; }

    public virtual OrganizationCategory OrgaCate { get; set; } = null!;

    public virtual UserRoleDefinition Role { get; set; } = null!;
}
