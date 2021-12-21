using System.Data;
using System.Threading.Tasks;

namespace TxCommand.Abstractions
{
    /// <summary>
    /// An empty interface used to identify the different variants of <see cref="ITxCommand"/>.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Validates the command before execution, ensuring the command contains valid arguments.
        /// </summary>
        void Validate();
    }

    /// <summary>
    /// A transaction command is an abstraction used to execute a command which requires
    /// a database transaction in order to operate correctly. <see cref="ITxCommand{TDatabase,TTransaction}"/> provides a method
    /// to execute the command, providing it with a <see cref="IDbTransaction"/>.
    /// </summary>
    public interface ITxCommand<in TDatabase, in TTransaction> : ICommand
        where TDatabase : class
        where TTransaction : class
    {
        /// <summary>
        /// Executes the implementing command, providing a <see cref="TTransaction"/>, allowing
        /// the command to execute database operations within the bounds of a transaction.
        /// </summary>
        /// <param name="database">A database connection.</param>
        /// <param name="transaction">A database transaction for the current scope.</param>
        /// <returns></returns>
        Task ExecuteAsync(TDatabase database, TTransaction transaction);
    }

    /// <summary>
    /// A transaction command is an abstraction used to execute a command which requires
    /// a database transaction in order to operate correctly. <see cref="ITxCommand{TDatabase,TTransaction,TResult}"/> provides a method
    /// to execute the command, providing it with a <see cref="TTransaction"/>.
    ///
    /// This command interface behaves the same as the non-generic interface, however,
    /// this provides a type argument, <typeparamref name="TResult"/>, allowing the
    /// command to output data.
    /// </summary>
    public interface ITxCommand<in TDatabase, in TTransaction, TResult> : ICommand
    {
        /// <summary>
        /// Executes the implementing command, providing a <see cref="TTransaction"/>, allowing
        /// the command to execute database operations within the bounds of a transaction.
        /// </summary>
        /// <param name="database">A database connection.</param>
        /// <param name="transaction">A database transaction for the current scope.</param>
        /// <returns></returns>
        Task<TResult> ExecuteAsync(TDatabase database, TTransaction transaction);
    }
}
