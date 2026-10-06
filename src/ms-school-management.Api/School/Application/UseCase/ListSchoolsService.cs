using AutoMapper;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Application.UseCase;

public sealed class ListSchoolsService : IListSchoolsUseCase
{
    private readonly ISchoolRepository _repository;
    private readonly IMapper _mapper;

    public ListSchoolsService(ISchoolRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<SchoolListDto>> ExecuteAsync()
    {
        var schools = await _repository.FindAllAsync();
        return _mapper.Map<IReadOnlyList<SchoolListDto>>(schools);
    }
}
