namespace ms_school_management.Api.School.Domain.Exceptions;

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
