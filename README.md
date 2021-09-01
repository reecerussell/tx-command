![Actions](https://github.com/reecerussell/tx-command/actions/workflows/publish.yaml/badge.svg)
[![codecov](https://codecov.io/gh/reecerussell/tx-command/branch/master/graph/badge.svg?token=2o5osNgjr8)](https://codecov.io/gh/reecerussell/tx-command)
[![CodeFactor](https://www.codefactor.io/repository/github/reecerussell/tx-command/badge)](https://www.codefactor.io/repository/github/reecerussell/tx-command)
![Nuget](https://img.shields.io/nuget/v/TxCommand)
[![Nuget](https://img.shields.io/nuget/dt/TxCommand)](https://www.nuget.org/packages/TxCommand/)

# TxCommand

TxCommand is a simple commanding package which provides commanding interfaces that can be executed within a transaction. TxCommand is built in a way where it can be extended to support multiple platforms and drivers.

| Package                | Version                                                         | Downloads                                                                                                                  |
| ---------------------- | --------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| TxCommand              | ![Nuget](https://img.shields.io/nuget/v/TxCommand)              | [![Nuget](https://img.shields.io/nuget/dt/TxCommand)](https://www.nuget.org/packages/TxCommand/)                           |
| TxCommand.Abstractions | ![Nuget](https://img.shields.io/nuget/v/TxCommand.Abstractions) | [![Nuget](https://img.shields.io/nuget/dt/TxCommand.Abstractions)](https://www.nuget.org/packages/TxCommand.Abstractions/) |
| TxCommand.Sql | ![Nuget](https://img.shields.io/nuget/v/TxCommand.Sql) | [![Nuget](https://img.shields.io/nuget/dt/TxCommand.Sql)](https://www.nuget.org/packages/TxCommand.Sql/) |
| TxCommand.Sql.Abstractions | ![Nuget](https://img.shields.io/nuget/v/TxCommand.Sql.Abstractions) | [![Nuget](https://img.shields.io/nuget/dt/TxCommand.Sql.Abstractions)](https://www.nuget.org/packages/TxCommand.Sql.Abstractions/) |

## Get Started

For this example, we'll be using the Sql variant of TxCommand, `TxCommand.Sql`. To get started, install the `TxCommand.Sql` package to your project - this can be done either with the NuGet Package Manager or the NuGet CLI.

```
> Install-Package TxCommand.Sql
```

After installing the TxCommand, the package can be easily configured in your DI setup. For example:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // TxCommand.Sql depends on an IDbConnection, so here we configure
    // an instance of MySqlConnection.
    services.AddTransient(_ => new MySqlConnection("<connection string>"));

    // Configure TxCommand and the Sql package.
    services.AddTxCommand()
        .AddSql();
}
```

Once the DI is configured, you're good to go. The next step is to setup a command. Below is an example of a command that is used to insert a `Car` record into a database.

```csharp
using System;
using System.Data;
using TxCommand.Abstractions;

...

// This is the command which is used to create a car record. It implemented the
// ITxCommand interface, which has an optional type parameter used as a result.
public class CreateCarCommand : ITxCommand<int>
{
    public string Reg { get; set; }

    public CreateCarCommand(string reg)
    {
        Reg = reg;
    }

    // This is the main entrypoint to the command.
    public async Task<int> ExecuteAsync(IDbConnection connection, IDbTransaction transaction)
    {
        const string query = "INSERT INTO `Cars` (`Reg`) VALUES (@Reg); SELECT LAST_INSERT_ID();";

        return await connection.ExecuteScalarAsync<int>(query, new {Reg}, transaction);
    }

    // Validate is used to validate that the data passed to the command
    // is valid, before execution.
    public void Validate()
    {
        if (string.IsNullOrEmpty(Reg))
        {
            throw new ArgumentException("Reg cannot be empty", nameof(Reg));
        }
    }
}
```

Now we got the command sorted, the final step is to execute it. To execute the command we use the `ISession` interface. A `Session` is used to execute a set of command within a single transaction. An instance of `ISession` can be instantiated using the `ISessionFactory` dependency.

```csharp
using TxCommand.Abstractions;

...

public class CarFactory
{
    private readonly ISessionFactory _sessionFactory;

    // ISessionFactory can be injected into another service, using the DI container.
    public CarFactory(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    public async Task<int> CreateAsync(string reg)
    {
        // A session should be disposed to commit the transaction. Alternatively,
        // session.CommitAsync() can be called - or even session.RollbackAsync();
        using (var session = _sessionFactory.Create())
        {
            // Create a new instance of the command.
            var command = new CreateCarCommand(reg);

            // Then call execute! The session will first call command.Validate(),
            // then it will be executed and return the result of the command.
            return await session.ExecuteAsync(command);
        }
    }
}

```

## Contributing

Not much here, but feel free to raise an issue or open a Pull Request if you think of an enhancement or spot a bug!
