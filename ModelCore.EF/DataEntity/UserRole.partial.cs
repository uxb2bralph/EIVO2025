using System.ComponentModel.DataAnnotations.Schema;

namespace ModelCore.DataEntity;

public partial class UserRole
{
    [NotMapped]
    public virtual UserProfile UserProfile {
        get => UIDNavigation;
        set => UIDNavigation = value;
    }

    [NotMapped]
    public virtual OrganizationCategory OrganizationCategory
    {
        get => OrgaCate;
        set => OrgaCate = value;
    }

    [NotMapped]
    public virtual UserRoleDefinition UserRoleDefinition
    {
        get => Role;
        set => Role = value;
    }
}
