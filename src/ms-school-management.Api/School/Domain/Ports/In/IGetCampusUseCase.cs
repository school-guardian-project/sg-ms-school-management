using ms_school_management.Api.School.Application.Dto;

namespace ms_school_management.Api.School.Domain.Ports.In;

public interface IGetCampusUseCase
{
    Task<CampusDetailDto?> ExecuteAsync(Guid campusId, CancellationToken ct = default);
    Task<CampusDetailDto?> ExecuteByNameAsync(Guid schoolId, string name, CancellationToken ct = default);
}
