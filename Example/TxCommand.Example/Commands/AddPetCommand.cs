using Dapper;
using System.Data;
using System.Threading.Tasks;
using TxCommand.Abstractions;

namespace TxCommand.Example.Commands
{
    public class AddPetCommand : ITxCommand
    {
        public int PersonId { get; set; }
        public string PetName { get; set; }

        public AddPetCommand(int personId, string petName)
        {
            PersonId = personId;
            PetName = petName;
        }

        public async Task ExecuteAsync(IDbTransaction transaction)
        {
            const string query = "INSERT INTO [Pets] ([PersonId],[Name]) VALUES (@PersonId,@PetName)";

            await transaction.Connection.ExecuteAsync(query, new {PersonId, PetName}, transaction);
        }
    }
}
