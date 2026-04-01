using System.Xml.Serialization;
    
namespace ModelCore.Schema.TurnKey.Invoice {
    
    /// <remarks/>
    [System.SerializableAttribute()]
    public partial class CancelInvoice {
        
        /// <remarks/>
        public string? CancelInvoiceNumber;
        
        /// <remarks/>
        public string? InvoiceDate;
        
        /// <remarks/>
        public string? BuyerId;
        
        /// <remarks/>
        public string? SellerId;
        
        /// <remarks/>
        public string? CancelDate;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="time")]
        public System.DateTime CancelTime;
        
        /// <remarks/>
        public string? CancelReason;
        
        /// <remarks/>
        public string? ReturnTaxDocumentNumber;
        
        /// <remarks/>
        public string? Remark;
        
        /// <remarks/>
        public string? Reserved1;
        
        /// <remarks/>
        public string? Reserved2;
    }
}
