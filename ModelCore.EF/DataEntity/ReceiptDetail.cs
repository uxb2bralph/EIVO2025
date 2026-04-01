using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class ReceiptDetail
{
    public int DetailID { get; set; }

    public int ReceiptID { get; set; }

    public string? Description { get; set; }

    public decimal? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? Amount { get; set; }

    public string? Remark { get; set; }

    public short? SequenceNO { get; set; }

    public virtual ReceiptItem Receipt { get; set; } = null!;
}
