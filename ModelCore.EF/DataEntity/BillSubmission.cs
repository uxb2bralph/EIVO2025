using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class BillSubmission
{
    public int BillID { get; set; }

    public DateTime BillDate { get; set; }

    public virtual ICollection<MonthlyBilling> MonthlyBilling { get; set; } = new List<MonthlyBilling>();
}
