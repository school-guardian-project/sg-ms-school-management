using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Application.Mapper;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Application.UseCase;

public sealed class GetCampusService : IGetCampusUseCase
{
    private readonly ICampusRepository _campusRepository;

    public GetCampusService(ICampusRepository campusRepository)
    {
        _campusRepository = campusRepository;
    }

    public async Task<CampusDetailDto?> ExecuteAsync(Guid campusId, CancellationToken ct = default)
    {
        var campus = await _campusRepository.FindByIdAsync(campusId, ct);
        return campus is null ? null : CampusMapper.ToCampusDetail(campus);
    }

    public async Task<CampusDetailDto?> ExecuteByNameAsync(Guid schoolId, string name, CancellationToken ct = default)
    {
        var campus = await _campusRepository.FindBySchoolAndNameAsync(schoolId, name, ct);
        return campus is null ? null : CampusMapper.ToCampusDetail(campus);
    }
}
