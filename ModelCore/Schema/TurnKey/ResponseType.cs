using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using ModelCore.Schema.TXN;

namespace ModelCore.Schema.TurnKey
{



    //[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    //[System.SerializableAttribute()]
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(TypeName = "B1401" /*AnonymousType = true*/)]
    //public partial class RootResponseForB1401 : RootResponse
    //{

    //    /// <remarks/>
    //    [System.Xml.Serialization.XmlElementAttribute("Allowance", Order = 1)]
    //    public ModelCore.Schema.TurnKey.B1401.Allowance[] Allowance;
    //}

    //[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    //[System.SerializableAttribute()]
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    //[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    //[XmlInclude(typeof(ModelCore.Schema.TurnKey.RootResponseForB1401))]
    //public partial class RootB1401 : Root
    //{

    //}

        
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "A1401",Namespace="urn:GEINV:eInvoiceMessage:A1401:3.1" /*AnonymousType = true*/)]
    public partial class RootResponseSingleA1401 : RootResponse
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Invoice", Order = 1)]
        public ModelCore.Schema.TurnKey.A1401.Invoice Invoice;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
        public int InvoiceID;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 3)]
        public String DataNumber;

    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    [XmlInclude(typeof(ModelCore.Schema.TurnKey.RootResponseSingleA1401))]
    public partial class RootSingleA1401 : Root
    {

    }

}

namespace ModelCore.Schema.TurnKey.A1401 {
    using System.Xml.Serialization;

    public partial class Invoice
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 3)]
        public String DataNumber;

        [System.Xml.Serialization.XmlElementAttribute(Order = 4)]
        public String InvoiceID;

    }

}