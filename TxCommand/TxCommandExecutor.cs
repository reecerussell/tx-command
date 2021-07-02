using System;
using System.Data;
using System.Threading.Tasks;
using TxCommand.Abstractions;

namespace TxCommand
{
    /// <summary>
    /// An implementation of <see cref="ITxCommandExecutor"/>.
    /// </summary>
    public class TxCommandExecutor : ITxCommandExecutor
    {
        private readonly IDbConnection _connection;
        private IDbTransaction _transaction;

        private bool _disposed = false;
        private bool _completed = true;

        public ExecutedDelegate OnExecuted;
        public CommitDelegate OnCommitted;
        public RollbackDelegate OnRolledBack;

        /// <summary>
        /// Initializes a new instance of <see cref="TxCommandExecutor"/>.
        /// </summary>
        /// <param name="connection">The backing connection to the transaction.</param>
        public TxCommandExecutor(IDbConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Commits the underlying transaction.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Throws if the <see cref="TxCommandExecutor"/> has been disposed.</exception>
        public virtual void Commit()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TxCommandExecutor));
            }

            _transaction?.Commit();
            _completed = true;

            OnCommitted?.Invoke();
        }

        /// <summary>
        /// Rolls back the underlying transaction.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Throws if the <see cref="TxCommandExecutor"/> has been disposed.</exception>
        public virtual void Rollback()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TxCommandExecutor));
            }

            _transaction?.Rollback();
            _completed = true;

            OnRolledBack?.Invoke();
        }

        /// <summary>
        /// Executes the given <see cref="ITxCommand"/>, <paramref name="command"/>, within the bounds of the current transaction.
        /// If an exception is thrown while executing <paramref name="command"/>, the current transaction will be rolled back.
        ///
        /// Validates <paramref name="command"/> before executing.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">Throws if the <see cref="TxCommandExecutor"/> has been disposed.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="command"/> is null.</exception>
        public virtual async Task ExecuteAsync(ITxCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TxCommandExecutor));
            }

            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }

            if (_transaction == null)
            {
                _transaction = _connection.BeginTransaction();
                _completed = false;
            }

            try
            {
                command.Validate();
                await command.ExecuteAsync(_connection, _transaction);

                OnExecuted?.Invoke(command);
            }
            catch
            {
                Rollback();

                throw;
            }
        }

        /// <summary>
        /// Executes the given <see cref="ITxCommand"/>, <paramref name="command"/>, within the bounds of the current transaction.
        /// If an exception is thrown while executing <paramref name="command"/>, the current transaction will be rolled back.
        ///
        /// Validates <paramref name="command"/> before executing.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>Returns the output of <paramref name="command"/>.</returns>
        /// <exception cref="ObjectDisposedException">Throws if the <see cref="TxCommandExecutor"/> has been disposed.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="command"/> is null.</exception>
        public virtual async Task<TResult> ExecuteAsync<TResult>(ITxCommand<TResult> command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TxCommandExecutor));
            }

            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }

            if (_transaction == null)
            {
                _transaction = _connection.BeginTransaction();
                _completed = false;
            }

            try
            {
                command.Validate();

                var result = await command.ExecuteAsync(_connection, _transaction);

                OnExecuted?.Invoke(command);

                return result;
            }
            catch
            {
                Rollback();

                throw;
            }
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

            _transaction?.Dispose();
            _disposed = true;
        }
    }
}
