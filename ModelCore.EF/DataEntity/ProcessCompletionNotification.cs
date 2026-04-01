using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class ProcessCompletionNotification
{
    public int TaskID { get; set; }

    public virtual ProcessRequest Task { get; set; } = null!;
}
