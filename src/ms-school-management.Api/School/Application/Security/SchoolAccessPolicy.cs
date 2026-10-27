using ms_school_management.Api.School.Domain.Exceptions;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Application.Security;

/// <summary>
/// - Sin tenant filtrable (SuperAdmin, o llamada interna gRPC sin usuario): acceso total.
/// - Admin, Student, Driver, Parent: solo el colegio que resuelve
///   <see cref="ITenantSchoolResolver"/> (el de su SchoolAdmin o el de su sede).
/// - Tenant filtrable cuyo colegio no se puede resolver: sin acceso. A diferencia
///   del listado, aqui un null no significa "sin filtro".
/// </summary>
public sealed class SchoolAccessPolicy : ISchoolAccessPolicy
{
    private readonly ITenantProvider _tenant;
    private readonly ITenantSchoolResolver _tenantSchoolResolver;

    public SchoolAccessPolicy(ITenantProvider tenant, ITenantSchoolResolver tenantSchoolResolver)
    {
        _tenant = tenant;
        _tenantSchoolResolver = tenantSchoolResolver;
    }

    public async Task EnsureCanAccessAsync(Guid schoolId, CancellationToken ct = default)
    {
        if (!_tenant.ShouldFilter) return;

        var allowedSchoolId = await _tenantSchoolResolver.ResolveAllowedSchoolIdAsync(ct);
        if (allowedSchoolId != schoolId)
        {
            throw new SchoolAccessDeniedException(schoolId);
        }
    }
}
