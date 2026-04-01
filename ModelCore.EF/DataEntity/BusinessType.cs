using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class BusinessType
{
    public int BusinessID { get; set; }

    public string Business { get; set; } = null!;

    public virtual ICollection<BusinessRelationship> BusinessRelationship { get; set; } = new List<BusinessRelationship>();

    public virtual ICollection<DocumentTypeFlow> DocumentTypeFlow { get; set; } = new List<DocumentTypeFlow>();
}
