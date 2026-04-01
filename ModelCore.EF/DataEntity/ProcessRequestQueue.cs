using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class ProcessRequestQueue
{
    public int TaskID { get; set; }

    public int? ActorID { get; set; }

    public DateTime? BookingTime { get; set; }

    public virtual ProcessorUnit? Actor { get; set; }

    public virtual ProcessRequest Task { get; set; } = null!;
}
