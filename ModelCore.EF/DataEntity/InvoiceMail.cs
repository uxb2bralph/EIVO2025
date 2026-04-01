using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceMail
{
    /// <summary>
    /// Primary Key
    /// </summary>
    public int InvoiceID { get; set; }

    public int MailID { get; set; }

    public virtual InvoiceItem Invoice { get; set; } = null!;

    public virtual InvoiceMailTracking Mail { get; set; } = null!;
}
