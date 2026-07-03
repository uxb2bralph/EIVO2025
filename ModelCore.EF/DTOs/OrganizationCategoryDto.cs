using ModelCore.DataEntity;

namespace ModelCore.DTOs
{
    public static class OrganizationCategoryMappingExtensions
    {
        public static OrganizationCategoryDto ToDto(this OrganizationCategory src) => new()
        {
            OrgaCateID = src.OrgaCateID,
            CompanyID = src.CompanyID,
            CategoryID = src.CategoryID,
        };
    }

    public class OrganizationCategoryDto
    {
        public int OrgaCateID { get; set; }

        public int CompanyID { get; set; }

        public int CategoryID { get; set; }

        //public ModelCore.DataEntity.CategoryDefinition? Category { get; set; }

        //public ModelCore.DataEntity.Organization? Company { get; set; }

        //public ICollection<ModelCore.DataEntity.OrganizationCategoryUserRole> OrganizationCategoryUserRole { get; set; } = new List<ModelCore.DataEntity.OrganizationCategoryUserRole>();

        //public ICollection<ModelCore.DataEntity.UserInbox> UserInbox { get; set; } = new List<ModelCore.DataEntity.UserInbox>();

        //public ICollection<ModelCore.DataEntity.UserRole> UserRole { get; set; } = new List<ModelCore.DataEntity.UserRole>();
    }
}
