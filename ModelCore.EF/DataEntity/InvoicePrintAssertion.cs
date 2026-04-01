using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoicePrintAssertion
{
    public int InvoiceID { get; set; }

    public DateTime? PrintDate { get; set; }

    public virtual InvoiceItem Invoice { get; set; } = null!;
}
