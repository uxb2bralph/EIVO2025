using System;
using System.Collections.Generic;

namespace ModelCore.TurnkeyModel;

public partial class V_Invoice
{
    public string SEQNO { get; set; } = null!;

    public string SUBSEQNO { get; set; } = null!;

    public string? STATUS { get; set; }

    public string? DocType { get; set; }

    public string? TrackCode { get; set; }

    public string? No { get; set; }

    public string? InvoiceNo { get; set; }

    public DateTime? InvoiceDate { get; set; }

    public DateTime? MESSAGE_DTS { get; set; }
}
