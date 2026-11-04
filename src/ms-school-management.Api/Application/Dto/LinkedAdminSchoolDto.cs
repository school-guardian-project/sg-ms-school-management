namespace ms_school_management.Api.School.Application.Dto;

/// <summary>Resultado de registrar la relacion perfil-colegio.</summary>
public record LinkedAdminSchoolDto(Guid LinkId, Guid SchoolId, string SchoolName);
