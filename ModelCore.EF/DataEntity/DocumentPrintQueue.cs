using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DocumentPrintQueue
{
    public int DocID { get; set; }

    public int? UID { get; set; }

    public DateTime? SubmitDate { get; set; }

    public long SubmitID { get; set; }

    public virtual CDS_Document Doc { get; set; } = null!;

    public virtual UserProfile? UIDNavigation { get; set; }
}
