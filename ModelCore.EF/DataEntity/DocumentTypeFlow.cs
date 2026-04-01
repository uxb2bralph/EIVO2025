using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DocumentTypeFlow
{
    public int TypeID { get; set; }

    public int FlowID { get; set; }

    public int CompanyID { get; set; }

    public int BusinessID { get; set; }

    public virtual BusinessType Business { get; set; } = null!;

    public virtual Organization Company { get; set; } = null!;

    public virtual DocumentFlow Flow { get; set; } = null!;

    public virtual DocumentType Type { get; set; } = null!;
}
