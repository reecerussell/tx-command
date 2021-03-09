namespace TxCommand.Abstractions
{
    /// <summary>
    /// Used to initialise new instances of <see cref="ITxCommandExecutor"/>. This is used
    /// as <see cref="ITxCommandExecutor"/>s should be used on a per-scope basis, so this
    /// gives the ability to create a new instance for a required scope.
    /// </summary>
    public interface ITxCommandExecutorFactory
    {
        /// <summary>
        /// Used to return a new instance of <see cref="ITxCommandExecutor"/>. 
        /// </summary>
        /// <returns>A new instance of <see cref="ITxCommandExecutor"/>.</returns>
        ITxCommandExecutor Create();
    }
}
