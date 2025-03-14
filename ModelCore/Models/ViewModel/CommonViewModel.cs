using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc.Razor;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.Locale;
using ModelCore.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ModelCore.Models.ViewModel
{
    public partial class InquireInvoiceViewModel : CommonQueryViewModel
    {
        public int? CompanyID { get; set; }
        public String? DataNo { get; set; }
        public String? Consumption { get; set; }
        public String? BuyerReceiptNo { get; set; }
        public String? BuyerName { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool? IsWinning { get; set; }
        public int? Winning { get; set; }
        public bool? Cancelled { get; set; }
        public bool? IsAttached { get; set; }
        public bool? QueryAtStart { get; set; }
        public int? SellerID { get; set; }
        public bool? IsNoticed { get; set; }
        public String? CarrierType { get; set; }
        public String? CarrierNo { get; set; }
        public String? PrintMark { get; set; }
        public int? InvoiceID { get; set; }
        public String? InvoiceNo { get; set; }
        public String? EndNo { get; set; }
        public String? AgencyCode { get; set; }
        public String? CustomerID { get; set; }
        public Naming.InvoiceProcessType? ProcessType { get; set; }
        public int? Attachment { get; set; }
        [JsonIgnore]
        public DateTime? InvoiceDateFrom
        {
            get => DateFrom;
            set => DateFrom = value;
        }

        [JsonIgnore]
        public DateTime? AllowanceDateFrom
        {
            get => DateFrom;
            set => DateFrom = value;
        }

        [JsonIgnore]
        public DateTime? InvoiceDateTo
        {
            get => DateTo;
            set => DateTo = value;
        }

        [JsonIgnore]
        public DateTime? AllowanceDateTo
        {
            get => DateTo;
            set => DateTo = value;
        }

        public DateTime? InvoiceDate { get; set; }
        public Naming.DocumentTypeDefinition? DocType { get; set; }
        public String? RandomNo { get; set; }
        [JsonIgnore]
        public String? AllowanceNo
        {
            get => DataNo;
            set => DataNo = value;
        }

        [JsonIgnore]
        public bool? CurrencySummary { get; set; }
        public String? ReceiptNo { get; set; }
        public String? Status { get; set; }
        [JsonIgnore]
        public bool? Printed { get; set; }
        [JsonIgnore]
        public bool? HasAddr { get; set; }
        [JsonIgnore]
        public bool? MIG { get; set; }
        public string? Period { get; set; }
        public string? Donation { get; set; }
    }

    public enum DataQueryType
    {
        Invoice = 1,        //發票
        VoidInvoice = 2,    //作廢發票
        Allowance = 3,      //折讓單
        VoidAllowance = 4,  //作廢折讓單
        InovoiceNoAllocation = 5,  //發票配號狀態
        CountInvoice = 11,        //發票資料筆數
        CountVoidInvoice = 12,    //作廢發票資料筆數
        CountAllowance = 13,      //折讓單資料筆數
        CountVoidAllowance = 14,  //作廢折讓單資料筆數

    }


    public partial class InvoiceDataQueryViewModel : InquireInvoiceViewModel
    {
        public DataQueryType? QueryType { get; set; }
        public int? Year { get; set; }
        public int? PeriodNo { get; set; }
        public String? IssuerNo { get; set; }
    }

    public partial class InquireNoIntervalViewModel
    {
        public int? Year { get; set; }
        public int? PeriodNo { get; set; }
        public int? SellerID { get; set; }
        public String? SelectIndication { get; set; }
        public bool? BranchRelation { get; set; }
        public bool? WriteToMIG { get; set; }
    }

    public partial class BusinessRelationshipViewModel : OrganizationViewModel
    {
        public int? CompanyStatus { get; set; }
        public int? BusinessType { get; set; }
        public int? MasterID
        {
            get => CompanyID;
            set => CompanyID = value;
        }
        public int? RelativeID { get; set; }
        public Naming.InvoiceCenterBusinessType? BusinessID
        {
            get => (Naming.InvoiceCenterBusinessType?)BusinessType;
            set => BusinessType = (int?)value;
        }
        public String? MasterNo { get; set; }
        public String? MasterName { get; set; }
        public string? Encoding { get; set; }
    }

    public partial class UserRoleViewModel : CommonQueryViewModel
    {
        public int? UID { get; set; }
        [JsonIgnore]
        public int? SellerID { get; set; }
        public String? EncSellerID
        {
            get => SellerID.HasValue ? SellerID.Value.EncryptKey() : null;
            set => SellerID = (value != null ? value.DecryptKeyValue() : (int?)null);
        }
        public Naming.RoleID? RoleID { get; set; }

    }

    public partial class UserProfileViewModel : UserRoleViewModel
    {
        //[Required(ErrorMessage = "Please enter {0}")]
        //[Display(Name = "PID")]
        //[EmailAddress]
        public String? PID { get; set; }
        public String? UserName { get; set; }

        //[Required(ErrorMessage = "Please enter {0}")]
        //[DataType(DataType.Password)]
        //[Display(Name = "Password")]
        public String? Password { get; set; }
        public String? Password1 { get; set; }
        public String? EMail { get; set; }
        public String? Address { get; set; }
        public String? Phone { get; set; }
        public String? MobilePhone { get; set; }
        public String? Phone2 { get; set; }
        public bool? WaitForCheck { get; set; }
        public Guid? ResetID { get; set; }
        public bool? ResetPass { get; set; }
    }

    public partial class InvoiceNoIntervalViewModel : CommonQueryViewModel
    {
        public int? IntervalID { get; set; }
        public int? TrackID { get; set; }
        public int? SellerID { get; set; }
        public int? StartNo { get; set; }
        public int? EndNo { get; set; }
        public int? Parts { get; set; }
        public int? LockID { get; set; }
        public String? DeviceName { get; set; }
    }

    public class UploadInvoiceTrackCodeModel : InvoiceNoIntervalViewModel
    {
        public String? ReceiptNo { get; set; }
        public short? Year { get; set; }
        public int? PeriodNo { get; set; }
        public String? TrackCode { get; set; }
    }

    public partial class AuthQueryViewModel : QueryViewModel
    {
        public int? AgentID { get; set; }
        public String? AccessToken
        {
            get => KeyID;
            set => KeyID = value;
        }

    }

    public partial class CommonQueryViewModel : AuthQueryViewModel
    {
        public String? QueryAction { get; set; }
        public String? Title { get; set; }
        public String? FieldName { get; set; }
    }

    public partial class UserAccountQueryViewModel : UserProfileViewModel
    {
        public int? LevelID { get; set; }
    }

    public partial class TrackCodeQueryViewModel : CommonQueryViewModel
    {
        public int? Year { get; set; }
        public int? PeriodNo { get; set; }
    }

    public partial class ExchangeRateQueryViewModel : TrackCodeQueryViewModel
    {
        public String? Currency { get; set; }
        public int? PeriodID { get; set; }
        public int? CurrencyID { get; set; }
        public decimal? ExchangeRate { get; set; }
    }

    public partial class TrackCodeViewModel
    {
        public int? TrackID { get; set; }
        public int? id
        {
            get => TrackID;
            set => TrackID = value;
        }
        public short? Year { get; set; }
        public short? PeriodNo { get; set; }
        public String? TrackCode { get; set; }
        public Naming.InvoiceTypeDefinition? InvoiceType { get; set; }

    }

    public partial class WinningNumberViewModel
    {
        public int? WinningID { get; set; }
        public int? Year { get; set; }
        public int? Period { get; set; }
        public int? Rank { get; set; }
        public String? WinningNo { get; set; }
    }

    public enum FilterType
    {
        Exclude = 0,
        Include = 1,
    }
    public partial class MailTrackingViewModel : InquireInvoiceViewModel
    {
        public String? StartNo { get; set; }
        public int? DeliveryStatus { get; set; }
        public int? UserType { get; set; }  //Amy-1121115
        public Naming.ChannelIDType? ChannelID { get; set; }
        public FilterType? DateFilter { get; set; }
        public String? ExclusiveReceiptNo { get; set; }
    }

    public class QueryViewModel
    {
        public int? PageSize { get; set; } = AppSettings.Default.PageSize;
        public int? PageIndex { get; set; }
        public int? PageOffset { get; set; } = 0;
        public string[]? SortName { get; set; }
        public int?[]? SortType { get; set; }
        public bool? Paging { get; set; }
        public String? KeyID { get; set; }
        [JsonIgnore]
        public String? Data { get => KeyID; set => KeyID = value; } 
        public String? FileDownloadToken { get; set; }
        public int?[]? Sort { get; set; }
        public String? DialogID { get; set; }
        public Naming.DataResultMode? ResultMode { get; set; }
        public int? RecordCount { get; set; }
        public String? OnPageCallScript { get; set; }
        public bool? ForTest { get; set; }
        public String? QueryForm { get; set; }
        public bool? StartQuery { get; set; }
        public bool? Encrypt { get; set; }
        public String? QuickSearch { get; set; }
        public Naming.FieldDisplayType? DisplayType { get; set; }
        public String[]? KeyItems { get; set; }
        public String? Message { get; set; }
        public String? UrlAction { get; set; }
        public String? ActionTitle { get; set; }
        [JsonIgnore]
        public bool? InitQuery { get; set; }
        [JsonIgnore]
        public String? ResultView { get; set; }
        public String? EncResultView
        {
            get => ResultView?.EncryptData();
            set
            {
                if (value != null)
                    ResultView = value.DecryptData();
            }
        }
        [JsonIgnore]
        public String? QueryResult { get; set; }
        public String? EncQueryResult
        {
            get => QueryResult?.EncryptData();
            set
            {
                if (value != null)
                    QueryResult = value.DecryptData();
            }
        }
        public String? AlertMessage { get; set; }
        public String? EncodedAlertMessage
        {
            get => AlertMessage != null ? Convert.ToBase64String(Encoding.Default.GetBytes(AlertMessage)) : null;
            set
            {
                if (value != null)
                {
                    AlertMessage = Encoding.Default.GetString(Convert.FromBase64String(value));
                }
            }
        }

        public String? AlertTitle { get; set; }
        [JsonIgnore]
        public String? EmptyQueryResult { get; set; }
        [JsonIgnore]
        public String? PartialView { get; set; }
        public bool? RowNumber { get; set; }
        public bool? GroupingQuery { get; set; }
        [JsonIgnore]
        public String? DeleteAction { get; set; }
        [JsonIgnore]
        public String? LoadAction { get; set; }
        [JsonIgnore]
        public String? EditAction { get; set; }
        [JsonIgnore]
        public String? CommitAction { get; set; }
        [JsonIgnore]
        public String? DownloadAction { get; set; }
        [JsonIgnore]
        public String? ResultAction { get; set; }

        public String? EmptyKeyID { get; set; }
        public int[]? ChkItem { get; set; }
        public bool? ForceTodo { get; set; }
        [JsonIgnore]
        public List<QueryResultDataColumnHelper>? DataColumns { get; set; }
    }

    public class QueryResultDataColumnHelper
    {
        public Func<object, HelperResult>? ShowHeader { get; set; }
        public Func<object, HelperResult>? ShowBody { get; set; }
        public Func<object, HelperResult>? ShowFooter { get; set; }
    }

    public class BusinessRelationshipQueryViewModel : BusinessRelationshipViewModel
    {
        public String? BranchNo { get; set; }
    }

    public class DocumentQueryViewModel : ProcessRequestViewModel
    {
        [JsonIgnore]
        public int? id
        {
            get
            {
                return DocID;
            }

            set
            {
                DocID = value;
            }
        }
        public int? DocID { get; set; }
        public String? Reason { get; set; }
        public bool? NameOnly { get; set; }
        public bool? AppendAttachment { get; set; }
        public String? MailTo { get; set; }
        [JsonIgnore]
        public int? LogID { get => DocID; set => DocID = value; }
    }

    public class ExceptionLogQueryViewModel : QueryViewModel
    {
        public int? CompanyID { get; set; }
        public int? MaxLogID { get; set; }
    }

    public class RenderStyleViewModel : DocumentQueryViewModel
    {
        public bool? PrintBack { get; set; }
        public bool? PrintCuttingLine { get; set; }
        public String? PaperStyle { get; set; }
        public bool? PrintBuyerAddr { get; set; }
        public bool? UseCustomView { get; set; }
        public bool? CreateNew { get; set; }

        [JsonIgnore]
        public bool? ForMailingPackage { get; set; }
        public decimal? Zoom { get; set; }
        public bool? IsPDF { get; set; }
        public String? HeaderView { get; set; }
    }

    public class ProcessRequestViewModel : AuthQueryViewModel
    {
        public int? Sender { get; set; }
        public int? TaskID { get; set; }
        public Naming.InvoiceProcessType? ProcessType { get; set; }
        public String? Comment { get; set; }
        public ProcessModelDefinition? ProcessModel { get; set; }
        public enum ProcessModelDefinition
        {
            ByTask = 1,
        }
    }
    public class InvoiceRequestViewModel : ProcessRequestViewModel
    {
        public ProcessRequestCondition.ConditionType?[]? ConditionID { get; set; }
        public String? ClientID { get; set; }
        public Schema.EIVO.InvoiceRoot? InvoiceRoot { get; set; }
        public Schema.EIVO.CancelInvoiceRoot? CancelInvoiceRoot { get; set; }
        public Schema.EIVO.AllowanceRoot? AllowanceRoot { get; set; }
        public Schema.EIVO.CancelAllowanceRoot? CancelAllowanceRoot { get; set; }
        public String? StoragePath { get; set; }
        public DateTime? ApplyInvoiceDate { get; set; }
        public String[]? DataNo { get; set; }
    }

    public class MIGResponseViewModel : AuthQueryViewModel
    {
        public Naming.InvoiceProcessType? ProcessType { get; set; }
        public int[]? LastReceivedKey { get; set; }
        public MIGContent[]? Items { get; set; }
    }

    public class MIGContent
    {
        public int? DocID { get; set; }
        public DateTime? DocDate { get; set; }
        public String? No { get; set; }
        public String? ReceiptNo { get; set; }
        public String? MIG { get; set; }
    }

    public class ActionResultViewModel
    {
        public bool? Result { get; set; }
        public String? Message { get; set; }
        public String[]? ErrorCode { get; set; }
    }

    public class ProcessRequestQueryViewModel : QueryViewModel
    {
        public int? TaskID { get; set; }
        public DateTime? SubmitDateFrom { get; set; }
        public DateTime? SubmitDateTo { get; set; }
        public DateTime? ProcessStartFrom { get; set; }
        public DateTime? ProcessStartTo { get; set; }
        public DateTime? ProcessCompleteFrom { get; set; }
        public DateTime? ProcessCompleteTo { get; set; }
    }

    public class ProductCatalogQueryViewModel : QueryViewModel
    {
        public int? ProductID { get; set; }
        public String? Barcode { get; set; }
        public String? ProductName { get; set; }
        public String? Spec { get; set; }
        public String? PieceUnit { get; set; }
        public String? Remark { get; set; }
        public decimal? SalePrice { get; set; }
        public int? SupplierID { get; set; }
    }

    public partial class AttachmentViewModel : QueryViewModel
    {
        public String? KeyName { get; set; }
        public int? DocID { get; set; }
        public int? TaskID { get; set; }
        public String? StoredPath { get; set; }
        public String? FileName { get; set; }
        public String? ButtonField { get; set; }
        public String? GetFormData { get; set; }
        public String? FileDownloadName { get; set; }
        public String? ContentType { get; set; }
        public bool? IsAsync { get; set; }
    }

    public partial class MonthlyReportQueryViewModel : ProcessRequestViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? SellerID { get; set; }
        [JsonIgnore]
        public DateTime? InvoiceDateFrom
        {
            get => DateFrom;
            set => DateFrom = value;
        }

        [JsonIgnore]
        public DateTime? InvoiceDateTo
        {
            get => DateTo;
            set => DateTo = value;
        }
    }

    public partial class OrganizationQueryViewModel : QueryViewModel
    {
        public String? ReceiptNo { get; set; }
        public Naming.MemberStatusDefinition? OrganizationStatus { get; set; }
        public String? CompanyName { get; set; }
        public CategoryDefinition.CategoryEnum? CategoryID { get; set; }
        public int? AgentID { get; set; }
        public bool? BranchRelation { get; set; }
        public OrganizationCustomSettingsModel? CustomSettings { get; set; }

    }

    public class MailMessageViewModel : QueryViewModel
    {
        public int? CompanyID { get; set; }
        public String? MailTo { get; set; }
        public String? Subject { get; set; }
        public String? MailMessage { get; set; }
    }

    public partial class DataTableQueryViewModel : QueryViewModel
    {
        public String? TableName { get; set; }
        public DataTableColumn[]? KeyItem { get; set; }
        public DataTableColumn[]? DataItem { get; set; }
        //public KeyValuePair<String,Object>[] DataItem { get; set; }
    }

    public partial class DataTableColumn
    {
        public String? Name { get; set; }
        public String? Value { get; set; }
    }


}