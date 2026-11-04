namespace ms_school_management.Api.School.Application.Security;

/// <summary>
/// Regla de pertenencia: un usuario solo accede a su propio colegio.
/// </summary>
public interface ISchoolAccessPolicy
{
    /// <summary>
    /// Lanza <see cref="Domain.Exceptions.SchoolAccessDeniedException"/> cuando
    /// el usuario actual no puede acceder al colegio indicado.
    /// </summary>
    Task EnsureCanAccessAsync(Guid schoolId, CancellationToken ct = default);
}
