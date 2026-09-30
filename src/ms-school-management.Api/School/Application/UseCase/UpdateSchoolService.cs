using AutoMapper;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Application.UseCase;

public sealed class UpdateSchoolService : IUpdateSchoolUseCase
{
    private readonly ISchoolRepository _repository;
    private readonly IMapper _mapper;

    public UpdateSchoolService(ISchoolRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task ExecuteAsync(Guid id, SchoolRequestDto request)
    {
        var school = await _repository.FindByIdAsync(id)
            ?? throw new KeyNotFoundException($"School {id} not found");

        _mapper.Map(request, school);
        await _repository.UpdateAsync(school);
    }
}
