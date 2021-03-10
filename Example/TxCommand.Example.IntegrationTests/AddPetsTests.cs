using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TxCommand.Example.IntegrationTests.Setup;
using Xunit;

namespace TxCommand.Example.IntegrationTests
{
    [Collection("IntegrationTests")]
    public class AddPetsTests : IAsyncLifetime
    {
        private readonly ServiceProvider _services;
        private readonly IDbConnection _connection;

        private Exception _exception;
        private int _personId;

        public AddPetsTests(DatabaseSetup database)
        {
            _services = new ServiceCollection()
                .AddScoped<IDbConnection>(_ => new SqlConnection(
                    $"Server=localhost,12937;Database=Test;User Id=sa;Password={DatabaseSetup.SaPassword};"))
                .AddTxCommand()
                .AddTransient<IPetService, PetService>()
                .BuildServiceProvider();

            _connection = database.Connection;
        }

        public async Task InitializeAsync()
        {
            _personId = await _connection.ExecuteScalarAsync<int>("INSERT INTO [People] ([Name]) VALUES ('John'); SELECT SCOPE_IDENTITY()");

            try
            {
                var pets = new [] {"Dog", "Cat", "Snake"};
                var service = _services.GetRequiredService<IPetService>();
                await service.AddPets(_personId, pets);
            }
            catch (Exception e)
            {
                _exception = e;
            }
        }

        public async Task DisposeAsync()
        {
            await _connection.ExecuteAsync("DELETE FROM [Pets];");
            await _connection.ExecuteAsync("DELETE FROM [People];");

            await _services.DisposeAsync();
        }

        [Fact]
        public void ThenTheExceptionIsNull()
        {
            Assert.Null(_exception);
        }

        [Fact]
        public async Task ThenTheNumberOfPetsCreatedIs3()
        {
            var count = await _connection.QuerySingleAsync<int>(
                $"SELECT COUNT(*) FROM [Pets] WHERE [PersonId] = {_personId}");

            Assert.Equal(3, count);
        }

        [Fact]
        public async Task ThenThePetDataIsCorrect()
        {
            var pets = (await _connection.QueryAsync<string>(
                $"SELECT [Name] FROM [Pets] WHERE [PersonId] = {_personId}"))
                .ToList();

            Assert.Contains(pets, p => p == "Dog");
            Assert.Contains(pets, p => p == "Cat");
            Assert.Contains(pets, p => p == "Snake");
        }
    }
}
