using System;
using System.Threading;
using System.Threading.Tasks;

namespace TxCommand.Abstractions
{
#if NETSTANDARD2_0_OR_GREATER

    public interface ISession<out TDatabase, out TTransaction> : IDisposable
        where TDatabase : class
        where TTransaction : class
    {
        Task ExecuteAsync(ITxCommand<TDatabase, TTransaction> command, CancellationToken cancellationToken = default);

        Task<TResult> ExecuteAsync<TResult>(ITxCommand<TDatabase, TTransaction, TResult> command,
            CancellationToken cancellationToken = default);

        Task CommitAsync(CancellationToken cancellationToken = default);

        void Commit(CancellationToken cancellationToken = default);

        Task RollbackAsync(CancellationToken cancellationToken = default);
    }

#endif

#if NET5_0

    public interface ISession<out TDatabase, out TTransaction> : IDisposable, IAsyncDisposable
        where TDatabase : class
        where TTransaction : class
    {
        Task ExecuteAsync(ITxCommand<TDatabase, TTransaction> command, CancellationToken cancellationToken = default);

        Task<TResult> ExecuteAsync<TResult>(ITxCommand<TDatabase, TTransaction, TResult> command,
            CancellationToken cancellationToken = default);

        Task CommitAsync(CancellationToken cancellationToken = default);

        void Commit(CancellationToken cancellationToken = default);

        Task RollbackAsync(CancellationToken cancellationToken = default);
    }

#endif
}
