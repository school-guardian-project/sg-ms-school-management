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
    private readonly ISearchSchoolsUseCase _searchSchools;

    public SchoolController(
        ICreateSchoolUseCase createSchool,
        IGetSchoolUseCase getSchool,
        IListSchoolsUseCase listSchools,
        IUpdateSchoolUseCase updateSchool,
        IDeleteSchoolUseCase deleteSchool,
        ISearchSchoolsUseCase searchSchools)
    {
        _createSchool = createSchool;
        _getSchool = getSchool;
        _listSchools = listSchools;
        _updateSchool = updateSchool;
        _deleteSchool = deleteSchool;
        _searchSchools = searchSchools;
    }

    [HttpPost]
    public async Task<IActionResult> Create(SchoolRequestDto request)
    {
        var id = await _createSchool.ExecuteAsync(request);
        return CreatedAtAction(nameof(Get), new { id }, null);
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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, SchoolRequestDto request)
    {
        await _updateSchool.ExecuteAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _deleteSchool.ExecuteAsync(id);
        return NoContent();
    }
}
