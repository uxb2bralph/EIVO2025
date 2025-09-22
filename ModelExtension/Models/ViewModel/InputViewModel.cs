using ModelCore.DataEntity;
using ModelCore.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;

namespace ModelCore.Models.ViewModel
{
    public class InputViewModel
    {
        public String? Name { get; set; }
        public String? Id { get; set; }
        public String? Label { get; set; }
        public String? Value { get; set; }
        public String? PlaceHolder { get; set; }
        public String? ErrorMessage { get; set; }
        public bool? IsValid { get; set; }
        public String? InputType { get; set; }
        public Object? DefaultValue { get; set; }
        public String? ButtonStyle { get; set; }
        public String? IconStyle { get; set; }
        public String? Href { get; set; }
    }

    public partial class MailTrackingCsvViewModel
    {
        public int? PackageID { get; set; }
        public String? MailNo1 { get; set; }
        public String? MailNo2 { get; set; }
        public String? MailNo3 { get; set; } //Amy-1121110-郵件種類碼
        public DateTime? DeliveryDate { get; set; }
        public int?[]? InvoiceID { get; set; }
    }

    public partial class TaxMediaQueryViewModel : QueryViewModel
    {
        public CategoryDefinition.CategoryEnum? BusinessBorder { get; set; } 
        public int? SellerID { get; set; }
        public String? TaxNo { get; set; }
        public int? Year { get; set; }
        public int? PeriodNo { get; set; }
        public bool? AdjustTax { get; set; }
    }

    public partial class DataQueryViewModel : QueryViewModel
    {
        public String? CommandText { get; set; }
    }

    public partial class SellerSelectorViewModel : CommonQueryViewModel
    {
        public bool? SelectAll { get; set; }
        public String? SelectorIndication { get; set; }
        public String? SelectorIndicationValue { get; set; }
        public String? JS_OnSelect { get; set; }
        public int? SellerID { get; set; }
        [JsonIgnore]
        public Organization? DefaultItem { get; set; }
    }

    public partial class ReviseInvoiceViewModel : InquireInvoiceViewModel
    {
        public Naming.VoidActionMode? Mode { get; set; }
        public ReviseInvoiceContent? ReviseContent { get; set; }
    }

    public partial class CounterpartBusinessSelectorViewModel : CommonQueryViewModel //11405 added by Amy
    {
        public string BuyerReceiptNo { get; set; }
        public string FieldName { get; set; }
        public string FieldID { get; set; }
        public object DefaultValue { get; set; }
        public bool? SelectAll { get; set; }
        public string SelectorIndication { get; set; }
        public string JS_OnSelect { get; set; }
        public int? BusinessID { get; set; }
    }

}