using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Application.UseCase;

public sealed class LinkAdminSchoolService : ILinkAdminSchoolUseCase
{
    private readonly ISchoolAdminRepository _schoolAdminRepository;
    private readonly ISchoolRepository _schoolRepository;

    public LinkAdminSchoolService(
        ISchoolAdminRepository schoolAdminRepository,
        ISchoolRepository schoolRepository)
    {
        _schoolAdminRepository = schoolAdminRepository;
        _schoolRepository = schoolRepository;
    }

    public async Task<LinkedAdminSchoolDto> ExecuteAsync(
        Guid profileId, Guid schoolId, CancellationToken ct = default)
    {
        // El colegio tiene que existir: el id viene del exterior (ms-iam, que a su
        // vez lo recibio del frontend) y no queremos dejar una relacion colgando
        // hacia un id que no corresponde. La FK lo impediria, pero un
        // NotFound explicito le dice al consumidor cual de los dos ids falló.
        var school = await _schoolRepository.FindByIdAsync(schoolId);
        if (school is null)
        {
            throw new SchoolNotFoundException(schoolId);
        }

        var link = await _schoolAdminRepository.UpsertAsync(profileId, schoolId, ct);

        return new LinkedAdminSchoolDto(link.Id, school.Id, school.Name);
    }
}

/// <summary>
/// El colegio referenciado no existe. Se propaga al cliente gRPC como
/// <c>NOT_FOUND</c> y a REST como 404: un id de colegio que no existe es un dato
/// invalido, no una falla del servidor.
/// </summary>
public sealed class SchoolNotFoundException : Exception
{
    public SchoolNotFoundException(Guid schoolId)
        : base($"School {schoolId} was not found")
    {
    }
}