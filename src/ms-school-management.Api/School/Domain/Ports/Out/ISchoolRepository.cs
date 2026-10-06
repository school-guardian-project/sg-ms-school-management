using ms_school_management.Api.School.Domain.Model;

namespace ms_school_management.Api.School.Domain.Ports.Out;

public interface ISchoolRepository
{
    Task SaveAsync(Domain.Model.School school);
    Task<Domain.Model.School?> FindByIdAsync(Guid id);
    Task<IReadOnlyList<Domain.Model.School>> FindAllAsync();
    Task UpdateAsync(Domain.Model.School school);
    Task DeleteAsync(Guid id);
    Task<string?> GetCityNameAsync(Guid cityId, CancellationToken ct = default);
}
