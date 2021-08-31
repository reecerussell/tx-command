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
        private readonly CancellationTokenSource _ctx;
        private readonly ITransactionProvider<TDatabase, TTransaction> _provider;

        private bool _completed = true;
        private bool _disposed = false;

        public Session(ITransactionProvider<TDatabase, TTransaction> provider)
        {
            _ctx = new CancellationTokenSource();
            _provider = provider;
        }

        public async Task ExecuteAsync(ITxCommand<TDatabase, TTransaction> command)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Session<TDatabase, TTransaction>));
            }

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var token = _ctx.Token;
            await _provider.EnsureTransactionAsync(token);
            _completed = false;

            try
            {
                command.Validate();

                var (database, transaction) = _provider.GetExecutionArguments();
                await command.ExecuteAsync(database, transaction);
            }
            catch (Exception)
            {
                await RollbackAsync();

                throw;
            }
        }

        public async Task<TResult> ExecuteAsync<TResult>(ITxCommand<TDatabase, TTransaction, TResult> command)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Session<TDatabase, TTransaction>));
            }

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var token = _ctx.Token;
            await _provider.EnsureTransactionAsync(token);
            _completed = false;

            try
            {
                command.Validate();

                var (database, transaction) = _provider.GetExecutionArguments();
                return await command.ExecuteAsync(database, transaction);
            }
            catch (Exception)
            {
                await RollbackAsync();

                throw;
            }
        }

        public async Task CommitAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Session<TDatabase, TTransaction>));
            }

            if (_completed)
            {
                throw new TransactionNotStartedException(nameof(CommitAsync));
            }

            await _provider.CommitAsync(_ctx.Token);
            _completed = true;
        }

        public void Commit()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Session<TDatabase, TTransaction>));
            }

            if (_completed)
            {
                throw new TransactionNotStartedException(nameof(Commit));
            }

            _provider.Commit();
            _completed = true;
        }

        public async Task RollbackAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Session<TDatabase, TTransaction>));
            }

            if (_completed)
            {
                throw new TransactionNotStartedException(nameof(CommitAsync));
            }

            await _provider.RollbackAsync(_ctx.Token);
            _completed = true;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (!_completed)
            {
                Commit();
            }

            _ctx.Cancel();
            _ctx.Dispose();

            _disposed = true;
        }

#if NET5_0

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            if (!_completed)
            {
                await CommitAsync();
            }

            _ctx.Cancel();
            _ctx.Dispose();

            _disposed = true;
        }

#endif
    }
}
