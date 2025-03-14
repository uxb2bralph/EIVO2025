using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Http;
using ModelCore.DataEntity;
using ModelCore.Locale;
using Newtonsoft.Json;

namespace ModelCore.Models.ViewModel
{
    public class OrganizationViewModel : CommonQueryViewModel
    {
        public Naming.MemberStatusDefinition? OrganizationStatus { get; set; }

        public String? ContactName { get; set; }
        public String? Fax { get; set; }
        public String? LogoURL { get; set; }
        public String? CompanyName { get; set; }
        public int? CompanyID { get; set; }
        public String? ReceiptNo { get; set; }
        public String? Phone { get; set; }
        public String? ContactFax { get; set; }
        public String? ContactPhone { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? CreationDate { get; set; }
        public String? ContactMobilePhone { get; set; }
        public String? BusinessContactPhone { get; set; }

        public String? RegAddr { get; set; }
        public String? UndertakerName { get; set; }
        public String? Addr { get; set; }
        public String? EnglishName { get; set; }
        public String? EnglishAddr { get; set; }
        public String? EnglishRegAddr { get; set; }
        public String? ContactEmail { get; set; }
        public String? UndertakerPhone { get; set; }
        public String? UndertakerFax { get; set; }
        public String? UndertakerMobilePhone { get; set; }
        public String? InvoiceSignature { get; set; }
        public String? UndertakerID { get; set; }
        public String? ContactTitle { get; set; }
        public CategoryDefinition.CategoryEnum? CategoryID { get; set; } = CategoryDefinition.CategoryEnum.發票開立營業人;
        public int? CurrentLevel { get; set; }
        public DateTime? LastTimeToAcknowledge { get; set; }
        public int? RequestPeriodicalinterval { get; set; }
        public bool? SetToPrintInvoice { get; set; }
        public bool? AutoBlankTrack { get; set; }
        public bool? AutoBlankTrackEmittance { get; set; }

        public String? InvoicePrintView { get; set; }
        public String? C0401POSView { get; set; }
        public bool? IronSteelIndustry { get; set; }
        public bool? Entrusting { get; set; }
        public String? AuthorizationNo { get; set; }
        public Guid TokenID { get; set; }
        public bool? SetToOutsourcingCS { get; set; }
        public String? AllowancePrintView { get; set; }
        public bool? SetToNotifyCounterpartBySMS { get; set; }
        public bool? DownloadDataNumber { get; set; }
        public bool? DownloadDispatch { get; set; }
        public bool? UploadBranchTrackBlank { get; set; }
        public bool? PrintAll { get; set; }
        public Naming.InvoiceTypeDefinition? SettingInvoiceType { get; set; }
        public bool? SubscribeB2BInvoicePDF { get; set; }
        public bool? UseB2BStandalone { get; set; }
        public bool? DisableIssuingNotice { get; set; }
        public bool? EntrustToPrint { get; set; }
        public bool? EnableTrackCodeInvoiceNoValidation { get; set; }
        public Naming.InvoiceNoticeStatus? NoticeSetting { get; set; }
        public int[]? NoticeStatus { get; set; }
        public bool? IgnoreDuplicatedDataNumber { get; set; }
        public Naming.InvoiceProcessType? DefaultProcessType { get; set; }
        public String? CustomerNo { get; set; }
        public String? CustomNotification { get; set; }
        public String? CustomNotificationView { get; set; }
        public String[]? Settings { get; set; }

        [JsonIgnore]
        public bool SendAllowanceMIGManually => Settings != null && Settings.Contains("SendAllowanceMIGManually");
        [JsonIgnore]
        public bool HybridB2B => Settings != null && Settings.Contains("HybridB2B");
        [JsonIgnore]
        public bool InvoiceNotUploadedAlert => Settings != null && Settings.Contains("InvoiceNotUploadedAlert");
        [JsonIgnore]
        public bool DisableC0401Template => Settings != null && Settings.Contains("DisableC0401Template");
        [JsonIgnore]
        public bool InvoiceExchange => Settings != null && Settings.Contains("InvoiceExchange");
        [JsonIgnore]
        public bool IgnoreDuplicatedNo => Settings != null && Settings.Contains("IgnoreDuplicatedNo");
        public bool ForcedAuditNo => Settings != null && Settings.Contains("ForcedAuditNo");
        public int? InvoiceNoSafetyStock { get; set; }
        public int?[]? GradeCount { get; set; }
        public int?[]? BasicFee { get; set; }
        public int?[]? UpperBound { get; set; }
        public decimal?[]? UnitFee { get; set; }
        public Naming.DocumentTypeDefinition?[]? CalcType { get; set; }
        public String[]? ItemName { get; set; }
        public int?[]? Fee { get; set; }
        public DateTime?[]? BillingDate { get; set; }
        public ExtraBillingItem.BillingTypeEnum?[]? BillingType { get; set; }
        public int? SellerID { get; set; }
        public int? BillingCycle { get; set; }
        public String? TaxNo { get; set; }
        public DateTime? AuthorizationNotBefore { get; set; }
        public DateTime? AuthorizationNotAfter { get; set; }
        public DateTime? InvoiceRequestNotBefore { get; set; }
        public DateTime? InvoiceRequestNotAfter { get; set; }

    }

    public class OrganizationCertificateViewModel : OrganizationViewModel
    {
        public String? PIN { get; set; }
        public IFormFile? PfxFile { get; set; }
    }
}

namespace ModelCore.DataEntity
{
    public partial class CustomSmtpHost : ModelCore.Models.ViewModel.QueryViewModel
    {

    }
}