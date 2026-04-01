using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DocumentSubscriptionQueue
{
    public int DocID { get; set; }

    public int? Status { get; set; }

    public DateTime? WaitUntil { get; set; }

    public virtual CDS_Document Doc { get; set; } = null!;
}
