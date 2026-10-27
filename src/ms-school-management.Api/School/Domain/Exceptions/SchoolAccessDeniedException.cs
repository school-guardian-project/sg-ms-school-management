namespace ms_school_management.Api.School.Domain.Exceptions;

/// <summary>
/// El usuario autenticado intenta leer o modificar un colegio que no es el suyo.
/// </summary>
public sealed class SchoolAccessDeniedException : Exception
{
    public SchoolAccessDeniedException(Guid schoolId)
        : base($"Access to school {schoolId} is not allowed for the current user")
    {
        SchoolId = schoolId;
    }

    public Guid SchoolId { get; }
}
