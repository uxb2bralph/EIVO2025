using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class ExceptionLog
{
    public int LogID { get; set; }

    public int? CompanyID { get; set; }

    public int? DocID { get; set; }

    public int? TypeID { get; set; }

    public string? DataContent { get; set; }

    public DateTime? LogTime { get; set; }

    public string? Message { get; set; }

    public bool? IsCSV { get; set; }

    public virtual Organization? Company { get; set; }

    public virtual CDS_Document? Doc { get; set; }

    public virtual ExceptionReplication? ExceptionReplication { get; set; }

    public virtual ICollection<ProcessRequest> ProcessRequest { get; set; } = new List<ProcessRequest>();

    public virtual DocumentType? Type { get; set; }
}
