using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DerivedDocument
{
    public int DocID { get; set; }

    public int SourceID { get; set; }

    public virtual CDS_Document Doc { get; set; } = null!;

    public virtual CDS_Document Source { get; set; } = null!;
}
