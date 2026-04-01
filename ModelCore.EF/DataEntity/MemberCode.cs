using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class MemberCode
{
    public int? CompanyID { get; set; }

    public DateTime? CreateTime { get; set; }

    public Guid UUID { get; set; }

    public string HashID { get; set; } = null!;

    public int CodeID { get; set; }

    public virtual Organization? Company { get; set; }

    public virtual ICollection<InvoiceUserCarrier> InvoiceUserCarrier { get; set; } = new List<InvoiceUserCarrier>();
}
