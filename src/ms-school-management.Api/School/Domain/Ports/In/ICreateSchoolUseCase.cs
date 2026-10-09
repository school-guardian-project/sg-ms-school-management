using ms_school_management.Api.School.Application.Dto;

namespace ms_school_management.Api.School.Domain.Ports.In;

public interface ICreateSchoolUseCase
{
    Task<Guid> ExecuteAsync(SchoolRequestDto request);
}
