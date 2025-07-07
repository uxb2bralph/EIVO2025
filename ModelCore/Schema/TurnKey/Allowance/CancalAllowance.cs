using System.Xml.Serialization;

namespace ModelCore.Schema.TurnKey.Allowance {
    
    
    /// <remarks/>
    
    [System.SerializableAttribute()]
    public partial class CancelAllowance {
        
        /// <remarks/>
        public string? CancelAllowanceNumber;
        
        /// <remarks/>
        public AllowanceTypeEnum AllowanceType = AllowanceTypeEnum.Item2;
        
        /// <remarks/>
        public string? AllowanceDate;
        
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
        public string? Remark;
    }
   
}
