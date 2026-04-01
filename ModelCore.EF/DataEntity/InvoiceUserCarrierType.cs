using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceUserCarrierType
{
    public int TypeID { get; set; }

    public string CarrierType { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<InvoiceUserCarrier> InvoiceUserCarrier { get; set; } = new List<InvoiceUserCarrier>();
}
