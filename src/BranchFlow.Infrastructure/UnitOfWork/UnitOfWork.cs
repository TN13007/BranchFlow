using BranchFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace BranchFlow.Infrastructure.UnitOfWork;

/// <summary>
/// Coordinates persistence and transaction boundaries using the request-scoped DbContext.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly BranchFlowDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(BranchFlowDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            throw new InvalidOperationException(
                "A transaction is already active in this unit of work.");
        }

        if (_context.Database.CurrentTransaction is not null)
        {
            throw new InvalidOperationException(
                "The DbContext already has an active transaction outside this unit of work.");
        }

        _currentTransaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction transaction = GetCurrentTransaction();

        try
        {
            await transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await DisposeCurrentTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction transaction = GetCurrentTransaction();

        try
        {
            await transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await DisposeCurrentTransactionAsync();
        }
    }

    public ValueTask DisposeAsync()
    {
        return DisposeCurrentTransactionAsync();
    }

    private IDbContextTransaction GetCurrentTransaction()
    {
        return _currentTransaction
            ?? throw new InvalidOperationException(
                "No active transaction exists in this unit of work.");
    }

    private async ValueTask DisposeCurrentTransactionAsync()
    {
        IDbContextTransaction? transaction = _currentTransaction;
        _currentTransaction = null;

        if (transaction is not null)
        {
            await transaction.DisposeAsync();
        }
    }
}
