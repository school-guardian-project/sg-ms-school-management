using Microsoft.AspNetCore.Mvc;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Application.UseCase;
using ms_school_management.Api.School.Domain.Model;
using ms_school_management.Api.School.Domain.Ports.Out;
using ms_school_management.Api.School.Infrastructure.Controller;

using ms_school_management.Tests.Fakes;

namespace ms_school_management.Tests.School;

/// <summary>
/// Camino REST del GET /api/v1/campuses/{id}: el caso de uso ya existia (se
/// consume desde el RPC GetCampus de gRPC); lo que se cubre aqui es que el
/// controller traduzca bien "no existe" a 404 y "existe" a 200 con el dto.
/// </summary>
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class CampusControllerTests
{
    private sealed class FakeCampusRepository : ICampusRepository
    {
        private readonly List<SchoolCampus> _campuses;

        public FakeCampusRepository(params SchoolCampus[] campuses) => _campuses = campuses.ToList();

        public Task<List<SchoolCampus>> ListBySchoolIdAsync(Guid schoolId, CancellationToken ct) =>
            Task.FromResult(_campuses.Where(c => c.SchoolId == schoolId).ToList());

        public Task<SchoolCampus?> FindByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(_campuses.FirstOrDefault(c => c.Id == id));

        public Task<SchoolCampus?> FindBySchoolAndNameAsync(Guid schoolId, string name, CancellationToken ct) =>
            Task.FromResult(_campuses.FirstOrDefault(c => c.SchoolId == schoolId && c.Name == name));

        public Task AddAsync(SchoolCampus campus, CancellationToken ct)
        {
            _campuses.Add(campus);
            return Task.CompletedTask;
        }
    }

    private static CampusController Controller(params SchoolCampus[] campuses) =>
        new(new GetCampusService(new FakeCampusRepository(campuses), new AllowAllSchoolAccessPolicy()));

    [Test]
    public async Task Get_CampusExistente_RetornaOkConElDetalle()
    {
        var id = Guid.NewGuid();
        var schoolId = Guid.NewGuid();
        var campus = new SchoolCampus
        {
            Id = id,
            SchoolId = schoolId,
            Name = "Sede norte",
            Address = "Calle 10 # 20-30",
            Status = Status.Active
        };

        var result = await Controller(campus).Get(id, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var dto = (CampusDetailDto)((OkObjectResult)result).Value!;
        Assert.That(dto.Id, Is.EqualTo(id));
        Assert.That(dto.SchoolId, Is.EqualTo(schoolId));
        Assert.That(dto.Name, Is.EqualTo("Sede norte"));
        Assert.That(dto.Address, Is.EqualTo("Calle 10 # 20-30"));
        Assert.That(dto.Status, Is.EqualTo("Active"));
    }

    [Test]
    public async Task Get_CampusInexistente_RetornaNotFound()
    {
        var result = await Controller().Get(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
}
