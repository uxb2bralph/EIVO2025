using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 註記提示發票載具
/// </summary>
public partial class InvoiceCarrier
{
    /// <summary>
    /// Primary Key
    /// </summary>
    public int InvoiceID { get; set; }

    /// <summary>
    /// 載具類別
    /// 1：悠遊卡
    /// 2：UXB2B條碼卡
    /// </summary>
    public string CarrierType { get; set; } = null!;

    /// <summary>
    /// 載具卡號
    /// </summary>
    public string? CarrierNo { get; set; }

    public string? CarrierNo2 { get; set; }

    public virtual InvoiceItem Invoice { get; set; } = null!;
}
