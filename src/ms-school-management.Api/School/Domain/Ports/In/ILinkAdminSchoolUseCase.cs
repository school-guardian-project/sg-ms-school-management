using ms_school_management.Api.School.Application.Dto;

namespace ms_school_management.Api.School.Domain.Ports.In;

/// <summary>Registra que un perfil administra un colegio. Idempotente.</summary>
public interface ILinkAdminSchoolUseCase
{
    Task<LinkedAdminSchoolDto> ExecuteAsync(Guid profileId, Guid schoolId, CancellationToken ct = default);
}
