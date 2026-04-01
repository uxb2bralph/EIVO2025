using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class UserRoleDefinition
{
    public int RoleID { get; set; }

    public string? SiteMenu { get; set; }

    public string Role { get; set; } = null!;

    public virtual ICollection<OrganizationCategoryUserRole> OrganizationCategoryUserRole { get; set; } = new List<OrganizationCategoryUserRole>();

    public virtual ICollection<UserInbox> UserInbox { get; set; } = new List<UserInbox>();

    public virtual ICollection<UserMenu> UserMenu { get; set; } = new List<UserMenu>();

    public virtual ICollection<UserRole> UserRole { get; set; } = new List<UserRole>();
}
