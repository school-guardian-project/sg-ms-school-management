using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Application.UseCase;

public sealed class ListCampusesBySchoolService : IListCampusesBySchoolUseCase
{
    private readonly ICampusRepository _campusRepository;

    public ListCampusesBySchoolService(ICampusRepository campusRepository)
    {
        _campusRepository = campusRepository;
    }

    public async Task<List<CampusListDto>> ExecuteAsync(Guid schoolId, CancellationToken ct)
    {
        var campuses = await _campusRepository.ListBySchoolIdAsync(schoolId, ct);
        return campuses.Select(c => new CampusListDto(c.Id, c.Name, c.Address)).ToList();
    }
}
