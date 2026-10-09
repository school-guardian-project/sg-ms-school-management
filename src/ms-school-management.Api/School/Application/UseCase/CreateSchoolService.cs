using AutoMapper;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Domain.Model;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;

namespace ms_school_management.Api.School.Application.UseCase;

public sealed class CreateSchoolService : ICreateSchoolUseCase
{
    private readonly ISchoolRepository _repository;
    private readonly IMapper _mapper;

    public CreateSchoolService(ISchoolRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Guid> ExecuteAsync(SchoolRequestDto request)
    {
        SchoolLocationRequestRules.Validate(request.Latitude, request.Longitude);
        var school = _mapper.Map<Domain.Model.School>(request);
        school.Id = Guid.NewGuid();
        school.Status = Status.Active;

        await _repository.SaveWithCampusesAsync(school, [SchoolCampus.Central(school)]);
        return school.Id;
    }
}

/// <summary>
/// Crea el colegio y sus sedes de forma atomica. Si una sede falla, no queda
/// colegio a medias: el cliente reintenta el alta completa sin encontrar un
/// colegio huerfano.
/// </summary>
public sealed class CreateSchoolWithCampusesService : ICreateSchoolWithCampusesUseCase
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly ICampusRepository _campusRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSchoolWithCampusesService(
        ISchoolRepository schoolRepository,
        ICampusRepository campusRepository,
        IUnitOfWork unitOfWork)
    {
        _schoolRepository = schoolRepository;
        _campusRepository = campusRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateSchoolWithCampusesResponseDto> ExecuteAsync(
        CreateSchoolWithCampusesDto request)
    {
        ValidateCampusNames(request.CampusNames);

        var school = new Domain.Model.School
        {
            Id = Guid.NewGuid(),
            CityId = request.CityId,
            Name = request.Name.Trim(),
            Address = request.Address.Trim(),
            Phone = request.Phone,
            Email = request.Email.Trim(),
            Website = request.Website,
            Theme = request.Theme,
            Logo = request.Logo ?? Array.Empty<byte>(),
            Status = Status.Active
        };

        // Colegio y sedes se crean juntos o no se crea ninguno. Sin esto, un fallo
        // al insertar la tercera sede deja un colegio sin sedes, que es el estado
        // inutilizable que este caso de uso existe para evitar.
        await _unitOfWork.BeginAsync(CancellationToken.None);
        try
        {
            await _schoolRepository.SaveAsync(school);

            var campuses = new List<CreatedCampusDto>(request.CampusNames.Count);
            foreach (var campusName in request.CampusNames)
            {
                var campus = new Domain.Model.SchoolCampus
                {
                    Id = Guid.NewGuid(),
                    SchoolId = school.Id,
                    Name = campusName.Trim(),
                    // La direccion de la sede se hereda de la del colegio: el
                    // frontend no pide direccion por sede al crear el colegio, y
                    // dejarla vacia romperia el NOT NULL de School.SchoolCampus.
                    Address = school.Address,
                    Status = Status.Active
                };

                await _campusRepository.AddAsync(campus, CancellationToken.None);
                campuses.Add(new CreatedCampusDto(campus.Id, campus.Name, campus.Address));
            }

            await _unitOfWork.CommitAsync(CancellationToken.None);

            return new CreateSchoolWithCampusesResponseDto(
                school.Id, school.Name, school.Address, school.Phone, school.Email,
                school.CityId, school.Status.ToString(), campuses);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private static void ValidateCampusNames(IReadOnlyList<string> campusNames)
    {
        if (campusNames is null || campusNames.Count == 0)
        {
            throw new CampusNamesRequiredException();
        }

        var normalized = campusNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .ToList();

        if (normalized.Count != campusNames.Count)
        {
            throw new CampusNamesRequiredException();
        }

        // Nombres repetidos producen dos sedes con el mismo nombre, que despues no
        // se distinguen ni en la UI ni en el gRPC GetCampusByName.
        if (normalized.Distinct(StringComparer.OrdinalIgnoreCase).Count() != normalized.Count)
        {
            throw new DuplicateCampusNameException(normalized);
        }
    }
}

/// <summary>
/// No se envio ninguna sede. Es 400 y no 422: el campo es obligatorio y su
/// ausencia es un error de forma del request, no de semantica de negocio.
/// </summary>
public sealed class CampusNamesRequiredException : Exception
{
    public CampusNamesRequiredException()
        : base("At least one campus name is required when creating a school")
    {
    }
}

public sealed class DuplicateCampusNameException : Exception
{
    public DuplicateCampusNameException(IReadOnlyList<string> names)
        : base($"Campus names must be unique; received: {string.Join(", ", names)}")
    {
    }
}