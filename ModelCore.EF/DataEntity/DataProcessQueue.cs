using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DataProcessQueue
{
    public int DocID { get; set; }

    public int StepID { get; set; }

    public int ProcessType { get; set; }

    public DateTime DispatchDate { get; set; }

    public DateTime? BookingTime { get; set; }

    public int? NoticeID { get; set; }

    public virtual CDS_Document Doc { get; set; } = null!;

    public virtual DataNotice? Notice { get; set; }
}
