namespace ms_school_management.Api.School.Application.Security;

/// <summary>
/// Colegios que el usuario actual puede ver: todos (SuperAdmin o llamada
/// interna sin usuario), solo el suyo, o ninguno cuando su colegio no se pudo
/// resolver. Es la unica fuente de la regla para el listado y el detalle.
/// </summary>
public sealed class SchoolScope
{
    public static SchoolScope All { get; } = new(isUnrestricted: true, schoolId: null);

    public static SchoolScope Nothing { get; } = new(isUnrestricted: false, schoolId: null);

    public static SchoolScope Only(Guid schoolId) => new(isUnrestricted: false, schoolId);

    private SchoolScope(bool isUnrestricted, Guid? schoolId)
    {
        IsUnrestricted = isUnrestricted;
        SchoolId = schoolId;
    }

    public bool IsUnrestricted { get; }

    public Guid? SchoolId { get; }

    public bool Allows(Guid schoolId) => IsUnrestricted || SchoolId == schoolId;
}
