using AutoMapper;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Application.UseCase;

public sealed class ListSchoolsService : IListSchoolsUseCase
{
    private readonly ISchoolRepository _repository;
    private readonly ITenantSchoolResolver _tenantSchoolResolver;
    private readonly IMapper _mapper;

    public ListSchoolsService(
        ISchoolRepository repository,
        ITenantSchoolResolver tenantSchoolResolver,
        IMapper mapper)
    {
        _repository = repository;
        _tenantSchoolResolver = tenantSchoolResolver;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<SchoolListDto>> ExecuteAsync()
    {
        var schools = await _repository.FindAllAsync();

        // Filtrado multi-tenant: con JWT valido de rol 1-4 solo se expone el
        // colegio del tenant. Sin token (gRPC, flujos internos) no se filtra.
        var allowedSchoolId = await _tenantSchoolResolver.ResolveAllowedSchoolIdAsync();
        if (allowedSchoolId is { } schoolId)
        {
            schools = schools.Where(s => s.Id == schoolId).ToList();
        }

        return _mapper.Map<IReadOnlyList<SchoolListDto>>(schools);
    }
}
