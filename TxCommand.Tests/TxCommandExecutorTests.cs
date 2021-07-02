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
        public async Task Commit_CommitsTheUnderlyingTransaction_WithNoError()
        {
            var transaction = new Mock<IDbTransaction>();
            transaction.Setup(x => x.Commit()).Verifiable();

            var connection = new Mock<IDbConnection>();
            connection.SetupGet(x => x.State).Returns(ConnectionState.Open);
            connection.Setup(x => x.BeginTransaction()).Returns(transaction.Object);

            var commandExecutor = new TxCommandExecutor(connection.Object);

            var callbackCalled = false;
            commandExecutor.OnCommitted += () =>
            {
                callbackCalled = true;
            };

            // Start a transaction.
            await commandExecutor.ExecuteAsync(Mock.Of<ITxCommand>());

            // Act
            commandExecutor.Commit();

            Assert.True(callbackCalled);

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
        public void Commit_WithUninitializedTransaction_DoesNotThrowNullReference()
        {
            var commandExecutor = new TxCommandExecutor(Mock.Of<IDbConnection>());
            commandExecutor.Commit();
        }

        [Fact]
        public async Task Rollback_RollsBackTheUnderlyingTransaction_WithNoError()
        {
            var transaction = new Mock<IDbTransaction>();
            transaction.Setup(x => x.Rollback()).Verifiable();

            var connection = new Mock<IDbConnection>();
            connection.Setup(x => x.BeginTransaction()).Returns(transaction.Object);

            var commandExecutor = new TxCommandExecutor(connection.Object);

            var callbackCalled = false;
            commandExecutor.OnRolledBack += () =>
            {
                callbackCalled = true;
            };

            // Start a transaction.
            await commandExecutor.ExecuteAsync(Mock.Of<ITxCommand>());

            // Act
            commandExecutor.Rollback();

            Assert.True(callbackCalled);

            transaction.Verify(x => x.Rollback(), Times.Once);
        }

        [Fact]
        public void Rollback_WithDisposedCommandExecutor_ThrowsObjectDisposedException()
        {
            var connection = new Mock<IDbConnection>();
            connection.SetupGet(x => x.State).Returns(ConnectionState.Open);
            connection.Setup(x => x.BeginTransaction()).Returns(Mock.Of<IDbTransaction>());

            var commandExecutor = new TxCommandExecutor(connection.Object);
            commandExecutor.Dispose();

            Assert.Throws<ObjectDisposedException>(() => commandExecutor.Rollback());
        }

        [Fact]
        public void Rollback_WithUninitializedTransaction_DoesNotThrowNullReference()
        {
            var commandExecutor = new TxCommandExecutor(Mock.Of<IDbConnection>());
            commandExecutor.Rollback();
        }

        [Fact]
        public async Task ExecuteAsync_GivenCommand_ExecutesTheCommand()
        {
            var connection = new Mock<IDbConnection>();
            var transaction = Mock.Of<IDbTransaction>();
            connection.Setup(x => x.BeginTransaction()).Returns(transaction).Verifiable();

            var command = new Mock<ITxCommand>();
            command.Setup(x => x.Validate()).Verifiable();
            command.Setup(x => x.ExecuteAsync(connection.Object, transaction)).Returns(Task.CompletedTask).Verifiable();

            var commandExecutor = new TxCommandExecutor(connection.Object);

            var callbackCalled = false;
            commandExecutor.OnExecuted += (c) =>
            {
                callbackCalled = true;
                Assert.Equal(command.Object, c);
            };

            await commandExecutor.ExecuteAsync(command.Object);

            Assert.True(callbackCalled);

            command.Verify(x => x.Validate(), Times.Once);
            command.Verify(x => x.ExecuteAsync(connection.Object, transaction), Times.Once);
            connection.Verify(x => x.BeginTransaction());
        }

        [Fact]
        public async Task ExecuteAsync_GivenClosedConnection_OpensTheConnection()
        {
            var connection = new Mock<IDbConnection>();
            var transaction = Mock.Of<IDbTransaction>();
            connection.Setup(x => x.BeginTransaction()).Returns(transaction).Verifiable();
            connection.Setup(x => x.Open()).Verifiable();
            connection.SetupGet(x => x.State).Returns(ConnectionState.Closed);

            var command = new Mock<ITxCommand>();
            command.Setup(x => x.Validate()).Verifiable();
            command.Setup(x => x.ExecuteAsync(connection.Object, transaction)).Returns(Task.CompletedTask).Verifiable();

            var commandExecutor = new TxCommandExecutor(connection.Object);
            await commandExecutor.ExecuteAsync(command.Object);

            command.Verify(x => x.Validate(), Times.Once);
            command.Verify(x => x.ExecuteAsync(connection.Object, transaction), Times.Once);
            connection.Verify(x => x.BeginTransaction());
            connection.Verify(x => x.Open());
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
            command.Setup(x => x.ExecuteAsync(connection.Object, transaction.Object)).Returns(Task.CompletedTask).Verifiable();

            var commandExecutor = new TxCommandExecutor(connection.Object);
            commandExecutor.Dispose();

            var ex = await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await commandExecutor.ExecuteAsync(command.Object));
            Assert.Equal(nameof(TxCommandExecutor), ex.ObjectName);

            command.Verify(x => x.ExecuteAsync(connection.Object, transaction.Object), Times.Never);
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
            command.Setup(x => x.ExecuteAsync(connection.Object, transaction.Object)).Throws(testException).Verifiable();

            var commandExecutor = new TxCommandExecutor(connection.Object);

            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await commandExecutor.ExecuteAsync(command.Object));
            Assert.Equal(ex, testException);

            transaction.Verify(x => x.Rollback(), Times.Once);
            command.Verify(x => x.ExecuteAsync(connection.Object, transaction.Object), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhereValidationFails_RollsBackAndThrows()
        {
            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            transaction.Setup(x => x.Rollback()).Verifiable();
            connection.Setup(x => x.BeginTransaction()).Returns(transaction.Object);

            var command = new Mock<ITxCommand>();
            var testException = new Exception("Test");
            command.Setup(x => x.Validate()).Throws(testException).Verifiable();

            var commandExecutor = new TxCommandExecutor(connection.Object);

            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await commandExecutor.ExecuteAsync(command.Object));
            Assert.Equal(ex, testException);

            transaction.Verify(x => x.Rollback(), Times.Once);
            command.Verify(x => x.Validate(), Times.Once);
            command.Verify(x => x.ExecuteAsync(connection.Object, transaction.Object), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsyncWithResult_GivenCommand_ExecutesTheCommand()
        {
            const string testResult = "Test";

            var connection = new Mock<IDbConnection>();
            var transaction = Mock.Of<IDbTransaction>();
            connection.Setup(x => x.BeginTransaction()).Returns(transaction).Verifiable();

            var command = new Mock<ITxCommand<string>>();
            command.Setup(x => x.Validate()).Verifiable();
            command.Setup(x => x.ExecuteAsync(connection.Object, transaction)).ReturnsAsync(testResult).Verifiable();

            var commandExecutor = new TxCommandExecutor(connection.Object);

            var callbackCalled = false;
            commandExecutor.OnExecuted += (c) =>
            {
                callbackCalled = true;
                Assert.Equal(command.Object, c);
            };

            var result =await commandExecutor.ExecuteAsync(command.Object);

            Assert.Equal(testResult, result);
            Assert.True(callbackCalled);

            command.Verify(x => x.Validate(), Times.Once);
            command.Verify(x => x.ExecuteAsync(connection.Object, transaction), Times.Once);
            connection.Verify(x => x.BeginTransaction());
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
            command.Setup(x => x.ExecuteAsync(connection.Object, transaction.Object)).ReturnsAsync(testResult).Verifiable();

            var commandExecutor = new TxCommandExecutor(connection.Object);
            commandExecutor.Dispose();

            var ex = await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await commandExecutor.ExecuteAsync(command.Object));
            Assert.Equal(nameof(TxCommandExecutor), ex.ObjectName);

            command.Verify(x => x.ExecuteAsync(connection.Object, transaction.Object), Times.Never);
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
            command.Setup(x => x.ExecuteAsync(connection.Object, transaction.Object)).Throws(testException).Verifiable();

            var commandExecutor = new TxCommandExecutor(connection.Object);

            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await commandExecutor.ExecuteAsync(command.Object));
            Assert.Equal(ex, testException);

            transaction.Verify(x => x.Rollback(), Times.Once);
            command.Verify(x => x.ExecuteAsync(connection.Object, transaction.Object), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsyncWithResult_WhereValidationFails_RollsBackAndThrows()
        {
            var connection = new Mock<IDbConnection>();
            var transaction = new Mock<IDbTransaction>();
            transaction.Setup(x => x.Rollback()).Verifiable();
            connection.Setup(x => x.BeginTransaction()).Returns(transaction.Object);

            var command = new Mock<ITxCommand<string>>();
            var testException = new Exception("Test");
            command.Setup(x => x.Validate()).Throws(testException).Verifiable();

            var commandExecutor = new TxCommandExecutor(connection.Object);

            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await commandExecutor.ExecuteAsync(command.Object));
            Assert.Equal(ex, testException);

            transaction.Verify(x => x.Rollback(), Times.Once);
            command.Verify(x => x.Validate(), Times.Once);
            command.Verify(x => x.ExecuteAsync(connection.Object, transaction.Object), Times.Never);
        }

        [Fact]
        public async Task Dispose_GivenInCompleteTransaction_CommitsAndDisposedTransaction()
        {
            var transaction = new Mock<IDbTransaction>();
            transaction.Setup(x => x.Commit()).Verifiable();
            transaction.Setup(x => x.Dispose()).Verifiable();

            var connection = new Mock<IDbConnection>();
            connection.SetupGet(x => x.State).Returns(ConnectionState.Open);
            connection.Setup(x => x.BeginTransaction()).Returns(transaction.Object);

            var commandExecutor = new TxCommandExecutor(connection.Object);
            
            // Start a transaction.
            await commandExecutor.ExecuteAsync(Mock.Of<ITxCommand>());

            // Act
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

            var commandExecutor = new TxCommandExecutor(connection.Object);
            commandExecutor.Dispose(); // call dispose to mark executor as disposed.

            // Act
            commandExecutor.Dispose();

            // Ensure transaction is only called in setup dispose call.
            transaction.Verify(x => x.Commit(), Times.Never);
            transaction.Verify(x => x.Dispose(), Times.Never);
        }

        [Fact]
        public void Dispose_WithUninitializedTransaction_DoesNotThrowNullReference()
        {
            var commandExecutor = new TxCommandExecutor(Mock.Of<IDbConnection>());
            commandExecutor.Dispose();
        }
    }
}
