using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceProduct
{
    public int ProductID { get; set; }

    public string? Brief { get; set; }

    public virtual ICollection<InvoiceProductItem> InvoiceProductItem { get; set; } = new List<InvoiceProductItem>();

    public virtual ICollection<InvoiceItem> Invoice { get; set; } = new List<InvoiceItem>();
}
