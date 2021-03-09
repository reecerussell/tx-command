using System.Data;
using TxCommand.Abstractions;

namespace TxCommand
{
    /// <summary>
    /// A simple implementation of <see cref="ITxCommandExecutorFactory"/>, to create new instances
    /// of <see cref="ITxCommandExecutor"/>, with a <see cref="IDbConnection"/>.
    /// </summary>
    public class TxCommandExecutorFactory : ITxCommandExecutorFactory
    {
        private readonly IDbConnection _connection;

        public TxCommandExecutorFactory(IDbConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Returns a new instance of <see cref="TxCommandExecutor"/>, with the underlying connection.
        /// </summary>
        /// <returns>A new instance of <see cref="TxCommandExecutor"/>.</returns>
        public ITxCommandExecutor Create()
        {
            return new TxCommandExecutor(_connection);
        }
    }
}
