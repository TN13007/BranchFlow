namespace BranchFlow.Infrastructure.UnitOfWork;

/// <summary>
/// Coordinates saving changes and transactions for one scoped DbContext.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes tracked by the shared DbContext.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a database transaction for the current unit of work.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the active database transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the active database transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
