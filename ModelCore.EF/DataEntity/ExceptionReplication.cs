using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 異常記錄待回應檔
/// </summary>
public partial class ExceptionReplication
{
    public int LogID { get; set; }

    public virtual ExceptionLog Log { get; set; } = null!;
}
