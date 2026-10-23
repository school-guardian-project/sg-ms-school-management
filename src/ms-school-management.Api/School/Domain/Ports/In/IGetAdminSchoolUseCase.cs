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
