using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class UserProfileExtension
{
    public int UID { get; set; }

    public string? IDNo { get; set; }

    public string? Sex { get; set; }

    public DateTime? Birthday { get; set; }

    public string? NightPhone { get; set; }

    public string? TwoFactorKey { get; set; }

    public virtual UserProfile UIDNavigation { get; set; } = null!;
}
