using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class ReceiptCancellation
{
    public int ReceiptID { get; set; }

    public string CancellationNo { get; set; } = null!;

    public DateTime? CancelDate { get; set; }

    public string? Remark { get; set; }

    public virtual ReceiptItem Receipt { get; set; } = null!;
}
