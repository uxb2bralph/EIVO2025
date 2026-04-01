using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class OrganizationBranch
{
    public int BranchID { get; set; }

    public string? BranchName { get; set; }

    public int CompanyID { get; set; }

    public string? BranchNo { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Addr { get; set; }

    /// <summary>
    /// 連絡人電子郵件
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// 電話
    /// </summary>
    public string? Phone { get; set; }

    public virtual Organization Company { get; set; } = null!;
}
