using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using TxCommand.Abstractions;

namespace TxCommand.Mongo.Net5.Tests.Commands
{
    public class CreatePersonCommand : ITxCommand<string>
    {
        public string Name { get; }

        public CreatePersonCommand(string name)
        {
            Name = name;
        }

        public async Task<string> ExecuteAsync(IMongoClient client, IClientSessionHandle session)
        {
            var database = client.GetDatabase("test");
            var collection = database.GetCollection<BsonDocument>("people");

            var id = ObjectId.GenerateNewId();
            await collection.InsertOneAsync(session, new BsonDocument
            {
                ["_id"] = id,
                ["name"] = Name,
            });

            return id.ToString();
        }

        public void Validate()
        {
            if (Name == null)
            {
                throw new ArgumentNullException(nameof(Name));
            }
        }
    }
}
