using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using TxCommand.Abstractions;

namespace TxCommand.Mongo.Net5.Tests.Commands
{
    public class CreatePetCommand : ITxCommand
    {
        public string Name { get; }
        public string OwnerId { get; }

        public CreatePetCommand(string name, string ownerId)
        {
            Name = name;
            OwnerId = ownerId;
        }

        public async Task ExecuteAsync(IMongoClient client, IClientSessionHandle session)
        {
            var database = client.GetDatabase("test");
            var collection = database.GetCollection<BsonDocument>("pets");

            await collection.InsertOneAsync(session, new BsonDocument
            {
                ["_id"] = ObjectId.GenerateNewId(),
                ["name"] = Name,
                ["ownerId"] = OwnerId
            });
        }

        public void Validate()
        {
            if (Name == null)
            {
                throw new ArgumentNullException(nameof(Name));
            }

            if (OwnerId == null)
            {
                throw new ArgumentNullException(nameof(OwnerId));
            }
        }
    }
}
