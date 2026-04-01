using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 資料異動待回應檔
/// </summary>
public partial class ReplicationNotification
{
    public int DocID { get; set; }

    public int TypeID { get; set; }

    public virtual DocumentReplication DocumentReplication { get; set; } = null!;
}
