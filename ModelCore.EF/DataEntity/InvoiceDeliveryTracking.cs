using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceDeliveryTracking
{
    public int TrackingID { get; set; }

    /// <summary>
    /// Primary Key
    /// </summary>
    public int InvoiceID { get; set; }

    public string? TrackingNo1 { get; set; }

    public string? TrackingNo2 { get; set; }

    public DateTime DeliveryDate { get; set; }

    public int DeliveryStatus { get; set; }

    public virtual LevelExpression DeliveryStatusNavigation { get; set; } = null!;

    public virtual InvoiceItem Invoice { get; set; } = null!;
}
