using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class ProcessRequestCondition
{
    public int TaskID { get; set; }

    public int ConditionID { get; set; }

    public virtual ProcessRequest Task { get; set; } = null!;
}
