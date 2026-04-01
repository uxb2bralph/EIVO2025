using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class VoidInvoiceRequest
{
    public int DocID { get; set; }

    public DateTime VoidDate { get; set; }

    public string? Reason { get; set; }

    public int? RequestType { get; set; }

    public string? InvoiceContent { get; set; }

    public DateTime? CommitDate { get; set; }

    public string? ReviseContent { get; set; }

    public string? InvoiceNo { get; set; }

    public virtual CDS_Document Doc { get; set; } = null!;
}
