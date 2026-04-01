using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class ProductItemCategory
{
    public int PICID { get; set; }

    public string ItemNo { get; set; } = null!;

    public string ItemName { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public decimal UnitePrice { get; set; }
}
