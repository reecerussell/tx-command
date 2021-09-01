using System.Data;

namespace TxCommand.Abstractions
{
    /// <inheritdoc />
    public interface ITxCommand : ITxCommand<IDbConnection, IDbTransaction>
    {
    }

    /// <inheritdoc />
    public interface ITxCommand<TResult> : ITxCommand<IDbConnection, IDbTransaction, TResult>
    {
    }
}
