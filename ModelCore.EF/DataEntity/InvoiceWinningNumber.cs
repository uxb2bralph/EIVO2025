using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 發票中獎號碼主檔
/// </summary>
public partial class InvoiceWinningNumber
{
    public int InvoiceID { get; set; }

    public int? WinningID { get; set; }

    public DateTime? DownloadDate { get; set; }

    public string? PrizeType { get; set; }

    public int? Bonus { get; set; }

    public virtual InvoiceItem Invoice { get; set; } = null!;

    public virtual UniformInvoiceWinningNumber? Winning { get; set; }
}
