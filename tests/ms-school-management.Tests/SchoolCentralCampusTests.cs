using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Application.UseCase;
using ms_school_management.Api.School.Domain.Model;
using ms_school_management.Api.School.Domain.Ports.Out;
using ms_school_management.Api.School.Infrastructure.Controller.Mapper;
using Xunit;
using SchoolModel = ms_school_management.Api.School.Domain.Model.School;

namespace ms_school_management.Tests;

public class SchoolCentralCampusTests
{
    private sealed class Repository : ISchoolRepository
    {
        public SchoolModel? School { get; private set; }
        public IReadOnlyList<SchoolCampus> Campuses { get; private set; } = [];
        public Task SaveWithCampusesAsync(SchoolModel school, IReadOnlyList<SchoolCampus> campuses)
        {
            School = school;
            Campuses = campuses;
            return Task.CompletedTask;
        }
        public Task SaveAsync(SchoolModel school) => throw new NotSupportedException();
        public Task<SchoolModel?> FindByIdAsync(Guid id) => throw new NotSupportedException();
        public Task<IReadOnlyList<SchoolModel>> FindAllAsync() => throw new NotSupportedException();
        public Task UpdateAsync(SchoolModel school) => throw new NotSupportedException();
        public Task UpdateWithCampusesAsync(SchoolModel school, IReadOnlyList<SchoolCampus> campuses) => throw new NotSupportedException();
        public Task<IReadOnlyList<SchoolCampus>> FindCampusesBySchoolAsync(Guid schoolId) => throw new NotSupportedException();
        public Task DeleteAsync(Guid id) => throw new NotSupportedException();
        public Task<string?> GetCityNameAsync(Guid cityId, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }

    private static IMapper Mapper()
    {
        return new MapperConfiguration(cfg => cfg.AddProfile<SchoolProfile>(), NullLoggerFactory.Instance).CreateMapper();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WithCampusesPersistsCentralAndOptionalAdditionalCampus(bool additional)
    {
        var repository = new Repository();
        var request = new SchoolWithCampusesRequestDto(Guid.NewGuid(), [], "Test school",
            "Central street 123", 2.9m, -75.2m, 3001234567, "school@example.invalid", null, "Primaria",
            additional ? [new SchoolCampusRequestDto(null, "North", "North street 456", null, null)] : []);
        var result = await new CreateSchoolWithCampusesService(repository, Mapper()).ExecuteAsync(request);
        Assert.Equal(additional ? 2 : 1, result.Campuses.Count);
        var central = repository.Campuses[0];
        Assert.Equal("Sede central", central.Name);
        Assert.Equal(result.Id, central.SchoolId);
        Assert.Equal(request.Address, central.Address);
        Assert.Equal(request.Latitude, central.Latitude);
        Assert.Equal(request.Longitude, central.Longitude);
        Assert.Equal(Status.Active, central.Status);
        Assert.Equal(result.Campuses[0].Id, central.Id);
    }

    [Fact]
    public async Task PlainCreationAlsoPersistsCentralInSameOperation()
    {
        var repository = new Repository();
        var request = new SchoolRequestDto(Guid.NewGuid(), [], "Test school", "Central street 123",
            null, null, 3001234567, "school@example.invalid", null, "Primaria");
        var id = await new CreateSchoolService(repository, Mapper()).ExecuteAsync(request);
        var campus = Assert.Single(repository.Campuses);
        Assert.Equal(id, campus.SchoolId);
        Assert.Equal(request.Address, campus.Address);
    }

    [Fact]
    public async Task ExistingClientsCanSupplyCentralWithoutCreatingDuplicate()
    {
        var repository = new Repository();
        var request = new SchoolWithCampusesRequestDto(Guid.NewGuid(), [], "Test school",
            "Central street 123", null, null, 3001234567, "school@example.invalid", null, "Primaria",
            [new SchoolCampusRequestDto(null, " SEDE CENTRAL ", "Explicit central address", null, null)]);
        var result = await new CreateSchoolWithCampusesService(repository, Mapper()).ExecuteAsync(request);
        var campus = Assert.Single(result.Campuses);
        Assert.Equal("SEDE CENTRAL", campus.Name);
        Assert.Equal("Explicit central address", campus.Address);
    }
}
