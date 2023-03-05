using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;
using TxCommand.Abstractions;
using TxCommand.Abstractions.Exceptions;

namespace TxCommand
{
    public class TransactionProvider : ITransactionProvider<IMongoClient, IClientSessionHandle>
    {
        private readonly IMongoClient _client;
        private readonly MongoOptions _options;
        private IClientSessionHandle _session;

        private bool _disposed = false;

        public TransactionProvider(IMongoClient client, MongoOptions options)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public virtual async Task EnsureTransactionAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            ThrowIfCancelled(cancellationToken);

            if (_session == null)
            {
                _session = await _client.StartSessionAsync(_options.SessionOptions, cancellationToken);
            }

            if (!_session.IsInTransaction)
            {
                _session.StartTransaction(_options.TransactionOptions);
            }
        }

        public virtual Task CommitAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            ThrowIfCancelled(cancellationToken);

            if (_session?.IsInTransaction != true)
            {
                throw new TransactionNotStartedException(nameof(CommitAsync));
            }

            return _session.CommitTransactionAsync(cancellationToken);
        }

        public virtual void Commit(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            ThrowIfCancelled(cancellationToken);

            if (_session?.IsInTransaction != true)
            {
                throw new TransactionNotStartedException(nameof(Commit));
            }

            _session.CommitTransaction(cancellationToken);
        }

        public virtual Task RollbackAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            ThrowIfCancelled(cancellationToken);

            if (_session?.IsInTransaction != true)
            {
                throw new TransactionNotStartedException(nameof(RollbackAsync));
            }

            return _session.AbortTransactionAsync(cancellationToken);
        }

        public virtual (IMongoClient database, IClientSessionHandle transaction) GetExecutionArguments()
        {
            ThrowIfDisposed();

            return (_client, _session);
        }

        public virtual void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _session?.Dispose();
            _disposed = true;
        }
        
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TransactionProvider));
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
