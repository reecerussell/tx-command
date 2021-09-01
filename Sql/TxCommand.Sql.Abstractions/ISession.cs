using System.Data;

namespace TxCommand.Abstractions
{
    /// <inheritdoc />
    public interface ISession : ISession<IDbConnection, IDbTransaction>
    {
    }
}
