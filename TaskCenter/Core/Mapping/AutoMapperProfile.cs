using AutoMapper;
using ModelCore.DataEntity;
using TaskCenter.Core.DTOs;

namespace TaskCenter.Core.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //CreateMap<UserProfile, UserProfileDto>()
            //    .ForMember(dest => dest.PID, opt => opt.MapFrom(src => src.PID));

            //CreateMap<UserProfileDto, UserProfile>()
            //    .ForMember(dest => dest.PID, opt => opt.MapFrom(src => src.PID))
            //    // ignore navigation and collections to avoid EF/LINQ issues
            //    .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
