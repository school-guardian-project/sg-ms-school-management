using Grpc.Core;
using ms_school_management.Api.School.Application.Dto;
using ms_school_management.Api.School.Application.UseCase;
using ms_school_management.Api.School.Domain.Ports.In;

namespace ms_school_management.Api.Infrastructure.Grpc;

/// <summary>
/// Superficie gRPC de ms-school-management. La consumen ms-iam (para emitir el
/// claim <c>schoolId</c> en el token) y ms-user-management (para validar colegio
/// y sede al registrar). No la usa el frontend: el navegador y el movil hablan
/// REST a traves de Kong.
/// </summary>
public class SchoolManagementGrpcService : SchoolManagementService.SchoolManagementServiceBase
{
    private readonly IGetAdminSchoolUseCase _getAdminSchoolUseCase;
    private readonly IGetCampusUseCase _getCampusUseCase;
    private readonly ILinkAdminSchoolUseCase _linkAdminSchoolUseCase;
    private readonly IGetSchoolUseCase _getSchoolUseCase;

    public SchoolManagementGrpcService(
        IGetAdminSchoolUseCase getAdminSchoolUseCase,
        IGetCampusUseCase getCampusUseCase,
        ILinkAdminSchoolUseCase linkAdminSchoolUseCase,
        IGetSchoolUseCase getSchoolUseCase)
    {
        _getAdminSchoolUseCase = getAdminSchoolUseCase;
        _getCampusUseCase = getCampusUseCase;
        _linkAdminSchoolUseCase = linkAdminSchoolUseCase;
        _getSchoolUseCase = getSchoolUseCase;
    }

    public override async Task<GetAdminSchoolResponse> GetAdminSchool(
        GetAdminSchoolRequest request, ServerCallContext context)
    {
        var profileId = ParseId(request.ProfileId, "profile_id");
        var school = await _getAdminSchoolUseCase.ExecuteAsync(profileId, context.CancellationToken);

        // Sin relacion admin-colegio todavia: respuesta valida con found = false,
        // no un error. El consumidor decide si eso le basta o debe rechazarlo.
        if (school is null)
        {
            return new GetAdminSchoolResponse { Found = false };
        }

        return new GetAdminSchoolResponse
        {
            Found = true,
            Id = school.Id.ToString(),
            Name = school.Name,
            CityId = school.CityId.ToString(),
            Status = school.Status
        };
    }

    public override async Task<GetCampusResponse> GetCampus(
        GetCampusRequest request, ServerCallContext context)
    {
        var campus = await _getCampusUseCase.ExecuteAsync(ParseId(request.Id, "id"), context.CancellationToken);
        return ToResponse(campus);
    }

    public override async Task<GetCampusResponse> GetCampusByName(
        GetCampusByNameRequest request, ServerCallContext context)
    {
        var schoolId = ParseId(request.SchoolId, "school_id");
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new RpcException(new global::Grpc.Core.Status(
                StatusCode.InvalidArgument, "name is required"));
        }

        var campus = await _getCampusUseCase.ExecuteByNameAsync(
            schoolId, request.Name.Trim(), context.CancellationToken);

        return ToResponse(campus);
    }

    public override async Task<LinkAdminSchoolResponse> LinkAdminSchool(
        LinkAdminSchoolRequest request, ServerCallContext context)
    {
        var profileId = ParseId(request.ProfileId, "profile_id");
        var schoolId = ParseId(request.SchoolId, "school_id");

        try
        {
            var linked = await _linkAdminSchoolUseCase.ExecuteAsync(
                profileId, schoolId, context.CancellationToken);

            return new LinkAdminSchoolResponse
            {
                Linked = true,
                LinkId = linked.LinkId.ToString(),
                SchoolName = linked.SchoolName
            };
        }
        catch (SchoolNotFoundException ex)
        {
            // El id de colegio no existe. En gRPC es NOT_FOUND y no UNKNOWN: quien
            // llama necesita distinguir "el colegio no existe" de "el servicio
            // fallo", porque son acciones distintas del lado consumidor.
            throw new RpcException(new global::Grpc.Core.Status(
                StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<GetSchoolResponse> GetSchool(
        GetSchoolRequest request, ServerCallContext context)
    {
        var schoolId = ParseId(request.Id, "id");
        var school = await _getSchoolUseCase.ExecuteAsync(schoolId);

        if (school is null)
        {
            return new GetSchoolResponse { Found = false };
        }

        return new GetSchoolResponse
        {
            Found = true,
            Id = school.Id.ToString(),
            Name = school.Name,
            CityId = school.CityId.ToString(),
            Status = school.Status
        };
    }

    private static GetCampusResponse ToResponse(CampusDetailDto? campus)
    {
        if (campus is null)
        {
            return new GetCampusResponse { Found = false };
        }

        return new GetCampusResponse
        {
            Found = true,
            Id = campus.Id.ToString(),
            SchoolId = campus.SchoolId.ToString(),
            Name = campus.Name,
            Address = campus.Address,
            Status = campus.Status
        };
    }

    private static Guid ParseId(string value, string field)
    {
        if (!Guid.TryParse(value, out var id))
        {
            throw new RpcException(new global::Grpc.Core.Status(
                StatusCode.InvalidArgument, $"Invalid {field}, use UUID: {value}"));
        }

        return id;
    }
}