using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class ProcessExceptionNotification
{
    public int TaskID { get; set; }

    /// <summary>
    /// 主鍵
    /// </summary>
    public int CompanyID { get; set; }

    public DateTime? BookingTime { get; set; }

    public virtual Organization Company { get; set; } = null!;

    public virtual ProcessRequest Task { get; set; } = null!;
}
