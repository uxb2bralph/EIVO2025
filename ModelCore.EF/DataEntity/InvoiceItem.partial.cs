using System.ComponentModel.DataAnnotations.Schema;

namespace ModelCore.DataEntity;

public partial class InvoiceItem
{
    [NotMapped]
    public virtual CDS_Document CDS_Document
    {
        get => Invoice;
        set => Invoice = value;
    }
}
