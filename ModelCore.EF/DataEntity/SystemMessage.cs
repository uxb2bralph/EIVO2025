using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class SystemMessage
{
    public int MsgID { get; set; }

    public string MessageContents { get; set; } = null!;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool AlwaysShow { get; set; }

    public DateTime? UpdateTime { get; set; }

    public DateTime CreateTime { get; set; }
}
