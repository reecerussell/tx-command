using Dapper;
using System;
using System.Data;
using System.Threading.Tasks;
using TxCommand.Abstractions;

namespace TxCommand.Sql.Tests.Sql
{
    public class CreatePersonCommand : ITxCommand<int>
    {
        public string Name { get; set; }

        public CreatePersonCommand(string name)
        {
            Name = name;
        }

        public async Task<int> ExecuteAsync(IDbConnection connection, IDbTransaction transaction)
        {
            const string query = "INSERT INTO [People] ([Name]) VALUES (@Name); SELECT SCOPE_IDENTITY();";

            return await connection.ExecuteScalarAsync<int>(query, new {Name}, transaction);
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Name))
            {
                throw new ArgumentException("Name cannot be empty", nameof(Name));
            }
        }
    }
}
