using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class DataNotice
{
    public int NoticeID { get; set; }

    public string? RenderStyle { get; set; }

    public virtual ICollection<DataProcessLog> DataProcessLog { get; set; } = new List<DataProcessLog>();

    public virtual ICollection<DataProcessQueue> DataProcessQueue { get; set; } = new List<DataProcessQueue>();
}
