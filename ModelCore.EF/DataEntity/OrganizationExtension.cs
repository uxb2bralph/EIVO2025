using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class OrganizationExtension
{
    /// <summary>
    /// 主鍵
    /// </summary>
    public int CompanyID { get; set; }

    public string? CustomerNo { get; set; }

    public string? TaxNo { get; set; }

    public string? CustomNotification { get; set; }

    public string? BusinessContactPhone { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public DateTime? GoLiveDate { get; set; }

    public bool? AutoBlankTrack { get; set; }

    public bool? AutoBlankTrackEmittance { get; set; }

    public DateTime? CreationDate { get; set; }

    public int? InvoiceNoSafetyStock { get; set; }

    public string? MailSubjectAlias { get; set; }

    public bool? AutoTrackCodeAssignment { get; set; }

    public bool? AutoPartsTrackCodeAssignment { get; set; }

    public int? ReservedBooklets { get; set; }

    public DateTime? AuthorizationNotBefore { get; set; }

    public DateTime? AuthorizationNotAfter { get; set; }

    public DateTime? InvoiceRequestNotBefore { get; set; }

    public DateTime? InvoiceRequestNotAfter { get; set; }

    public virtual Organization Company { get; set; } = null!;
}
