using FluentAssertions;
using Moq;
using System;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace TxCommand.Sql.Tests
{
    public class TransactionProviderTests
    {
        [Fact]
        public void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new TransactionProvider(null, new SqlOptions()));
            ex.ParamName.Should().Be("connection");
        }

        [Fact]
        public void Ctor_GivenNullOptions_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new TransactionProvider(Mock.Of<IDbConnection>(), null));
            ex.ParamName.Should().Be("options");
        }

        [Fact]
        public async Task EnsureTransactionAsync_WhereNoTransactionIsActive_BeginsNewTransaction()
        {
            var database = new Mock<IDbConnection>();
            database.SetupGet(x => x.State).Returns(ConnectionState.Closed);
            database.Setup(x => x.Open()).Verifiable();

            var transaction = Mock.Of<IDbTransaction>();
            database.Setup(x => x.BeginTransaction(IsolationLevel.ReadUncommitted)).Returns(transaction).Verifiable();

            var provider = new TransactionProvider(database.Object, new SqlOptions{IsolationLevel = IsolationLevel.ReadUncommitted});
            await provider.EnsureTransactionAsync(CancellationToken.None);

            database.VerifyAll();
        }

        [Fact]
        public async Task EnsureTransactionAsync_WhereTransactionIsActive_DoesNotBeginNewTransaction()
        {
            var database = new Mock<IDbConnection>();
            database.SetupGet(x => x.State).Returns(ConnectionState.Open);

            var provider = new TransactionProvider(database.Object, new SqlOptions { IsolationLevel = IsolationLevel.ReadUncommitted });
            provider.GetType().GetField("_transaction", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(provider, Mock.Of<IDbTransaction>());

            await provider.EnsureTransactionAsync(CancellationToken.None);

            database.VerifyAll();
            database.Verify(x => x.BeginTransaction(), Times.Never);
        }

        [Fact]
        public async Task EnsureTransactionAsync_WhereProviderIsDisposed_ThrowsObjectDisposedException()
        {
            var database = new Mock<IDbConnection>();

            var provider = new TransactionProvider(database.Object, new SqlOptions { IsolationLevel = IsolationLevel.ReadUncommitted });
            provider.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(provider, true);

            await Assert.ThrowsAsync<ObjectDisposedException>(() => provider.EnsureTransactionAsync(CancellationToken.None));
            
            database.Verify(x => x.BeginTransaction(), Times.Never);
        }
        
        [Fact]
        public async Task EnsureTransactionAsync_WhereCancellationIsRequested_ThrowsOperationCancelledException()
        {
            var database = new Mock<IDbConnection>();
            var provider = new TransactionProvider(database.Object, new SqlOptions { IsolationLevel = IsolationLevel.ReadUncommitted });

            var ctx = new CancellationTokenSource();
            var token = ctx.Token;
            ctx.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(() => provider.EnsureTransactionAsync(token));
            
            database.Verify(x => x.BeginTransaction(), Times.Never);
        }

        [Fact]
        public async Task CommitAsync_WhereTransactionIsActive_Commits()
        {
            var transaction = new Mock<IDbTransaction>();
            transaction.Setup(x => x.Commit()).Verifiable();
            
            var provider = new TransactionProvider(Mock.Of<IDbConnection>(), new SqlOptions { IsolationLevel = IsolationLevel.ReadUncommitted });
            provider.GetType().GetField("_transaction", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(provider, transaction.Object);

            await provider.CommitAsync(CancellationToken.None);

            transaction.VerifyAll();
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public async Task CommitAsync_WhereProviderIsDisposed_ThrowsObjectDisposedException()
        {
            var database = new Mock<IDbConnection>();

            var provider = new TransactionProvider(database.Object, new SqlOptions { IsolationLevel = IsolationLevel.ReadUncommitted });
            provider.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(provider, true);

            await Assert.ThrowsAsync<ObjectDisposedException>(() => provider.CommitAsync(CancellationToken.None));
        }
        
        [Fact]
        public async Task CommitAsync_WhereCancellationIsRequested_ThrowsOperationCancelledException()
        {
            var transaction = new Mock<IDbTransaction>();
            transaction.Setup(x => x.Rollback()).Verifiable();

            var provider = new TransactionProvider(Mock.Of<IDbConnection>(), new SqlOptions { IsolationLevel = IsolationLevel.ReadUncommitted });
            provider.GetType().GetField("_transaction", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(provider, transaction.Object);

            var ctx = new CancellationTokenSource();
            var token = ctx.Token;
            ctx.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(() => provider.CommitAsync(token));
            
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public void Commit_WhereTransactionIsActive_Commits()
        {
            var transaction = new Mock<IDbTransaction>();
            transaction.Setup(x => x.Commit()).Verifiable();

            var provider = new TransactionProvider(Mock.Of<IDbConnection>(), new SqlOptions { IsolationLevel = IsolationLevel.ReadUncommitted });
            provider.GetType().GetField("_transaction", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(provider, transaction.Object);

            provider.Commit();

            transaction.VerifyAll();
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Commit_WhereProviderIsDisposed_ThrowsObjectDisposedException()
        {
            var database = new Mock<IDbConnection>();

            var provider = new TransactionProvider(database.Object, new SqlOptions { IsolationLevel = IsolationLevel.ReadUncommitted });
            provider.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(provider, true);

            Assert.Throws<ObjectDisposedException>(() => provider.Commit());
        }
        
        [Fact]
        public void Commit_WhereCancellationIsRequested_ThrowsOperationCancelledException()
        {
            var transaction = new Mock<IDbTransaction>();
            transaction.Setup(x => x.Rollback()).Verifiable();

            var provider = new TransactionProvider(Mock.Of<IDbConnection>(), new SqlOptions { IsolationLevel = IsolationLevel.ReadUncommitted });
            provider.GetType().GetField("_transaction", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(provider, transaction.Object);

            var ctx = new CancellationTokenSource();
            var token = ctx.Token;
            ctx.Cancel();

            Assert.Throws<OperationCanceledException>(() => provider.Commit(token));
            
            transaction.Verify(x => x.Commit(), Times.Never);
        }

        [Fact]
        public async Task RollbackAsync_WhereTransactionIsActive_RollsBack()
        {
            var transaction = new Mock<IDbTransaction>();
            transaction.Setup(x => x.Rollback()).Verifiable();

            var provider = new TransactionProvider(Mock.Of<IDbConnection>(), new SqlOptions { IsolationLevel = IsolationLevel.ReadUncommitted });
            provider.GetType().GetField("_transaction", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(provider, transaction.Object);

            await provider.RollbackAsync(CancellationToken.None);

            transaction.VerifyAll();
            transaction.Verify(x => x.Rollback(), Times.Once);
        }

        [Fact]
        public async Task RollbackAsync_WhereProviderIsDisposed_ThrowsObjectDisposedException()
        {
            var database = new Mock<IDbConnection>();

            var provider = new TransactionProvider(database.Object, new SqlOptions { IsolationLevel = IsolationLevel.ReadUncommitted });
            provider.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(provider, true);

            await Assert.ThrowsAsync<ObjectDisposedException>(() => provider.RollbackAsync(CancellationToken.None));
        }
        
        [Fact]
        public async Task RollbackAsync_WhereCancellationIsRequested_ThrowsOperationCancelledException()
        {
            var transaction = new Mock<IDbTransaction>();
            transaction.Setup(x => x.Rollback()).Verifiable();

            var provider = new TransactionProvider(Mock.Of<IDbConnection>(), new SqlOptions { IsolationLevel = IsolationLevel.ReadUncommitted });
            provider.GetType().GetField("_transaction", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(provider, transaction.Object);


            var ctx = new CancellationTokenSource();
            var token = ctx.Token;
            ctx.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(() => provider.RollbackAsync(token));
            
            transaction.Verify(x => x.Rollback(), Times.Never);
        }

        [Fact]
        public void GetExecutionArguments_WhereTransactionIsNotActive_ReturnsDatabase()
        {
            var database = Mock.Of<IDbConnection>();

            var provider = new TransactionProvider(database, new SqlOptions { IsolationLevel = IsolationLevel.ReadUncommitted });
            provider.GetType().GetField("_transaction", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(provider, null);

            var (db, tx) = provider.GetExecutionArguments();
            db.Should().Be(database);
            tx.Should().BeNull();
        }

        [Fact]
        public void GetExecutionArguments_WhereTransactionIsNotActive_ReturnsDatabaseAndTransaction()
        {
            var database = Mock.Of<IDbConnection>();
            var transaction = Mock.Of<IDbTransaction>();

            var provider = new TransactionProvider(database, new SqlOptions { IsolationLevel = IsolationLevel.ReadUncommitted });
            provider.GetType().GetField("_transaction", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(provider, transaction);

            var (db, tx) = provider.GetExecutionArguments();
            db.Should().Be(database);
            tx.Should().Be(transaction);
        }

        [Fact]
        public void GetExecutionArguments_WhereProviderIsDisposed_ThrowsObjectDisposedException()
        {
            var database = new Mock<IDbConnection>();

            var provider = new TransactionProvider(database.Object, new SqlOptions { IsolationLevel = IsolationLevel.ReadUncommitted });
            provider.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(provider, true);

            Assert.Throws<ObjectDisposedException>(() => provider.GetExecutionArguments());
        }
    }
}
