using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DocumentFlowBranch
{
    public int StepID { get; set; }

    public int BranchStep { get; set; }

    public string? BranchName { get; set; }

    public int FlowID { get; set; }

    public virtual DocumentFlowControl BranchStepNavigation { get; set; } = null!;

    public virtual DocumentFlow Flow { get; set; } = null!;

    public virtual DocumentFlowControl Step { get; set; } = null!;
}
