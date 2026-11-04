using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ms_school_management.Api.Infrastructure.Auth;
using ms_school_management.Api.School.Application.Security;
using ms_school_management.Api.School.Domain.Exceptions;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Tests.Auth;

public class TenantScopeTests
{
    private static readonly Guid SchoolId = Guid.NewGuid();

    [Test]
    public void TenantProvider_UsuarioSinRoleId_SeFiltra()
    {
        var tenant = TenantFor(Authenticated());

        Assert.That(tenant.ShouldFilter, Is.True);
    }

    [Test]
    public void TenantProvider_SuperAdmin_NoSeFiltra()
    {
        var tenant = TenantFor(Authenticated(new Claim(SgClaimTypes.RoleId, "5")));

        Assert.That(tenant.ShouldFilter, Is.False);
    }

    [Test]
    public void TenantProvider_Admin_SeFiltra()
    {
        var tenant = TenantFor(Authenticated(new Claim(SgClaimTypes.RoleId, "1")));

        Assert.That(tenant.ShouldFilter, Is.True);
    }

    [Test]
    public void TenantProvider_SinUsuario_NoSeFiltra_LlamadaInterna()
    {
        // gRPC interno: no hay usuario autenticado.
        var tenant = TenantFor(new ClaimsPrincipal(new ClaimsIdentity()));

        Assert.That(tenant.ShouldFilter, Is.False);
    }

    [Test]
    public async Task Politica_TenantSinColegio_ScopeNada_YDetalleDenegado()
    {
        var policy = new SchoolAccessPolicy(new StubTenant(shouldFilter: true), new StubResolver(null));

        Assert.That(await policy.GetScopeAsync(), Is.SameAs(SchoolScope.Nothing));
        Assert.ThrowsAsync<SchoolAccessDeniedException>(() => policy.EnsureCanAccessAsync(SchoolId));
    }

    [Test]
    public async Task Politica_TenantConColegio_SoloElSuyo()
    {
        var policy = new SchoolAccessPolicy(new StubTenant(shouldFilter: true), new StubResolver(SchoolId));

        var scope = await policy.GetScopeAsync();

        Assert.That(scope.Allows(SchoolId), Is.True);
        Assert.That(scope.Allows(Guid.NewGuid()), Is.False);
    }

    [Test]
    public async Task Politica_SinFiltro_VeTodos()
    {
        var policy = new SchoolAccessPolicy(new StubTenant(shouldFilter: false), new StubResolver(null));

        Assert.That(await policy.GetScopeAsync(), Is.SameAs(SchoolScope.All));
        Assert.DoesNotThrowAsync(() => policy.EnsureCanAccessAsync(Guid.NewGuid()));
    }

    private static ClaimsPrincipal Authenticated(params Claim[] claims) =>
        new(new ClaimsIdentity(
            claims.Append(new Claim(SgClaimTypes.Subject, Guid.NewGuid().ToString())), "Bearer"));

    private static TenantProvider TenantFor(ClaimsPrincipal user) =>
        new(new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = user } });

    private sealed class StubTenant(bool shouldFilter) : ITenantProvider
    {
        public int? RoleId => null;
        public Guid? CampusId => null;
        public Guid? SchoolId => null;
        public Guid? ProfileId => null;
        public bool ShouldFilter { get; } = shouldFilter;
    }

    private sealed class StubResolver(Guid? schoolId) : ITenantSchoolResolver
    {
        public Task<Guid?> ResolveAllowedSchoolIdAsync(CancellationToken ct = default) => Task.FromResult(schoolId);
    }
}
