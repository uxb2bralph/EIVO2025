using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 發票歸戶關聯檔
/// </summary>
public partial class InvoiceByHousehold
{
    public int InvoiceID { get; set; }

    public int CarrierID { get; set; }

    public virtual InvoiceUserCarrier Carrier { get; set; } = null!;

    public virtual InvoiceItem Invoice { get; set; } = null!;
}
