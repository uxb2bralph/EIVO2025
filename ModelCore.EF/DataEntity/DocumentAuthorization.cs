using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DocumentAuthorization
{
    public int DocID { get; set; }

    public virtual CDS_Document CDS_Document { get; set; } = null!;
}
