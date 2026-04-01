using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoicePurchaseOrderUpload
{
    public int UploadID { get; set; }

    public string FilePath { get; set; } = null!;

    public DateTime? UploadDate { get; set; }

    public int? UID { get; set; }

    public virtual ICollection<InvoicePurchaseOrder> InvoicePurchaseOrder { get; set; } = new List<InvoicePurchaseOrder>();

    public virtual UserProfile? UIDNavigation { get; set; }
}
