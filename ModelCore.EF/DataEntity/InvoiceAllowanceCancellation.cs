using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 作廢折讓主檔
/// </summary>
public partial class InvoiceAllowanceCancellation
{
    public int AllowanceID { get; set; }

    /// <summary>
    /// 作廢日期
    /// </summary>
    public DateTime? CancelDate { get; set; }

    /// <summary>
    /// 作廢折讓備註
    /// 作廢折讓時必填，填寫作廢原因
    /// </summary>
    public string? Remark { get; set; }

    public string? CancelReason { get; set; }

    public virtual InvoiceAllowance Allowance { get; set; } = null!;
}
