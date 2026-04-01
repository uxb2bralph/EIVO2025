using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class ProcessRequestDocument
{
    public int? TaskID { get; set; }

    public int DocID { get; set; }

    public DateTime CreateDate { get; set; }

    public virtual CDS_Document Doc { get; set; } = null!;

    public virtual ProcessRequest? Task { get; set; }
}
