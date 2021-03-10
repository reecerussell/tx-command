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
        private readonly IDbTransaction _transaction;

        private bool _disposed = false;
        private bool _completed = false;

        /// <summary>
        /// Initializes a new instance of <see cref="TxCommandExecutor"/>. If the given <see cref="IDbConnection"/>,
        /// <paramref name="connection"/> is closed, an attempt will be made to establish a connection.
        /// </summary>
        /// <param name="connection">The backing connection to the transaction.</param>
        public TxCommandExecutor(IDbConnection connection)
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            _transaction = connection.BeginTransaction();
        }

        /// <summary>
        /// Commits the underlying transaction.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Throws if the <see cref="TxCommandExecutor"/> has been disposed.</exception>
        public void Commit()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TxCommandExecutor));
            }

            _transaction.Commit();
            _completed = true;
        }

        /// <summary>
        /// Rolls back the underlying transaction.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Throws if the <see cref="TxCommandExecutor"/> has been disposed.</exception>
        public void Rollback()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TxCommandExecutor));
            }

            _transaction.Rollback();
            _completed = true;
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
        public async Task ExecuteAsync(ITxCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TxCommandExecutor));
            }

            try
            {
                command.Validate();
                await command.ExecuteAsync(_transaction);
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
        public async Task<TResult> ExecuteAsync<TResult>(ITxCommand<TResult> command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TxCommandExecutor));
            }

            try
            {
                command.Validate();
                return await command.ExecuteAsync(_transaction);
            }
            catch
            {
                Rollback();

                throw;
            }
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

            _transaction?.Dispose();
            _disposed = true;
        }
    }
}
