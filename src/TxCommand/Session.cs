using System;
using System.Threading;
using System.Threading.Tasks;
using TxCommand.Abstractions;
using TxCommand.Abstractions.Exceptions;

namespace TxCommand
{
    public class Session<TDatabase, TTransaction> : ISession<TDatabase, TTransaction>
        where TDatabase : class
        where TTransaction : class
    {
        private readonly ITransactionProvider<TDatabase, TTransaction> _provider;

        private bool _completed = true;
        private bool _disposed = false;

        public SessionEvent OnCommitted { get; set; }
        public SessionEvent OnRolledBack { get; set; }
        public ExecutedEvent OnExecuted { get; set; }

        public Session(ITransactionProvider<TDatabase, TTransaction> provider)
        {
            _provider = provider;
        }

        public virtual async Task ExecuteAsync(ITxCommand<TDatabase, TTransaction> command,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            ThrowIfCancelled(cancellationToken);

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            await _provider.EnsureTransactionAsync(cancellationToken);
            _completed = false;

            try
            {
                command.Validate();

                var (database, transaction) = _provider.GetExecutionArguments();
                await command.ExecuteAsync(database, transaction);

                OnExecuted?.Invoke(command);
            }
            catch (Exception)
            {
                await RollbackAsync(cancellationToken);

                throw;
            }
        }

        public virtual async Task<TResult> ExecuteAsync<TResult>(ITxCommand<TDatabase, TTransaction, TResult> command,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            ThrowIfCancelled(cancellationToken);

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            await _provider.EnsureTransactionAsync(cancellationToken);
            _completed = false;

            try
            {
                command.Validate();

                var (database, transaction) = _provider.GetExecutionArguments();
                var result = await command.ExecuteAsync(database, transaction);

                OnExecuted?.Invoke(command);

                return result;
            }
            catch (Exception)
            {
                await RollbackAsync(cancellationToken);

                throw;
            }
        }

        public virtual async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            ThrowIfCancelled(cancellationToken);

            if (_completed)
            {
                throw new TransactionNotStartedException(nameof(CommitAsync));
            }

            await _provider.CommitAsync(cancellationToken);
            _completed = true;

            OnCommitted?.Invoke();
        }

        public virtual void Commit(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            ThrowIfCancelled(cancellationToken);

            if (_completed)
            {
                throw new TransactionNotStartedException(nameof(Commit));
            }

            _provider.Commit(cancellationToken);
            _completed = true;

            OnCommitted?.Invoke();
        }

        public virtual async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            ThrowIfCancelled(cancellationToken);

            if (_completed)
            {
                throw new TransactionNotStartedException(nameof(CommitAsync));
            }

            await _provider.RollbackAsync(cancellationToken);
            _completed = true;

            OnRolledBack?.Invoke();
        }

        public virtual void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (!_completed)
            {
                Commit();
            }

            _disposed = true;
        }

#if NET5_0

        public virtual async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            if (!_completed)
            {
                await CommitAsync();
            }

            _disposed = true;
        }

#endif
        
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Session<TDatabase, TTransaction>));
            }
        }
        
        private static void ThrowIfCancelled(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }
    }
}
