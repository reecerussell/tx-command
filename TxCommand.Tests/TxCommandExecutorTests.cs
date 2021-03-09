using Moq;
using System;
using System.Data;
using System.Threading.Tasks;
using TxCommand.Abstractions;
using Xunit;

namespace TxCommand.Tests
{
    public class TxCommandExecutorTests
    {
        [Fact]
        public void Constructor_GivenConnection_BeginsTransaction()
        {
            var connection = new Mock<IDbConnection>();
            connection.SetupGet(x => x.State).Returns(ConnectionState.Open).Verifiable();
            connection.Setup(x => x.BeginTransaction()).Returns(Mock.Of<IDbTransaction>()).Verifiable();

            _ = new TxCommandExecutor(connection.Object);

            connection.VerifyGet(x => x.State, Times.Once);
            connection.Verify(x => x.Open(), Times.Never);
            connection.Verify(x => x.BeginTransaction(), Times.Once);
        }

        [Fact]
        public void Constructor_GivenClosedConnection_OpensConnection()
        {
            var connection = new Mock<IDbConnection>();
            connection.Setup(x => x.BeginTransaction()).Returns(Mock.Of<IDbTransaction>()).Verifiable();

            _ = new TxCommandExecutor(connection.Object);

            connection.Verify(x => x.Open(), Times.Once);
            connection.Verify(x => x.BeginTransaction(), Times.Once);
        }

