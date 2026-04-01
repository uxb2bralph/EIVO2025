using System;

namespace ModelCore.DTOs
{
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
