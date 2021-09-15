using MongoDB.Driver;

namespace TxCommand.Abstractions
{
    /// <summary>
    /// A session is an interface used to execute commands within a transaction.
    /// </summary>
    public interface ISession : ISession<IMongoClient, IClientSessionHandle>
    {
    }
}
