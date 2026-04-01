using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoicePaperRequest
{
    public int InvoiceID { get; set; }

    public string Token { get; set; } = null!;

    public DateTime? RequestDate { get; set; }

    public virtual InvoiceItem Invoice { get; set; } = null!;
}
