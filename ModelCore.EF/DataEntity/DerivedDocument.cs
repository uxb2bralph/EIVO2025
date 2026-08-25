using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DerivedDocument
{
    public int DocID { get; set; }

    public int SourceID { get; set; }

    public virtual CDS_Document CDS_Document { get; set; } = null!;

    public virtual CDS_Document ParentDocument { get; set; } = null!;
}
