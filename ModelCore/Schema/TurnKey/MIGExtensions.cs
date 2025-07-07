using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ModelCore.Schema.TurnKey
{
    public static class MIGExtensions
    {

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

    public enum TaxTypeEnum
    {

        /// <remarks/>
        [XmlEnum("1")]
        Item1 = 1,

        /// <remarks/>
        [XmlEnum("2")]
        Item2,

        /// <remarks/>
        [XmlEnum("3")]
        Item3,

        /// <remarks/>
        [XmlEnum("4")]
        Item4,

        /// <remarks/>
        [XmlEnum("9")]
        Item9 = 9,
    }

}
