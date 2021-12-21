using System.Data;
using TxCommand.Abstractions;

namespace TxCommand
{
    /// <inheritdoc cref="Session{TDatabase,TTransaction}" />
    public class Session : Session<IDbConnection, IDbTransaction>, ISession
    {
        public Session(ITransactionProvider<IDbConnection, IDbTransaction> provider) 
            : base(provider)
        {
        }
    }
}
