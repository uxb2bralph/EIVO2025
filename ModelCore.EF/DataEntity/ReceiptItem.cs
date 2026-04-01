using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class ReceiptItem
{
    public int ReceiptID { get; set; }

    /// <summary>
    /// 發票號碼
    /// </summary>
    public string No { get; set; } = null!;

    /// <summary>
    /// 發票日期
    /// </summary>
    public DateTime ReceiptDate { get; set; }

    public int SellerID { get; set; }

    public int BuyerID { get; set; }

    /// <summary>
    /// 數量
    /// </summary>
    public decimal? TotalAmount { get; set; }

    public virtual Organization Buyer { get; set; } = null!;

    public virtual CDS_Document Receipt { get; set; } = null!;

    public virtual ReceiptCancellation? ReceiptCancellation { get; set; }

    public virtual ICollection<ReceiptDetail> ReceiptDetail { get; set; } = new List<ReceiptDetail>();

    public virtual Organization Seller { get; set; } = null!;
}
