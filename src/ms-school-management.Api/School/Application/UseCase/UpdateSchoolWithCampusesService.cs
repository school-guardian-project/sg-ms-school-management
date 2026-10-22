using AutoMapper;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Model;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Application.UseCase;

public sealed class UpdateSchoolWithCampusesService : IUpdateSchoolWithCampusesUseCase
{
    private readonly ISchoolRepository _repository;
    private readonly IMapper _mapper;

    public UpdateSchoolWithCampusesService(ISchoolRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task ExecuteAsync(Guid id, SchoolWithCampusesRequestDto request)
    {
        SchoolLocationRequestRules.Validate(request.Latitude, request.Longitude);
        SchoolCampusRequestRules.Validate(request.Campuses);
        var removedCampusIds = request.RemovedCampusIds?.Distinct().ToList() ?? [];
        if (removedCampusIds.Contains(Guid.Empty)
            || request.Campuses.Any(campus => campus.Id.HasValue && removedCampusIds.Contains(campus.Id.Value)))
        {
            throw new ArgumentException("A removed campus must have a valid id and cannot also be updated.");
        }
        var school = await _repository.FindByIdAsync(id)
            ?? throw new KeyNotFoundException($"School {id} not found");

        _mapper.Map(request, school);
        var campuses = request.Campuses.Select(campus => new SchoolCampus
        {
            Id = campus.Id ?? Guid.Empty,
            SchoolId = id,
            Name = campus.Name.Trim(),
            Address = campus.Address.Trim(),
            Latitude = campus.Latitude,
            Longitude = campus.Longitude,
            Status = Status.Active,
        }).ToList();
        campuses.AddRange(removedCampusIds.Select(campusId => new SchoolCampus
        {
            Id = campusId,
            SchoolId = id,
            Status = Status.Inactive,
        }));
        await _repository.UpdateWithCampusesAsync(school, campuses);
    }
}
