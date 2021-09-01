using TxCommand.Abstractions;

namespace TxCommand
{
    /// <summary>
    /// Used to provide eventing hooks on <see cref="Session{TDatabase,TTransaction}"/>.
    /// Typically called when a session has been committed, or rolled back.
    /// </summary>
    public delegate void SessionEvent();

    /// <summary>
    /// Used to provide an eventing hook on <see cref="Session{TDatabase,TTransaction}"/>,
    /// which is called when a command has been successfully executed.
    /// </summary>
    /// <param name="command"></param>
    public delegate void ExecutedEvent(ICommand command);
}
