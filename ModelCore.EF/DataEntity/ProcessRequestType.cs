using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class ProcessRequestType
{
    public int ProcessType { get; set; }

    public string? ChannelName { get; set; }

    public string? ChannelInProgress { get; set; }

    public string? ChannelResponse { get; set; }

    public string? DescriptionID { get; set; }

    public virtual ICollection<ProcessRequest> ProcessRequest { get; set; } = new List<ProcessRequest>();

    public virtual ICollection<ProcessRequestTypeLocale> ProcessRequestTypeLocale { get; set; } = new List<ProcessRequestTypeLocale>();
}
