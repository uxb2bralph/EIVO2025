using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class MessageType
{
    public int MessageID { get; set; }

    public string? Message { get; set; }

    public string? UIControl { get; set; }

    public string? MailControl { get; set; }

    public int? PageSize { get; set; }

    public string? DetailControl { get; set; }

    public virtual InboxItems? InboxItems { get; set; }

    public virtual ICollection<SMSNotificationLog> SMSNotificationLog { get; set; } = new List<SMSNotificationLog>();

    public virtual ICollection<SMSNotificationQueue> SMSNotificationQueue { get; set; } = new List<SMSNotificationQueue>();

    public virtual ICollection<UserInbox> UserInbox { get; set; } = new List<UserInbox>();
}
