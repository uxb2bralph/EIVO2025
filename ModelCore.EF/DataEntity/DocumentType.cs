using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 文件定對檔
/// </summary>
public partial class DocumentType
{
    /// <summary>
    /// 主鍵
    /// </summary>
    public int TypeID { get; set; }

    /// <summary>
    /// 文件名稱
    /// </summary>
    public string? TypeName { get; set; }

    public virtual ICollection<CDS_Document> CDS_Document { get; set; } = new List<CDS_Document>();

    public virtual ICollection<DocumentDownloadLog> DocumentDownloadLog { get; set; } = new List<DocumentDownloadLog>();

    public virtual ICollection<DocumentPrintLog> DocumentPrintLog { get; set; } = new List<DocumentPrintLog>();

    public virtual ICollection<DocumentReplication> DocumentReplication { get; set; } = new List<DocumentReplication>();

    public virtual ICollection<DocumentReturn> DocumentReturn { get; set; } = new List<DocumentReturn>();

    public virtual ICollection<DocumentTypeFlow> DocumentTypeFlow { get; set; } = new List<DocumentTypeFlow>();

    public virtual ICollection<ExceptionLog> ExceptionLog { get; set; } = new List<ExceptionLog>();

    public virtual ICollection<Organization> Company { get; set; } = new List<Organization>();

    public virtual ICollection<CDS_Document> Doc { get; set; } = new List<CDS_Document>();
}
