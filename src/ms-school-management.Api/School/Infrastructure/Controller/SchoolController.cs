using Microsoft.AspNetCore.Mvc;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Ports.In;

namespace ms_school_management.Api.School.Infrastructure.Controller;

[ApiController]
[Route("api/v1/schools")]
public sealed class SchoolController : ControllerBase
{
    private readonly ICreateSchoolUseCase _createSchool;
    private readonly IGetSchoolUseCase _getSchool;
    private readonly IListSchoolsUseCase _listSchools;
    private readonly IUpdateSchoolUseCase _updateSchool;
    private readonly IDeleteSchoolUseCase _deleteSchool;
    private readonly ICreateSchoolWithCampusesUseCase _createWithCampuses;
    private readonly IUpdateSchoolWithCampusesUseCase _updateWithCampuses;
    private readonly IListSchoolCampusesUseCase _listCampuses;

    public SchoolController(
        ICreateSchoolUseCase createSchool,
        IGetSchoolUseCase getSchool,
        IListSchoolsUseCase listSchools,
        IUpdateSchoolUseCase updateSchool,
        IDeleteSchoolUseCase deleteSchool,
        ICreateSchoolWithCampusesUseCase createWithCampuses,
        IUpdateSchoolWithCampusesUseCase updateWithCampuses,
        IListSchoolCampusesUseCase listCampuses)
    {
        _createSchool = createSchool;
        _getSchool = getSchool;
        _listSchools = listSchools;
        _updateSchool = updateSchool;
        _deleteSchool = deleteSchool;
        _createWithCampuses = createWithCampuses;
        _updateWithCampuses = updateWithCampuses;
        _listCampuses = listCampuses;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SchoolRequestDto request)
    {
        var id = await _createSchool.ExecuteAsync(request);
        return CreatedAtAction(nameof(Get), new { id }, null);
    }

    [HttpPost("with-campuses")]
    public async Task<IActionResult> CreateWithCampuses([FromBody] SchoolWithCampusesRequestDto request)
    {
        var result = await _createWithCampuses.ExecuteAsync(request);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var schools = await _listSchools.ExecuteAsync();
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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SchoolRequestDto request)
    {
        await _updateSchool.ExecuteAsync(id, request);
        return NoContent();
    }

    [HttpPut("{id:guid}/with-campuses")]
    public async Task<IActionResult> UpdateWithCampuses(Guid id, [FromBody] SchoolWithCampusesRequestDto request)
    {
        await _updateWithCampuses.ExecuteAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _deleteSchool.ExecuteAsync(id);
        return NoContent();
    }
}
