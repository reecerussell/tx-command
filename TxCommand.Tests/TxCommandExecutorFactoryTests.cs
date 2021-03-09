using Moq;
using System.Data;
using Xunit;

namespace TxCommand.Tests
{
    public class TxCommandExecutorFactoryTests
    {
        [Fact]
        public void Create_WithIDBConnectionDependency_ReturnsNewCommandExecutor()
        {
            var connection = new Mock<IDbConnection>();
            connection.SetupGet(x => x.State).Returns(ConnectionState.Open).Verifiable();

            var factory = new TxCommandExecutorFactory(connection.Object);

            _ = factory.Create();

            // Proves connection was passed through to the new command executor.
            connection.Verify(x => x.State, Times.Once);
        }
    }
}
