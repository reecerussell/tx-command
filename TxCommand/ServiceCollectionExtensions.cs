using System;
using Microsoft.Extensions.DependencyInjection;
using TxCommand.Abstractions;

namespace TxCommand
{
    /// <summary>
    /// ServiceCollectionExtensions provides a collection of extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the required services for using the TxCommand interfaces.
        /// </summary>
        public static IServiceCollection AddTxCommand(this IServiceCollection services, Action<ITxCommandBuilder> builder)
        {
            var builderInstance = new TxCommandBuilder(services);
            builder?.Invoke(builderInstance);

            return services;
        }
    }
}
