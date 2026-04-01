using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DocumentReasonForRefusal
{
    public int DocID { get; set; }

    public string? Reason { get; set; }

    public DateTime TimeToRefuse { get; set; }

    public virtual DocumentProcessLog DocumentProcessLog { get; set; } = null!;
}
