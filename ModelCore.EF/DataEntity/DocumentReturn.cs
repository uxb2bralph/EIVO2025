using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DocumentReturn
{
    public int DocID { get; set; }

    public int TypeID { get; set; }

    public DateTime? ActionDate { get; set; }

    public virtual CDS_Document Doc { get; set; } = null!;

    public virtual DocumentType Type { get; set; } = null!;
}
