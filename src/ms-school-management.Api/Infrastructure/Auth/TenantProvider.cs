using System.Security.Claims;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.Infrastructure.Auth;

/// <summary>
/// Lee los claims del access token RS256 de ms-iam, ya validado por
/// <see cref="JwtAuthenticationExtensions"/>: sub (profileId), roleId
/// (1=Admin, 2=Student, 3=Driver, 4=Parent, 5=SuperAdmin), campusId y
/// schoolId (ambos opcionales).
/// </summary>
public sealed class TenantProvider : ITenantProvider
{
    private const int SuperAdminRoleId = 5;

    private readonly ClaimsPrincipal? _user;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _user = httpContextAccessor.HttpContext?.User;
    }

    public int? RoleId => ParseInt(GetClaim(SgClaimTypes.RoleId));

    public Guid? CampusId => ParseGuid(GetClaim(SgClaimTypes.CampusId));

    public Guid? SchoolId => ParseGuid(GetClaim(SgClaimTypes.SchoolId));

    public Guid? ProfileId => ParseGuid(GetClaim(SgClaimTypes.Subject));

    public bool ShouldFilter =>
        _user?.Identity?.IsAuthenticated == true && RoleId is not null && RoleId != SuperAdminRoleId;

    private string? GetClaim(string type) =>
        _user?.FindFirst(type)?.Value;

    private static int? ParseInt(string? value) =>
        int.TryParse(value, out var parsed) ? parsed : null;

    private static Guid? ParseGuid(string? value) =>
        Guid.TryParse(value, out var parsed) ? parsed : null;
}
