using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class UserProfileProperty
{
    public int UID { get; set; }

    public int PropertyID { get; set; }

    public string? Property { get; set; }

    public virtual UserProfile UIDNavigation { get; set; } = null!;
}
