using TxCommand.Abstractions;

namespace TxCommand
{
    /// <summary>
    /// A delegate used to execute a callback when a command has been executed.
    /// </summary>
    /// <param name="command">The executed command.</param>
    public delegate void ExecutedDelegate(ICommand command);

    /// <summary>
    /// A delegate used to execute a callback when a transaction is committed.
    /// </summary>
    public delegate void CommitDelegate();

    /// <summary>
    /// A delegate used to execute a callback when a transaction is rolled back.
    /// </summary>
    public delegate void RollbackDelegate();
}
