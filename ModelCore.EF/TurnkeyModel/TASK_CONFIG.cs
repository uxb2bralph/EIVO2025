using System;
using System.Collections.Generic;

namespace ModelCore.TurnkeyModel;

public partial class TASK_CONFIG
{
    public string CATEGORY_TYPE { get; set; } = null!;

    public string PROCESS_TYPE { get; set; } = null!;

    public string TASK { get; set; } = null!;

    public string? SRC_PATH { get; set; }

    public string? TARGET_PATH { get; set; }

    public string? FILE_FORMAT { get; set; }

    public string? VERSION { get; set; }

    public string? ENCODING { get; set; }

    public string? TRANS_CHINESE_DATE { get; set; }
}
