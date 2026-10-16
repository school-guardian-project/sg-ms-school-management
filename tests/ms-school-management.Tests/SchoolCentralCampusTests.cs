using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Application.UseCase;
using ms_school_management.Api.School.Domain.Model;
using ms_school_management.Api.School.Domain.Ports.Out;
using ms_school_management.Api.School.Infrastructure.Controller.Mapper;
using NUnit.Framework;
using SchoolModel = ms_school_management.Api.School.Domain.Model.School;

namespace ms_school_management.Tests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
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

    [TestCase(false)]
    [TestCase(true)]
    public async Task WithCampusesPersistsCentralAndOptionalAdditionalCampus(bool additional)
    {
        var repository = new Repository();
        var request = new SchoolWithCampusesRequestDto(Guid.NewGuid(), [], "Test school",
            "Central street 123", 2.9m, -75.2m, 3001234567, "school@example.invalid", null, "Primaria",
            additional ? [new SchoolCampusRequestDto(null, "North", "North street 456", null, null)] : []);
        var result = await new CreateSchoolWithCampusDetailsService(repository, Mapper()).ExecuteAsync(request);
        Assert.That(result.Campuses.Count, Is.EqualTo(additional ? 2 : 1));
        var central = repository.Campuses[0];
        Assert.That(central.Name, Is.EqualTo("Sede central"));
        Assert.That(central.SchoolId, Is.EqualTo(result.Id));
        Assert.That(central.Address, Is.EqualTo(request.Address));
        Assert.That(central.Latitude, Is.EqualTo(request.Latitude));
        Assert.That(central.Longitude, Is.EqualTo(request.Longitude));
        Assert.That(central.Status, Is.EqualTo(Status.Active));
        Assert.That(central.Id, Is.EqualTo(result.Campuses[0].Id));
    }

    [Test]
    public async Task PlainCreationAlsoPersistsCentralInSameOperation()
    {
        var repository = new Repository();
        var request = new SchoolRequestDto(Guid.NewGuid(), [], "Test school", "Central street 123",
            null, null, 3001234567, "school@example.invalid", null, "Primaria");
        var id = await new CreateSchoolService(repository, Mapper()).ExecuteAsync(request);
        var campusItems = repository.Campuses;
        Assert.That(campusItems, Has.Exactly(1).Items);
        var campus = campusItems.Single();
        Assert.That(campus.SchoolId, Is.EqualTo(id));
        Assert.That(campus.Address, Is.EqualTo(request.Address));
    }

    [Test]
    public async Task ExistingClientsCanSupplyCentralWithoutCreatingDuplicate()
    {
        var repository = new Repository();
        var request = new SchoolWithCampusesRequestDto(Guid.NewGuid(), [], "Test school",
            "Central street 123", null, null, 3001234567, "school@example.invalid", null, "Primaria",
            [new SchoolCampusRequestDto(null, " SEDE CENTRAL ", "Explicit central address", null, null)]);
        var result = await new CreateSchoolWithCampusDetailsService(repository, Mapper()).ExecuteAsync(request);
        var campusItems = result.Campuses;
        Assert.That(campusItems, Has.Exactly(1).Items);
        var campus = campusItems.Single();
        Assert.That(campus.Name, Is.EqualTo("SEDE CENTRAL"));
        Assert.That(campus.Address, Is.EqualTo("Explicit central address"));
    }
}
