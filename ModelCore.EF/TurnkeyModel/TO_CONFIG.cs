using System;
using System.Collections.Generic;

namespace ModelCore.TurnkeyModel;

public partial class TO_CONFIG
{
    public string PARTY_ID { get; set; } = null!;

    public string? PARTY_DESCRIPTION { get; set; }

    public string? ROUTING_ID { get; set; }

    public string? ROUTING_DESCRIPTION { get; set; }

    public string? FROM_PARTY_ID { get; set; }
}
