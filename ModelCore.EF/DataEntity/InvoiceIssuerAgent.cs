using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class InvoiceIssuerAgent
{
    /// <summary>
    /// 主鍵
    /// </summary>
    public int AgentID { get; set; }

    public int IssuerID { get; set; }

    public int? RelationType { get; set; }

    public virtual Organization Agent { get; set; } = null!;

    public virtual Organization Issuer { get; set; } = null!;
}
