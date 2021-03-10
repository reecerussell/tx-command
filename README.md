# TxCommand

A simple commanding library with support for executing commands within a database transaction.

## Usage

Commands are executed with a CommandExecutor, which provides a database transaction. CommandExecutors are a single use object and should be used for a specific set of operations.

Below is an example of how to use the command executors in a service.

```csharp
public class PetService
{
    private readonly ITxCommandExecutorFactory _commandExecutorFactory;

    public PetService(ITxCommandExecutorFactory commandExecutorFactory)
    {
        _commandExecutorFactory = commandExecutorFactory;
    }

    // Adds a collection of pets, if any of them fail, the
    // transaction will be rolled back.
    public async Task AddPets(int personId, string[] pets)
    {
        // The executor commits the transaction on disposal.
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
```

Here is an example of a transient service, `CreatePersonService`. This should be initialised on a per-use basis, as it has a command executor as a dependency.

```csharp
public class CreatePersonService : IDisposable
{
    private readonly ITxCommandExecutor _executor;

    public CreatePersonService(ITxCommandExecutor executor)
    {
        _executor = executor;
    }

    // Creates a new Person, then adds a Pet. If either command throws
    // an exception, the transaction will be rolled back.
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
        // On disposal, the executor is disposed, which commits the transaction.
        _executor?.Dispose();
    }
}
```