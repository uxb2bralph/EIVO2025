using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DocumentAuthorization
{
    public int DocID { get; set; }

    public virtual CDS_Document Doc { get; set; } = null!;
}
