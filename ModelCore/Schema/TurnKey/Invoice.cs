using System;
using System.Globalization;
using System.Xml.Serialization;

namespace ModelCore.Schema.TurnKey
{
    [System.SerializableAttribute()]
    public partial class Invoice
    {
        public Main? Main { get; set; }

        [System.Xml.Serialization.XmlArrayItemAttribute("ProductItem", IsNullable = false)] 
        public DetailsProductItem[]? Details { get; set; }
        
        public Amount? Amount { get; set; }
        public string? TxnCode { get; set; }
    }
    
    public partial class Main
    {
        [XmlIgnore]
        public DateTime InvoiceDateTime { get; set; }
        
        public string? InvoiceNumber { get; set; }
        
        public string InvoiceDate 
        { 
            get => InvoiceDateTime.ToString("yyyyMMdd");
            set => InvoiceDateTime = DateTime.ParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture).Add(InvoiceDateTime.TimeOfDay);  
        }

        /// <remarks/>
        public String InvoiceTime
        {
            get => InvoiceDateTime.ToString("HH:mm:ss");
            set => InvoiceDateTime.Date.Add(TimeSpan.Parse(value));
        }
        
        public RoleDescription? Seller { get; set; }
        
        public RoleDescription? Buyer { get; set; }
        
        public BuyerRemarkEnum? BuyerRemark { get; set; }

        [XmlIgnore] 
        public bool BuyerRemarkSpecified { get; set; }
        
        public string? MainRemark { get; set; }
        
        public CustomsClearanceMarkEnum? CustomsClearanceMark { get; set; }

        [XmlIgnore] 
        public bool CustomsClearanceMarkSpecified { get; set; }
        
        public string? Category { get; set; }
        
        public string? RelateNumber { get; set; }
        
        public InvoiceTypeEnum? InvoiceType { get; set; }
        
        public string? GroupMark { get; set; }
        
        public DonateMarkEnum? DonateMark { get; set; }
        
        public string? CarrierType { get; set; }
        
        public string? CarrierId1 { get; set; }
        
        public string? CarrierId2 { get; set; }
        
        public string? PrintMark { get; set; }
        
        public string? NPOBAN { get; set; }
        
        public string? RandomNumber { get; set; }
        
        public string? BondedAreaConfirm { get; set; }
        
        public ZeroTaxRateReasonEnum? ZeroTaxRateReason { get; set; }

        [XmlIgnore] 
        public bool ZeroTaxRateReasonSpecified { get; set; }
        
        public string? Reserved1 { get; set; }
        
