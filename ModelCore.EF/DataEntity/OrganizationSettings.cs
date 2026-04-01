using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class OrganizationSettings
{
    /// <summary>
    /// 主鍵
    /// </summary>
    public int CompanyID { get; set; }

    public string Settings { get; set; } = null!;

    public virtual Organization Company { get; set; } = null!;
}
