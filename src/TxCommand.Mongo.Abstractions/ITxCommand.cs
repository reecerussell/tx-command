using MongoDB.Driver;

namespace TxCommand.Abstractions
{
    /// <summary>
    /// A command interface used to execute a Mongo operation within a transaction.
    /// </summary>
    public interface ITxCommand : ITxCommand<IMongoClient, IClientSessionHandle>
    {
    }

    /// <summary>
    /// A command interface used to execute a Mongo operation within a transaction, then to return a result.
    /// </summary>
    public interface ITxCommand<TResult> : ITxCommand<IMongoClient, IClientSessionHandle, TResult>
    {
    }
}
