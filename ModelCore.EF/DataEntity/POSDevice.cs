using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class POSDevice
{
    public int DeviceID { get; set; }

    public string POSNo { get; set; } = null!;

    /// <summary>
    /// 主鍵
    /// </summary>
    public int CompanyID { get; set; }

    public virtual Organization Company { get; set; } = null!;

    public virtual ICollection<POSInvoiceNoSegment> POSInvoiceNoSegment { get; set; } = new List<POSInvoiceNoSegment>();
}
