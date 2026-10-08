using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ms_school_management.Api.School.Application.Search;
using ms_school_management.Api.School.Application.Search.Strategy;
using ms_school_management.Api.School.Application.UseCase;
using ms_school_management.Api.School.Domain.Model;
using ms_school_management.Api.School.Domain.Ports.Out;
using ms_school_management.Api.School.Infrastructure.Controller.Mapper;
using SchoolModel = ms_school_management.Api.School.Domain.Model.School;

namespace ms_school_management.Tests.School;

public class SearchSchoolsServiceTests
{
    private static IMapper CreateMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<SchoolProfile>());

        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    private static SchoolModel NewSchool(string name, string address) => new()
    {
        Id = Guid.NewGuid(),
        CityId = Guid.NewGuid(),
        Logo = Array.Empty<byte>(),
        Name = name,
        Address = address,
        Phone = 300111222,
        Email = "info@example.edu.co",
        Status = Status.Active
    };

    private static SearchSchoolsService CreateService(params SchoolModel[] schools)
    {
        var listUseCase = new ListSchoolsService(
            new FakeSchoolRepository(schools),
            new NoFilterTenantSchoolResolver(),
            CreateMapper());
        return new SearchSchoolsService(listUseCase, new ISchoolSearchStrategy[]
        {
            new NameSearchStrategy()
        });
    }

    [Fact]
    public async Task ExecuteAsync_TerminoVacio_RetornaListaVacia()
    {
        var service = CreateService(NewSchool("San Martin", "Calle 1"));

        Assert.Empty(await service.ExecuteAsync("   "));
    }

    [Fact]
    public async Task ExecuteAsync_Nombre_RetornaSoloCoincidencias()
    {
        var service = CreateService(
            NewSchool("San Martin", "Calle 1"),
            NewSchool("Los Alpes", "Carrera 2"));

        var result = (await service.ExecuteAsync("martin")).ToList();

        var school = Assert.Single(result);
        Assert.Equal("San Martin", school.Name);
    }

    [Fact]
    public async Task ExecuteAsync_Direccion_RetornaSoloCoincidencias()
    {
        var service = CreateService(
            NewSchool("San Martin", "Calle 10"),
            NewSchool("Los Alpes", "Carrera 2"));

        var result = (await service.ExecuteAsync("calle 10")).ToList();

        var school = Assert.Single(result);
        Assert.Equal("Calle 10", school.Address);
    }

    [Fact]
    public async Task ExecuteAsync_SinCoincidencias_RetornaVacio()
    {
        var service = CreateService(NewSchool("San Martin", "Calle 1"));

        Assert.Empty(await service.ExecuteAsync("inexistente"));
    }

    private sealed class NoFilterTenantSchoolResolver : ITenantSchoolResolver
    {
        public Task<Guid?> ResolveAllowedSchoolIdAsync(CancellationToken ct = default) =>
            Task.FromResult<Guid?>(null);
    }

    private sealed class FakeSchoolRepository : ISchoolRepository
    {
        private readonly List<SchoolModel> _schools;

        public FakeSchoolRepository(params SchoolModel[] schools) => _schools = schools.ToList();

        public Task SaveAsync(SchoolModel school)
        {
            _schools.Add(school);
            return Task.CompletedTask;
        }

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
