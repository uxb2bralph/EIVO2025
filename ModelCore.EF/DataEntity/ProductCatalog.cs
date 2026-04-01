using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class ProductCatalog
{
    public int ProductID { get; set; }

    public string? Barcode { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal SalePrice { get; set; }

    public decimal? PurchasePrice { get; set; }

    public string? Spec { get; set; }

    public string? Remark { get; set; }

    public string? PieceUnit { get; set; }

    public virtual ICollection<Organization> Supplier { get; set; } = new List<Organization>();
}
