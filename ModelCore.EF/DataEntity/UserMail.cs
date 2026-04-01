using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class UserMail
{
    public int MsgID { get; set; }

    public virtual UserInbox Msg { get; set; } = null!;
}
