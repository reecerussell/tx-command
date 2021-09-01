﻿using Dapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Threading.Tasks;
using TxCommand.Abstractions;
using Xunit;

namespace TxCommand.Sql.Tests.MySql
{
    public class WhereSessionRollsBackTests : IAsyncLifetime
    {
        private IDbConnection _connection;

        private int _personId;
        private Exception _exception;

        public async Task InitializeAsync()
        {
            _connection = new MySqlConnection("server=localhost;database=Test;user=Test;password=Test");
            _connection.Open();

            await using var services = new ServiceCollection()
                .AddSingleton(_connection)
                .AddTxCommand(b => b.AddSql())
                .BuildServiceProvider();

            var factory = services.GetRequiredService<ISessionFactory>();

            try
            {
                using (var session = factory.Create())
                {
                    var createPerson = new CreatePersonCommand("John");
                    _personId = await session.ExecuteAsync(createPerson);

                    var createPet = new AddPetCommand(_personId, null); // name cannot be null
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
            await _connection.ExecuteAsync($"DELETE FROM `Pets` WHERE `PersonId` = {_personId}");
            await _connection.ExecuteAsync($"DELETE FROM `People` WHERE `Id` = {_personId}");
            _connection?.Dispose();
        }

        [Fact]
        public void TheExceptionShouldNotBeNull()
        {
            _exception.Should().NotBeNull();
        }

        [Fact]
        public async Task ThePersonAndPetAreNotCreated()
        {
            var count = await _connection.ExecuteScalarAsync<int>(
                $"SELECT COUNT(*) FROM `People` WHERE `Id` = {_personId}");
            count.Should().Be(0);

            count = await _connection.ExecuteScalarAsync<int>(
                $"SELECT COUNT(*) FROM `Pets` WHERE `PersonId` = {_personId}");
            count.Should().Be(0);
        }
    }
}
