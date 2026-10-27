using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Application.Mapper;
using ms_school_management.Api.School.Application.Security;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Application.UseCase;

public sealed class ListSchoolCampusesService : IListSchoolCampusesUseCase
{
    private readonly ISchoolRepository _repository;
    private readonly ISchoolAccessPolicy _accessPolicy;

    public ListSchoolCampusesService(ISchoolRepository repository, ISchoolAccessPolicy accessPolicy)
    {
        _repository = repository;
        _accessPolicy = accessPolicy;
    }

    public async Task<IReadOnlyList<SchoolCampusResponseDto>> ExecuteAsync(Guid schoolId)
    {
        await _accessPolicy.EnsureCanAccessAsync(schoolId);

        var campuses = await _repository.FindCampusesBySchoolAsync(schoolId);
        return campuses.Select(CampusMapper.ToSchoolCampusResponse).ToList();
    }
}
