using ms_school_management.Api.School.Application.Dto;

namespace ms_school_management.Api.School.Application.Search;

public interface ISchoolSearchStrategy
{
    bool CanHandle(string search);

    IEnumerable<SchoolListDto> Search(string search, IEnumerable<SchoolListDto> schools);
}
