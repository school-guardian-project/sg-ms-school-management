using AutoMapper;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Model;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Application.UseCase;

public sealed class CreateSchoolWithCampusDetailsService : ICreateSchoolWithCampusDetailsUseCase
{
    private readonly ISchoolRepository _repository;
    private readonly IMapper _mapper;

    public CreateSchoolWithCampusDetailsService(ISchoolRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<SchoolWithCampusesResponseDto> ExecuteAsync(SchoolWithCampusesRequestDto request)
    {
        SchoolLocationRequestRules.Validate(request.Latitude, request.Longitude);
        SchoolCampusRequestRules.Validate(request.Campuses);
        var school = _mapper.Map<Domain.Model.School>(request);
        school.Id = Guid.NewGuid();
        school.Status = Status.Active;

        var campuses = request.Campuses.Select(campus => new SchoolCampus
        {
            Id = Guid.NewGuid(),
            SchoolId = school.Id,
            Name = campus.Name.Trim(),
            Address = campus.Address.Trim(),
            Latitude = campus.Latitude,
            Longitude = campus.Longitude,
            Status = Status.Active,
        }).ToList();
        if (!campuses.Any(campus => string.Equals(campus.Name, SchoolCampus.CentralName, StringComparison.OrdinalIgnoreCase)))
            campuses.Insert(0, SchoolCampus.Central(school));

        await _repository.SaveWithCampusesAsync(school, campuses);
        return new SchoolWithCampusesResponseDto(
            school.Id,
            school.Name,
            campuses.Select(ToResponse).ToList());
    }

    private static SchoolCampusResponseDto ToResponse(SchoolCampus campus) =>
        new(campus.Id, campus.Name, campus.Address, campus.Latitude, campus.Longitude);
}
