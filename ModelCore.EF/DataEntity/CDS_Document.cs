using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 系統文件主檔
/// </summary>
public partial class CDS_Document
{
    public int DocID { get; set; }

    public int? DocType { get; set; }

    /// <summary>
    /// 文件建立時間
    /// </summary>
    public DateTime DocDate { get; set; }

    public int? CurrentStep { get; set; }

    public int? ChannelID { get; set; }

    public int? ProcessType { get; set; }

    public virtual ICollection<Attachment> Attachment { get; set; } = new List<Attachment>();

    public virtual LevelExpression? CurrentStepNavigation { get; set; }

    public virtual CustomerDefined? CustomerDefined { get; set; }

    public virtual ICollection<DataProcessLog> DataProcessLog { get; set; } = new List<DataProcessLog>();

    public virtual ICollection<DataProcessQueue> DataProcessQueue { get; set; } = new List<DataProcessQueue>();

    public virtual DerivedDocument? DerivedDocumentDoc { get; set; }

    public virtual ICollection<DerivedDocument> DerivedDocumentSource { get; set; } = new List<DerivedDocument>();

    public virtual DocumentType? DocTypeNavigation { get; set; }

    public virtual DocumentAuthorization? DocumentAuthorization { get; set; }

    public virtual ICollection<DocumentDownloadLog> DocumentDownloadLog { get; set; } = new List<DocumentDownloadLog>();

    public virtual DocumentDownloadQueue? DocumentDownloadQueue { get; set; }

    public virtual DocumentFlowStep? DocumentFlowStep { get; set; }

    public virtual DocumentMappingQueue? DocumentMappingQueue { get; set; }

    public virtual DocumentOwner? DocumentOwner { get; set; }

    public virtual ICollection<DocumentPrintLog> DocumentPrintLog { get; set; } = new List<DocumentPrintLog>();

    public virtual DocumentPrintQueue? DocumentPrintQueue { get; set; }

    public virtual ICollection<DocumentProcessLog> DocumentProcessLog { get; set; } = new List<DocumentProcessLog>();

    public virtual ICollection<DocumentReplication> DocumentReplication { get; set; } = new List<DocumentReplication>();

    public virtual ICollection<DocumentReturn> DocumentReturn { get; set; } = new List<DocumentReturn>();

    public virtual DocumentSubscriptionQueue? DocumentSubscriptionQueue { get; set; }

    public virtual ICollection<ExceptionLog> ExceptionLog { get; set; } = new List<ExceptionLog>();

    public virtual InvoiceAllowance? InvoiceAllowance { get; set; }

    public virtual InvoiceItem? InvoiceItem { get; set; }

    public virtual IssuingNotice? IssuingNotice { get; set; }

    public virtual ProcessRequestDocument? ProcessRequestDocument { get; set; }

    public virtual ReceiptItem? ReceiptItem { get; set; }

    public virtual ICollection<SMSNotificationLog> SMSNotificationLog { get; set; } = new List<SMSNotificationLog>();

    public virtual ICollection<SMSNotificationQueue> SMSNotificationQueue { get; set; } = new List<SMSNotificationQueue>();

    public virtual ICollection<UserInbox> UserInbox { get; set; } = new List<UserInbox>();

    public virtual VoidInvoiceRequest? VoidInvoiceRequest { get; set; }

    public virtual ICollection<DocumentType> Type { get; set; } = new List<DocumentType>();
}
