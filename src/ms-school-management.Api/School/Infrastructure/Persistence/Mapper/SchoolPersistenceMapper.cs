using ms_school_management.Api.School.Domain.Model;
using ms_school_management.Api.School.Infrastructure.Persistence.Entity;

namespace ms_school_management.Api.School.Infrastructure.Persistence.Mapper;

/// <summary>
/// Conversiones entity &lt;-&gt; dominio de la capa de persistencia. Vive en
/// Infrastructure (depende de las entities de EF) y la usan los repositorios:
/// SchoolRepositoryImpl, CampusRepositoryImpl y SchoolAdminRepositoryImpl.
/// </summary>
public static class SchoolPersistenceMapper
{
    /// <summary>Colegio persistido -> modelo de dominio.</summary>
    public static Domain.Model.School ToDomain(SchoolEntity e) => new()
    {
        Id = e.Id,
        CityId = e.CityId,
        Logo = e.Logo,
        Name = e.Name,
        Address = e.Address,
        Latitude = e.Latitude,
        Longitude = e.Longitude,
        Phone = e.Phone,
        Email = e.Email,
        Website = e.Website,
        Theme = e.Theme,
        Status = Enum.Parse<Status>(e.Status)
    };

    /// <summary>Modelo de dominio -> colegio a persistir.</summary>
    public static SchoolEntity ToEntity(Domain.Model.School school) => new()
    {
        Id = school.Id,
        CityId = school.CityId,
        Logo = school.Logo,
        Name = school.Name,
        Address = school.Address,
        Latitude = school.Latitude,
        Longitude = school.Longitude,
        Phone = school.Phone,
        Email = school.Email,
        Website = school.Website,
        Theme = school.Theme,
        Status = school.Status.ToString(),
    };

    /// <summary>Sede a persistir (incluye coordenadas).</summary>
    public static SchoolCampusEntity ToEntity(SchoolCampus campus) => new()
    {
        Id = campus.Id,
        SchoolId = campus.SchoolId,
        Name = campus.Name,
        Address = campus.Address,
        Latitude = campus.Latitude,
        Longitude = campus.Longitude,
        Status = campus.Status.ToString(),
    };

    /// <summary>Sede persistida -> modelo de dominio, con coordenadas.</summary>
    public static SchoolCampus ToDomain(SchoolCampusEntity campus) => new()
    {
        Id = campus.Id,
        SchoolId = campus.SchoolId,
        Name = campus.Name,
        Address = campus.Address,
        Latitude = campus.Latitude,
        Longitude = campus.Longitude,
        Status = Enum.Parse<Status>(campus.Status),
    };

    /// <summary>
    /// Sede persistida -> modelo de dominio SIN coordenadas: es la proyeccion
    /// historica de CampusRepositoryImpl (el listado y la busqueda por nombre/id
    /// solo necesitan id, colegio, nombre, direccion y estado). Se mantiene tal
    /// cual para no alterar la respuesta de esos casos de uso.
    /// </summary>
    public static SchoolCampus ToCampusWithoutCoordinates(SchoolCampusEntity e) => new()
    {
        Id = e.Id,
        SchoolId = e.SchoolId,
        Name = e.Name,
        Address = e.Address,
        Status = Enum.Parse<Status>(e.Status)
    };

    /// <summary>Vinculo perfil-colegio persistido -> modelo de dominio.</summary>
    public static SchoolAdmin ToDomain(SchoolAdminEntity e) => new()
    {
        Id = e.Id,
        SchoolId = e.SchoolId,
        ProfileId = e.ProfileId,
        Status = Enum.Parse<Status>(e.Status)
    };
}
