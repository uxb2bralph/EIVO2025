using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class UserProfile
{
    public int UID { get; set; }

    public string? UserName { get; set; }

    public string PID { get; set; } = null!;

    public string? Password { get; set; }

    public string? ContactTitle { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Region { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public string? MobilePhone { get; set; }

    public string? Phone { get; set; }

    public string? Phone2 { get; set; }

    public string? Fax { get; set; }

    public string? EMail { get; set; }

    public DateTime? Expiration { get; set; }

    public int? Creator { get; set; }

    public int? AuthID { get; set; }

    public int? LevelID { get; set; }

    public string? ThemeName { get; set; }

    public string? Password2 { get; set; }

    public string? MailID { get; set; }

    public virtual UserProfile? Auth { get; set; }

    public virtual UserProfile? CreatorNavigation { get; set; }

    public virtual ICollection<DocumentDownloadLog> DocumentDownloadLog { get; set; } = new List<DocumentDownloadLog>();

    public virtual ICollection<DocumentPrintLog> DocumentPrintLog { get; set; } = new List<DocumentPrintLog>();

    public virtual ICollection<DocumentPrintQueue> DocumentPrintQueue { get; set; } = new List<DocumentPrintQueue>();

    public virtual ICollection<DocumentProcessLog> DocumentProcessLog { get; set; } = new List<DocumentProcessLog>();

    public virtual ICollection<UserProfile> InverseAuth { get; set; } = new List<UserProfile>();

    public virtual ICollection<UserProfile> InverseCreatorNavigation { get; set; } = new List<UserProfile>();

    public virtual ICollection<InvoiceCancellationUpload> InvoiceCancellationUpload { get; set; } = new List<InvoiceCancellationUpload>();

    public virtual ICollection<InvoiceNoSegmentDisposition> InvoiceNoSegmentDisposition { get; set; } = new List<InvoiceNoSegmentDisposition>();

    public virtual ICollection<InvoicePrintQueue> InvoicePrintQueue { get; set; } = new List<InvoicePrintQueue>();

    public virtual ICollection<InvoicePurchaseOrderUpload> InvoicePurchaseOrderUpload { get; set; } = new List<InvoicePurchaseOrderUpload>();

    public virtual ICollection<InvoiceUserCarrier> InvoiceUserCarrier { get; set; } = new List<InvoiceUserCarrier>();

    public virtual LevelExpression? Level { get; set; }

    public virtual ICollection<ProcessRequest> ProcessRequest { get; set; } = new List<ProcessRequest>();

    public virtual ResetUserPassword? ResetUserPassword { get; set; }

    public virtual ICollection<UserAuth> UserAuth { get; set; } = new List<UserAuth>();

    public virtual ICollection<UserInbox> UserInbox { get; set; } = new List<UserInbox>();

    public virtual UserProfileExtension? UserProfileExtension { get; set; }

    public virtual ICollection<UserProfileProperty> UserProfileProperty { get; set; } = new List<UserProfileProperty>();

    public virtual UserProfileStatus? UserProfileStatus { get; set; }

    public virtual ICollection<UserRole> UserRole { get; set; } = new List<UserRole>();

    public virtual ICollection<UserToken> UserToken { get; set; } = new List<UserToken>();

    public virtual ICollection<OrganizationDepartment> Department { get; set; } = new List<OrganizationDepartment>();
}
