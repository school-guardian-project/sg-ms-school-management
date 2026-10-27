using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Application.Mapper;
using ms_school_management.Api.School.Application.Security;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Application.UseCase;

public sealed class GetCampusService : IGetCampusUseCase
{
    private readonly ICampusRepository _campusRepository;
    private readonly ISchoolAccessPolicy _accessPolicy;

    public GetCampusService(ICampusRepository campusRepository, ISchoolAccessPolicy accessPolicy)
    {
        _campusRepository = campusRepository;
        _accessPolicy = accessPolicy;
    }

    public async Task<CampusDetailDto?> ExecuteAsync(Guid campusId, CancellationToken ct = default)
    {
        var campus = await _campusRepository.FindByIdAsync(campusId, ct);
        if (campus is null) return null;

        await _accessPolicy.EnsureCanAccessAsync(campus.SchoolId, ct);
        return CampusMapper.ToCampusDetail(campus);
    }

    public async Task<CampusDetailDto?> ExecuteByNameAsync(Guid schoolId, string name, CancellationToken ct = default)
    {
        await _accessPolicy.EnsureCanAccessAsync(schoolId, ct);

        var campus = await _campusRepository.FindBySchoolAndNameAsync(schoolId, name, ct);
        return campus is null ? null : CampusMapper.ToCampusDetail(campus);
    }
}
