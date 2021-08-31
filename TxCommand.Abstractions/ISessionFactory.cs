namespace TxCommand.Abstractions
{
    /// <summary>
    /// Used to create new instances of <see cref="TSession"/>.
    /// </summary>
    public interface ISessionFactory<out TSession>
        where TSession : class
    {
        /// <summary>
        /// Creates and returns a new instance of <see cref="TSession" />.
        /// </summary>
        TSession Create();
    }
}
