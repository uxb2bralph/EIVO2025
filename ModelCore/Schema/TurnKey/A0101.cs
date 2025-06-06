﻿//------------------------------------------------------------------------------
// <auto-generated>
//     這段程式碼是由工具產生的。
//     執行階段版本:4.0.30319.42000
//
//     對這個檔案所做的變更可能會造成錯誤的行為，而且如果重新產生程式碼，
//     變更將會遺失。
// </auto-generated>
//------------------------------------------------------------------------------

// 
// 此原始程式碼由 xsd 版本=4.8.3928.0 自動產生。
// 
namespace ModelCore.Schema.TurnKey.A0101 {
    using System;
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="urn:GEINV:eInvoiceMessage:A0101:4.0")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="urn:GEINV:eInvoiceMessage:A0101:4.0", IsNullable=false)]
    public partial class Invoice {
        
        /// <remarks/>
        public Main Main;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("ProductItem", IsNullable=false)]
        public DetailsProductItem[] Details;
        
        /// <remarks/>
        public Amount Amount;
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:GEINV:eInvoiceMessage:A0101:4.0")]
    public partial class Main {
        
        [XmlIgnore]
        public DateTime InvoiceDateTime { get; set; }
        /// <remarks/>
        public string InvoiceNumber;

        /// <remarks/>
        public string InvoiceDate { get => InvoiceDateTime.ToString("yyyyMMdd"); }

        /// <remarks/>
        public String InvoiceTime
        {
            get => InvoiceDateTime.ToString("HH:mm:ss");
            // set => InvoiceDateTime = DateTime.Parse(value);
        }

        /// <remarks/>
        public MainSeller Seller;
        
        /// <remarks/>
        public MainBuyer Buyer;
        
        /// <remarks/>
        public BuyerRemarkEnum BuyerRemark;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BuyerRemarkSpecified;
        
        /// <remarks/>
        public string MainRemark;
        
        /// <remarks/>
        public CustomsClearanceMarkEnum CustomsClearanceMark;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CustomsClearanceMarkSpecified;
        
        /// <remarks/>
        public string Category;
        
        /// <remarks/>
        public string RelateNumber;
        
        /// <remarks/>
        public InvoiceTypeEnum InvoiceType;
        
        /// <remarks/>
        public string GroupMark;
        
        /// <remarks/>
        public DonateMarkEnum DonateMark;
        
        /// <remarks/>
        public ZeroTaxRateReasonEnum ZeroTaxRateReason;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ZeroTaxRateReasonSpecified;
        
        /// <remarks/>
        public string Reserved1;
        
        /// <remarks/>
        public string Reserved2;
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="urn:GEINV:eInvoiceMessage:A0101:4.0")]
    public partial class MainSeller {
        
        /// <remarks/>
        public string Identifier;
        
        /// <remarks/>
        public string Name;
        
        /// <remarks/>
        public string Address;
        
        /// <remarks/>
        public string PersonInCharge;
        
        /// <remarks/>
        public string TelephoneNumber;
        
        /// <remarks/>
        public string FacsimileNumber;
        
        /// <remarks/>
        public string EmailAddress;
        
        /// <remarks/>
        public string CustomerNumber;
        
        /// <remarks/>
        public string RoleRemark;
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:GEINV:eInvoiceMessage:A0101:4.0")]
    public partial class Amount {
        
        /// <remarks/>
        public decimal SalesAmount;
        
        /// <remarks/>
        public TaxTypeEnum TaxType;
        
        /// <remarks/>
        public decimal TaxRate;
        
        /// <remarks/>
        public decimal TaxAmount;
        
        /// <remarks/>
        public decimal TotalAmount;
        
        /// <remarks/>
        public decimal DiscountAmount;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DiscountAmountSpecified;
        
        /// <remarks/>
        public decimal OriginalCurrencyAmount;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OriginalCurrencyAmountSpecified;
        
        /// <remarks/>
        public decimal ExchangeRate;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExchangeRateSpecified;
        
        /// <remarks/>
        public CurrencyCodeEnum Currency;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CurrencySpecified;
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:GEINV:eInvoiceMessage:A0101:4.0")]
    public enum TaxTypeEnum {
        
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
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:GEINV:eInvoiceMessage:A0101:4.0")]
    public enum CurrencyCodeEnum {

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
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="urn:GEINV:eInvoiceMessage:A0101:4.0")]
    public partial class MainBuyer {
        
        /// <remarks/>
        public string Identifier;
        
        /// <remarks/>
        public string Name;
        
        /// <remarks/>
        public string Address;
        
        /// <remarks/>
        public string PersonInCharge;
        
        /// <remarks/>
        public string TelephoneNumber;
        
        /// <remarks/>
        public string FacsimileNumber;
        
        /// <remarks/>
        public string EmailAddress;
        
        /// <remarks/>
        public string CustomerNumber;
        
        /// <remarks/>
        public string RoleRemark;
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:GEINV:eInvoiceMessage:A0101:4.0")]
    public enum BuyerRemarkEnum {
        
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
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:GEINV:eInvoiceMessage:A0101:4.0")]
    public enum CustomsClearanceMarkEnum {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Item1 = 1,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("2")]
        Item2,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:GEINV:eInvoiceMessage:A0101:4.0")]
    public enum InvoiceTypeEnum {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("07")]
        Item07 = 7,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("08")]
        Item08,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:GEINV:eInvoiceMessage:A0101:4.0")]
    public enum DonateMarkEnum {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("0")]
        Item0,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Item1,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:GEINV:eInvoiceMessage:A0101:4.0")]
    public enum ZeroTaxRateReasonEnum {
        
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
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="urn:GEINV:eInvoiceMessage:A0101:4.0")]
    public partial class DetailsProductItem {
        
        /// <remarks/>
        public string Description;
        
        /// <remarks/>
        public decimal Quantity;
        
        /// <remarks/>
        public string Unit;
        
        /// <remarks/>
        public decimal UnitPrice;
        
        /// <remarks/>
        public TaxTypeEnum TaxType = TaxTypeEnum.Item1;
        
        /// <remarks/>
        public decimal Amount;
        
        /// <remarks/>
        public string SequenceNumber;
        
        /// <remarks/>
        public string Remark;
        
        /// <remarks/>
        public string RelateNumber;
    }
}
