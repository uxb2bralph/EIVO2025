using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 發票折讓主檔
/// </summary>
public partial class InvoiceAllowance
{
    public int AllowanceID { get; set; }

    /// <summary>
    /// 折讓證明單號碼
    /// </summary>
    public string? AllowanceNumber { get; set; }

    /// <summary>
    /// 折讓種類
    /// 1:買方開立折讓證明單
    /// 2:賣方折讓證明單通知
    /// 
    /// </summary>
    public byte? AllowanceType { get; set; }

    /// <summary>
    /// 折讓證明單日期
    /// </summary>
    public DateTime? AllowanceDate { get; set; }

    /// <summary>
    /// 金額(不含稅之進貨額)合計
    /// </summary>
    public decimal? TotalAmount { get; set; }

    /// <summary>
    /// 營業稅額合計
    /// </summary>
    public decimal? TaxAmount { get; set; }

    public int? InvoiceID { get; set; }

    public string? SellerId { get; set; }

    public string? BuyerId { get; set; }

    public int? CurrencyID { get; set; }

    public DateTime? IssueDate { get; set; }

    public virtual CDS_Document Allowance { get; set; } = null!;

    public virtual CurrencyType? Currency { get; set; }

    public virtual InvoiceItem? Invoice { get; set; }

    public virtual InvoiceAllowanceBuyer? InvoiceAllowanceBuyer { get; set; }

    public virtual InvoiceAllowanceCancellation? InvoiceAllowanceCancellation { get; set; }

    public virtual InvoiceAllowanceItemExtension? InvoiceAllowanceItemExtension { get; set; }

    public virtual InvoiceAllowanceSeller? InvoiceAllowanceSeller { get; set; }

    public virtual ICollection<InvoiceAllowanceItem> Item { get; set; } = new List<InvoiceAllowanceItem>();
}
