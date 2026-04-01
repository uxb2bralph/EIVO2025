using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoicePurchaseOrderAudit
{
    public int SellerID { get; set; }

    public string OrderNo { get; set; } = null!;

    /// <summary>
    /// Primary Key
    /// </summary>
    public int? InvoiceID { get; set; }

    public virtual InvoiceItem? Invoice { get; set; }

    public virtual Organization Seller { get; set; } = null!;
}
