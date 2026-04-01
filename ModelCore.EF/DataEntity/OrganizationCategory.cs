using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class OrganizationCategory
{
    public int OrgaCateID { get; set; }

    public int CompanyID { get; set; }

    public int CategoryID { get; set; }

    public virtual CategoryDefinition Category { get; set; } = null!;

    public virtual Organization Company { get; set; } = null!;

    public virtual ICollection<OrganizationCategoryUserRole> OrganizationCategoryUserRole { get; set; } = new List<OrganizationCategoryUserRole>();

    public virtual ICollection<UserInbox> UserInbox { get; set; } = new List<UserInbox>();

    public virtual ICollection<UserRole> UserRole { get; set; } = new List<UserRole>();
}
