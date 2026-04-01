using System.ComponentModel.DataAnnotations.Schema;

namespace ModelCore.DataEntity;

public partial class UserProfileStatus
{
    [NotMapped]
    public virtual UserProfile UserProfile
    {
        get => UIDNavigation;
        set => UIDNavigation = value;
    }
}
