using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using ModelCore.Schema.TXN;

namespace ModelCore.Schema.EIVO
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "A0101" /*AnonymousType = true*/)]
    public partial class RootResponseForA0101 : RootResponse
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Invoice")]
        public ModelCore.Schema.TurnKey.A0101.Invoice[] Invoice;
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "A0201" /*AnonymousType = true*/)]
    public partial class RootResponseForA0201 : RootResponse
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CancelInvoice")]
        public ModelCore.Schema.TurnKey.A0201.CancelInvoice[] CancelInvoice;
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    [XmlInclude(typeof(ModelCore.Schema.EIVO.RootResponseForA0101))]
    public partial class RootA0101 : Root
    {

    }


    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    [XmlInclude(typeof(ModelCore.Schema.EIVO.RootResponseForA0201))]
    public partial class RootA0201 : Root
    {

    }


    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "B0101" /*AnonymousType = true*/)]
    public partial class RootResponseForB0101 : RootResponse
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Allowance")]
        public ModelCore.Schema.TurnKey.B0101.Allowance[] Allowance;
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "B0201" /*AnonymousType = true*/)]
    public partial class RootResponseForB0201 : RootResponse
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CancelAllowance")]
        public ModelCore.Schema.TurnKey.B0201.CancelAllowance[] CancelAllowance;
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    [XmlInclude(typeof(ModelCore.Schema.EIVO.RootResponseForB0101))]
    public partial class RootB0101 : Root
    {

    }


    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    [XmlInclude(typeof(ModelCore.Schema.EIVO.RootResponseForB0201))]
    public partial class RootB0201 : Root
    {

    }


    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "B2CInvoiceMapping" /*AnonymousType = true*/)]
    public partial class RootResponseForB2CInvoiceMapping : RootResponse
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("InvoiceMapRoot")]
        public ModelCore.Schema.EIVO.InvoiceMapRoot InvoiceMapRoot;
    }
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    [XmlInclude(typeof(ModelCore.Schema.EIVO.RootResponseForB2CInvoiceMapping))]
    public partial class RootB2CInvoiceMapping : Root
    {

    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "InvoiceTrackCode" /*AnonymousType = true*/)]
    public partial class RootResponseForInvoiceTrackCode : RootResponse
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("InvoiceTrackCodeRoot")]
        public ModelCore.Schema.EIVO.InvoiceTrackCodeRoot InvoiceTrackCodeRoot;
    }
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    [XmlInclude(typeof(ModelCore.Schema.EIVO.RootResponseForInvoiceTrackCode))]
    public partial class RootInvoiceTrackCode : Root
    {

    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "E0402" /*AnonymousType = true*/)]
    public partial class RootResponseForE0402 : RootResponse
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BranchTrackBlank", Order = 1)]
        public ModelCore.Schema.TurnKey.E0402.BranchTrackBlank E0402BranchTrackBlank ;
    }
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    [XmlInclude(typeof(ModelCore.Schema.EIVO.RootResponseForE0402))]
    public partial class RootE0402 : Root
    {

    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "E0402" /*AnonymousType = true*/)]
    public partial class RootResponseForBranchTrackBlank : RootResponse
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BranchTrackBlank", Order = 1)]
        public ModelCore.Schema.TurnKey.E0402.BranchTrackBlank TrackBlank;
    }
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    [XmlInclude(typeof(ModelCore.Schema.EIVO.RootResponseForBranchTrackBlank))]
    public partial class RootBranchTrackBlank : Root
    {

    }

}
