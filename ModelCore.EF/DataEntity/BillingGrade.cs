using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class BillingGrade
{
    /// <summary>
    /// 主鍵
    /// </summary>
    public int CompanyID { get; set; }

    public int GradeCount { get; set; }

    public int BasicFee { get; set; }

    public virtual Organization Company { get; set; } = null!;
}
