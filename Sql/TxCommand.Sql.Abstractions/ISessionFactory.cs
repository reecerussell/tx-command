using System.Data;

namespace TxCommand.Abstractions
{
    /// <summary>
    /// Used to create new instance of <see cref="ISession"/> for Sql.
    /// </summary>
    public interface ISessionFactory : ISessionFactory<ISession>
    {
        /// <summary>
        /// Create returns a new session with the given <paramref name="connection"/>.
        /// This is an overload used to provide <see cref="IDbConnection"/> that is
        /// different to the one configured in dependency injection.
        /// </summary>
        /// <param name="connection">The connection to use.</param>
        /// <returns>A new instance of <see cref="ISession"/>.</returns>
        ISession Create(IDbConnection connection);
    }
}
