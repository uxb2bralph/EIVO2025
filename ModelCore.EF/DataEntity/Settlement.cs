using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class Settlement
{
    public int SettlementID { get; set; }

    public DateTime SettlementDate { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndExclusiveDate { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public virtual ICollection<MonthlyBilling> MonthlyBilling { get; set; } = new List<MonthlyBilling>();
}
