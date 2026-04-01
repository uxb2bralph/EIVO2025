using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class ExtraBillingItem
{
    public int ItemID { get; set; }

    /// <summary>
    /// 主鍵
    /// </summary>
    public int CompanyID { get; set; }

    public string? ItemName { get; set; }

    public int Fee { get; set; }

    public DateTime? BillingDate { get; set; }

    public int? BillingType { get; set; }

    public virtual Organization Company { get; set; } = null!;
}
