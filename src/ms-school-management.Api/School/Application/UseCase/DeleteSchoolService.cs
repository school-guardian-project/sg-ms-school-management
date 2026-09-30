using ms_school_management.Api.School.Domain.Model;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Application.UseCase;

public sealed class DeleteSchoolService : IDeleteSchoolUseCase
{
    private readonly ISchoolRepository _repository;

    public DeleteSchoolService(ISchoolRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(Guid id)
    {
        var school = await _repository.FindByIdAsync(id)
            ?? throw new KeyNotFoundException($"School {id} not found");

        school.Status = Status.Inactive;
        await _repository.UpdateAsync(school);
    }
}
