using AutoMapper;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Model;

namespace ms_school_management.Api.School.Infrastructure.Controller.Mapper;

public class SchoolProfile : Profile
{
    public SchoolProfile()
    {
        CreateMap<SchoolRequestDto, School>();
        CreateMap<School, SchoolListDto>();
        CreateMap<School, SchoolResponseDto>();
    }
}