        [Fact]
        public void Commit_CommitsTheUnderlyingTransaction_WithNoError()
        {
            var transaction = new Mock<IDbTransaction>();
            transaction.Setup(x => x.Commit()).Verifiable();

            var connection = new Mock<IDbConnection>();
            connection.Setup(x => x.BeginTransaction()).Returns(transaction.Object);

            var commandExecutor = new TxCommandExecutor(connection.Object);
            commandExecutor.Commit();

            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Commit_WithDisposedCommandExecutor_ThrowsObjectDisposedException()
        {
            var connection = new Mock<IDbConnection>();
            connection.Setup(x => x.BeginTransaction()).Returns(Mock.Of<IDbTransaction>());

            var commandExecutor = new TxCommandExecutor(connection.Object);
            commandExecutor.Dispose();

            Assert.Throws<ObjectDisposedException>(() => commandExecutor.Commit());
        }

        [Fact]
        public void Rollback_RollsBackTheUnderlyingTransaction_WithNoError()
        {
            var transaction = new Mock<IDbTransaction>();
            transaction.Setup(x => x.Rollback()).Verifiable();

            var connection = new Mock<IDbConnection>();
            connection.Setup(x => x.BeginTransaction()).Returns(transaction.Object);

            var commandExecutor = new TxCommandExecutor(connection.Object);
            commandExecutor.Rollback();

            transaction.Verify(x => x.Rollback(), Times.Once);
        }

        [Fact]
        public void Rollback_WithDisposedCommandExecutor_ThrowsObjectDisposedException()
        {
            var connection = new Mock<IDbConnection>();
            connection.Setup(x => x.BeginTransaction()).Returns(Mock.Of<IDbTransaction>());

            var commandExecutor = new TxCommandExecutor(connection.Object);
            commandExecutor.Dispose();

            Assert.Throws<ObjectDisposedException>(() => commandExecutor.Rollback());
        }

        [Fact]
        public async Task ExecuteAsync_GivenCommand_ExecutesTheCommand()
        {
            var connection = new Mock<IDbConnection>();
            var transaction = Mock.Of<IDbTransaction>();
            connection.Setup(x => x.BeginTransaction()).Returns(transaction);

            var command = new Mock<ITxCommand>();
            command.Setup(x => x.ExecuteAsync(transaction)).Returns(Task.CompletedTask).Verifiable();

            var commandExecutor = new TxCommandExecutor(connection.Object);
            await commandExecutor.ExecuteAsync(command.Object);

            command.Verify(x => x.ExecuteAsync(transaction), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_GivenNullCommand_ThrowsArgumentNullException()
        {
            var connection = new Mock<IDbConnection>();
            var transaction = Mock.Of<IDbTransaction>();
            connection.Setup(x => x.BeginTransaction()).Returns(transaction);

            var commandExecutor = new TxCommandExecutor(connection.Object);

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await commandExecutor.ExecuteAsync(null));

            Assert.Equal("command", ex.ParamName);
        }

        [Fact]
        public async Task ExecuteAsync_WithDisposedCommandExecutor_ThrowsObjectDisposedException()
        {
            var transaction = new Mock<IDbTransaction>();
            var connection = new Mock<IDbConnection>();
            connection.Setup(x => x.BeginTransaction()).Returns(transaction.Object);

            var command = new Mock<ITxCommand>();
            command.Setup(x => x.ExecuteAsync(transaction.Object)).Returns(Task.CompletedTask).Verifiable();

            var commandExecutor = new TxCommandExecutor(connection.Object);
            commandExecutor.Dispose();

            var ex = await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await commandExecutor.ExecuteAsync(command.Object));
            Assert.Equal(nameof(TxCommandExecutor), ex.ObjectName);

            command.Verify(x => x.ExecuteAsync(transaction.Object), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WhereCommandThrowsException_RollsBackAndThrows()
        {
            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            transaction.Setup(x => x.Rollback()).Verifiable();
            connection.Setup(x => x.BeginTransaction()).Returns(transaction.Object);

            var command = new Mock<ITxCommand>();
            var testException = new Exception("Test");
            command.Setup(x => x.ExecuteAsync(transaction.Object)).Throws(testException).Verifiable();

            var commandExecutor = new TxCommandExecutor(connection.Object);

            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await commandExecutor.ExecuteAsync(command.Object));
            Assert.Equal(ex, testException);

            transaction.Verify(x => x.Rollback(), Times.Once);
            command.Verify(x => x.ExecuteAsync(transaction.Object), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsyncWithResult_GivenCommand_ExecutesTheCommand()
        {
            const string testResult = "Test";

            var connection = new Mock<IDbConnection>();
            var transaction = Mock.Of<IDbTransaction>();
            connection.Setup(x => x.BeginTransaction()).Returns(transaction);

            var command = new Mock<ITxCommand<string>>();
            command.Setup(x => x.ExecuteAsync(transaction)).ReturnsAsync(testResult).Verifiable();

            var commandExecutor = new TxCommandExecutor(connection.Object);
            var result = await commandExecutor.ExecuteAsync(command.Object);
            Assert.Equal(testResult, result);

            command.Verify(x => x.ExecuteAsync(transaction), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsyncWithResult_GivenNullCommand_ThrowsArgumentNullException()
        {
            var connection = new Mock<IDbConnection>();
            var transaction = Mock.Of<IDbTransaction>();
            connection.Setup(x => x.BeginTransaction()).Returns(transaction);

            var commandExecutor = new TxCommandExecutor(connection.Object);

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await commandExecutor.ExecuteAsync<string>(null));

            Assert.Equal("command", ex.ParamName);
        }

        [Fact]
        public async Task ExecuteAsyncWithResult_WithDisposedCommandExecutor_ThrowsObjectDisposedException()
        {
            const string testResult = "Test";

            var transaction = new Mock<IDbTransaction>();
            var connection = new Mock<IDbConnection>();
            connection.SetupGet(x => x.State).Returns(ConnectionState.Open);
            connection.Setup(x => x.BeginTransaction()).Returns(transaction.Object);

            var command = new Mock<ITxCommand<string>>();
            command.Setup(x => x.ExecuteAsync(transaction.Object)).ReturnsAsync(testResult).Verifiable();

            var commandExecutor = new TxCommandExecutor(connection.Object);
            commandExecutor.Dispose();

            var ex = await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await commandExecutor.ExecuteAsync(command.Object));
            Assert.Equal(nameof(TxCommandExecutor), ex.ObjectName);

            command.Verify(x => x.ExecuteAsync(transaction.Object), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsyncWithResult_WhereCommandThrowsException_RollsBackAndThrows()
        {
            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            transaction.Setup(x => x.Rollback()).Verifiable();
            connection.Setup(x => x.BeginTransaction()).Returns(transaction.Object);

            var command = new Mock<ITxCommand<string>>();
            var testException = new Exception("Test");
            command.Setup(x => x.ExecuteAsync(transaction.Object)).Throws(testException).Verifiable();

            var commandExecutor = new TxCommandExecutor(connection.Object);

            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await commandExecutor.ExecuteAsync(command.Object));
            Assert.Equal(ex, testException);

            transaction.Verify(x => x.Rollback(), Times.Once);
            command.Verify(x => x.ExecuteAsync(transaction.Object), Times.Once);
        }

        [Fact]
        public void Dispose_GivenIndisposedCommandExecutor_CommitsAndDisposesTransaction()
        {
            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            transaction.Setup(x => x.Commit()).Verifiable();
            transaction.Setup(x => x.Dispose()).Verifiable();
            connection.Setup(x => x.BeginTransaction()).Returns(transaction.Object);

            var commandExecutor = new TxCommandExecutor(connection.Object);
            commandExecutor.Dispose();

            transaction.Verify(x => x.Commit(), Times.Once);
            transaction.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public void Dispose_GivenDisposedCommandExecutor_DoesNothing()
        {
            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            transaction.Setup(x => x.Commit()).Verifiable();
            transaction.Setup(x => x.Dispose()).Verifiable();
            connection.Setup(x => x.BeginTransaction()).Returns(transaction.Object);

            var commandExecutor = new TxCommandExecutor(connection.Object);
            commandExecutor.Dispose(); // call dispose to mark executor as disposed.

            // Act
            commandExecutor.Dispose();

            // Ensure transaction is only called in setup dispose call.
            transaction.Verify(x => x.Commit(), Times.Once);
            transaction.Verify(x => x.Dispose(), Times.Once);
        }
    }
}
