using Dapper;
using System.Data;
using System.Threading.Tasks;
using TxCommand.Abstractions;

namespace TxCommand.Example.Commands
{
    public class CreatePersonCommand : ITxCommand<int>
    {
        public string Name { get; set; }

        public CreatePersonCommand(string name)
        {
            Name = name;
        }

        public async Task<int> ExecuteAsync(IDbTransaction transaction)
        {
            const string query = "INSERT INTO [People] ([Name]) VALUES (@Name); SELECT SCOPE_IDENTITY();";

            return await transaction.Connection.ExecuteScalarAsync<int>(query, new {Name}, transaction);
        }
    }
}
