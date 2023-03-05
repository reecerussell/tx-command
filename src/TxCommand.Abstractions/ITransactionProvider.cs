using System;
using System.Threading;
using System.Threading.Tasks;

namespace TxCommand.Abstractions
{
    /// <summary>
    /// Used in a Session to interface with database providers.
    /// </summary>
    /// <typeparam name="TDatabase"></typeparam>
    /// <typeparam name="TTransaction"></typeparam>
    public interface ITransactionProvider<TDatabase, TTransaction> : IDisposable
        where TDatabase : class
        where TTransaction : class
    {
        /// <summary>
        /// Ensures that a transaction has started.
        /// </summary>
        Task EnsureTransactionAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Commits the underlying database transaction.
        /// </summary>
        Task CommitAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Commits the underlying database transaction.
        /// </summary>
        void Commit(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the underlying database transaction.
        /// </summary>
        Task RollbackAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Used by Session to provide a command with the arguments required to execute.
        /// </summary>
        /// <returns>Returns the an instance of <see cref="TDatabase"/> and the current <see cref="TTransaction"/>.</returns>
        (TDatabase database, TTransaction transaction) GetExecutionArguments();
    }
}
