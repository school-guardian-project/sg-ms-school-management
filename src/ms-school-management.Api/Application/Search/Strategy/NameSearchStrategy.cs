using ms_school_management.Api.School.Application.Dto;

namespace ms_school_management.Api.School.Application.Search.Strategy;

public class NameSearchStrategy : ISchoolSearchStrategy
{
    public bool CanHandle(string search)
    {
        return true;
    }

    public IEnumerable<SchoolListDto> Search(string search, IEnumerable<SchoolListDto> schools)
    {
        return schools.Where(school =>
            school.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
            || school.Address.Contains(search, StringComparison.OrdinalIgnoreCase));
    }
}
