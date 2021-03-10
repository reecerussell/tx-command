using System;
using System.Threading.Tasks;
using TxCommand.Abstractions;
using TxCommand.Example.Commands;

namespace TxCommand.Example
{
    public interface ICreatePersonService : IDisposable
    {
        Task<int> Create(string personName, string petName);
    }

    public class CreatePersonService : ICreatePersonService
    {
        private readonly ITxCommandExecutor _executor;

        public CreatePersonService(ITxCommandExecutor executor)
        {
            _executor = executor;
        }

        public async Task<int> Create(string personName, string petName)
        {
            var createPersonCommand = new CreatePersonCommand(personName);
            var personId = await _executor.ExecuteAsync(createPersonCommand);

            var addPetCommand = new AddPetCommand(personId, petName);
            await _executor.ExecuteAsync(addPetCommand);

            return personId;
        }

        public void Dispose()
        {
            _executor?.Dispose();
        }
    }
}
