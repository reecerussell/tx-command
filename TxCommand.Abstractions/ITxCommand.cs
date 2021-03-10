using System.Data;
using System.Threading.Tasks;

namespace TxCommand.Abstractions
{
    /// <summary>
    /// A transaction command is an abstraction used to execute a command which requires
    /// a database transaction in order to operate correctly. <see cref="ITxCommand"/> provides a method
    /// to execute the command, providing it with a <see cref="IDbTransaction"/>.
    /// </summary>
    public interface ITxCommand
    {
        /// <summary>
        /// Executes the implementing command, providing a <see cref="IDbTransaction"/>, allowing
        /// the command to execute database operations within the bounds of a transaction.
        /// </summary>
        /// <param name="transaction">A database transaction for the current scope.</param>
        /// <returns></returns>
        Task ExecuteAsync(IDbTransaction transaction);

        /// <summary>
        /// Validates the command before execution, ensuring the command contains valid arguments.
        /// </summary>
        void Validate();
    }

    /// <summary>
    /// A transaction command is an abstraction used to execute a command which requires
    /// a database transaction in order to operate correctly. <see cref="ITxCommand"/> provides a method
    /// to execute the command, providing it with a <see cref="IDbTransaction"/>.
    ///
    /// This command interface behaves the same as the non-generic interface, however,
    /// this provides a type argument, <typeparamref name="TResult"/>, allowing the
    /// command to output data.
    /// </summary>
    public interface ITxCommand<TResult>
    {
        /// <summary>
        /// Executes the implementing command, providing a <see cref="IDbTransaction"/>, allowing
        /// the command to execute database operations within the bounds of a transaction.
        /// </summary>
        /// <param name="transaction">A database transaction for the current scope.</param>
        /// <returns></returns>
        Task<TResult> ExecuteAsync(IDbTransaction transaction);

        /// <summary>
        /// Validates the command before execution, ensuring the command contains valid arguments.
        /// </summary>
        void Validate();
    }
}
