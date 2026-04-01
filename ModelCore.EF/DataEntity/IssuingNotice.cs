using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class IssuingNotice
{
    public int DocID { get; set; }

    public DateTime? IssueDate { get; set; }

    public virtual CDS_Document Doc { get; set; } = null!;
}
