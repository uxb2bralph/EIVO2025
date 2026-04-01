using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class MenuControl
{
    public int MenuID { get; set; }

    public string SiteMenu { get; set; } = null!;

    public virtual ICollection<UserMenu> UserMenu { get; set; } = new List<UserMenu>();
}
