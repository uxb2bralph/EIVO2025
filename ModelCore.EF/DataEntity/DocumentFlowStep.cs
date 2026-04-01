using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DocumentFlowStep
{
    public int DocID { get; set; }

    public int CurrentFlowStep { get; set; }

    public virtual DocumentFlowControl CurrentFlowStepNavigation { get; set; } = null!;

    public virtual CDS_Document Doc { get; set; } = null!;
}
