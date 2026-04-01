using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 資料異動待回應檔
/// </summary>
public partial class WelfareReplication
{
    public int WelfareID { get; set; }

    public virtual InvoiceWelfareAgency Welfare { get; set; } = null!;
}
