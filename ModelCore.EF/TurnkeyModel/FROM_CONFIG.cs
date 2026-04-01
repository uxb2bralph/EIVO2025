using System;
using System.Collections.Generic;

namespace ModelCore.TurnkeyModel;

public partial class FROM_CONFIG
{
    public string? TRANSPORT_ID { get; set; }

    public string? TRANSPORT_PASSWORD { get; set; }

    public string PARTY_ID { get; set; } = null!;

    public string? PARTY_DESCRIPTION { get; set; }

    public string? ROUTING_ID { get; set; }

    public string? ROUTING_DESCRIPTION { get; set; }

    public string? SIGN_ID { get; set; }

    public string? SUBSTITUTE_PARTY_ID { get; set; }
}
