using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Application.UseCase;
using ms_school_management.Api.School.Domain.Ports.In;

namespace ms_school_management.Api.School.Infrastructure.Controller;

[ApiController]
[Route("api/v1/schools")]
public sealed class SchoolController : ControllerBase
{
    private readonly ICreateSchoolUseCase _createSchool;
    private readonly ICreateSchoolWithCampusDetailsUseCase _createSchoolWithCampuses;
    private readonly IGetSchoolUseCase _getSchool;
    private readonly IListSchoolsUseCase _listSchools;
    private readonly IUpdateSchoolUseCase _updateSchool;
    private readonly IUpdateSchoolWithCampusesUseCase _updateWithCampuses;
    private readonly IListSchoolCampusesUseCase _listCampuses;
    private readonly IDeleteSchoolUseCase _deleteSchool;
    private readonly ISearchSchoolsUseCase _searchSchools;

    public SchoolController(
        ICreateSchoolUseCase createSchool,
        ICreateSchoolWithCampusDetailsUseCase createSchoolWithCampuses,
        IGetSchoolUseCase getSchool,
        IListSchoolsUseCase listSchools,
        IUpdateSchoolUseCase updateSchool,
        IUpdateSchoolWithCampusesUseCase updateWithCampuses,
        IListSchoolCampusesUseCase listCampuses,
        IDeleteSchoolUseCase deleteSchool,
        ISearchSchoolsUseCase searchSchools)
    {
        _createSchool = createSchool;
        _createSchoolWithCampuses = createSchoolWithCampuses;
        _getSchool = getSchool;
        _listSchools = listSchools;
        _updateSchool = updateSchool;
        _updateWithCampuses = updateWithCampuses;
        _listCampuses = listCampuses;
        _deleteSchool = deleteSchool;
        _searchSchools = searchSchools;
    }

    [EnableRateLimiting("writes")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SchoolRequestDto request)
    {
        var id = await _createSchool.ExecuteAsync(request);
        return CreatedAtAction(nameof(Get), new { id }, null);
    }

    /// <summary>
    /// Alta de colegio con sus sedes. Es la ruta que consume el formulario de
    /// registro de colegio del frontend, donde las sedes se capturan como una
    /// lista dinamica de sedes adicionales opcionales.
    ///
    /// Devuelve 201 con el colegio y las sedes creadas (con sus ids) para que el
    /// cliente pueda usarlos de inmediato, por ejemplo al registrar al primer
    /// admin con su sede.
    /// </summary>
    [EnableRateLimiting("writes")]
    [HttpPost("with-campuses")]
    public async Task<IActionResult> CreateWithCampuses([FromBody] SchoolWithCampusesRequestDto request)
    {
        try
        {
            var result = await _createSchoolWithCampuses.ExecuteAsync(request);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid request",
                detail: ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var schools = await _listSchools.ExecuteAsync();
        return Ok(schools);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string search)
    {
        var schools = await _searchSchools.ExecuteAsync(search);
        return Ok(schools);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var school = await _getSchool.ExecuteAsync(id);
        return school is null ? NotFound() : Ok(school);
    }

    [HttpGet("{id:guid}/campuses")]
    public async Task<IActionResult> ListCampuses(Guid id)
    {
        var campuses = await _listCampuses.ExecuteAsync(id);
        return Ok(campuses);
    }

    [EnableRateLimiting("writes")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SchoolRequestDto request)
    {
        await _updateSchool.ExecuteAsync(id, request);
        return NoContent();
    }

    [EnableRateLimiting("writes")]
    [HttpPut("{id:guid}/with-campuses")]
    public async Task<IActionResult> UpdateWithCampuses(Guid id, [FromBody] SchoolWithCampusesRequestDto request)
    {
        await _updateWithCampuses.ExecuteAsync(id, request);
        return NoContent();
    }

    [EnableRateLimiting("writes")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _deleteSchool.ExecuteAsync(id);
        return NoContent();
    }
}
