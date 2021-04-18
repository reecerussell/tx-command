using Dapper;
using System;
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

        public async Task ExecuteAsync(IDbConnection connection, IDbTransaction transaction)
        {
            const string query = "INSERT INTO [Pets] ([PersonId],[Name]) VALUES (@PersonId,@PetName)";

            await connection.ExecuteAsync(query, new {PersonId, PetName}, transaction);
        }

        public void Validate()
        {
            if (PersonId < 1)
            {
                throw new ArgumentException("PersonId must be at least 1", nameof(PersonId));
            }

            if (string.IsNullOrEmpty(PetName))
            {
                throw new ArgumentException("Pet name can not be empty", nameof(PetName));
            }
        }
    }
}
