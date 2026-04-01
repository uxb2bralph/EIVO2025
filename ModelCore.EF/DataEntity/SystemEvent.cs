using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class SystemEvent
{
    public int EventID { get; set; }

    public DateTime EventDate { get; set; }

    public string Subject { get; set; } = null!;

    public string? EventContent { get; set; }

    public string? ReferenceUrl { get; set; }

    public string? ReferenceTransformer { get; set; }

    public virtual ICollection<UserInbox> UserInbox { get; set; } = new List<UserInbox>();
}
