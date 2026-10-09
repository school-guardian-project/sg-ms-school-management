using AutoMapper;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Model;

namespace ms_school_management.Api.School.Infrastructure.Controller.Mapper;

public class SchoolProfile : Profile
{
    public SchoolProfile()
    {
        CreateMap<SchoolRequestDto, Domain.Model.School>();
        CreateMap<SchoolWithCampusesRequestDto, Domain.Model.School>();
        CreateMap<Domain.Model.School, SchoolListDto>();
        CreateMap<Domain.Model.School, SchoolResponseDto>();
    }
}
