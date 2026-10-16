using ms_school_management.Api.School.Domain.Model;

namespace ms_school_management.Api.School.Domain.Ports.Out;

public interface ISchoolAdminRepository
{
    Task<SchoolAdmin?> FindByProfileIdAsync(Guid profileId, CancellationToken ct);

    /// <summary>
    /// Crea o actualiza la relacion perfil-colegio. Idempotente por ProfileId: si
    /// ya existe, cambia el colegio en vez de insertar una fila que violaria
    /// UQ_SchoolAdmin_Profile. Necesario porque el consumidor de Kafka puede
    /// reintentar la misma orden mas de una vez.
    /// </summary>
    Task<SchoolAdmin> UpsertAsync(Guid profileId, Guid schoolId, CancellationToken ct);
}