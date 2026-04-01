using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelCore.DataEntity
{
    public partial class Organization
    {
        [NotMapped]
        public virtual ICollection<InvoiceIssuerAgent> InvoiceIssuerAgent { get => InvoiceIssuerAgent; set => InvoiceIssuerAgent = value; }
        [NotMapped]
        public virtual ICollection<InvoiceIssuerAgent> AsInvoiceIssuer { get => InvoiceIssuerAgentIssuer; set => InvoiceIssuerAgentIssuer = value; }
        [NotMapped]
        public virtual ICollection<InvoiceItem> InvoiceItems { get => InvoiceItemSeller; set => InvoiceItemSeller = value; }

    }
}
