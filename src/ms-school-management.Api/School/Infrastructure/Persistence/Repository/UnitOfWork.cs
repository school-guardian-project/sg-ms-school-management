using Microsoft.EntityFrameworkCore.Storage;
using ms_school_management.Api.School.Domain.Ports.Out;
using ms_school_management.Api.School.Infrastructure.Persistence.Context;

namespace ms_school_management.Api.School.Infrastructure.Persistence.Repository;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly SchoolManagementContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(SchoolManagementContext context)
    {
        _context = context;
    }

    public async Task BeginAsync(CancellationToken ct)
    {
        if (_transaction is not null)
        {
            return;
        }

        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitAsync(CancellationToken ct)
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.CommitAsync(ct);
        await DisposeAsync();
    }

    public async Task RollbackAsync(CancellationToken ct)
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.RollbackAsync(ct);
        await DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.DisposeAsync();
        _transaction = null;
    }
}