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
        /// <param name="services">An instance of <see cref="IServiceCollection"/>.</param>
        /// <returns><paramref name="services"/> with the TxCommand services.</returns>
        public static IServiceCollection AddTxCommand(this IServiceCollection services)
        {
            return services.AddTransient<ITxCommandExecutorFactory, TxCommandExecutorFactory>()
                .AddTransient<ITxCommandExecutor>(provider =>
                {
                    var factory = provider.GetRequiredService<ITxCommandExecutorFactory>();

                    return factory.Create();
                });
        }
    }
}
