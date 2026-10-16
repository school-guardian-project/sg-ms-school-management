namespace ms_school_management.Api.School.Domain.Ports.Out;

/// <summary>
/// Resuelve el unico colegio que el tenant actual puede ver, o null cuando no
/// aplica filtro (sin token, SuperAdmin, o tenant sin colegio resoluble).
/// </summary>
public interface ITenantSchoolResolver
{
    Task<Guid?> ResolveAllowedSchoolIdAsync(CancellationToken ct = default);
}
