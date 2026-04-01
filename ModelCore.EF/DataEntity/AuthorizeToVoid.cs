using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class AuthorizeToVoid
{
    /// <summary>
    /// Primary Key
    /// </summary>
    public int InvoiceID { get; set; }

    public int? VoidMode { get; set; }

    public virtual InvoiceItem Invoice { get; set; } = null!;
}
