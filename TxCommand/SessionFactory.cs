using Microsoft.Extensions.DependencyInjection;
using System;
using TxCommand.Abstractions;

namespace TxCommand
{
    /// <inheritdoc />
    public class SessionFactory<TSession> : ISessionFactory<TSession> where TSession : class
    {
        private readonly IServiceProvider _services;

        public SessionFactory(IServiceProvider services)
        {
            _services = services;
        }

        public TSession Create() => _services.GetRequiredService<TSession>();
    }
}
