using MongoDB.Driver;
using TxCommand.Abstractions;

namespace TxCommand
{
    /// <summary>
    /// Extends <see cref="Session{IMongoClient,IClientSessionHandle}"/> to implement <see cref="ISession"/>.
    /// </summary>
    public class Session : Session<IMongoClient, IClientSessionHandle>, ISession
    {
        public Session(ITransactionProvider<IMongoClient, IClientSessionHandle> provider) 
            : base(provider)
        {
        }
    }
}
