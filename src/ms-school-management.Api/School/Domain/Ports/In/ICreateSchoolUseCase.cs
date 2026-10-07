using ms_school_management.Api.School.Application.Dto;

namespace ms_school_management.Api.School.Domain.Ports.In;

public interface ICreateSchoolUseCase
{
    Task<Guid> ExecuteAsync(SchoolRequestDto request);
}

/// <summary>Alta de colegio con sus sedes en una sola transaccion.</summary>
public interface ICreateSchoolWithCampusesUseCase
{
    Task<CreateSchoolWithCampusesResponseDto> ExecuteAsync(CreateSchoolWithCampusesDto request);
}
