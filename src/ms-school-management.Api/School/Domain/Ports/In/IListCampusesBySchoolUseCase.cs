namespace ms_school_management.Api.School.Domain.Ports.In;

public interface IListCampusesBySchoolUseCase
{
    Task<List<School.Application.Dto.CampusListDto>> ExecuteAsync(Guid schoolId, CancellationToken ct);
}
