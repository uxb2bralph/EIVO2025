using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DocumentFlow
{
    public int FlowID { get; set; }

    public string? FlowName { get; set; }

    public int? InitialStep { get; set; }

    public virtual ICollection<DocumentFlowBranch> DocumentFlowBranch { get; set; } = new List<DocumentFlowBranch>();

    public virtual ICollection<DocumentFlowControl> DocumentFlowControl { get; set; } = new List<DocumentFlowControl>();

    public virtual ICollection<DocumentTypeFlow> DocumentTypeFlow { get; set; } = new List<DocumentTypeFlow>();

    public virtual DocumentFlowControl? InitialStepNavigation { get; set; }
}
