using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class VacantInvoiceNo
{
    public int VacancyID { get; set; }

    public int IntervalID { get; set; }

    public int InvoiceNo { get; set; }

    public int? PrevID { get; set; }

    public int? NextID { get; set; }

    public virtual InvoiceNoInterval Interval { get; set; } = null!;

    public virtual ICollection<VacantInvoiceNo> InverseNext { get; set; } = new List<VacantInvoiceNo>();

    public virtual ICollection<VacantInvoiceNo> InversePrev { get; set; } = new List<VacantInvoiceNo>();

    public virtual VacantInvoiceNo? Next { get; set; }

    public virtual VacantInvoiceNo? Prev { get; set; }
}
