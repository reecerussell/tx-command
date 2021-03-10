using System.Threading.Tasks;
using TxCommand.Abstractions;
using TxCommand.Example.Commands;

namespace TxCommand.Example
{
    public interface IPetService
    {
        Task AddPet(int personId, string pet);
        Task AddPets(int personId, string[] pets);
    }

    public class PetService : IPetService
    {
        private readonly ITxCommandExecutorFactory _commandExecutorFactory;

        public PetService(ITxCommandExecutorFactory commandExecutorFactory)
        {
            _commandExecutorFactory = commandExecutorFactory;
        }

        public async Task AddPet(int personId, string pet)
        {
            using (var executor = _commandExecutorFactory.Create())
            {
                var command = new AddPetCommand(personId, pet);

                await executor.ExecuteAsync(command);
            }
        }

        public async Task AddPets(int personId, string[] pets)
        {
            using (var executor = _commandExecutorFactory.Create())
            {
                foreach (var pet in pets)
                {
                    var command = new AddPetCommand(personId, pet);

                    await executor.ExecuteAsync(command);
                }
            }
        }
    }
}
