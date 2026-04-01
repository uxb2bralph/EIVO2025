using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class BusinessRelationship
{
    public int MasterID { get; set; }

    public int RelativeID { get; set; }

    public int BusinessID { get; set; }

    public int? CurrentLevel { get; set; }

    /// <summary>
    /// 機關名稱
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// 電話
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Addr { get; set; }

    /// <summary>
    /// 連絡人電子郵件
    /// </summary>
    public string? ContactEmail { get; set; }

    public string? CustomerNo { get; set; }

    public virtual BusinessType Business { get; set; } = null!;

    public virtual LevelExpression? CurrentLevelNavigation { get; set; }

    public virtual Organization Master { get; set; } = null!;

    public virtual Organization Relative { get; set; } = null!;
}
