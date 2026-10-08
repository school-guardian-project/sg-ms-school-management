using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using ms_school_management.Api.School.Application.UseCase;
using ms_school_management.Api.School.Domain.Model;
using ms_school_management.Api.School.Domain.Ports.Out;
using ms_school_management.Api.School.Infrastructure.Controller.Mapper;
using ms_school_management.Api.School.Infrastructure.Persistence.Repository;
using SchoolModel = ms_school_management.Api.School.Domain.Model.School;

namespace ms_school_management.Tests.School;

public class ListSchoolsServiceTests
{
    private static readonly Guid SabanaId = Guid.Parse("a2000001-0000-4000-8000-000000000001");
    private static readonly Guid OrienteId = Guid.Parse("a2000002-0000-4000-8000-000000000002");
    private static readonly Guid CampusSabanaId = Guid.Parse("a3000005-0000-4000-8000-000000000005");
    private static readonly Guid ProfileId = Guid.NewGuid();

    private static IMapper CreateMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<SchoolProfile>());

        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    private static SchoolModel NewSchool(Guid id, string name) => new()
    {
        Id = id,
        CityId = Guid.NewGuid(),
        Logo = Array.Empty<byte>(),
        Name = name,
        Address = "Calle 1",
        Phone = 300111222,
        Email = "info@example.edu.co",
        Status = Status.Active
    };

    private static ListSchoolsService CreateService(ITenantProvider tenant)
    {
        var schools = new[]
        {
            NewSchool(SabanaId, "Institucion Educativa Sabana"),
            NewSchool(OrienteId, "Colegio Tecnico Del Oriente")
        };

        var resolver = new TenantSchoolResolver(
            tenant,
            new FakeSchoolAdminRepository(adminSchoolId: SabanaId),
            new FakeCampusRepository(),
            new MemoryCache(new MemoryCacheOptions()));

        return new ListSchoolsService(new FakeSchoolRepository(schools), resolver, CreateMapper());
    }

    [Fact]
    public async Task ExecuteAsync_SinToken_RetornaTodosLosColegios()
    {
        var service = CreateService(new FakeTenantProvider(shouldFilter: false));

        var result = await service.ExecuteAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ExecuteAsync_Admin_ResuelvePorSchoolAdminConPrioridad()
    {
        // El registro SchoolAdmin (Sabana) debe ganar sobre el claim (Oriente).
        var service = CreateService(new FakeTenantProvider(
            shouldFilter: true, roleId: 1, profileId: ProfileId, schoolId: OrienteId));

        var result = await service.ExecuteAsync();

        var school = Assert.Single(result);
        Assert.Equal("Institucion Educativa Sabana", school.Name);
    }

    [Fact]
    public async Task ExecuteAsync_AdminSinRegistro_UsaClaimSchoolId()
    {
        var service = CreateService(new FakeTenantProvider(
            shouldFilter: true, roleId: 1, profileId: null, schoolId: OrienteId));

        var result = await service.ExecuteAsync();

        var school = Assert.Single(result);
        Assert.Equal("Colegio Tecnico Del Oriente", school.Name);
    }

    [Fact]
    public async Task ExecuteAsync_StudentConCampus_RetornaColegioDuenoDeSuSede()
    {
        var service = CreateService(new FakeTenantProvider(
            shouldFilter: true, roleId: 2, campusId: CampusSabanaId));

        var result = await service.ExecuteAsync();

        var school = Assert.Single(result);
        Assert.Equal(SabanaId, school.Id);
    }

    [Fact]
    public async Task ExecuteAsync_StudentSinCampus_NoFiltra()
    {
        var service = CreateService(new FakeTenantProvider(shouldFilter: true, roleId: 2));

        var result = await service.ExecuteAsync();

        Assert.Equal(2, result.Count);
    }

    private sealed class FakeTenantProvider : ITenantProvider
    {
        public FakeTenantProvider(
            bool shouldFilter,
            int? roleId = null,
            Guid? campusId = null,
            Guid? schoolId = null,
            Guid? profileId = null)
        {
            ShouldFilter = shouldFilter;
            RoleId = roleId;
            CampusId = campusId;
            SchoolId = schoolId;
            ProfileId = profileId;
        }

        public int? RoleId { get; }
        public Guid? CampusId { get; }
        public Guid? SchoolId { get; }
        public Guid? ProfileId { get; }
        public bool ShouldFilter { get; }
    }

    private sealed class FakeSchoolAdminRepository : ISchoolAdminRepository
    {
        private readonly Guid? _adminSchoolId;

        public FakeSchoolAdminRepository(Guid? adminSchoolId) => _adminSchoolId = adminSchoolId;

        public Task<SchoolAdmin?> FindByProfileIdAsync(Guid profileId, CancellationToken ct) =>
            Task.FromResult(profileId == ProfileId && _adminSchoolId is { } schoolId
                ? new SchoolAdmin { Id = Guid.NewGuid(), SchoolId = schoolId, ProfileId = profileId, Status = Status.Active }
                : null);

        public Task<SchoolAdmin> UpsertAsync(Guid profileId, Guid schoolId, CancellationToken ct) =>
            throw new NotSupportedException();
    }

    private sealed class FakeCampusRepository : ICampusRepository
    {
        public Task<List<SchoolCampus>> ListBySchoolIdAsync(Guid schoolId, CancellationToken ct) =>
            Task.FromResult(new List<SchoolCampus>());

        public Task<SchoolCampus?> FindByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(id == CampusSabanaId
                ? new SchoolCampus { Id = id, SchoolId = SabanaId, Name = "Sede A", Address = "Calle 2", Status = Status.Active }
                : null);

        public Task<SchoolCampus?> FindBySchoolAndNameAsync(Guid schoolId, string name, CancellationToken ct) =>
            Task.FromResult<SchoolCampus?>(null);

        public Task AddAsync(SchoolCampus campus, CancellationToken ct) =>
            throw new NotSupportedException();
    }

    private sealed class FakeSchoolRepository : ISchoolRepository
    {
        private readonly List<SchoolModel> _schools;

        public FakeSchoolRepository(params SchoolModel[] schools) => _schools = schools.ToList();

        public Task SaveAsync(SchoolModel school) => Task.CompletedTask;

        public Task<SchoolModel?> FindByIdAsync(Guid id) =>
            Task.FromResult(_schools.FirstOrDefault(s => s.Id == id));

        public Task<IReadOnlyList<SchoolModel>> FindAllAsync() =>
            Task.FromResult<IReadOnlyList<SchoolModel>>(_schools);

        public Task UpdateAsync(SchoolModel school) => Task.CompletedTask;

        public Task DeleteAsync(Guid id) => Task.CompletedTask;

        public Task<string?> GetCityNameAsync(Guid cityId, CancellationToken ct = default) =>
            Task.FromResult<string?>(null);
    }
}
