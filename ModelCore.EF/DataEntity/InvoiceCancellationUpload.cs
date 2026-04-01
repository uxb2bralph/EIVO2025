using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceCancellationUpload
{
    public int UploadID { get; set; }

    public string FilePath { get; set; } = null!;

    public DateTime? UploadDate { get; set; }

    public int? UID { get; set; }

    public virtual UserProfile? UIDNavigation { get; set; }

    public virtual ICollection<InvoiceCancellation> Invoice { get; set; } = new List<InvoiceCancellation>();
}
