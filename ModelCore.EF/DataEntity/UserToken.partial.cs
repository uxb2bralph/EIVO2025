using System.ComponentModel.DataAnnotations.Schema;

namespace ModelCore.DataEntity;

public partial class UserToken
{
    [NotMapped]
    public virtual UserProfile UserProfile
    {
        get => UIDNavigation;
        set => UIDNavigation = value;
    }
}
