using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceItemExtension
{
    /// <summary>
    /// Primary Key
    /// </summary>
    public int InvoiceID { get; set; }

    public string? ExtraRemark { get; set; }

    public string? ProjectNo { get; set; }

    public string? PurchaseNo { get; set; }

    /// <summary>
    /// 顯示印花稅圖章
    /// 0:不需要
    /// 1:需要
    /// 
    /// </summary>
    public byte? StampDutyFlag { get; set; }

    public virtual InvoiceItem Invoice { get; set; } = null!;
}
