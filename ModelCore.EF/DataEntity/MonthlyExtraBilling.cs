using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class MonthlyExtraBilling
{
    public int SettlementID { get; set; }

    public int ItemID { get; set; }

    /// <summary>
    /// 主鍵
    /// </summary>
    public int CompanyID { get; set; }

    public string? ItemName { get; set; }

    public int Fee { get; set; }

    public virtual MonthlyBilling MonthlyBilling { get; set; } = null!;
}
