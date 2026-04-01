using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class UserAuth
{
    public int AuthID { get; set; }

    public int UID { get; set; }

    public string Thumbprint { get; set; } = null!;

    public string X509Certificate { get; set; } = null!;

    public virtual UserProfile UIDNavigation { get; set; } = null!;
}
