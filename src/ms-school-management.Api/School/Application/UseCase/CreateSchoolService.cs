using AutoMapper;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Model;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Application.UseCase;

public sealed class CreateSchoolService : ICreateSchoolUseCase
{
    private readonly ISchoolRepository _repository;
    private readonly IMapper _mapper;

    public CreateSchoolService(ISchoolRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Guid> ExecuteAsync(SchoolRequestDto request)
    {
        SchoolLocationRequestRules.Validate(request.Latitude, request.Longitude);
        var school = _mapper.Map<Domain.Model.School>(request);
        school.Id = Guid.NewGuid();
        school.Status = Status.Active;

        await _repository.SaveWithCampusesAsync(school, [SchoolCampus.Central(school)]);
        return school.Id;
    }
}
