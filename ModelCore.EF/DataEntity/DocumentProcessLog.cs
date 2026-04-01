using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DocumentProcessLog
{
    public int DocID { get; set; }

    public DateTime StepDate { get; set; }

    public int FlowStep { get; set; }

    public int? UID { get; set; }

    public string? Description { get; set; }

    public virtual CDS_Document Doc { get; set; } = null!;

    public virtual DocumentReasonForRefusal? DocumentReasonForRefusal { get; set; }

    public virtual LevelExpression FlowStepNavigation { get; set; } = null!;

    public virtual UserProfile? UIDNavigation { get; set; }
}
