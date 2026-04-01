using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class MasterOrganization
{
    /// <summary>
    /// 主鍵
    /// </summary>
    public int MasterID { get; set; }

    /// <summary>
    /// 機關名稱
    /// </summary>
    public string? EnterpriseName { get; set; }

    public virtual Organization Master { get; set; } = null!;

    public virtual ICollection<Organization> Branch { get; set; } = new List<Organization>();
}
