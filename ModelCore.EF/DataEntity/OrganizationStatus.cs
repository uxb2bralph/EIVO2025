using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class OrganizationStatus
{
    public int CompanyID { get; set; }

    public int? CurrentLevel { get; set; }

    public DateTime? LastTimeToAcknowledge { get; set; }

    public int? RequestPeriodicalInterval { get; set; }

    public bool? SetToPrintInvoice { get; set; }

    public string? InvoicePrintView { get; set; }

    public bool? IronSteelIndustry { get; set; }

    public bool? Entrusting { get; set; }

    public string? AuthorizationNo { get; set; }

    public Guid? TokenID { get; set; }

    public bool? SetToOutsourcingCS { get; set; }

    public string? AllowancePrintView { get; set; }

    public bool? SetToNotifyCounterpartBySMS { get; set; }

    public bool? DownloadDataNumber { get; set; }

    public bool? DownloadDispatch { get; set; }

    public bool? UploadBranchTrackBlank { get; set; }

    public bool? PrintAll { get; set; }

    public int? SettingInvoiceType { get; set; }

    public bool? SubscribeB2BInvoicePDF { get; set; }

    public bool? UseB2BStandalone { get; set; }

    public bool? DisableIssuingNotice { get; set; }

    public bool? DisableWinningNotice { get; set; }

    public bool? EntrustToPrint { get; set; }

    public bool? EnableTrackCodeInvoiceNoValidation { get; set; }

    public bool? EnableBuyerIDValidation { get; set; }

    public int? InvoiceNoticeSetting { get; set; }

    public string? NotificationFooterView { get; set; }

    public bool? IgnoreDuplicatedDataNumber { get; set; }

    public int? InvoiceClientDefaultProcessType { get; set; }

    public string? CustomNotificationView { get; set; }

    public virtual Organization Company { get; set; } = null!;

    public virtual LevelExpression? CurrentLevelNavigation { get; set; }

    public virtual UserToken? Token { get; set; }
}
