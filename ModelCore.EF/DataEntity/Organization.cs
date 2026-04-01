using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 機關、公司、組織單位
/// </summary>
public partial class Organization
{
    /// <summary>
    /// 連絡人
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// 傳真
    /// </summary>
    public string? Fax { get; set; }

    public string? LogoURL { get; set; }

    /// <summary>
    /// 機關名稱
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// 主鍵
    /// </summary>
    public int CompanyID { get; set; }

    /// <summary>
    /// 統一編號
    /// </summary>
    public string? ReceiptNo { get; set; }

    /// <summary>
    /// 電話
    /// </summary>
    public string? Phone { get; set; }

    public string? ContactFax { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactMobilePhone { get; set; }

    public string? RegAddr { get; set; }

    /// <summary>
    /// 負責人姓名
    /// </summary>
    public string? UndertakerName { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Addr { get; set; }

    public string? EnglishName { get; set; }

    public string? EnglishAddr { get; set; }

    public string? EnglishRegAddr { get; set; }

    /// <summary>
    /// 連絡人電子郵件
    /// </summary>
    public string? ContactEmail { get; set; }

    public string? UndertakerPhone { get; set; }

    public string? UndertakerFax { get; set; }

    public string? UndertakerMobilePhone { get; set; }

    public string? InvoiceSignature { get; set; }

    public string? UndertakerID { get; set; }

    /// <summary>
    /// 連絡人職稱
    /// </summary>
    public string? ContactTitle { get; set; }

    public virtual BillingExtension? BillingExtension { get; set; }

    public virtual ICollection<BillingGrade> BillingGrade { get; set; } = new List<BillingGrade>();

    public virtual ICollection<BillingIncrement> BillingIncrement { get; set; } = new List<BillingIncrement>();

    public virtual ICollection<BusinessRelationship> BusinessRelationshipMaster { get; set; } = new List<BusinessRelationship>();

    public virtual ICollection<BusinessRelationship> BusinessRelationshipRelative { get; set; } = new List<BusinessRelationship>();

    public virtual ICollection<CustomSmtpHost> CustomSmtpHost { get; set; } = new List<CustomSmtpHost>();

    public virtual ICollection<DocumentOwner> DocumentOwner { get; set; } = new List<DocumentOwner>();

    public virtual ICollection<DocumentTypeFlow> DocumentTypeFlow { get; set; } = new List<DocumentTypeFlow>();

    public virtual ICollection<EnterpriseGroupMember> EnterpriseGroupMember { get; set; } = new List<EnterpriseGroupMember>();

    public virtual ICollection<ExceptionLog> ExceptionLog { get; set; } = new List<ExceptionLog>();

    public virtual ICollection<ExtraBillingItem> ExtraBillingItem { get; set; } = new List<ExtraBillingItem>();

    public virtual ICollection<InvoiceAllowanceBuyer> InvoiceAllowanceBuyer { get; set; } = new List<InvoiceAllowanceBuyer>();

    public virtual ICollection<InvoiceAllowanceSeller> InvoiceAllowanceSeller { get; set; } = new List<InvoiceAllowanceSeller>();

    public virtual ICollection<InvoiceBusiness> InvoiceBusinessBuyer { get; set; } = new List<InvoiceBusiness>();

    public virtual ICollection<InvoiceBusiness> InvoiceBusinessSeller { get; set; } = new List<InvoiceBusiness>();

    public virtual ICollection<InvoiceBuyer> InvoiceBuyer { get; set; } = new List<InvoiceBuyer>();

    public virtual ICollection<InvoiceIssuerAgent> InvoiceIssuerAgentAgent { get; set; } = new List<InvoiceIssuerAgent>();

    public virtual ICollection<InvoiceIssuerAgent> InvoiceIssuerAgentIssuer { get; set; } = new List<InvoiceIssuerAgent>();

    public virtual ICollection<InvoiceItem> InvoiceItemDonation { get; set; } = new List<InvoiceItem>();

    public virtual ICollection<InvoiceItem> InvoiceItemSeller { get; set; } = new List<InvoiceItem>();

    public virtual ICollection<InvoicePurchaseOrderAudit> InvoicePurchaseOrderAudit { get; set; } = new List<InvoicePurchaseOrderAudit>();

    public virtual ICollection<InvoiceSeller> InvoiceSeller { get; set; } = new List<InvoiceSeller>();

    public virtual ICollection<InvoiceTrackCodeAssignment> InvoiceTrackCodeAssignment { get; set; } = new List<InvoiceTrackCodeAssignment>();

    public virtual ICollection<InvoiceWelfareAgency> InvoiceWelfareAgency { get; set; } = new List<InvoiceWelfareAgency>();

    public virtual MasterOrganization? MasterOrganization { get; set; }

    public virtual ICollection<MemberCode> MemberCode { get; set; } = new List<MemberCode>();

    public virtual ICollection<MonthlyBilling> MonthlyBilling { get; set; } = new List<MonthlyBilling>();

    public virtual ICollection<OrganizationBranch> OrganizationBranch { get; set; } = new List<OrganizationBranch>();

    public virtual ICollection<OrganizationCategory> OrganizationCategory { get; set; } = new List<OrganizationCategory>();

    public virtual OrganizationCustomSetting? OrganizationCustomSetting { get; set; }

    public virtual ICollection<OrganizationDepartment> OrganizationDepartment { get; set; } = new List<OrganizationDepartment>();

    public virtual OrganizationExtension? OrganizationExtension { get; set; }

    public virtual ICollection<OrganizationSettings> OrganizationSettings { get; set; } = new List<OrganizationSettings>();

    public virtual OrganizationStatus? OrganizationStatus { get; set; }

    public virtual OrganizationToken? OrganizationToken { get; set; }

    public virtual ICollection<POSDevice> POSDevice { get; set; } = new List<POSDevice>();

    public virtual ICollection<ProcessExceptionNotification> ProcessExceptionNotification { get; set; } = new List<ProcessExceptionNotification>();

    public virtual ICollection<ProcessRequest> ProcessRequest { get; set; } = new List<ProcessRequest>();

    public virtual ICollection<ReceiptItem> ReceiptItemBuyer { get; set; } = new List<ReceiptItem>();

    public virtual ICollection<ReceiptItem> ReceiptItemSeller { get; set; } = new List<ReceiptItem>();

    public virtual ICollection<SMSNotificationLog> SMSNotificationLog { get; set; } = new List<SMSNotificationLog>();

    public virtual WelfareAgency? WelfareAgency { get; set; }

    public virtual ICollection<MasterOrganization> Master { get; set; } = new List<MasterOrganization>();

    public virtual ICollection<ProductCatalog> Product { get; set; } = new List<ProductCatalog>();

    public virtual ICollection<DocumentType> Type { get; set; } = new List<DocumentType>();
}
