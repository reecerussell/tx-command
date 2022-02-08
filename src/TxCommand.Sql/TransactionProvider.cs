using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using TxCommand.Abstractions;

namespace TxCommand
{
    /// <inheritdoc cref="ITransactionProvider{IDbConnection, IDbTransaction}" />
    public class TransactionProvider : ITransactionProvider<IDbConnection, IDbTransaction>
    {
        private readonly IDbConnection _connection;
        private readonly SqlOptions _options;
        private IDbTransaction _transaction;

        private bool _disposed = false;

        public TransactionProvider(IDbConnection connection, SqlOptions options)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public virtual Task EnsureTransactionAsync(CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TransactionProvider));
            }

            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }

            if (_transaction == null)
            {
                _transaction = _connection.BeginTransaction(_options.IsolationLevel);
            }

            return Task.CompletedTask;
        }

        public virtual Task CommitAsync(CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TransactionProvider));
            }

            _transaction.Commit();

            return Task.CompletedTask;
        }

        public virtual void Commit()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TransactionProvider));
            }

            _transaction.Commit();
        }

        public virtual Task RollbackAsync(CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TransactionProvider));
            }

            _transaction.Rollback();

            return Task.CompletedTask;
        }

        public virtual (IDbConnection database, IDbTransaction transaction) GetExecutionArguments()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TransactionProvider));
            }

            return (_connection, _transaction);
        }

        public virtual void Dispose()
        {
            _transaction?.Dispose();
            _disposed = true;
        }
    }
}
