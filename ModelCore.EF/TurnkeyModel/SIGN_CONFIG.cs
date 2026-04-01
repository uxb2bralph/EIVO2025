using System;
using System.Collections.Generic;

namespace ModelCore.TurnkeyModel;

public partial class SIGN_CONFIG
{
    public string SIGN_ID { get; set; } = null!;

    public string? SIGN_TYPE { get; set; }

    public string? PFX_PATH { get; set; }

    public string? SIGN_PASSWORD { get; set; }
}
