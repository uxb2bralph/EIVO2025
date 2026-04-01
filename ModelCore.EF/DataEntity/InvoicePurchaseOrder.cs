using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoicePurchaseOrder
{
    public int InvoiceID { get; set; }

    public int? UploadID { get; set; }

    public string? OrderNo { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public virtual InvoiceItem Invoice { get; set; } = null!;

    public virtual InvoicePurchaseOrderUpload? Upload { get; set; }
}
