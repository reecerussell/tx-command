using System;
using TxCommand.Abstractions;

namespace TxCommand
{
    /// <summary>
    /// Extends <see cref="SessionFactory{ISession}"/> to implement <see cref="ISessionFactory"/>.
    /// </summary>
    public class SessionFactory : SessionFactory<ISession>, ISessionFactory
    {
        public SessionFactory(IServiceProvider services) : base(services)
        {
        }
    }
}
