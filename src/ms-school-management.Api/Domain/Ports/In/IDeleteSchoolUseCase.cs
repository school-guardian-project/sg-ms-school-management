namespace ms_school_management.Api.School.Domain.Ports.In;

public interface IDeleteSchoolUseCase
{
    Task ExecuteAsync(Guid id);
}
