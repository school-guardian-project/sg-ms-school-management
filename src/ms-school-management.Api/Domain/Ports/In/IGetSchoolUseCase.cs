using ms_school_management.Api.School.Application.Dto;

namespace ms_school_management.Api.School.Domain.Ports.In;

public interface IGetSchoolUseCase
{
    Task<SchoolResponseDto?> ExecuteAsync(Guid id);
}
