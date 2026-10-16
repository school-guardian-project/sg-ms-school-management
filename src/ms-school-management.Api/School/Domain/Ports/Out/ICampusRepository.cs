using ms_school_management.Api.School.Domain.Model;

namespace ms_school_management.Api.School.Domain.Ports.Out;

public interface ICampusRepository
{
    Task<List<SchoolCampus>> ListBySchoolIdAsync(Guid schoolId, CancellationToken ct);
    Task<SchoolCampus?> FindByIdAsync(Guid id, CancellationToken ct);
    Task<SchoolCampus?> FindBySchoolAndNameAsync(Guid schoolId, string name, CancellationToken ct);
    Task AddAsync(SchoolCampus campus, CancellationToken ct);
}