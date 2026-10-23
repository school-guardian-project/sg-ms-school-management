namespace ms_school_management.Api.School.Application.Dto;

/// <summary>Sede resuelta por id o por (colegio, nombre).</summary>
public record CampusDetailDto(Guid Id, Guid SchoolId, string Name, string Address, string Status);
