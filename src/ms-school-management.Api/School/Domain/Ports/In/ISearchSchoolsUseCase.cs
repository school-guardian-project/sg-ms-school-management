using ms_school_management.Api.School.Application.Dto;

namespace ms_school_management.Api.School.Domain.Ports.In;

public interface ISearchSchoolsUseCase
{
    Task<IEnumerable<SchoolListDto>> ExecuteAsync(string search);
}
