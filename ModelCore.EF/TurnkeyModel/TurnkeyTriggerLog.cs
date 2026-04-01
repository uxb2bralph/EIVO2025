using System;
using System.Collections.Generic;

namespace ModelCore.TurnkeyModel;

public partial class TurnkeyTriggerLog
{
    public string SEQNO { get; set; } = null!;

    public string SUBSEQNO { get; set; } = null!;

    public string? UUID { get; set; }

    public string? MESSAGE_TYPE { get; set; }

    public string? CATEGORY_TYPE { get; set; }

    public string? PROCESS_TYPE { get; set; }

    public string? FROM_PARTY_ID { get; set; }

    public string? TO_PARTY_ID { get; set; }

    public string? MESSAGE_DTS { get; set; }

    public string? CHARACTER_COUNT { get; set; }

    public string? STATUS { get; set; }

    public string? IN_OUT_BOUND { get; set; }

    public string? FROM_ROUTING_ID { get; set; }

    public string? TO_ROUTING_ID { get; set; }

    public string? INVOICE_IDENTIFIER { get; set; }

    public int? LockID { get; set; }

    public int LogID { get; set; }
}
