using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Model;

namespace ms_school_management.Api.School.Application.Mapper;

/// <summary>
/// Conversiones de sede (dominio -> DTO). Concentra el mapeo que antes vivia
/// inline en GetCampusService, ListSchoolCampusesService,
/// ListCampusesBySchoolService y CreateSchoolWithCampusDetailsService, para que
/// la misma sede se traduzca igual en REST, en el alta con sedes y en gRPC.
/// </summary>
public static class CampusMapper
{
    /// <summary>Sede -> detalle expuesto por GET /campuses/{id} y por el RPC GetCampus.</summary>
    public static CampusDetailDto ToCampusDetail(SchoolCampus campus)
        => new(campus.Id, campus.SchoolId, campus.Name, campus.Address, campus.Status.ToString());

    /// <summary>Sede -> respuesta del alta/edicion y del listado /schools/{id}/campuses.</summary>
    public static SchoolCampusResponseDto ToSchoolCampusResponse(SchoolCampus campus)
        => new(campus.Id, campus.Name, campus.Address, campus.Latitude, campus.Longitude, campus.Status.ToString());

    /// <summary>Sede -> resumen ligero (id, nombre, direccion) del listado por colegio.</summary>
    public static CampusListDto ToCampusList(SchoolCampus campus)
        => new(campus.Id, campus.Name, campus.Address);
}
