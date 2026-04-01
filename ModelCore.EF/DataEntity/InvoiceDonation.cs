using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 註記發票捐贈
/// </summary>
public partial class InvoiceDonation
{
    /// <summary>
    /// Primary Key
    /// </summary>
    public int InvoiceID { get; set; }

    /// <summary>
    /// 機構代碼
    /// </summary>
    public string AgencyCode { get; set; } = null!;

    public virtual InvoiceItem Invoice { get; set; } = null!;
}
