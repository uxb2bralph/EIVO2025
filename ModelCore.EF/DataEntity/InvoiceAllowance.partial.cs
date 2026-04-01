using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelCore.DataEntity;

public partial class InvoiceAllowance
{

    [NotMapped]
    public virtual ICollection<InvoiceAllowanceItem> InvoiceAllowanceDetails
    {
        get => Item;
        set => Item = value;
    }

    [NotMapped]
    public virtual CDS_Document CDS_Document
    {
        get => Allowance;
        set => Allowance = value;
    }
}
