using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using ms_school_management.Api.Infrastructure.Auth;
using ms_school_management.Api.School.Application.Security;
using ms_school_management.Api.School.Application.UseCase;
using ms_school_management.Api.School.Domain.Exceptions;
using ms_school_management.Api.School.Domain.Model;
using ms_school_management.Api.School.Domain.Ports.Out;
using ms_school_management.Api.School.Infrastructure.Controller;
using NUnit.Framework;

namespace ms_school_management.Tests.Auth;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class SchoolAuthorizationTests
{
    private const int AdminRole = 1;
    private const int ParentRole = 4;

    private static readonly Guid OwnSchool = Guid.NewGuid();
    private static readonly Guid OtherSchool = Guid.NewGuid();

    // --- Regla de pertenencia (capa de aplicacion) ---

    [Test]
    public void SinTenantFiltrable_SuperAdminOGrpcInterno_AccedeACualquierColegio()
    {
        var policy = Policy(shouldFilter: false, roleId: null, resolvedSchool: null);

        Assert.DoesNotThrowAsync(() => policy.EnsureCanAccessAsync(OtherSchool));
    }

    [Test]
    public void Admin_SuPropioColegio_Accede()
    {
        var policy = Policy(shouldFilter: true, roleId: AdminRole, resolvedSchool: OwnSchool);

        Assert.DoesNotThrowAsync(() => policy.EnsureCanAccessAsync(OwnSchool));
    }

    [Test]
    public void Admin_OtroColegio_SeNiega()
    {
        var policy = Policy(shouldFilter: true, roleId: AdminRole, resolvedSchool: OwnSchool);

        var ex = Assert.ThrowsAsync<SchoolAccessDeniedException>(() => policy.EnsureCanAccessAsync(OtherSchool));
        Assert.That(ex!.SchoolId, Is.EqualTo(OtherSchool));
    }

    [Test]
    public void Acudiente_OtroColegio_SeNiega()
    {
        var policy = Policy(shouldFilter: true, roleId: ParentRole, resolvedSchool: OwnSchool);

        Assert.ThrowsAsync<SchoolAccessDeniedException>(() => policy.EnsureCanAccessAsync(OtherSchool));
    }

    [Test]
    public void TenantFiltrableSinColegioResoluble_SeNiega()
    {
        // En el listado null significa "sin filtro"; para el acceso puntual no.
        var policy = Policy(shouldFilter: true, roleId: ParentRole, resolvedSchool: null);

        Assert.ThrowsAsync<SchoolAccessDeniedException>(() => policy.EnsureCanAccessAsync(OwnSchool));
    }

    [Test]
    public void GetCampus_SedeDeOtroColegio_SeNiega()
    {
        var campus = new SchoolCampus { Id = Guid.NewGuid(), SchoolId = OtherSchool, Name = "Sede", Address = "Calle 1", Status = Status.Active };
        var service = new GetCampusService(
            new SingleCampusRepository(campus),
            Policy(shouldFilter: true, roleId: AdminRole, resolvedSchool: OwnSchool));

        Assert.ThrowsAsync<SchoolAccessDeniedException>(() => service.ExecuteAsync(campus.Id));
    }

    [Test]
    public async Task GetCampus_SedeDeSuColegio_SeDevuelve()
    {
        var campus = new SchoolCampus { Id = Guid.NewGuid(), SchoolId = OwnSchool, Name = "Sede", Address = "Calle 1", Status = Status.Active };
        var service = new GetCampusService(
            new SingleCampusRepository(campus),
            Policy(shouldFilter: true, roleId: AdminRole, resolvedSchool: OwnSchool));

        var result = await service.ExecuteAsync(campus.Id);

        Assert.That(result?.Id, Is.EqualTo(campus.Id));
    }

    [Test]
    public async Task GetCampus_Inexistente_DevuelveNullSinConsultarLaPolitica()
    {
        var service = new GetCampusService(
            new SingleCampusRepository(),
            Policy(shouldFilter: true, roleId: AdminRole, resolvedSchool: null));

        Assert.That(await service.ExecuteAsync(Guid.NewGuid()), Is.Null);
    }

    // --- Traduccion HTTP ---

    [Test]
    public void FiltroDeAcceso_TraduceLaExcepcionA403()
    {
        var context = new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
            Exception = new SchoolAccessDeniedException(OtherSchool),
        };

        new SchoolAccessDeniedFilterAttribute().OnException(context);

