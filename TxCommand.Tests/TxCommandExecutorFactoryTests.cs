using Moq;
using System.Data;
using System.Threading.Tasks;
using TxCommand.Abstractions;
using Xunit;

namespace TxCommand.Tests
{
    public class TxCommandExecutorFactoryTests
    {
        [Fact]
        public async Task Create_WithIDBConnectionDependency_ReturnsNewCommandExecutor()
        {
            var connection = new Mock<IDbConnection>();
            connection.SetupGet(x => x.State).Returns(ConnectionState.Open).Verifiable();

            var factory = new TxCommandExecutorFactory(connection.Object);

            // Proves connection was passed through to the new command executor.
            var commandExecutor = factory.Create();
            await commandExecutor.ExecuteAsync(Mock.Of<ITxCommand>());
            connection.Verify(x => x.State, Times.Once);
        }
    }
}
