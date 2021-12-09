using Microsoft.Extensions.DependencyInjection;
using System;
using TxCommand.Abstractions;

namespace TxCommand
{
    /// <inheritdoc />
    public class SessionFactory<TSession> : ISessionFactory<TSession> where TSession : class
    {
        protected readonly IServiceProvider Services;

        public SessionFactory(IServiceProvider services)
        {
            Services = services;
        }

        public TSession Create() => Services.GetRequiredService<TSession>();
    }
}
