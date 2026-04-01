using System;
using System.Collections.Generic;

namespace ModelCore.TurnkeyModel;

public partial class TURNKEY_MESSAGE_LOG_DETAIL
{
    public string SEQNO { get; set; } = null!;

    public string SUBSEQNO { get; set; } = null!;

    public string? PROCESS_DTS { get; set; }

    public string TASK { get; set; } = null!;

    public string? STATUS { get; set; }

    public string? FILENAME { get; set; }

    public string? UUID { get; set; }
}
