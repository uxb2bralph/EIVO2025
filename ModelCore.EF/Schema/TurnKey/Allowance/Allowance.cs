using System.Xml.Serialization;

namespace ModelCore.Schema.TurnKey.Allowance
{


    /// <remarks/>
    [System.SerializableAttribute()]
    public partial class Allowance
    {

        /// <remarks/>
        public Main? Main;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("ProductItem", IsNullable = false)]
        public DetailsProductItem[]? Details;

        /// <remarks/>
        public Amount? Amount;
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    public partial class Main
    {

        /// <remarks/>
        public string? AllowanceNumber;

        /// <remarks/>
        public string? AllowanceDate;

        /// <remarks/>
        public RoleDescription? Seller;

        /// <remarks/>
        public RoleDescription? Buyer;

        /// <remarks/>
        public AllowanceTypeEnum AllowanceType = AllowanceTypeEnum.Item2;

        /// <remarks/>
        public string? OriginalInvoiceSellerId;

        /// <remarks/>
        public string? OriginalInvoiceBuyerId;
    }

    /// <remarks/>

    [System.SerializableAttribute()]
    public partial class Amount
    {

        /// <remarks/>
        public decimal? TaxAmount;

        /// <remarks/>
        public decimal? TotalAmount;
    }

    /// <remarks/>

    [System.SerializableAttribute()]
    public enum AllowanceTypeEnum
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Item1 = 1,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Item2,
    }

    /// <remarks/>

    [System.SerializableAttribute()]
    public partial class DetailsProductItem
    {

        /// <remarks/>
        public string? OriginalInvoiceDate;

        /// <remarks/>
        public string? OriginalInvoiceNumber;

        /// <remarks/>
        public string? OriginalSequenceNumber;

        /// <remarks/>
        public string? OriginalDescription;

        /// <remarks/>
        public decimal Quantity;

        /// <remarks/>
        public string? Unit;

        /// <remarks/>
        public decimal UnitPrice;

        /// <remarks/>
        public decimal Amount;

        /// <remarks/>
        public decimal Tax;

        /// <remarks/>
        public string? AllowanceSequenceNumber;

        /// <remarks/>
        public TaxTypeEnum TaxType;
    }

}
