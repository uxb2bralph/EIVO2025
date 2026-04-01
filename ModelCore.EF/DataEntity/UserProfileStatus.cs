using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 會員狀態主檔
/// </summary>
public partial class UserProfileStatus
{
    public int UID { get; set; }

    public int? CurrentLevel { get; set; }

    public virtual LevelExpression? CurrentLevelNavigation { get; set; }

    public virtual UserProfile UIDNavigation { get; set; } = null!;
}
