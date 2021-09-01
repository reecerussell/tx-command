using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TxCommand.Abstractions;
using TxCommand.Abstractions.Exceptions;
using Xunit;

namespace TxCommand.Tests
{
    public interface ITestDatabase {}

    public interface ITestTransaction {}

    public class SessionTests
    {
        #region TxCommand

        [Fact]
        public async Task ExecuteAsync_GivenTxCommand_ExecutesSuccessfully()
        {
            var database = Mock.Of<ITestDatabase>();
            var transaction = Mock.Of<ITestTransaction>();

            var provider = new Mock<ITransactionProvider<ITestDatabase, ITestTransaction>>();
            provider.Setup(x => x.EnsureTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            provider.Setup(x => x.GetExecutionArguments())
                .Returns((database, transaction));
            
            provider.Setup(x => x.Commit())
                .Verifiable();

            var command = new Mock<ITxCommand<ITestDatabase, ITestTransaction>>();
            command.Setup(x => x.Validate()).Verifiable();
            command.Setup(x => x.ExecuteAsync(database, transaction))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var callbackCalled = false;

            using (var session = new Session<ITestDatabase, ITestTransaction>(provider.Object))
            {
                session.OnExecuted += cmd => callbackCalled = cmd == command.Object;

                var task = session.ExecuteAsync(command.Object);
                await task;

                task.IsCompletedSuccessfully.Should().BeTrue();
            }

            callbackCalled.Should().BeTrue();

            provider.VerifyAll();
            provider.Verify(x => x.Commit(), Times.Once);
            provider.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            provider.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);

            command.VerifyAll();
            command.Verify(x => x.ExecuteAsync(It.IsAny<ITestDatabase>(), It.IsAny<ITestTransaction>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_GivenTxCommandWhereValidateFails_ExecutesSuccessfully()
        {
            var provider = new Mock<ITransactionProvider<ITestDatabase, ITestTransaction>>();
            provider.Setup(x => x.EnsureTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            provider.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var command = new Mock<ITxCommand<ITestDatabase, ITestTransaction>>();
            var testException = new Exception("test");
            command.Setup(x => x.Validate())
                .Throws(testException)
                .Verifiable();

            using (var session = new Session<ITestDatabase, ITestTransaction>(provider.Object))
            {
                var ex = await Assert.ThrowsAsync<Exception>(() => session.ExecuteAsync(command.Object));
                ex.Should().Be(testException);
            }

            provider.VerifyAll();
            provider.Verify(x => x.Commit(), Times.Never);
            provider.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            provider.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);

            command.VerifyAll();
            command.Verify(x => x.ExecuteAsync(It.IsAny<ITestDatabase>(), It.IsAny<ITestTransaction>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_GivenTxCommandWhereExecuteFails_ExecutesSuccessfully()
        {
            var database = Mock.Of<ITestDatabase>();
            var transaction = Mock.Of<ITestTransaction>();

            var provider = new Mock<ITransactionProvider<ITestDatabase, ITestTransaction>>();
            provider.Setup(x => x.EnsureTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            provider.Setup(x => x.GetExecutionArguments())
                .Returns((database, transaction));

            provider.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var command = new Mock<ITxCommand<ITestDatabase, ITestTransaction>>();
            var testException = new Exception("test");
            command.Setup(x => x.Validate()).Verifiable();
            command.Setup(x => x.ExecuteAsync(database, transaction))
                .Throws(testException)
                .Verifiable();

            using (var session = new Session<ITestDatabase, ITestTransaction>(provider.Object))
            {
                var ex = await Assert.ThrowsAsync<Exception>(() => session.ExecuteAsync(command.Object));
                ex.Should().Be(testException);
            }

            provider.VerifyAll();
            provider.Verify(x => x.Commit(), Times.Never);
            provider.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            provider.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);

            command.VerifyAll();
            command.Verify(x => x.ExecuteAsync(It.IsAny<ITestDatabase>(), It.IsAny<ITestTransaction>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_GivenNullTxCommand_ThrowsArgumentNullException()
        {
            var provider = Mock.Of<ITransactionProvider<ITestDatabase, ITestTransaction>>();
            var session = new Session<ITestDatabase, ITestTransaction>(provider);

            ITxCommand<ITestDatabase, ITestTransaction> command = null;
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => session.ExecuteAsync(command));
            ex.ParamName.Should().Be("command");
        }

        [Fact]
        public async Task ExecuteAsync_GivenTxCommandWhereSessionHasBeenDisposed_ThrowsObjectDisposedException()
        {
            var provider = Mock.Of<ITransactionProvider<ITestDatabase, ITestTransaction>>();
            var session = new Session<ITestDatabase, ITestTransaction>(provider);

            session.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(session, true);

            var command = Mock.Of<ITxCommand<ITestDatabase, ITestTransaction>>();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => session.ExecuteAsync(command));
        }

        #endregion

        #region TxCommandT

        [Fact]
        public async Task ExecuteAsync_GivenTxCommandT_ExecutesSuccessfully()
        {
            var database = Mock.Of<ITestDatabase>();
            var transaction = Mock.Of<ITestTransaction>();

            var provider = new Mock<ITransactionProvider<ITestDatabase, ITestTransaction>>();
            provider.Setup(x => x.EnsureTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            provider.Setup(x => x.GetExecutionArguments())
                .Returns((database, transaction));

            provider.Setup(x => x.Commit())
                .Verifiable();

            var command = new Mock<ITxCommand<ITestDatabase, ITestTransaction, string>>();
            const string testResult = "Hello World";
            command.Setup(x => x.Validate()).Verifiable();
            command.Setup(x => x.ExecuteAsync(database, transaction))
                .ReturnsAsync(testResult)
                .Verifiable();

            var callbackCalled = false;

            using (var session = new Session<ITestDatabase, ITestTransaction>(provider.Object))
            {
                session.OnExecuted += cmd => callbackCalled = cmd == command.Object;

                var task = session.ExecuteAsync(command.Object);
                await task;

                task.IsCompletedSuccessfully.Should().BeTrue();
                task.Result.Should().Be(testResult);
            }

            callbackCalled.Should().BeTrue();

            provider.VerifyAll();
            provider.Verify(x => x.Commit(), Times.Once);
            provider.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            provider.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);

            command.VerifyAll();
            command.Verify(x => x.ExecuteAsync(It.IsAny<ITestDatabase>(), It.IsAny<ITestTransaction>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_GivenTxCommandTWhereValidateFails_ExecutesSuccessfully()
        {
            var provider = new Mock<ITransactionProvider<ITestDatabase, ITestTransaction>>();
            provider.Setup(x => x.EnsureTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            provider.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var command = new Mock<ITxCommand<ITestDatabase, ITestTransaction, string>>();
            var testException = new Exception("test");
            command.Setup(x => x.Validate())
                .Throws(testException)
                .Verifiable();

            using (var session = new Session<ITestDatabase, ITestTransaction>(provider.Object))
            {
                var ex = await Assert.ThrowsAsync<Exception>(() => session.ExecuteAsync(command.Object));
                ex.Should().Be(testException);
            }

            provider.VerifyAll();
            provider.Verify(x => x.Commit(), Times.Never);
            provider.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            provider.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);

            command.VerifyAll();
            command.Verify(x => x.ExecuteAsync(It.IsAny<ITestDatabase>(), It.IsAny<ITestTransaction>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_GivenTxCommandTWhereExecuteFails_ExecutesSuccessfully()
        {
            var database = Mock.Of<ITestDatabase>();
            var transaction = Mock.Of<ITestTransaction>();

            var provider = new Mock<ITransactionProvider<ITestDatabase, ITestTransaction>>();
            provider.Setup(x => x.EnsureTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            provider.Setup(x => x.GetExecutionArguments())
                .Returns((database, transaction));

            provider.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var command = new Mock<ITxCommand<ITestDatabase, ITestTransaction, string>>();
            var testException = new Exception("test");
            command.Setup(x => x.Validate()).Verifiable();
            command.Setup(x => x.ExecuteAsync(database, transaction))
                .Throws(testException)
                .Verifiable();

            using (var session = new Session<ITestDatabase, ITestTransaction>(provider.Object))
            {
                var ex = await Assert.ThrowsAsync<Exception>(() => session.ExecuteAsync(command.Object));
                ex.Should().Be(testException);
            }

            provider.VerifyAll();
            provider.Verify(x => x.Commit(), Times.Never);
            provider.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            provider.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);

            command.VerifyAll();
            command.Verify(x => x.ExecuteAsync(It.IsAny<ITestDatabase>(), It.IsAny<ITestTransaction>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_GivenNullTxCommandT_ThrowsArgumentNullException()
        {
            var provider = Mock.Of<ITransactionProvider<ITestDatabase, ITestTransaction>>();
            var session = new Session<ITestDatabase, ITestTransaction>(provider);

            ITxCommand<ITestDatabase, ITestTransaction, string> command = null;
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => session.ExecuteAsync(command));
            ex.ParamName.Should().Be("command");
        }

        [Fact]
        public async Task ExecuteAsync_GivenTxCommandTWhereSessionHasBeenDisposed_ThrowsObjectDisposedException()
        {
            var provider = Mock.Of<ITransactionProvider<ITestDatabase, ITestTransaction>>();
            var session = new Session<ITestDatabase, ITestTransaction>(provider);

            session.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(session, true);

            var command = Mock.Of<ITxCommand<ITestDatabase, ITestTransaction, string>>();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => session.ExecuteAsync(command));
        }

        #endregion

        #region CommitAsync

        [Fact]
        public async Task CommitAsync_WhereNotCompleted_CommitsProvider()
        {
            var provider = new Mock<ITransactionProvider<ITestDatabase, ITestTransaction>>();
            provider.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                .Verifiable();

            var session = new Session<ITestDatabase, ITestTransaction>(provider.Object);
            
            session.GetType().GetField("_completed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(session, false);

            var callbackInvoked = false;
            session.OnCommitted += () => callbackInvoked = true;

            await session.CommitAsync();

            callbackInvoked.Should().BeTrue();

            provider.VerifyAll();
            provider.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CommitAsync_WhereCompleted_ThrowsTransactionNotStartedException()
        {
            var provider = new Mock<ITransactionProvider<ITestDatabase, ITestTransaction>>();

            var session = new Session<ITestDatabase, ITestTransaction>(provider.Object);

            session.GetType().GetField("_completed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(session, true);

            await Assert.ThrowsAsync<TransactionNotStartedException>(() => session.CommitAsync());

            provider.VerifyAll();
            provider.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CommitAsync_WhereDisposed_ThrowsObjectDisposedException()
        {
            var provider = new Mock<ITransactionProvider<ITestDatabase, ITestTransaction>>();

            var session = new Session<ITestDatabase, ITestTransaction>(provider.Object);

            session.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(session, true);

            await Assert.ThrowsAsync<ObjectDisposedException>(() => session.CommitAsync());

            provider.VerifyAll();
            provider.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region Commit

        [Fact]
        public void Commit_WhereNotCompleted_CommitsProvider()
        {
            var provider = new Mock<ITransactionProvider<ITestDatabase, ITestTransaction>>();
            provider.Setup(x => x.Commit())
                .Verifiable();

            var session = new Session<ITestDatabase, ITestTransaction>(provider.Object);

            session.GetType().GetField("_completed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(session, false);

            var callbackInvoked = false;
            session.OnCommitted += () => callbackInvoked = true;

            session.Commit();

            callbackInvoked.Should().BeTrue();

            provider.VerifyAll();
            provider.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Commit_WhereCompleted_ThrowsTransactionNotStartedException()
        {
            var provider = new Mock<ITransactionProvider<ITestDatabase, ITestTransaction>>();

            var session = new Session<ITestDatabase, ITestTransaction>(provider.Object);

            session.GetType().GetField("_completed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(session, true);

            Assert.Throws<TransactionNotStartedException>(() => session.Commit());

            provider.VerifyAll();
            provider.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Commit_WhereDisposed_ThrowsObjectDisposedException()
        {
            var provider = new Mock<ITransactionProvider<ITestDatabase, ITestTransaction>>();

            var session = new Session<ITestDatabase, ITestTransaction>(provider.Object);

            session.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(session, true);

            Assert.Throws<ObjectDisposedException>(() => session.Commit());

            provider.VerifyAll();
            provider.Verify(x => x.Commit(), Times.Never);
        }

        #endregion

        #region RollbackAsync

        [Fact]
        public async Task RollbackAsync_WhereNotCompleted_CommitsProvider()
        {
            var provider = new Mock<ITransactionProvider<ITestDatabase, ITestTransaction>>();
            provider.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
                .Verifiable();

            var session = new Session<ITestDatabase, ITestTransaction>(provider.Object);

            session.GetType().GetField("_completed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(session, false);

            var callbackInvoked = false;
            session.OnRolledBack += () => callbackInvoked = true;

            await session.RollbackAsync();

            callbackInvoked.Should().BeTrue();

            provider.VerifyAll();
            provider.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RollbackAsync_WhereCompleted_ThrowsTransactionNotStartedException()
        {
            var provider = new Mock<ITransactionProvider<ITestDatabase, ITestTransaction>>();

            var session = new Session<ITestDatabase, ITestTransaction>(provider.Object);

            session.GetType().GetField("_completed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(session, true);

            await Assert.ThrowsAsync<TransactionNotStartedException>(() => session.RollbackAsync());

            provider.VerifyAll();
            provider.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RollbackAsync_WhereDisposed_ThrowsObjectDisposedException()
        {
            var provider = new Mock<ITransactionProvider<ITestDatabase, ITestTransaction>>();

            var session = new Session<ITestDatabase, ITestTransaction>(provider.Object);

            session.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(session, true);

            await Assert.ThrowsAsync<ObjectDisposedException>(() => session.RollbackAsync());

            provider.VerifyAll();
            provider.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region Dispose

        [Fact]
        public void Dispose_WhereNotAlreadyDisposed_Commits()
        {
            var provider = new Mock<ITransactionProvider<ITestDatabase, ITestTransaction>>();
            provider.Setup(x => x.Commit())
                .Verifiable();

            var session = new Session<ITestDatabase, ITestTransaction>(provider.Object);

            session.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(session, false);
            session.GetType().GetField("_completed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(session, false);

            session.Dispose();

            provider.VerifyAll();
            provider.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Dispose_WhereAlreadyDisposed_DoesNothing()
        {
            var provider = new Mock<ITransactionProvider<ITestDatabase, ITestTransaction>>();

            var session = new Session<ITestDatabase, ITestTransaction>(provider.Object);

            session.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(session, true);
            session.GetType().GetField("_completed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(session, false);

            session.Dispose();

            provider.VerifyAll();
            provider.Verify(x => x.Commit(), Times.Never);
        }

        #endregion
    }
}
