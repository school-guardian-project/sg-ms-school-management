using AutoMapper;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Model;

namespace ms_school_management.Api.School.Infrastructure.Controller.Mapper;

public class SchoolProfile : Profile
{
    public SchoolProfile()
    {
        CreateMap<SchoolRequestDto, Domain.Model.School>()
            .ForMember(dest => dest.Status, opt =>
            {
                opt.PreCondition(src => !string.IsNullOrWhiteSpace(src.Status));
                opt.MapFrom(src => Enum.Parse<Status>(src.Status!, true));
            });
        CreateMap<SchoolWithCampusesRequestDto, Domain.Model.School>()
            .ForMember(dest => dest.Status, opt =>
            {
                opt.PreCondition(src => !string.IsNullOrWhiteSpace(src.Status));
                opt.MapFrom(src => Enum.Parse<Status>(src.Status!, true));
            });
        CreateMap<Domain.Model.School, SchoolListDto>()
            .ForCtorParam(nameof(SchoolListDto.Status), opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<Domain.Model.School, SchoolResponseDto>()
            .ForCtorParam(nameof(SchoolResponseDto.Status), opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
