using System.ComponentModel.DataAnnotations.Schema;

namespace ModelCore.DataEntity;

public partial class DerivedDocument
{
    [NotMapped]
    public virtual CDS_Document CDS_Document
    {
        get => Doc;
        set => Doc = value;
    }

    [NotMapped]
    public virtual CDS_Document ParentDocument
    {
        get => Source;
        set => Source = value;
    }
}
