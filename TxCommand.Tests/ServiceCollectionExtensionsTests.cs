using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Data;
using TxCommand.Abstractions;
using Xunit;

namespace TxCommand.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTxCommand_SetsUpServices_ReturnsInitialCollectionWithServices()
        {
            var services = new ServiceCollection()
                .AddScoped(_ => Mock.Of<IDbConnection>());

            services.AddTxCommand();

            var provider = services.BuildServiceProvider();

            Assert.IsType<TxCommandExecutorFactory>(provider.GetRequiredService<ITxCommandExecutorFactory>());
            Assert.IsType<TxCommandExecutor>(provider.GetRequiredService<ITxCommandExecutor>());
        }

        [Fact]
        public void AddTxCommand_RetrievingITxCommandExecutorFactoryInSameScope_ReturnsNewInstances()
        {
            var services = new ServiceCollection()
                .AddScoped(_ => Mock.Of<IDbConnection>());

            services.AddTxCommand();

            var provider = services.BuildServiceProvider()
                .CreateScope()
                .ServiceProvider;

            var factory1 = provider.GetRequiredService<ITxCommandExecutorFactory>();
            var factory2 = provider.GetRequiredService<ITxCommandExecutorFactory>();

            // Factories are transient.
            Assert.NotEqual(factory1, factory2);
        }

        [Fact]
        public void AddTxCommand_RetrievingITxCommandExecutorInSameScope_ReturnsNewInstances()
        {
            var services = new ServiceCollection()
                .AddScoped(_ => Mock.Of<IDbConnection>());

            services.AddTxCommand();

            var provider = services.BuildServiceProvider()
                .CreateScope()
                .ServiceProvider;

            var executor1 = provider.GetRequiredService<ITxCommandExecutor>();
            var executor2 = provider.GetRequiredService<ITxCommandExecutor>();

            // Executors are transient.
            Assert.NotEqual(executor1, executor2);
        }

        [Fact]
        public void ResolveCommandExecutorFactory_WithNoIDbConnectionService_Throws()
        {
            var services = new ServiceCollection();

            services.AddTxCommand();

            var provider = services.BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<ITxCommandExecutorFactory>());
        }
    }
}
