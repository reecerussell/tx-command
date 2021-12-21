using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using TxCommand.Abstractions;
using TxCommand.Mongo.Net5.Tests.Commands;
using Xunit;

namespace TxCommand.Mongo.Net5.Tests
{
    public class WhereSessionRollsBackTests : IAsyncLifetime
    {
        private IMongoCollection<BsonDocument> _people;
        private IMongoCollection<BsonDocument> _pets;
        private string _personId;
        private Exception _exception;

        public async Task InitializeAsync()
        {
            var mongo = new MongoClient("mongodb://localhost:30001/test");
            var db = mongo.GetDatabase("test");
            _people = db.GetCollection<BsonDocument>("people");
            _pets = db.GetCollection<BsonDocument>("pets");

            await using var services = new ServiceCollection()
                .AddSingleton<IMongoClient>(mongo)
                .AddTxCommand(b => b.AddMongo())
                .BuildServiceProvider();

            var factory = services.GetRequiredService<ISessionFactory>();

            try
            {
                await using (var session = factory.Create())
                {
                    var createPerson = new CreatePersonCommand("John");
                    _personId = await session.ExecuteAsync(createPerson);

                    var createPet = new CreatePetCommand("Dog", null); // null ownerId throws in validation
                    await session.ExecuteAsync(createPet);
                }
            }
            catch (Exception e)
            {
                _exception = e;
            }
        }

        public async Task DisposeAsync()
        {
            await _people.DeleteOneAsync(Builders<BsonDocument>.Filter.Eq(x => x["_id"], ObjectId.Parse(_personId)));
            await _pets.DeleteOneAsync(Builders<BsonDocument>.Filter.Eq(x => x["ownerId"], ObjectId.Parse(_personId)));
        }

        [Fact]
        public void TheExceptionShouldNotBeNull()
        {
            _exception.Should().NotBeNull();
        }

        [Fact]
        public async Task ThePersonAndPetAreNotCreated()
        {
            var count = await _people.CountDocumentsAsync(
                Builders<BsonDocument>.Filter.Eq(x => x["_id"], ObjectId.Parse(_personId)));
            count.Should().Be(0);

            count = await _pets.CountDocumentsAsync(
                Builders<BsonDocument>.Filter.Eq(x => x["ownerId"], _personId));
            count.Should().Be(0);
        }
    }
}
