using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class CustomerDefined
{
    public int DocID { get; set; }

    public string? IsolationFolder { get; set; }

    public string? DataContent { get; set; }

    public virtual CDS_Document Doc { get; set; } = null!;
}
