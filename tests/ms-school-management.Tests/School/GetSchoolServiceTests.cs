using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ms_school_management.Api.School.Application.UseCase;
using ms_school_management.Api.School.Domain.Model;
using ms_school_management.Api.School.Domain.Ports.Out;
using ms_school_management.Api.School.Infrastructure.Controller.Mapper;
using SchoolModel = ms_school_management.Api.School.Domain.Model.School;

namespace ms_school_management.Tests.School;

public class GetSchoolServiceTests
{
    private static IMapper CreateMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<SchoolProfile>());

        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    private static SchoolModel NewSchool(Guid id, Guid cityId) => new()
    {
        Id = id,
        CityId = cityId,
        Logo = Array.Empty<byte>(),
        Name = "San Martin",
        Address = "Calle 1",
        Phone = 300111222,
        Email = "info@sanmartin.edu.co",
        Status = Status.Active
    };

    [Fact]
    public async Task ExecuteAsync_ConCiudad_RellenaCityName()
    {
        var schoolId = Guid.NewGuid();
        var cityId = Guid.NewGuid();
        var repository = new FakeSchoolRepository(NewSchool(schoolId, cityId))
        {
            CityNames = { [cityId] = "Bogota" }
        };
        var service = new GetSchoolService(repository, CreateMapper());

        var result = await service.ExecuteAsync(schoolId);

        Assert.NotNull(result);
        Assert.Equal("Bogota", result.CityName);
        Assert.Equal(cityId, result.CityId);
    }

    [Fact]
    public async Task ExecuteAsync_CiudadSinRegistro_CityNameVacio()
    {
        var schoolId = Guid.NewGuid();
        var repository = new FakeSchoolRepository(NewSchool(schoolId, Guid.NewGuid()));
        var service = new GetSchoolService(repository, CreateMapper());

        var result = await service.ExecuteAsync(schoolId);

        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.CityName);
    }

    [Fact]
    public async Task ExecuteAsync_SinEscuela_RetornaNull()
    {
        var service = new GetSchoolService(new FakeSchoolRepository(), CreateMapper());

        Assert.Null(await service.ExecuteAsync(Guid.NewGuid()));
    }

    private sealed class FakeSchoolRepository : ISchoolRepository
    {
        private readonly List<SchoolModel> _schools;

        public FakeSchoolRepository(params SchoolModel[] schools) => _schools = schools.ToList();

        public readonly Dictionary<Guid, string> CityNames = new();

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
            Task.FromResult(CityNames.TryGetValue(cityId, out var name) ? name : null);
    }
}
