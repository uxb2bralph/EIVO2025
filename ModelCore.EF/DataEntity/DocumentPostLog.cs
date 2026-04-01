using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DocumentPostLog
{
    public int LogID { get; set; }

    public int InvoiceID { get; set; }

    /// <summary>
    /// 郵局號
    /// </summary>
    public string PostCode { get; set; } = null!;

    /// <summary>
    /// 掛號號碼
    /// </summary>
    public string RegisterCode { get; set; } = null!;

    /// <summary>
    /// 郵遞區號
    /// </summary>
    public string ZipCode { get; set; } = null!;

    /// <summary>
    /// 郵件種類碼
    /// </summary>
    public string MailType { get; set; } = null!;

    /// <summary>
    /// 掛號檢查碼
    /// </summary>
    public string ChkCode { get; set; } = null!;

    public DateTime? CreateDate { get; set; }

    public virtual InvoiceItem Invoice { get; set; } = null!;
}
