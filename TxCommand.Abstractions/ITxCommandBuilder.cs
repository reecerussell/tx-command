using Microsoft.Extensions.DependencyInjection;

namespace TxCommand.Abstractions
{
    /// <summary>
    /// Used to provide extension methods when setting up TxCommand
    /// in a DI container.
    /// </summary>
    public interface ITxCommandBuilder
    {
        IServiceCollection Services { get; }
    }
}
