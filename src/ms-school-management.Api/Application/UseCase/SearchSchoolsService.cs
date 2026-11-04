using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Application.Search;
using ms_school_management.Api.School.Domain.Ports.In;

namespace ms_school_management.Api.School.Application.UseCase;

public sealed class SearchSchoolsService : ISearchSchoolsUseCase
{
    private readonly IListSchoolsUseCase _listSchools;
    private readonly IEnumerable<ISchoolSearchStrategy> _strategies;

    public SearchSchoolsService(IListSchoolsUseCase listSchools, IEnumerable<ISchoolSearchStrategy> strategies)
    {
        _listSchools = listSchools;
        _strategies = strategies;
    }

    public async Task<IEnumerable<SchoolListDto>> ExecuteAsync(string search)
    {
        search = search?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(search)) return [];

        var strategy = _strategies.FirstOrDefault(x => x.CanHandle(search));

        if (strategy is null) return [];

        var schools = await _listSchools.ExecuteAsync();

        return strategy.Search(search, schools);
    }
}
