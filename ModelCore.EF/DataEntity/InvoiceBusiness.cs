using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceBusiness
{
    public int SellerID { get; set; }

    public string BuyerReceiptNo { get; set; } = null!;

    public int? BuyerID { get; set; }

    public string? PostCode { get; set; }

    public string? Address { get; set; }

    public string? Name { get; set; }

    public string? CustomerID { get; set; }

    public string? ContactName { get; set; }

    public string? Phone { get; set; }

    public string? EMail { get; set; }

    public string? CustomerName { get; set; }

    public string? Fax { get; set; }

    public string? PersonInCharge { get; set; }

    public string? RoleRemark { get; set; }

    public virtual Organization? Buyer { get; set; }

    public virtual Organization Seller { get; set; } = null!;
}
