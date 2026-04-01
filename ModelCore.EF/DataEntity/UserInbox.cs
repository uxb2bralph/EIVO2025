using System;
using System.Collections.Generic;

namespace ModelCore.DataEntity;

public partial class UserInbox
{
    public int MsgID { get; set; }

    public int RoleID { get; set; }

    public int OrgaCateID { get; set; }

    public string? DataSource { get; set; }

    public int MessageID { get; set; }

    public DateTime MsgDate { get; set; }

    public int? Sender { get; set; }

    public int? EventID { get; set; }

    public int? DocID { get; set; }

    public virtual CDS_Document? Doc { get; set; }

    public virtual SystemEvent? Event { get; set; }

    public virtual MessageType Message { get; set; } = null!;

    public virtual OrganizationCategory OrgaCate { get; set; } = null!;

    public virtual UserRoleDefinition Role { get; set; } = null!;

    public virtual UserProfile? SenderNavigation { get; set; }

    public virtual UserMail? UserMail { get; set; }
}
