namespace ms_school_management.Api.School.Application.Dto;

/// <summary>Colegio que administra un perfil, con lo que un consumidor necesita para
/// autorizarlo o mostrarlo: id, nombre, ciudad y estado.</summary>
public record AdminSchoolDto(Guid Id, string Name, Guid CityId, string Status);

/// <summary>Sede resuelta por id o por (colegio, nombre).</summary>
public record CampusDetailDto(Guid Id, Guid SchoolId, string Name, string Address, string Status);

/// <summary>Resultado de registrar la relacion perfil-colegio.</summary>
public record LinkedAdminSchoolDto(Guid LinkId, Guid SchoolId, string SchoolName);