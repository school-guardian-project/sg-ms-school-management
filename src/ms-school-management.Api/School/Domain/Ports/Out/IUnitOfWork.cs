namespace ms_school_management.Api.School.Domain.Ports.Out;

/// <summary>
/// Frontera de transaccion para operaciones que tocan mas de una tabla.
/// ms-school-management solo tiene una base de datos, asi que la transaccion no
/// cruza servicios: es puramente para que un colegio y sus sedes se creen o no
/// se creen.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    Task BeginAsync(CancellationToken ct);
    Task CommitAsync(CancellationToken ct);
    Task RollbackAsync(CancellationToken ct);
}