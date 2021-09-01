using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TxCommand.Abstractions;
using Xunit;

namespace TxCommand.Tests
{
    public class SessionFactoryTests
    {
        [Fact]
        public void Create_ReturnsNewInstanceOfSession()
        {
            var services = new ServiceCollection()
                .AddTransient(_ => Mock.Of<ISession<ITestDatabase, ITestTransaction>>())
                .BuildServiceProvider();

            var factory = new SessionFactory<ISession<ITestDatabase, ITestTransaction>>(services);

            factory.Create().Should().NotBeNull();
        }
    }
}