        Assert.That(context.ExceptionHandled, Is.True);
        Assert.That((context.Result as ObjectResult)?.StatusCode, Is.EqualTo(403));
    }

    [Test]
    public void FiltroDeAcceso_IgnoraOtrasExcepciones()
    {
        var context = new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>())
        {
            Exception = new InvalidOperationException("boom"),
        };

        new SchoolAccessDeniedFilterAttribute().OnException(context);

        Assert.That(context.ExceptionHandled, Is.False);
    }

    // --- Reglas por endpoint (atributos del controller) ---

    [TestCase(nameof(SchoolController.Create))]
    [TestCase(nameof(SchoolController.CreateWithCampuses))]
    [TestCase(nameof(SchoolController.Delete))]
    public void AltaYBorrado_SoloSuperAdmin(string action)
    {
        var roles = Attributes<RequireRoleAttribute>(action);

        Assert.That(roles, Has.Count.EqualTo(1));
        Assert.That(RoleIds(roles[0]), Is.EquivalentTo(new[] { RequireRoleAttribute.SuperAdminRoleId }));
    }

    [TestCase(nameof(SchoolController.List), "schools.read")]
    [TestCase(nameof(SchoolController.Search), "schools.read")]
    [TestCase(nameof(SchoolController.Update), "schools.update")]
    [TestCase(nameof(SchoolController.UpdateWithCampuses), "schools.update")]
    [TestCase(nameof(SchoolController.Create), "schools.create")]
    [TestCase(nameof(SchoolController.CreateWithCampuses), "schools.create")]
    public void Endpoint_ExigeElPermiso(string action, string permission)
    {
        var permissions = Attributes<RequirePermissionAttribute>(action).Select(PermissionOf);

        Assert.That(permissions, Does.Contain(permission));
    }

    [TestCase(nameof(SchoolController.Get))]
    [TestCase(nameof(SchoolController.ListCampuses))]
    public void LecturaPuntual_SoloExigeToken_LaPertenenciaLaAplicaLaPolitica(string action)
    {
        // El movil la usa con cualquier rol para mostrar SU colegio y sus sedes.
        Assert.That(Attributes<RequireRoleAttribute>(action), Is.Empty);
        Assert.That(Attributes<RequirePermissionAttribute>(action), Is.Empty);
    }

    [Test]
    public void Controllers_TraducenLaDenegacionDeAcceso()
    {
        Assert.That(typeof(SchoolController).GetCustomAttribute<SchoolAccessDeniedFilterAttribute>(), Is.Not.Null);
        Assert.That(typeof(CampusController).GetCustomAttribute<SchoolAccessDeniedFilterAttribute>(), Is.Not.Null);
    }

    private static SchoolAccessPolicy Policy(bool shouldFilter, int? roleId, Guid? resolvedSchool) =>
        new(new FakeTenant(shouldFilter, roleId), new FakeResolver(resolvedSchool));

    private static List<T> Attributes<T>(string action) where T : Attribute =>
        typeof(SchoolController).GetMethod(action)!.GetCustomAttributes<T>().ToList();

    private static string[] RoleIds(RequireRoleAttribute attribute) =>
        (string[])typeof(RequireRoleAttribute)
            .GetField("_allowedRoleIds", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(attribute)!;

    private static string PermissionOf(RequirePermissionAttribute attribute) =>
        (string)typeof(RequirePermissionAttribute)
            .GetField("_permission", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(attribute)!;

    private sealed class FakeTenant(bool shouldFilter, int? roleId) : ITenantProvider
    {
        public int? RoleId { get; } = roleId;
        public Guid? CampusId => null;
        public Guid? SchoolId => null;
        public Guid? ProfileId => null;
        public bool ShouldFilter { get; } = shouldFilter;
    }

    private sealed class FakeResolver(Guid? schoolId) : ITenantSchoolResolver
    {
        public Task<Guid?> ResolveAllowedSchoolIdAsync(CancellationToken ct = default) => Task.FromResult(schoolId);
    }

    private sealed class SingleCampusRepository(params SchoolCampus[] campuses) : ICampusRepository
    {
        public Task<List<SchoolCampus>> ListBySchoolIdAsync(Guid schoolId, CancellationToken ct) =>
            Task.FromResult(campuses.Where(c => c.SchoolId == schoolId).ToList());

        public Task<SchoolCampus?> FindByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(campuses.FirstOrDefault(c => c.Id == id));

        public Task<SchoolCampus?> FindBySchoolAndNameAsync(Guid schoolId, string name, CancellationToken ct) =>
            Task.FromResult(campuses.FirstOrDefault(c => c.SchoolId == schoolId && c.Name == name));

        public Task AddAsync(SchoolCampus campus, CancellationToken ct) => Task.CompletedTask;
    }
}