        public string? Reserved2 { get; set; }
        public string? DataNumber { get; set; }
    }
    
    public partial class RoleDescription
    {
        public string? Identifier { get; set; }
        
        public string? Name { get; set; }
        
        public string? Address { get; set; }
        
        public string? PersonInCharge { get; set; }
        
        public string? TelephoneNumber { get; set; }
        
        public string? FacsimileNumber { get; set; }
        
        public string? EmailAddress { get; set; }
        
        public string? CustomerNumber { get; set; }
        
        public string? RoleRemark { get; set; }
    }
    
    public partial class Amount
    {
        public decimal? SalesAmount { get; set; }
        
        public decimal? FreeTaxSalesAmount { get; set; }
        
        public decimal? ZeroTaxSalesAmount { get; set; }
        
        public TaxTypeEnum TaxType { get; set; } = TaxTypeEnum.Item1;
        
        public decimal? TaxRate { get; set; }
        
        public decimal? TaxAmount { get; set; }
        
        public decimal? TotalAmount { get; set; }
        
        public decimal? DiscountAmount { get; set; }

        [XmlIgnore] 
        public bool DiscountAmountSpecified { get; set; }
        
        public decimal? OriginalCurrencyAmount { get; set; }

        [XmlIgnore] 
        public bool OriginalCurrencyAmountSpecified { get; set; }
        
        public decimal? ExchangeRate { get; set; }

        [XmlIgnore] 
        public bool ExchangeRateSpecified { get; set; }
        
        public CurrencyCodeEnum? Currency { get; set; }

        [XmlIgnore] 
        public bool CurrencySpecified { get; set; }
    }
    
    public partial class DetailsProductItem
    {
        public string? Description { get; set; }
        
        public decimal? Quantity { get; set; }
        
        public string? Unit { get; set; }
        
        public decimal? UnitPrice { get; set; }
        
        public TaxTypeEnum TaxType { get; set; } = TaxTypeEnum.Item1;
        
        public decimal? Amount { get; set; }
        
        public string? SequenceNumber { get; set; }
        
        public string? Remark { get; set; }
        
        public string? RelateNumber { get; set; }
    }

    public enum TaxTypeEnum
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Item1 = 1,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Item2,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Item3,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Item4,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("9")]
        Item9 = 9,
    }


    public enum CurrencyCodeEnum
    {
        TWD = 0,
        ADP,
        AED,
        AFA,
        ALL,
        AMD,
        ANG,
        AOA,
        AON,
        AOR,
        ARS,
        ATS,
        AUD,
        AWG,
        AZM,
        BAM,
        BBD,
        BDT,
        BEF,
        BGL,
        BGN,
        BHD,
        BIF,
        BMD,
        BND,
        BOB,
        BOV,
        BRL,
        BSD,
        BTN,
        BWP,
        BYB,
        BYR,
        BZD,
        CAD,
        CDF,
        CHF,
        CLF,
        CLP,
        CNY,
        COP,
        CRC,
        CUP,
        CVE,
        CYP,
        CZK,
        DEM,
        DJF,
        DKK,
        DOP,
        DZD,
        ECS,
        ECV,
        EEK,
        EGP,
        ERN,
        ESP,
        ETB,
        EUR,
        FIM,
        FJD,
        FKP,
        FRF,
        GBP,
        GEL,
        GHC,
        GIP,
        GMD,
        GNF,
        GRD,
        GTQ,
        GWP,
        GYD,
        HKD,
        HNL,
        HRK,
        HTG,
        HUF,
        IDR,
        IEP,
        ILS,
        INR,
        IQD,
        IRR,
        ISK,
        ITL,
        JMD,
        JOD,
        JPY,
        KES,
        KGS,
        KHR,
        KMF,
        KPW,
        KRW,
        KWD,
        KYD,
        KZT,
        LAK,
        LBP,
        LKR,
        LRD,
        LSL,
        LTL,
        LUF,
        LVL,
        LYD,
        MAD,
        MDL,
        MGF,
        MKD,
        MMK,
        MNT,
        MOP,
        MRO,
        MTL,
        MUR,
        MVR,
        MWK,
        MXN,
        MXV,
        MYR,
        MZM,
        NAD,
        NGN,
        NIO,
        NLG,
        NOK,
        NPR,
        NZD,
        OMR,
        PAB,
        PEN,
        PGK,
        PHP,
        PKR,
        PLN,
        PTE,
        PYG,
        QAR,
        ROL,
        RUB,
        RUR,
        RWF,
        SAR,
        SBD,
        SCR,
        SDD,
        SDP,
        SEK,
        SGD,
        SHP,
        SIT,
        SKK,
        SLL,
        SOS,
        SRG,
        STD,
        SVC,
        SYP,
        SZL,
        THB,
        TJR,
        TMM,
        TND,
        TOP,
        TPE,
        TRL,
        TTD,
        TZS,
        UAH,
        UGX,
        USD,
        USN,
        UYU,
        UZS,
        VEB,
        VND,
        VUV,
        WST,
        XAF,
        XAG,
        XAU,
        XBA,
        XBB,
        XBC,
        XBD,
        XCD,
        XDR,
        XEU,
        XFO,
        XFU,
        XOF,
        XPD,
        XPF,
        XPT,
        XXX,
        YER,
        YUM,
        ZAR,
        ZMK,
        ZRN,
        ZWD,
        VEF,
        AFN,
        AZN,
        GGP,
        GHS,
        IMP,
        JEP,
        MGA,
        MZN,
        RON,
        RSD,
        SDG,
        SPL,
        SRD,
        TJS,
        TRY,
        TVD
    }

    public enum BuyerRemarkEnum
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Item1 = 1,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Item2,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("3")]
        Item3,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("4")]
        Item4,
    }


    public enum CustomsClearanceMarkEnum
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Item1 = 1,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Item2,
    }

    public enum InvoiceTypeEnum
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("07")]
        Item07 = 7,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("08")]
        Item08,
    }

    public enum DonateMarkEnum
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Item0 = 0,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Item1,
    }

    public enum ZeroTaxRateReasonEnum
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("71")]
        Item71 = 71,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("72")]
        Item72,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("73")]
        Item73,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("74")]
        Item74,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("75")]
        Item75,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("76")]
        Item76,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("77")]
        Item77,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("78")]
        Item78,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("79")]
        Item79,
    }

}
