using System.ComponentModel.DataAnnotations.Schema;

namespace ModelCore.DataEntity;

public partial class CDS_Document
{
    [NotMapped]
    public virtual DerivedDocument? DerivedDocument
    {
        get => DerivedDocumentDoc;
        set => DerivedDocumentDoc = value;
    }

    [NotMapped]
    public virtual ICollection<DerivedDocument> ChildDocument { get => DerivedDocumentSource; set => DerivedDocumentSource = value; }

}
