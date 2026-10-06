using AutoMapper;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Application.UseCase;

public sealed class GetSchoolService : IGetSchoolUseCase
{
    private readonly ISchoolRepository _repository;
    private readonly IMapper _mapper;

    public GetSchoolService(ISchoolRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<SchoolResponseDto?> ExecuteAsync(Guid id)
    {
        var school = await _repository.FindByIdAsync(id);
        if (school is null) return null;

        var dto = _mapper.Map<SchoolResponseDto>(school);
        var cityName = await _repository.GetCityNameAsync(school.CityId);

        return dto with { CityName = cityName ?? string.Empty };
    }
}
