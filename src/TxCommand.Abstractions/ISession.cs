using System;
using System.Threading.Tasks;

namespace TxCommand.Abstractions
{
#if NETSTANDARD2_0_OR_GREATER

    public interface ISession<out TDatabase, out TTransaction> : IDisposable
        where TDatabase : class
        where TTransaction : class
    {
        Task ExecuteAsync(ITxCommand<TDatabase, TTransaction> command);

        Task<TResult> ExecuteAsync<TResult>(ITxCommand<TDatabase, TTransaction, TResult> command);

        Task CommitAsync();

        void Commit();

        Task RollbackAsync();
    }

#endif

#if NET5_0

    public interface ISession<out TDatabase, out TTransaction> : IDisposable, IAsyncDisposable
        where TDatabase : class
        where TTransaction : class
    {
        Task ExecuteAsync(ITxCommand<TDatabase, TTransaction> command);

        Task<TResult> ExecuteAsync<TResult>(ITxCommand<TDatabase, TTransaction, TResult> command);

        Task CommitAsync();

        void Commit();

        Task RollbackAsync();
    }

#endif
}
