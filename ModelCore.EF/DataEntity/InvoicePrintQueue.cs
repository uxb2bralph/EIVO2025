using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoicePrintQueue
{
    public int InvoiceID { get; set; }

    public int? UID { get; set; }

    public DateTime? SubmitDate { get; set; }

    public virtual InvoiceItem Invoice { get; set; } = null!;

    public virtual UserProfile? UIDNavigation { get; set; }
}
