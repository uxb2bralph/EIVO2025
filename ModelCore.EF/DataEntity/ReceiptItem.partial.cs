using System.ComponentModel.DataAnnotations.Schema;

namespace ModelCore.DataEntity;

public partial class ReceiptItem
{
    [NotMapped]
    public virtual CDS_Document CDS_Document
    {
        get => Receipt;
        set => Receipt = value;
    }
}
