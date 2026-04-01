using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DocumentDownloadLog
{
    public int LogID { get; set; }

    public int DocID { get; set; }

    public int TypeID { get; set; }

    public DateTime DownloadDate { get; set; }

    public int UID { get; set; }

    public virtual CDS_Document Doc { get; set; } = null!;

    public virtual DocumentType Type { get; set; } = null!;

    public virtual UserProfile UIDNavigation { get; set; } = null!;
}
