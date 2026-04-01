using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class UserMenu
{
    public int RoleID { get; set; }

    public int CategoryID { get; set; }

    public int MenuID { get; set; }

    public virtual CategoryDefinition Category { get; set; } = null!;

    public virtual MenuControl Menu { get; set; } = null!;

    public virtual UserRoleDefinition Role { get; set; } = null!;
}
