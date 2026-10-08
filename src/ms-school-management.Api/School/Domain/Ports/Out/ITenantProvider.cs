namespace ms_school_management.Api.School.Domain.Ports.Out;

/// <summary>
/// Expone el tenant del request actual a partir del JWT emitido por ms-iam.
/// Sin token (o token invalido) todo es null y <see cref="ShouldFilter"/> es
/// false: los flujos internos (gRPC, ms-user-management) siguen sin filtrar.
/// </summary>
public interface ITenantProvider
{
    int? RoleId { get; }
    Guid? CampusId { get; }
    Guid? SchoolId { get; }
    Guid? ProfileId { get; }

    /// <summary>
    /// True solo con usuario autenticado cuyo rol no sea SuperAdmin (5).
    /// </summary>
    bool ShouldFilter { get; }
}
