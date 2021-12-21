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

        public async Task EnsureTransactionAsync(CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TransactionProvider));
            }

            if (_session == null)
            {
                _session = await _client.StartSessionAsync(_options.SessionOptions, cancellationToken);
            }

            if (!_session.IsInTransaction)
            {
                _session.StartTransaction(_options.TransactionOptions);
            }
        }

        public Task CommitAsync(CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TransactionProvider));
            }

            if (_session?.IsInTransaction != true)
            {
                throw new TransactionNotStartedException(nameof(CommitAsync));
            }

            return _session.CommitTransactionAsync(cancellationToken);
        }

        public void Commit()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TransactionProvider));
            }

            if (_session?.IsInTransaction != true)
            {
                throw new TransactionNotStartedException(nameof(Commit));
            }

            _session.CommitTransaction();
        }

        public Task RollbackAsync(CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TransactionProvider));
            }

            if (_session?.IsInTransaction != true)
            {
                throw new TransactionNotStartedException(nameof(RollbackAsync));
            }

            return _session.AbortTransactionAsync(cancellationToken);
        }

        public (IMongoClient database, IClientSessionHandle transaction) GetExecutionArguments()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TransactionProvider));
            }

            return (_client, _session);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _session?.Dispose();
            _disposed = true;
        }
    }
}
