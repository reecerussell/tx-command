![Actions](https://github.com/reecerussell/tx-command/actions/workflows/publish.yaml/badge.svg)
[![codecov](https://codecov.io/gh/reecerussell/tx-command/branch/master/graph/badge.svg?token=2o5osNgjr8)](https://codecov.io/gh/reecerussell/tx-command)
[![CodeFactor](https://www.codefactor.io/repository/github/reecerussell/tx-command/badge)](https://www.codefactor.io/repository/github/reecerussell/tx-command)
![Nuget](https://img.shields.io/nuget/v/TxCommand)
[![Nuget](https://img.shields.io/nuget/dt/TxCommand)](https://www.nuget.org/packages/TxCommand/)


# TxCommand

A simple commanding library with support for executing commands within a database transaction.

|Package | Version| Downloads|
|--------|--------|---|
|TxCommand|![Nuget](https://img.shields.io/nuget/v/TxCommand)|[![Nuget](https://img.shields.io/nuget/dt/TxCommand)](https://www.nuget.org/packages/TxCommand/)
|TxCommand.Abstractions|![Nuget](https://img.shields.io/nuget/v/TxCommand.Abstractions)|[![Nuget](https://img.shields.io/nuget/dt/TxCommand.Abstractions)](https://www.nuget.org/packages/TxCommand.Abstractions/)|

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

## Dependency Injection

If you're using `Microsoft.Extensions.DependencyInjection` for dependency injection, `AddTxCommand()` can be called on a `IServiceCollection`.

```csharp
var services = new ServiceCollection()
    .AddTxCommand()
    .BuildServiceProvider();

var factory = services.GetRequiredService<ITxCommandExecutorFactory>();
var executor = services.GetRequiredService<ITxCommandExecutor>();
```