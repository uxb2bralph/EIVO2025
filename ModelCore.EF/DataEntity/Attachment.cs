using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class Attachment
{
    public string KeyName { get; set; } = null!;

    public string StoredPath { get; set; } = null!;

    public int? DocID { get; set; }

    public virtual CDS_Document? Doc { get; set; }
}
