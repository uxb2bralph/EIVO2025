using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DocumentOwner
{
    public int DocID { get; set; }

    public int OwnerID { get; set; }

    public string? ClientID { get; set; }

    public virtual CDS_Document Doc { get; set; } = null!;

    public virtual Organization Owner { get; set; } = null!;
}
