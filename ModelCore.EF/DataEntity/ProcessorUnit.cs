using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class ProcessorUnit
{
    public int ProcessorID { get; set; }

    public Guid ProcessorToken { get; set; }

    public virtual ICollection<ProcessRequestQueue> ProcessRequestQueue { get; set; } = new List<ProcessRequestQueue>();
}
