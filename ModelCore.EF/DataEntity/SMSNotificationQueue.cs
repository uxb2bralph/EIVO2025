using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class SMSNotificationQueue
{
    public int DocID { get; set; }

    public int MessageID { get; set; }

    public DateTime? SubmitDate { get; set; }

    public virtual CDS_Document Doc { get; set; } = null!;

    public virtual MessageType Message { get; set; } = null!;
}
