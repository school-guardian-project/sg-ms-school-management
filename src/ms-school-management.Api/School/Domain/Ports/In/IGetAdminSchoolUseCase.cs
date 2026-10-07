using ms_school_management.Api.School.Application.Dto;

namespace ms_school_management.Api.School.Domain.Ports.In;

/// <summary>
/// Resuelve el colegio de un administrador. Devuelve <c>null</c> cuando la
/// relacion aun no existe: es un caso valido (admin recien creado), no un error.
/// </summary>
public interface IGetAdminSchoolUseCase
{
    Task<AdminSchoolDto?> ExecuteAsync(Guid profileId, CancellationToken ct = default);
}

public interface IGetCampusUseCase
{
    Task<CampusDetailDto?> ExecuteAsync(Guid campusId, CancellationToken ct = default);
    Task<CampusDetailDto?> ExecuteByNameAsync(Guid schoolId, string name, CancellationToken ct = default);
}

/// <summary>Registra que un perfil administra un colegio. Idempotente.</summary>
public interface ILinkAdminSchoolUseCase
{
    Task<LinkedAdminSchoolDto> ExecuteAsync(Guid profileId, Guid schoolId, CancellationToken ct = default);
}