using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 社福機構
/// </summary>
public partial class WelfareAgency
{
    /// <summary>
    /// 主鍵
    /// </summary>
    public int AgencyID { get; set; }

    /// <summary>
    /// 機構代碼
    /// </summary>
    public string? AgencyCode { get; set; }

    public virtual Organization Agency { get; set; } = null!;

    public virtual ICollection<InvoiceWelfareAgency> InvoiceWelfareAgency { get; set; } = new List<InvoiceWelfareAgency>();
}
