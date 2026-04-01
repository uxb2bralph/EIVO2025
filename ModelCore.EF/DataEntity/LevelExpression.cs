using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class LevelExpression
{
    public int LevelID { get; set; }

    public string Expression { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<BusinessRelationship> BusinessRelationship { get; set; } = new List<BusinessRelationship>();

    public virtual ICollection<CDS_Document> CDS_Document { get; set; } = new List<CDS_Document>();

    public virtual ICollection<DocumentFlowControl> DocumentFlowControl { get; set; } = new List<DocumentFlowControl>();

    public virtual ICollection<DocumentProcessLog> DocumentProcessLog { get; set; } = new List<DocumentProcessLog>();

    public virtual ICollection<InvoiceDeliveryTracking> InvoiceDeliveryTracking { get; set; } = new List<InvoiceDeliveryTracking>();

    public virtual ICollection<InvoiceMailTracking> InvoiceMailTracking { get; set; } = new List<InvoiceMailTracking>();

    public virtual ICollection<OrganizationStatus> OrganizationStatus { get; set; } = new List<OrganizationStatus>();

    public virtual ICollection<UserProfile> UserProfile { get; set; } = new List<UserProfile>();

    public virtual ICollection<UserProfileStatus> UserProfileStatus { get; set; } = new List<UserProfileStatus>();
}
