using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceWelfareAgency
{
    public int WelfareID { get; set; }

    public int SellerID { get; set; }

    public int AgencyID { get; set; }

    public DateTime? CreateTime { get; set; }

    public virtual WelfareAgency Agency { get; set; } = null!;

    public virtual Organization Seller { get; set; } = null!;

    public virtual WelfareReplication? WelfareReplication { get; set; }
}
