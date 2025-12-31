using AutoMapper;
using ModelCore.DataEntity;

namespace ArchiveData.Mapping
{
    public class ArchiveMappingProfile : Profile
    {
        public ArchiveMappingProfile()
        {
            CreateMap<InvoiceItem, InvoiceDto>()
                .ForMember(d => d.InvoiceID, o => o.MapFrom(s => s.InvoiceID))
                .ForMember(d => d.InvoiceNo, o => o.MapFrom(s => (s.TrackCode ?? string.Empty) + (s.No ?? string.Empty)))
                .ForMember(d => d.InvoiceDate, o => o.MapFrom(s => s.InvoiceDate));

            CreateMap<InvoiceAllowance, AllowanceDto>()
                .ForMember(d => d.AllowanceNumber, o => o.MapFrom(s => s.AllowanceNumber))
                .ForMember(d => d.AllowanceDate, o => o.MapFrom(s => s.AllowanceDate));
        }
    }
}
