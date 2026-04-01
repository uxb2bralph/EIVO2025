using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DataProcessLog
{
    public int LogID { get; set; }

    public int DocID { get; set; }

    public DateTime LogDate { get; set; }

    public int Status { get; set; }

    public int? StepID { get; set; }

    public string? Content { get; set; }

    public int? ProcessType { get; set; }

    public int? NoticeID { get; set; }

    public virtual CDS_Document Doc { get; set; } = null!;

    public virtual DataNotice? Notice { get; set; }
}
