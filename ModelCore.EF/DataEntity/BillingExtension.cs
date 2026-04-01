using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class BillingExtension
{
    /// <summary>
    /// 主鍵
    /// </summary>
    public int CompanyID { get; set; }

    public int BillingCycleInMonth { get; set; }

    public virtual Organization Company { get; set; } = null!;
}
