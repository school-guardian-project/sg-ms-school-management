using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Application.UseCase;

public sealed class GetAdminSchoolService : IGetAdminSchoolUseCase
{
    private readonly ISchoolAdminRepository _schoolAdminRepository;
    private readonly ISchoolRepository _schoolRepository;

    public GetAdminSchoolService(
        ISchoolAdminRepository schoolAdminRepository,
        ISchoolRepository schoolRepository)
    {
        _schoolAdminRepository = schoolAdminRepository;
        _schoolRepository = schoolRepository;
    }

    public async Task<AdminSchoolDto?> ExecuteAsync(Guid profileId, CancellationToken ct = default)
    {
        var link = await _schoolAdminRepository.FindByProfileIdAsync(profileId, ct);
        if (link is null)
            return null;

        var school = await _schoolRepository.FindByIdAsync(link.SchoolId);
        if (school is null)
            return null;

        return new AdminSchoolDto(school.Id, school.Name, school.CityId, school.Status.ToString());
    }
}