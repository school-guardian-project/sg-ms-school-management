namespace ms_school_management.Api.School.Application.Dto;

public record SchoolRequestDto(
    Guid CityId,
    byte[] Logo,
    string Name,
    string Address,
    int Phone,
    string Email,
    string? Website,
    string? Theme);

/// <summary>
/// Alta de colegio con sus sedes. Las sedes se crean en la misma transaccion que
/// el colegio: un colegio sin sedes no es un colegio utilizable en este
/// producto (no habria donde registrar estudiantes ni rutas), asi que pedirlas
/// aqui evita tener que hacer un segundo paso que se puede dejar a medias.
/// </summary>
/// <param name="CampusNames">
/// Nombres de las sedes. No se pide <c>campusId</c>: la sede no existe todavia.
/// La direccion de cada sede se hereda de la direccion del colegio hasta que se
/// edite individualmente.
/// </param>
public record CreateSchoolWithCampusesDto(
    Guid CityId,
    string Name,
    string Address,
    int Phone,
    string Email,
    string? Website,
    string? Theme,
    IReadOnlyList<string> CampusNames,
    byte[]? Logo);

/// <summary>Respuesta del alta: colegio y las sedes creadas, con sus ids.</summary>
public record CreateSchoolWithCampusesResponseDto(
    Guid SchoolId,
    string Name,
    string Address,
    int Phone,
    string Email,
    Guid CityId,
    string Status,
    IReadOnlyList<CreatedCampusDto> Campuses);

public record CreatedCampusDto(Guid Id, string Name, string Address);