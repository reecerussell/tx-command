using System;
using System.Data;
using FluentAssertions;
using Moq;
using Xunit;

namespace TxCommand.Sql.Tests
{
    public class SessionFactoryTests
    {
        [Fact]
        public void Create_GivenConnection_ReturnsNewSession()
        {
            var services = new Mock<IServiceProvider>();
            services.Setup(x => x.GetService(typeof(SqlOptions)))
                .Returns(new SqlOptions());
            
            var factory = new SessionFactory(services.Object);

            var connection = Mock.Of<IDbConnection>();
            var session = factory.Create(connection);
            session.Should().NotBeNull();
        }

        [Fact]
        public void Create_GivenNullConnection_ThrowsArgumentNullException()
        {
            var factory = new SessionFactory(Mock.Of<IServiceProvider>());

            var ex = Assert.Throws<ArgumentNullException>(() => factory.Create(null));
            ex.ParamName.Should().Be("connection");
        }
    }
}
