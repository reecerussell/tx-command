using Dapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TxCommand.Abstractions;
using Xunit;

namespace TxCommand.Sql.Tests.Sql
{
    public class WhereExecutesSuccessfullyTests : IAsyncLifetime
    {
        private IDbConnection _connection;
        private int _personId;
        private Exception _exception;

        public async Task InitializeAsync()
        {
            _connection = new SqlConnection($"Server=localhost;Database=Test;User Id=sa;Password=MySuperSecur3Password!;");
            _connection.Open();

            await using var services = new ServiceCollection()
                .AddSingleton(_connection)
                .AddTxCommand(b => b.AddSql())
                .BuildServiceProvider();

            var factory = services.GetRequiredService<ISessionFactory>();

            try
            {
                await using (var session = factory.Create())
                {
                    var createPerson = new CreatePersonCommand("John");
                    _personId = await session.ExecuteAsync(createPerson);

                    var createPet = new AddPetCommand(_personId, "Dog");
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
            await _connection.ExecuteAsync($"DELETE FROM [Pets] WHERE [PersonId] = {_personId}");
            await _connection.ExecuteAsync($"DELETE FROM [People] WHERE [Id] = {_personId}");
            _connection?.Dispose();
        }

        [Fact]
        public void TheExceptionShouldBeNull()
        {
            _exception.Should().BeNull();
        }

        [Fact]
        public async Task ThePersonAndPetIsCreated()
        {
            var count = await _connection.ExecuteScalarAsync<int>(
                $"SELECT COUNT(*) FROM [People] WHERE [Id] = {_personId}");
            count.Should().Be(1);

            count = await _connection.ExecuteScalarAsync<int>(
                $"SELECT COUNT(*) FROM [Pets] WHERE [PersonId] = {_personId}");
            count.Should().Be(1);
        }
    }
}
