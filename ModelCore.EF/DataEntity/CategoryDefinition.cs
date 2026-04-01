using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 機關類別定義檔
/// </summary>
public partial class CategoryDefinition
{
    public int CategoryID { get; set; }

    /// <summary>
    /// 分類名稱
    /// </summary>
    public string Category { get; set; } = null!;

    /// <summary>
    /// 圖識來源網址
    /// </summary>
    public string? CharacterURL { get; set; }

    public virtual ICollection<OrganizationCategory> OrganizationCategory { get; set; } = new List<OrganizationCategory>();

    public virtual ICollection<UserMenu> UserMenu { get; set; } = new List<UserMenu>();
}
