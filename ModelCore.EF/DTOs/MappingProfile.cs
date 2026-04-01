using AutoMapper;

namespace ModelCore.DTOs
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ModelCore.DataEntity.UserProfile, UserProfileDto>();  //.ReverseMap();
            CreateMap<ModelCore.DataEntity.UserRole, UserRoleDto>()
                .ForMember(dest => dest.OrganizationCategory, opt => opt.MapFrom(src => src.OrgaCate));
            CreateMap<ModelCore.DataEntity.OrganizationCategory, OrganizationCategoryDto>();    //.ReverseMap();
        }
    }
}
