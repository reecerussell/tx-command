using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TxCommand.Example.IntegrationTests.Setup;
using Xunit;

namespace TxCommand.Example.IntegrationTests
{
    [Collection("IntegrationTests")]
    public class CreatePersonTests : IAsyncLifetime
    {
        private readonly ServiceProvider _services;
        private readonly IDbConnection _connection;

        private Exception _exception;
        private int _personId;

        public CreatePersonTests(DatabaseSetup database)
        {
            _services = new ServiceCollection()
                .AddScoped<IDbConnection>(_ => new SqlConnection(
                    $"Server=localhost,12937;Database=Test;User Id=sa;Password={DatabaseSetup.SaPassword};"))
                .AddTxCommand()
                .AddTransient<ICreatePersonService, CreatePersonService>()
                .BuildServiceProvider();
            _connection = database.Connection;
        }
        
        public async Task InitializeAsync()
        {
            try
            {
                using (var service = _services.GetRequiredService<ICreatePersonService>())
                {
                    _personId = await service.Create("John", "Doe");
                }
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
        public async Task ThenThePersonIsCreated()
        {
            var person = await _connection.QuerySingleAsync($"SELECT * FROM [People] WHERE [Id] = {_personId}");

            Assert.Equal("John", person.Name);
        }

        [Fact]
        public async Task ThenTheNumberOfPetsCreatedIs1()
        {
            var count = await _connection.ExecuteScalarAsync<int>(
                $"SELECT COUNT(*) FROM [Pets] WHERE [PersonId] = {_personId}");

            Assert.Equal(1, count);
        }

        [Fact]
        public async Task ThenThePetDataIsCorrect()
        {
            var pet = await _connection.QuerySingleAsync(
                $"SELECT TOP (1) * FROM [Pets] WHERE [PersonId] = {_personId}");

            Assert.Equal("Doe", pet.Name);
        }
    }
}
