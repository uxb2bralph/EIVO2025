using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoicePrizeWinningNumbers
{
    public int BNID { get; set; }

    public int Year { get; set; }

    public int StartMonth { get; set; }

    public int EndMonth { get; set; }

    public string? SpecialPrize { get; set; }

    public string? GrandPrize { get; set; }

    public string FirstPrize { get; set; } = null!;

    public string? AdditionalSixthPrize { get; set; }

    public string? Memo { get; set; }
}
