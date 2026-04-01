using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceTrackCode
{
    public int TrackID { get; set; }

    public string TrackCode { get; set; } = null!;

    public short Year { get; set; }

    public short PeriodNo { get; set; }

    /// <summary>
    /// 發票類別
    /// 1: 三聯式;
    /// 2: 二聯式;
    /// 3: 二聯式收銀機;
    /// 4. 特種稅額;
    /// 5: 電子計算機;
    /// 6: 三聯式收銀機
    /// 
    /// </summary>
    public byte? InvoiceType { get; set; }

    public int? PeriodID { get; set; }

    public virtual ICollection<InvoiceItem> InvoiceItem { get; set; } = new List<InvoiceItem>();

    public virtual ICollection<InvoiceTrackCodeAssignment> InvoiceTrackCodeAssignment { get; set; } = new List<InvoiceTrackCodeAssignment>();

    public virtual InvoicePeriod? Period { get; set; }
}
