using System;
using System.Collections.Generic;

namespace ModelCore.TurnkeyModel;

public partial class SCHEDULE_CONFIG
{
    public string TASK { get; set; } = null!;

    public string? ENABLE { get; set; }

    public string? SCHEDULE_TYPE { get; set; }

    public string? SCHEDULE_WEEK { get; set; }

    public string? SCHEDULE_TIME { get; set; }

    public string? SCHEDULE_PERIOD { get; set; }

    public string? SCHEDULE_RANGE { get; set; }
}
