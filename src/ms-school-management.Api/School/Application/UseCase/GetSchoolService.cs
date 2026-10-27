using AutoMapper;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Application.Security;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Application.UseCase;

public sealed class GetSchoolService : IGetSchoolUseCase
{
    private readonly ISchoolRepository _repository;
    private readonly IMapper _mapper;
    private readonly ISchoolAccessPolicy _accessPolicy;

    public GetSchoolService(ISchoolRepository repository, IMapper mapper, ISchoolAccessPolicy accessPolicy)
    {
        _repository = repository;
        _mapper = mapper;
        _accessPolicy = accessPolicy;
    }

    public async Task<SchoolResponseDto?> ExecuteAsync(Guid id)
    {
        await _accessPolicy.EnsureCanAccessAsync(id);

        var school = await _repository.FindByIdAsync(id);
        if (school is null) return null;

        var dto = _mapper.Map<SchoolResponseDto>(school);
        var cityName = await _repository.GetCityNameAsync(school.CityId);

        return dto with { CityName = cityName ?? string.Empty };
    }
}
