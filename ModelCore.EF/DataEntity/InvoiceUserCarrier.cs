using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceUserCarrier
{
    public int CarrierID { get; set; }

    public int? UID { get; set; }

    public int TypeID { get; set; }

    public string? CarrierNo { get; set; }

    public string CarrierNo2 { get; set; } = null!;

    public bool? DeleteFlag { get; set; }

    public int? CodeID { get; set; }

    public virtual MemberCode? Code { get; set; }

    public virtual ICollection<InvoiceByHousehold> InvoiceByHousehold { get; set; } = new List<InvoiceByHousehold>();

    public virtual InvoiceUserCarrierType Type { get; set; } = null!;

    public virtual UserProfile? UIDNavigation { get; set; }
}
