using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceMailTracking
{
    public int MailID { get; set; }

    public string? TrackingNo1 { get; set; }

    public string? TrackingNo2 { get; set; }

    public DateTime DeliveryDate { get; set; }

    public int DeliveryStatus { get; set; }

    public virtual LevelExpression DeliveryStatusNavigation { get; set; } = null!;

    public virtual ICollection<InvoiceMail> InvoiceMail { get; set; } = new List<InvoiceMail>();
}
