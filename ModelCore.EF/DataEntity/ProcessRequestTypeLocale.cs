using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class ProcessRequestTypeLocale
{
    public string LocaleID { get; set; } = null!;

    public int ProcessType { get; set; }

    public string? ChannelName { get; set; }

    public string? ChannelInProgress { get; set; }

    public string? ChannelResponse { get; set; }

    public virtual ProcessRequestType ProcessTypeNavigation { get; set; } = null!;
}
