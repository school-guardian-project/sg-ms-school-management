using Microsoft.AspNetCore.Mvc;
using ms_school_management.Api.School.Domain.Ports.In;

namespace ms_school_management.Api.School.Infrastructure.Controller;

/// <summary>
/// Lectura de sedes por id. Es el equivalente REST del RPC GetCampus de gRPC:
/// devuelve la misma entidad (mismo CampusDetailDto, misma forma JSON) para que
/// el frontend pueda resolver una sede sin pasar por el listado del colegio.
/// Solo lectura: altas y ediciones de sede siguen en las rutas de schools.
/// </summary>
[ApiController]
[Route("api/v1/campuses")]
public sealed class CampusController : ControllerBase
{
    private readonly IGetCampusUseCase _getCampus;

    public CampusController(IGetCampusUseCase getCampus)
    {
        _getCampus = getCampus;
    }

    /// <summary>
    /// Sede por id. Devuelve 404 si la sede no existe o no esta activa: el caso
    /// de uso senala la ausencia con <c>null</c> (la misma semantica que consume
    /// el adaptador gRPC, que responde <c>found = false</c>).
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var campus = await _getCampus.ExecuteAsync(id, cancellationToken);
        return campus is null ? NotFound() : Ok(campus);
    }
}
