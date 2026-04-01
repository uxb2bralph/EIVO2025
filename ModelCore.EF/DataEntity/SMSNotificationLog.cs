using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class SMSNotificationLog
{
    public int LogID { get; set; }

    public int? DocID { get; set; }

    public int MessageID { get; set; }

    public DateTime? SubmitDate { get; set; }

    public int? OwnerID { get; set; }

    public string? SendingMobil { get; set; }

    public string? SendingContent { get; set; }

    public virtual CDS_Document? Doc { get; set; }

    public virtual MessageType Message { get; set; } = null!;

    public virtual Organization? Owner { get; set; }
}
