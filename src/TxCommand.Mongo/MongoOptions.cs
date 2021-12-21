using MongoDB.Driver;

namespace TxCommand
{
    public class MongoOptions
    {
        public ClientSessionOptions SessionOptions { get; set; }
        public TransactionOptions TransactionOptions { get; set; }

        public MongoOptions()
        {
            SessionOptions = new ClientSessionOptions();
            TransactionOptions = new TransactionOptions();
        }
    }
}
