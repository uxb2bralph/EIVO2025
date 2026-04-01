using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class OrganizationCustomSetting
{
    /// <summary>
    /// 主鍵
    /// </summary>
    public int CompanyID { get; set; }

    public string? SettingData { get; set; }

    public virtual Organization Company { get; set; } = null!;
}
