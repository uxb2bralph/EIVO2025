using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceAllowanceItemExtension
{
    public int AllowanceID { get; set; }

    public string? ExtraRemark { get; set; }

    public virtual InvoiceAllowance Allowance { get; set; } = null!;
}
