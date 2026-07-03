using ModelCore.DataEntity;

namespace ModelCore.DTOs
{
    public static class UserRoleMappingExtensions
    {
        public static UserRoleDto ToDto(this UserRole src) => new()
        {
            UID = src.UID,
            RoleID = src.RoleID,
            OrgaCateID = src.OrgaCateID,
            OrganizationCategory = src.OrgaCate?.ToDto(),
        };
    }

    public class UserRoleDto
    {
        public int UID { get; set; }

        public int RoleID { get; set; }

        public int OrgaCateID { get; set; }

        public OrganizationCategoryDto? OrganizationCategory { get; set; }

        //public ModelCore.DataEntity.UserRoleDefinition? Role { get; set; }

        //public ModelCore.DataEntity.UserProfile? UIDNavigation { get; set; }

    }
}
