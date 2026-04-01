using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DocumentFlowControl
{
    public int StepID { get; set; }

    public int? NextStep { get; set; }

    public int? PrevStep { get; set; }

    public int LevelID { get; set; }

    public int FlowID { get; set; }

    public virtual ICollection<DocumentFlow> DocumentFlow { get; set; } = new List<DocumentFlow>();

    public virtual ICollection<DocumentFlowBranch> DocumentFlowBranchBranchStepNavigation { get; set; } = new List<DocumentFlowBranch>();

    public virtual ICollection<DocumentFlowBranch> DocumentFlowBranchStep { get; set; } = new List<DocumentFlowBranch>();

    public virtual ICollection<DocumentFlowStep> DocumentFlowStep { get; set; } = new List<DocumentFlowStep>();

    public virtual DocumentFlow Flow { get; set; } = null!;

    public virtual ICollection<DocumentFlowControl> InverseNextStepNavigation { get; set; } = new List<DocumentFlowControl>();

    public virtual ICollection<DocumentFlowControl> InversePrevStepNavigation { get; set; } = new List<DocumentFlowControl>();

    public virtual LevelExpression Level { get; set; } = null!;

    public virtual DocumentFlowControl? NextStepNavigation { get; set; }

    public virtual DocumentFlowControl? PrevStepNavigation { get; set; }
}
