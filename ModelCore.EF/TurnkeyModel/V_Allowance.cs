using System;
using System.Collections.Generic;

namespace ModelCore.TurnkeyModel;

public partial class V_Allowance
{
    public string SEQNO { get; set; } = null!;

    public string SUBSEQNO { get; set; } = null!;

    public string? STATUS { get; set; }

    public string? DocType { get; set; }

    public string? AllowanceNo { get; set; }

    public DateTime? AllowanceDate { get; set; }

    public DateTime? MESSAGE_DTS { get; set; }
}
