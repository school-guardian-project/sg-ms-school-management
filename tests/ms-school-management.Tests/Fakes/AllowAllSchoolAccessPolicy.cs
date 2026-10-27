using ms_school_management.Api.School.Application.Security;

namespace ms_school_management.Tests.Fakes;

/// <summary>
/// Politica de acceso que deja pasar todo: para los tests de casos de uso que
/// no prueban la regla de pertenencia (la cubre SchoolAccessPolicyTests).
/// </summary>
public sealed class AllowAllSchoolAccessPolicy : ISchoolAccessPolicy
{
    public Task EnsureCanAccessAsync(Guid schoolId, CancellationToken ct = default) => Task.CompletedTask;
}
