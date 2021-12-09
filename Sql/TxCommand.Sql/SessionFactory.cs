using System;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using TxCommand.Abstractions;

namespace TxCommand
{
    /// <summary>
    /// Implements <see cref="ISessionFactory"/>
    /// </summary>
    public class SessionFactory : SessionFactory<ISession>, ISessionFactory
    {
        public SessionFactory(IServiceProvider services) : base(services)
        {
        }

        /// <summary>
        /// Create returns a new session with the given <paramref name="connection"/>.
        /// This is an overload used to provide <see cref="IDbConnection"/> that is
        /// different to the one configured in dependency injection.
        /// </summary>
        /// <param name="connection">The connection to use.</param>
        /// <returns>A new instance of <see cref="ISession"/>.</returns>
        public ISession Create(IDbConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            var options = Services.GetRequiredService<SqlOptions>();
            var provider = new TransactionProvider(connection, options);

            return new Session(provider);
        }
    }
}
