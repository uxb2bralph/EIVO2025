using System;
using System.Collections.Generic;

namespace ModelCore.TurnkeyModel;

public partial class TURNKEY_USER_PROFILE
{
    public string USER_ID { get; set; } = null!;

    public string USER_PASSWORD { get; set; } = null!;

    public string? USER_ROLE { get; set; }
}
