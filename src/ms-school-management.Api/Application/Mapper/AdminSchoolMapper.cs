using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Model;

namespace ms_school_management.Api.School.Application.Mapper;

/// <summary>
/// Conversiones de la relacion perfil-colegio (dominio -> DTO). Concentra el
/// mapeo que antes estaba inline en GetAdminSchoolService y LinkAdminSchoolService.
/// </summary>
public static class AdminSchoolMapper
{
    /// <summary>Colegio administrado -> dto consumido por REST y por el RPC GetAdminSchool.</summary>
    public static AdminSchoolDto ToAdminSchool(Domain.Model.School school)
        => new(school.Id, school.Name, school.CityId, school.Status.ToString());

    /// <summary>Relacion recien registrada -> confirmacion del RPC LinkAdminSchool.</summary>
    public static LinkedAdminSchoolDto ToLinkedAdminSchool(Guid linkId, Domain.Model.School school)
        => new(linkId, school.Id, school.Name);
}
