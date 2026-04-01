using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class ResetUserPassword
{
    public int UID { get; set; }

    public Guid ResetID { get; set; }

    public virtual UserProfile UIDNavigation { get; set; } = null!;
}
