using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Application.UseCase;

public sealed class ListSchoolCampusesService : IListSchoolCampusesUseCase
{
    private readonly ISchoolRepository _repository;

    public ListSchoolCampusesService(ISchoolRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<SchoolCampusResponseDto>> ExecuteAsync(Guid schoolId)
    {
        var campuses = await _repository.FindCampusesBySchoolAsync(schoolId);
        return campuses.Select(campus => new SchoolCampusResponseDto(
            campus.Id, campus.Name, campus.Address, campus.Latitude, campus.Longitude)).ToList();
    }
}
