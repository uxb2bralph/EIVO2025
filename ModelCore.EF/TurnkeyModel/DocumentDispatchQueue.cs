using System;
using System.Collections.Generic;

namespace ModelCore.TurnkeyModel;

public partial class DocumentDispatchQueue
{
    public string DocType { get; set; } = null!;

    public string DocNo { get; set; } = null!;

    public string? Status { get; set; }

    public string FileName { get; set; } = null!;
}
