namespace ms_school_management.Api.School.Application.Dto;

/// <summary>Colegio que administra un perfil, con lo que un consumidor necesita para
/// autorizarlo o mostrarlo: id, nombre, ciudad y estado.</summary>
public record AdminSchoolDto(Guid Id, string Name, Guid CityId, string Status);
