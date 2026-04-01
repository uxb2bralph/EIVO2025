using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class UniformInvoiceWinningNumber
{
    public int WinningID { get; set; }

    public int Year { get; set; }

    public int Period { get; set; }

    public int Rank { get; set; }

    public string WinningNO { get; set; } = null!;

    public string? PrizeType { get; set; }

    public int? Bonus { get; set; }

    public virtual ICollection<InvoiceWinningNumber> InvoiceWinningNumber { get; set; } = new List<InvoiceWinningNumber>();
}
