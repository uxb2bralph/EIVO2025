using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 作廢發票主檔
/// </summary>
public partial class InvoiceCancellation
{
    /// <summary>
    /// 作廢發票號碼
    /// </summary>
    public string? CancellationNo { get; set; }

    /// <summary>
    /// 作廢日期
    /// </summary>
    public DateTime? CancelDate { get; set; }

    public string? CancelReason { get; set; }

    /// <summary>
    /// 專案作廢核准文號
    /// 若發票的作廢時間超過申報期間，則此欄位為必填欄位。若不填寫由上傳營業人自行負責。
    /// </summary>
    public string? ReturnTaxDocumentNo { get; set; }

    /// <summary>
    /// 作廢備註
    /// 作廢發票時必填，填寫作廢原因
    /// </summary>
    public string? Remark { get; set; }

    public int InvoiceID { get; set; }

    public virtual InvoiceItem Invoice { get; set; } = null!;

    public virtual ICollection<InvoiceCancellationUpload> Upload { get; set; } = new List<InvoiceCancellationUpload>();
}
