using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class BillingIncrement
{
    /// <summary>
    /// 主鍵
    /// </summary>
    public int CompanyID { get; set; }

    public int UpperBound { get; set; }

    public decimal? UnitFee { get; set; }

    public virtual Organization Company { get; set; } = null!;
}
