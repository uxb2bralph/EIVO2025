using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class MonthlyBilling
{
    public int SettlementID { get; set; }

    /// <summary>
    /// 主鍵
    /// </summary>
    public int CompanyID { get; set; }

    public int TotalIssueCount { get; set; }

    public int IssueChargeAmount { get; set; }

    public int? BillID { get; set; }

    public virtual BillSubmission? Bill { get; set; }

    public virtual Organization Company { get; set; } = null!;

    public virtual ICollection<MonthlyExtraBilling> MonthlyExtraBilling { get; set; } = new List<MonthlyExtraBilling>();

    public virtual Settlement Settlement { get; set; } = null!;
}
