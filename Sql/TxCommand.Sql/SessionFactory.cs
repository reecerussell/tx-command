using System;
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
    }
}
