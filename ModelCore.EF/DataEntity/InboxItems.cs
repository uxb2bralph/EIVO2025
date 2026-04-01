using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InboxItems
{
    public int MessageID { get; set; }

    public virtual MessageType Message { get; set; } = null!;
}
