using Microsoft.AspNetCore.Mvc;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Ports.In;

namespace ms_school_management.Api.School.Infrastructure.Controller;

[ApiController]
[Route("api/v1/schools")]
public sealed class CampusController : ControllerBase
{
    private readonly IListCampusesBySchoolUseCase _listCampusesUseCase;

    public CampusController(IListCampusesBySchoolUseCase listCampusesUseCase)
    {
        _listCampusesUseCase = listCampusesUseCase;
    }

    [HttpGet("{schoolId:guid}/campuses")]
    public async Task<IActionResult> ListBySchool(Guid schoolId, CancellationToken ct)
    {
        var campuses = await _listCampusesUseCase.ExecuteAsync(schoolId, ct);
        return Ok(campuses);
    }
}
