using Microsoft.Extensions.Caching.Memory;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Infrastructure.Persistence.Repository;

/// <summary>
/// Reglas de filtrado multi-tenant para GET /api/v1/schools:
/// - roleId 1 (Admin): SchoolAdmin de la tabla por profileId (doble garantia,
///   tiene prioridad) y si no, el claim schoolId.
/// - roleId 2/3/4 (Student/Driver/Parent): el colegio dueno de su sede
///   (SchoolCampus.SchoolId del claim campusId).
/// - Sin tenant filtrable (sin token, invalido o SuperAdmin): null.
///
/// Las tablas School.SchoolAdmin y School.SchoolCampus son propiedad de este
/// servicio, por lo que la resolucion es una lectura local. Se cachea por
/// tenant (IMemoryCache, 5 min) porque /schools y /schools/search golpean la
/// misma resolucion en cada request.
/// </summary>
public sealed class TenantSchoolResolver : ITenantSchoolResolver
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly ITenantProvider _tenant;
    private readonly ISchoolAdminRepository _schoolAdminRepository;
    private readonly ICampusRepository _campusRepository;
    private readonly IMemoryCache _cache;

    public TenantSchoolResolver(
        ITenantProvider tenant,
        ISchoolAdminRepository schoolAdminRepository,
        ICampusRepository campusRepository,
        IMemoryCache cache)
    {
        _tenant = tenant;
        _schoolAdminRepository = schoolAdminRepository;
        _campusRepository = campusRepository;
        _cache = cache;
    }

    public async Task<Guid?> ResolveAllowedSchoolIdAsync(CancellationToken ct = default)
    {
        if (!_tenant.ShouldFilter) return null;

        var cacheKey = $"tenant-school:{_tenant.RoleId}:{_tenant.ProfileId}:{_tenant.CampusId}:{_tenant.SchoolId}";

        if (_cache.TryGetValue(cacheKey, out Guid? cached)) return cached;

        var schoolId = _tenant.RoleId == 1
            ? await ResolveForAdminAsync(ct)
            : await ResolveForCampusMemberAsync(ct);

        _cache.Set(cacheKey, schoolId, CacheTtl);

        return schoolId;
    }

    private async Task<Guid?> ResolveForAdminAsync(CancellationToken ct)
    {
        if (_tenant.ProfileId is { } profileId)
        {
            var admin = await _schoolAdminRepository.FindByProfileIdAsync(profileId, ct);
            if (admin is not null) return admin.SchoolId;
        }

        return _tenant.SchoolId;
    }

    private async Task<Guid?> ResolveForCampusMemberAsync(CancellationToken ct)
    {
        if (_tenant.CampusId is not { } campusId) return null;

        var campus = await _campusRepository.FindByIdAsync(campusId, ct);

        return campus?.SchoolId;
    }
}
