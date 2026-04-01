using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

/// <summary>
/// 資料異動記錄檔
/// </summary>
public partial class DocumentReplication
{
    public int DocID { get; set; }

    /// <summary>
    /// 最近處理時間記錄
    /// </summary>
    public DateTime? LastActionTime { get; set; }

    /// <summary>
    /// 重試次數
    /// </summary>
    public int? RetrialCount { get; set; }

    /// <summary>
    /// 處理訊息
    /// </summary>
    public string? Message { get; set; }

    public int TypeID { get; set; }

    public virtual CDS_Document Doc { get; set; } = null!;

    public virtual ReplicationNotification? ReplicationNotification { get; set; }

    public virtual DocumentType Type { get; set; } = null!;
}
