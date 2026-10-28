using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ms_school_management.Api.Infrastructure.Auth;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Application.Mapper;
using ms_school_management.Api.School.Application.Search.Strategy;
using ms_school_management.Api.School.Application.Security;
using ms_school_management.Api.School.Application.UseCase;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;
using ms_school_management.Api.School.Infrastructure.Controller;
using ms_school_management.Api.School.Infrastructure.Persistence.Context;
using ms_school_management.Api.School.Infrastructure.Persistence.Entity;
using ms_school_management.Api.School.Infrastructure.Persistence.Repository;

namespace ms_school_management.Tests.School;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class SchoolStatusEndpointTests
{
    private WebApplication _app = null!;
    private SqliteConnection _connection = null!;
    private HttpClient _client = null!;
    private readonly Guid _schoolId = Guid.NewGuid();
    private readonly Guid _activeId = Guid.NewGuid();
    private readonly Guid _inactiveId = Guid.NewGuid();
    private readonly Guid _otherSchoolId = Guid.NewGuid();
    private readonly Guid _otherCampusId = Guid.NewGuid();

    [SetUp]
    public async Task SetUp()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddControllers().AddApplicationPart(typeof(SchoolController).Assembly);
        builder.Services.AddAuthentication("test").AddScheme<AuthenticationSchemeOptions, TestAuthentication>("test", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter("writes", policy =>
        {
            policy.PermitLimit = 100;
            policy.Window = TimeSpan.FromMinutes(1);
        }));
        builder.Services.AddDbContext<SchoolManagementContext>(options => options.UseSqlite(_connection));
        builder.Services.AddAutoMapper(cfg => cfg.AddProfile<SchoolProfile>());
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantProvider, TenantProvider>();
        builder.Services.AddScoped<ITenantSchoolResolver, TestSchoolResolver>();
        builder.Services.AddScoped<ISchoolAccessPolicy, SchoolAccessPolicy>();
        builder.Services.AddScoped<ISchoolRepository, SchoolRepositoryImpl>();
        builder.Services.AddScoped<ICampusRepository, CampusRepositoryImpl>();
        builder.Services.AddScoped<ICreateSchoolUseCase, CreateSchoolService>();
        builder.Services.AddScoped<ICreateSchoolWithCampusDetailsUseCase, CreateSchoolWithCampusDetailsService>();
        builder.Services.AddScoped<IGetSchoolUseCase, GetSchoolService>();
        builder.Services.AddScoped<IListSchoolsUseCase, ListSchoolsService>();
        builder.Services.AddScoped<IUpdateSchoolUseCase, UpdateSchoolService>();
        builder.Services.AddScoped<IUpdateSchoolWithCampusesUseCase, UpdateSchoolWithCampusesService>();
        builder.Services.AddScoped<IListSchoolCampusesUseCase, ListSchoolCampusesService>();
        builder.Services.AddScoped<IDeleteSchoolUseCase, DeleteSchoolService>();
        builder.Services.AddScoped<ISearchSchoolsUseCase, SearchSchoolsService>();
        builder.Services.AddScoped<IGetCampusUseCase, GetCampusService>();
        _app = builder.Build();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.UseRateLimiter();
        _app.MapControllers().RequireAuthorization();
        await _app.StartAsync();
        _client = _app.GetTestClient();
        _client.DefaultRequestHeaders.Add("X-Test-Role", "5");
        _client.DefaultRequestHeaders.Add("X-Test-School", _schoolId.ToString());
        using var scope = _app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchoolManagementContext>();
        await context.Database.EnsureCreatedAsync();
        context.Schools.AddRange(School(_schoolId), School(_otherSchoolId));
        context.Campuses.AddRange(
            Campus(_activeId, _schoolId, "Main", "Active"),
            Campus(_inactiveId, _schoolId, "North", "Inactive"),
            Campus(_otherCampusId, _otherSchoolId, "Other", "Inactive"));
        await context.SaveChangesAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        _client.Dispose();
        await _app.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Test]
    public async Task Default_campus_list_remains_active_only()
    {
        var response = await _client.GetAsync($"/api/v1/schools/{_schoolId}/campuses");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var campuses = await response.Content.ReadFromJsonAsync<SchoolCampusResponseDto[]>();
        Assert.That(campuses!.Select(c => c.Id), Is.EqualTo(new[] { _activeId }));
    }

    [Test]
    public async Task Management_list_includes_inactive_status_and_location_only_for_selected_school()
    {
        SetRole("1");
        var response = await _client.GetAsync($"/api/v1/schools/{_schoolId}/campuses?includeInactive=true");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var campuses = await response.Content.ReadFromJsonAsync<SchoolCampusResponseDto[]>();
        Assert.That(campuses, Has.Length.EqualTo(2));
        Assert.That(campuses!.Single(c => c.Id == _inactiveId).Status, Is.EqualTo("Inactive"));
        Assert.That(campuses!.Single(c => c.Id == _inactiveId).Latitude, Is.EqualTo(4.5m));
    }

    [Test]
    public async Task Admin_cannot_list_inactive_campuses_from_another_school()
    {
        SetRole("1");
        var response = await _client.GetAsync($"/api/v1/schools/{_otherSchoolId}/campuses?includeInactive=true");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task Inactive_campus_detail_still_returns_not_found()
    {
        var response = await _client.GetAsync($"/api/v1/campuses/{_inactiveId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [TestCase("1")]
    [TestCase("5")]
    public async Task Admin_and_superadmin_can_preserve_then_reactivate_an_existing_campus(string role)
    {
        SetRole(role);
        var response = await _client.PutAsJsonAsync($"/api/v1/schools/{_schoolId}/with-campuses", Request("Inactive"));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(await CampusStatus(_inactiveId), Is.EqualTo("Inactive"));
        response = await _client.PutAsJsonAsync($"/api/v1/schools/{_schoolId}/with-campuses", Request("Active"));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(await CampusStatus(_inactiveId), Is.EqualTo("Active"));
        Assert.That(await CampusStatus(_otherCampusId), Is.EqualTo("Inactive"));
    }

    [Test]
    public async Task Campus_reactivation_requires_update_permission()
    {
        SetRole("4");
        var response = await _client.PutAsJsonAsync($"/api/v1/schools/{_schoolId}/with-campuses", Request("Active"));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(await CampusStatus(_inactiveId), Is.EqualTo("Inactive"));
    }

    [Test]
    public async Task Cross_school_update_is_rejected_without_changing_campus_status()
    {
        SetRole("1");
        var response = await _client.PutAsJsonAsync($"/api/v1/schools/{_otherSchoolId}/with-campuses", Request("Active"));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(await CampusStatus(_inactiveId), Is.EqualTo("Inactive"));
    }

    [Test]
    public async Task Anonymous_management_read_is_rejected()
    {
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        var response = await _client.GetAsync($"/api/v1/schools/{_schoolId}/campuses?includeInactive=true");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Invalid_campus_status_returns_bad_request()
    {
        var response = await _client.PutAsJsonAsync($"/api/v1/schools/{_schoolId}/with-campuses", Request("Unknown"));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await CampusStatus(_inactiveId), Is.EqualTo("Inactive"));
    }

    [Test]
    public async Task School_reactivation_does_not_reactivate_its_campuses()
    {
        var response = await _client.DeleteAsync($"/api/v1/schools/{_schoolId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var detailResponse = await _client.GetAsync($"/api/v1/schools/{_schoolId}");
        Assert.That(detailResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var school = await detailResponse.Content.ReadFromJsonAsync<SchoolResponseDto>();
        Assert.That(school!.Status, Is.EqualTo("Inactive"));
        var request = new SchoolRequestDto(school.CityId, school.Logo, school.Name, school.Address,
            school.Latitude, school.Longitude, school.Phone, school.Email, school.Website, school.Theme, "Active");
        response = await _client.PutAsJsonAsync($"/api/v1/schools/{_schoolId}", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        using var scope = _app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchoolManagementContext>();
        Assert.That((await context.Schools.FindAsync(_schoolId))!.Status, Is.EqualTo("Active"));
        Assert.That(await CampusStatus(_inactiveId), Is.EqualTo("Inactive"));
    }

    [Test]
    public async Task Management_school_list_contains_both_states_for_client_filtering()
    {
        await _client.DeleteAsync($"/api/v1/schools/{_schoolId}");
        var response = await _client.GetAsync("/api/v1/schools");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var schools = await response.Content.ReadFromJsonAsync<SchoolListDto[]>();
        Assert.That(schools!.Select(s => s.Status), Is.EquivalentTo(new[] { "Active", "Inactive" }));
    }

    [Test]
    public async Task Campus_from_another_school_cannot_be_reactivated_even_by_submitting_its_id()
    {
        using var scope = _app.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IUpdateSchoolWithCampusesUseCase>();
        var request = Request("Active") with
        {
            Campuses = [new SchoolCampusRequestDto(_otherCampusId, "Other", "Other street", null, null, "Active")]
        };
        Assert.ThrowsAsync<KeyNotFoundException>(() => service.ExecuteAsync(_schoolId, request));
        Assert.That(await CampusStatus(_otherCampusId), Is.EqualTo("Inactive"));
    }

    private SchoolWithCampusesRequestDto Request(string status) => new(Guid.NewGuid(), [], "School",
        "Main street", null, null, 3001234567, "school@test.com", null, "Primaria",
        [new SchoolCampusRequestDto(_inactiveId, "North", "North street", 4.5m, -74m, status)]);

    [Test]
    public async Task Address_limits_are_consistent_for_creation_and_update(
        [Values(false, true)] bool create,
        [Values(30, 31, 255, 256)] int length,
        [Values(false, true)] bool coordinates)
    {
        var address = new string('A', length);
        var campus = new SchoolCampusRequestDto(create ? null : _inactiveId, "North", address,
            coordinates ? 0m : null, coordinates ? -180m : null);
        var request = Request("Active") with
        {
            Address = address,
            Latitude = coordinates ? 90m : null,
            Longitude = coordinates ? 180m : null,
            Campuses = [campus]
        };
        var response = create
            ? await _client.PostAsJsonAsync("/api/v1/schools/with-campuses", request)
            : await _client.PutAsJsonAsync($"/api/v1/schools/{_schoolId}/with-campuses", request);
        Assert.That(response.StatusCode, Is.EqualTo(length > 255 ? HttpStatusCode.BadRequest
            : create ? HttpStatusCode.Created : HttpStatusCode.NoContent));
        using var scope = _app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchoolManagementContext>();
        if (length > 255)
        {
            Assert.That(await context.Schools.CountAsync(), Is.EqualTo(2));
            Assert.That((await context.Schools.FindAsync(_schoolId))!.Address, Is.EqualTo("Main street"));
            Assert.That((await context.Campuses.FindAsync(_inactiveId))!.Address, Is.EqualTo("Campus street"));
            return;
        }
        var id = create
            ? (await response.Content.ReadFromJsonAsync<SchoolWithCampusesResponseDto>())!.Id
            : _schoolId;
        var school = (await context.Schools.FindAsync(id))!;
        var savedCampus = await context.Campuses.SingleAsync(c => c.SchoolId == id && c.Name == "North");
        Assert.That(school.Address, Is.EqualTo(address));
        Assert.That(savedCampus.Address, Is.EqualTo(address));
        Assert.That(school.Latitude, Is.EqualTo(request.Latitude));
        Assert.That(school.Longitude, Is.EqualTo(request.Longitude));
        Assert.That(savedCampus.Latitude, Is.EqualTo(campus.Latitude));
        Assert.That(savedCampus.Longitude, Is.EqualTo(campus.Longitude));
    }

    [Test]
    public async Task Incomplete_coordinate_pairs_return_problem_without_saving(
        [Values(false, true)] bool create,
        [Values(false, true)] bool campus,
        [Values(false, true)] bool latitudeOnly)
    {
        decimal? latitude = latitudeOnly ? 2.9m : null;
        decimal? longitude = latitudeOnly ? null : -75.2m;
        var request = Request("Active") with
        {
            Latitude = campus ? null : latitude,
            Longitude = campus ? null : longitude,
            Campuses = [new SchoolCampusRequestDto(create ? null : _inactiveId, "North",
                "North street", campus ? latitude : null, campus ? longitude : null)]
        };
        var response = create
            ? await _client.PostAsJsonAsync("/api/v1/schools/with-campuses", request)
            : await _client.PutAsJsonAsync($"/api/v1/schools/{_schoolId}/with-campuses", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
        Assert.That(problem!.Status, Is.EqualTo(400));
        Assert.That(problem.Detail, Does.Contain("coordinates must be supplied together"));
        using var scope = _app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchoolManagementContext>();
        Assert.That(await context.Schools.CountAsync(), Is.EqualTo(2));
        Assert.That((await context.Schools.FindAsync(_schoolId))!.Address, Is.EqualTo("Main street"));
        Assert.That(await CampusStatus(_inactiveId), Is.EqualTo("Inactive"));
    }

    [Test]
    public async Task Plain_school_writes_reject_incomplete_coordinates(
        [Values(false, true)] bool create,
        [Values(false, true)] bool latitudeOnly)
    {
        var request = new SchoolRequestDto(Guid.NewGuid(), [], "School", "Main street",
            latitudeOnly ? 2.9m : null, latitudeOnly ? null : -75.2m,
            3001234567, "school@test.com", null, "Primaria");
        var response = create
            ? await _client.PostAsJsonAsync("/api/v1/schools", request)
            : await _client.PutAsJsonAsync($"/api/v1/schools/{_schoolId}", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
        Assert.That(problem!.Detail, Does.Contain("coordinates must be supplied together"));
    }

    private void SetRole(string role)
    {
        _client.DefaultRequestHeaders.Remove("X-Test-Role");
        _client.DefaultRequestHeaders.Add("X-Test-Role", role);
    }

    private async Task<string> CampusStatus(Guid id)
    {
        using var scope = _app.Services.CreateScope();
        return (await scope.ServiceProvider.GetRequiredService<SchoolManagementContext>().Campuses.FindAsync(id))!.Status;
    }

    private static SchoolEntity School(Guid id) => new()
    {
        Id = id, CityId = Guid.NewGuid(), Logo = [], Name = "School", Address = "Main street",
        Phone = 3001234567, Email = "school@test.com", Status = "Active"
    };

    private static SchoolCampusEntity Campus(Guid id, Guid schoolId, string name, string status) => new()
    {
        Id = id, SchoolId = schoolId, Name = name, Address = "Campus street",
        Latitude = 4.5m, Longitude = -74m, Status = status
    };

    private sealed class TestSchoolResolver(ITenantProvider tenant) : ITenantSchoolResolver
    {
        public Task<Guid?> ResolveAllowedSchoolIdAsync(CancellationToken ct = default) =>
            Task.FromResult(tenant.ShouldFilter ? tenant.SchoolId : null);
    }

    private sealed class TestAuthentication(
        IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var role = Request.Headers["X-Test-Role"].ToString();
            if (string.IsNullOrEmpty(role)) return Task.FromResult(AuthenticateResult.NoResult());
            var claims = new List<Claim>
            {
                new(SgClaimTypes.RoleId, role),
                new(SgClaimTypes.SchoolId, Request.Headers["X-Test-School"].ToString())
            };
            if (role is "1" or "5")
            {
                claims.Add(new Claim(SgClaimTypes.Permissions, "schools.update"));
                claims.Add(new Claim(SgClaimTypes.Permissions, "schools.read"));
                if (role == "5")
                    claims.Add(new Claim(SgClaimTypes.Permissions, "schools.create"));
            }
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(user, Scheme.Name)));
        }
    }
}
