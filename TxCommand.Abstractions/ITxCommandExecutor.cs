using System;
using System.Data;
using System.Threading.Tasks;

namespace TxCommand.Abstractions
{
    /// <summary>
    /// Used to execute <see cref="ITxCommand"/>s, providing them with an open <see cref="IDbTransaction"/>.
    /// <see cref="ITxCommandExecutor"/> acts as a transaction, therefore should only used on a per-scope basis,
    /// and only for a specific collection of <see cref="ITxCommand"/>s with a specific area of concern.
    /// </summary>
    public interface ITxCommandExecutor : IDisposable
    {
        /// <summary>
        /// Commits the underlying <see cref="IDbTransaction"/>.
        /// </summary>
        void Commit();

        /// <summary>
        /// Rolls back the underlying <see cref="IDbTransaction"/>.
        /// </summary>
        void Rollback();

        /// <summary>
        /// Executes the given <see cref="ITxCommand"/>, <paramref name="command"/>, within the current transaction.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns></returns>
        Task ExecuteAsync(ITxCommand command);

        /// <summary>
        /// Executes the given <see cref="ITxCommand"/>, <paramref name="command"/>, within the current transaction.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>The command's output, <typeparamref name="TResult"/>.</returns>
        Task<TResult> ExecuteAsync<TResult>(ITxCommand<TResult> command);
    }
}
